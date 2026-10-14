using System;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace ItemStacking;

// 合并决策【单一源】
//
// ★ 为什么需要这一层
//   0.9.9 及以前，「这一堆能不能合并」的判定被【复制】到三个挂点
//   （MayStack / GetStackSlot / TryAcceptOnce），三处各自内联一套判据、顺序还不一致
//   ⇒ 同一个问题有三套答案、改一处漏两处。
//   本类把决策收敛成唯一入口 Evaluate(ctx, out reason)，三挂点只负责「搬运」。
//
// ★ 固定判定顺序（三种路径共用）
//   0) 总闸 StackEnabled=false           → Allow（完全交还原生）
//   0.5) 机器容器                         → Deny（数据保护）
//   1) 容器（含内部物品）                 → Deny（数据保护）
//   1.5) 状态一致性 StatesEqual           → Deny（不可豁免的最终硬闸门）
//   1.6) tag 值域校验 AtDefaultState      → Deny（补充闸门）
//   2) 物品黑名单 GetBlacklistReason       → Deny（id 级）
//   3) 容器黑名单 GetExcludeReason（双端） → Deny（便利）
//   4) 容器白名单 GetScopeReason           → Deny（便利）
//   5) 空 identifier                       → Deny
//   6) 其余                                 → Allow
//
// ★ 定序理由
//   · 1.5 在 3/4/5 前：状态不一致是物理事实，任何名单都不能放行。
//   · 3 在 4 前：容器黑名单赢（双名单命中黑名单优先）。
//   · 5 在最末：只在其余全通过、还差同名判定时才需要。
public partial class Main
{
    internal enum MergeVerdict { Allow, Deny, Unknown }

    // 决策上下文。
    //   容器归属只有一个权威来源 slotInventory：
    //     拖放路径 = 目标物品所在容器（__instance.parentInventory）；
    //     快移路径 = slot.inventory（落点槽所在容器）。
    //   本类统一由调用方在构造时把「权威容器」填进来，判据只读它，消除分裂。
    internal readonly struct MergeContext
    {
        internal readonly GameInventory slotInventory;
        internal readonly GameItem target;   // null = 空槽（属移动非合并）
        internal readonly GameItem source;

        internal MergeContext(GameInventory slotInventory, GameItem target, GameItem source)
        {
            this.slotInventory = slotInventory;
            this.target = target;
            this.source = source;
        }
    }

    // 合并决策的【唯一入口】。
    //   返回 Allow/Deny/Unknown；reason 给出可读原因（Allow 时为 null）。
    //   判据故障位机制：数据保护组故障 → Deny；便利组故障 → Allow。
    internal static MergeVerdict Evaluate(in MergeContext ctx, out string reason)
    {
        reason = null;
        try
        {
            // 0) 总闸：mod 关闭 = 完全交还原生。
            if (!StackEnabled) return MergeVerdict.Allow;

            // 空槽（target == null）是「移动」不是「合并」——不归本决策管。
            if (ctx.target == null) return MergeVerdict.Allow;

            // 清故障位，避免上一轮残留污染本轮判定。
            ConsumeJudgeFault(FaultProtection);
            ConsumeJudgeFault(FaultConvenience);

            // ── 0.5) 机器容器：禁止合并（数据保护）──
            //   原版机器逻辑假设 unitCount == 1（熔炉消耗/解序完成/打印升级等），
            //   堆入堆叠物品会触发崩坏或丢件。Deny → MayStack 返 false → 原生只走空位逐格填充。
            if (IsMachineContainer(ctx.slotInventory))
            {
                if (ConsumeJudgeFault(FaultProtection)) { WarnPolicyFault("mp.fault.machine", true); return MergeVerdict.Deny; }
                reason = "机器容器内不允许堆叠";
                return MergeVerdict.Deny;
            }
            if (ConsumeJudgeFault(FaultProtection))
            { WarnPolicyFault("mp.fault.machine", true); return MergeVerdict.Deny; }

            // ── 1) 容器（数据保护）──
            if (IsContainer(ctx.target) || IsContainer(ctx.source))
            {
                if (ConsumeJudgeFault(FaultProtection)) { WarnPolicyFault("mp.fault.container", true); return MergeVerdict.Deny; }
                reason = "容器（含内部物品）";
                return MergeVerdict.Deny;
            }
            if (ConsumeJudgeFault(FaultProtection))
            { WarnPolicyFault("mp.fault.container", true); return MergeVerdict.Deny; }

            // ── 1.5) 状态一致性（数据保护）【不可豁免的最终硬闸门】──
            //   位置：必须早于「允许」出口，且在白名单 / 物品名单【之前】——
            //   状态不一致是物理事实，任何名单都不能放行。
            bool statesEqual = StatesEqual(ctx.target, ctx.source);
            if (ConsumeJudgeFault(FaultProtection))
            { WarnPolicyFault("mp.fault.statecmp", true); return MergeVerdict.Deny; }
            if (!statesEqual)
            {
                reason = "状态不同（电量/纯度/品质/磨损等不一致）";
                return MergeVerdict.Deny;
            }

            // ── 1.6) tag 值域校验（数据保护）【状态相等的补充闸门】──
            //   背景：GetSplitItem 拆分时用 DirectoryMaster.Item(id) 重新生成默认物品
            //   ⇒ 拆出的物品带默认 tag（满电/满耐久/默认纯度），而非原堆实际 tag。
            //   本判据要求：关键 tag 的当前值必须【等于该 identifier 的默认模板值】才可堆，
            //   从根上杜绝拆分污染。
            //   零值变体（0.14.0）：赃物热度 STOLEN_VALUE_INT 必须缺省或 0。
            //   ★ 先消费故障位、再判结果（防「标记了故障位却返回 true」的 fail-open 残留污染下一判据）。
            if (ValueGateEnabled)
            {
                bool atDefault = AtDefaultState(ctx.target, ctx.source);
                if (ConsumeJudgeFault(FaultProtection))
                { WarnPolicyFault("mp.fault.valuegate", true); return MergeVerdict.Deny; }
                if (!atDefault)
                {
                    reason = "偏离默认状态（非满电/非满耐久/非默认纯度/非默认品质/非0热度赃物等，拆分会污染）";
                    return MergeVerdict.Deny;
                }
            }

            // ── 2) 物品黑名单（按 id；类判据已停用）（数据保护）──
            var block = GetBlacklistReason(ctx.target, ctx.source);
            if (ConsumeJudgeFault(FaultProtection)) { WarnPolicyFault("mp.fault.blacklist", true); return MergeVerdict.Deny; }
            if (block != null) { reason = block; return MergeVerdict.Deny; }

            // ── 3) 容器黑名单（便利）──
            var exclude = GetExcludeReason(ctx.target, ctx.source);
            if (ConsumeJudgeFault(FaultConvenience)) { WarnPolicyFault("mp.fault.exclude", false); return MergeVerdict.Allow; }
            if (exclude != null) { reason = exclude; return MergeVerdict.Deny; }

            // ── 4) 容器白名单（便利）──
            var scope = GetScopeReason(ctx.target);
            if (ConsumeJudgeFault(FaultConvenience)) { WarnPolicyFault("mp.fault.scope", false); return MergeVerdict.Allow; }
            if (scope != null) { reason = scope; return MergeVerdict.Deny; }

            // ── 5) 空 identifier：无法判定同名，交给原生（恒 false）。──
            if (string.IsNullOrEmpty(ctx.target.identifier) ||
                string.IsNullOrEmpty(ctx.source.identifier))
            {
                reason = "空 identifier";
                return MergeVerdict.Deny;
            }

            // 6) 其余通过。
            return MergeVerdict.Allow;
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            // 框架自身异常（非判据内部）—— 保守判 Deny（数据安全优先）。
            LogOnce("mergepolicy", Level.Warn, $"合并决策框架异常，本次拦截（保守）: {e.Message}");
            reason = "决策框架异常";
            return MergeVerdict.Deny;
        }
    }

    // 判据故障告警（一次性，避免刷屏）。
    //   分组由调用点【显式传入】protective —— 不靠 key 子串猜测。
    private static void WarnPolicyFault(string key, bool protective)
    {
        string group = protective ? "数据保护类" : "便利类";
        string action = protective ? "本次拦截（宁可拦不可丢）" : "本次放行（不影响游玩）";
        LogOnce(key, Level.Warn, $"[合并决策] {group}判据异常，{action}（后续同类故障不再重复告警，详见 [防护降级]）");
    }

    // 便捷封装：只取布尔「是否允许合并」。
    internal static bool EvaluateAllow(in MergeContext ctx, out string reason)
    {
        var v = Evaluate(ctx, out reason);
        return v != MergeVerdict.Deny;
    }

    // 回滚开关（cfg z9_UseMergePolicy，默认 true）。
    //   false ⇒ 三挂点退回 0.9.9 各自内联判据的老路径。
    //   ★ 老路径【不含状态一致性闸门】也【不含机器容器 Deny】——
    //     关掉 = 连这两道保护一起绕过，会放行状态不同的物品合并 + 机器容器内不再禁并。
    //     本开关仅供排障，切勿长期关闭。
    internal static bool UseMergePolicy => _useMergePolicy == null || _useMergePolicy.Value;
}
using System;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace ItemStacking;

public partial class Main
{
    // ══════════════════════════════════════════════════════════════════════
    //  0.10.0 · 阶段1：合并决策【单一源】
    //
    //  ── 为什么要这一层（重构动因）──
    //  0.9.9 及以前，「这一堆能不能合并」的判定被【复制】到三个挂点：
    //    ① GameItem.MayStack          （拖放 · 闸门1）
    //    ② GameItem.GetStackSlot      （拖放 · 闸门2）
    //    ③ SlotMarker.TryAcceptOnce   （全路径瓶颈，经 GetMergeGateReason）
    //  三处各自内联一套判据，且【顺序不一致】：
    //    · MayStack / GetStackSlot : 黑名单 → 排除 → 作用域
    //    · TryAcceptOnce           : 排除 → 作用域 → 黑名单 → 容器
    //  ⇒ 同一个问题有三套答案、改一处漏两处 —— 这是「堆叠范围不可控」的结构性根因。
    //
    //  ── 本类做什么 ──
    //  把决策收敛成唯一入口 MergePolicy.Evaluate(ctx, out reason)，输出：
    //    Allow   = 允许合并
    //    Deny    = 拒绝合并（reason 为人类可读原因）
    //    Unknown = 保留位（当前判据异常一律走 Deny，见 Evaluate 的 catch；暂无返回点）
    //  三个挂点只负责「搬运」，不再内联判据 —— 改规则只动本文件一处。
    //
    //  ── 固定判定顺序（定稿，三种路径共用）──
//    0. 总闸 StackEnabled == false            → Allow（完全交还原生）
//    1.5 机器容器 IsMachineContainer(slotInventory) → Deny（数据保护；方案 v2，
//       代码段标签 0.5，实居容器判据之前——机器禁并优先于「含物品容器」判据）
//    2. 容器 IsContainer(target|source)        → Deny
    //    2.5 ★ 状态一致性 StatesEqual(target,source) → Deny（不可豁免的最终硬闸门）
    //    3. 物品黑名单 GetBlacklistReason          → Deny（id 级；类判据已停用）
    //    4. 容器黑名单 GetExcludeReason（双端）     → Deny
    //    5. 容器白名单 GetScopeReason              → Deny
    //    6. 空 identifier                         → Deny
    //    7. 其余                                  → Allow
    //
    //  定序理由：
    //    · 2.5 在 3/4/5 前：状态不一致是物理事实，白名单 / 物品名单【都不能放行】。
    //    · 3 在 4/5 前：物品自身的独立内容（node*/module/*_keycard/文书…）优先于容器归属；
    //      GetBlacklistReason 现只做按 id 拦截（类判据已停用），属物品级语义，先行。
    //    · 4 在 5 前：容器黑名单赢（维持 0.9.3 用户拍板「双名单命中黑名单赢」）。
    //    · 6 在最末：只在其余全通过、还差同名判定时才需要。
    //    ★ 2026-10-14：原步骤 1「柜台」已随柜台禁堆功能删除。
    // ══════════════════════════════════════════════════════════════════════

    internal enum MergeVerdict { Allow, Deny, Unknown }

    // 决策上下文。★ 设计要点：容器归属只有一个权威来源 slotInventory ——
    //   拖放路径 = 目标物品所在容器（__instance.parentInventory）；
    //   快移路径 = slot.inventory（落点槽所在容器）。
    //   旧代码两处各自取容器（MayStack 取 parentInventory、TryAcceptOnce 取 slot.inventory），
    //   本类统一由调用方在构造时把「权威容器」填进来，判据只读它，消除分裂。
    internal readonly struct MergeContext
    {
        internal readonly GameInventory slotInventory;  // 权威容器（落点/目标所在）
        internal readonly GameItem target;              // 目标堆物品（null = 空槽，属移动非合并）
        internal readonly GameItem source;              // 要并入/拖动的物品

        internal MergeContext(GameInventory slotInventory, GameItem target, GameItem source)
        {
            this.slotInventory = slotInventory;
            this.target = target;
            this.source = source;
        }
    }

    // 合并决策的【唯一入口】。
    //   返回 Allow/Deny/Unknown；reason 给出可读原因（Allow 时为 null）。
    //   ★ 阶段2：逐判据调用，每个判据后立即查其【故障位】，按保护级别分级落点：
    //     数据保护组（容器/物品类判据）故障 → Deny（宁可拦不可丢）；
    //     便利组（容器黑白名单）故障 → Allow（不影响游玩）。
    //   ★ 2026-10-14：原「柜台判据已移交 MergeSteerer（导流）」一段随柜台禁堆功能删除；
    //     柜台现与普通容器同规则，本决策亦不再有任何柜台专属分支。
    //   ★ 判据顺序：容器 → 物品判据 → 容器黑名单 → 容器白名单 → 空id。
    //   故障位由判据函数在 catch 里置位（见 StackGates.cs 的 MarkJudgeFault）。
    internal static MergeVerdict Evaluate(in MergeContext ctx, out string reason)
    {
        reason = null;
        try
        {
            // 0) 总闸：mod 关闭 = 完全交还原生（不是"允许"，而是"不干预"）。
            if (!StackEnabled) return MergeVerdict.Allow;

            // 空槽（target == null）是「移动」不是「合并」——不归本决策管，直接放行。
            if (ctx.target == null) return MergeVerdict.Allow;

            // 清故障位，避免上一轮残留污染本轮判定。
            ConsumeJudgeFault(FaultProtection);
            ConsumeJudgeFault(FaultConvenience);

            // ★ 2026-10-14：原「柜台判据已移交 MergeSteerer（导流）」一段随柜台禁堆功能删除 ——
            //   柜台不再特殊，和普通容器走完全相同的判据链。

            // ── 0.5) ★ 机器容器：禁止合并（数据保护）──（方案 v2 §二 deny_vs_steer / §三 第1层）
            //   背景：原版机器逻辑假设 unitCount == 1（熔炉消耗/解序完成/打印升级等），
            //   堆入堆叠物品会触发崩坏或丢件；Deny → Patch_MayStack.Prefix 返回 false →
            //   原生拖放阶段③ `if (!MayStack) continue;` 跳过合并候选，只走空位逐格填充。
            //   ★ 与 Steerer 的关系：机器容器是「完全禁止合并」（数据保护），
            //     不走 MergeSteerer.ShouldSteerToEmptySlot（那是「允许合并但优先给空位」）。
            //   ★ 故障语义：IsMachineContainer 异常时返回 false 但已置 FaultProtection，
            //     下方第二处 ConsumeJudgeFault 兜住 → Deny（保守，宁可拦不可丢）。
            if (IsMachineContainer(ctx.slotInventory))
            {
                if (ConsumeJudgeFault(FaultProtection)) { WarnPolicyFault("mp.fault.machine", true); return MergeVerdict.Deny; }
                reason = "机器容器内不允许堆叠";
                return MergeVerdict.Deny;
            }
            if (ConsumeJudgeFault(FaultProtection))
            { WarnPolicyFault("mp.fault.machine", true); return MergeVerdict.Deny; }   // 判据故障 → 保守拦截

            // ── 1) 容器（数据保护）──
            if (IsContainer(ctx.target) || IsContainer(ctx.source))
            {
                if (ConsumeJudgeFault(FaultProtection)) { WarnPolicyFault("mp.fault.container", true); return MergeVerdict.Deny; }
                reason = "容器（含内部物品）";
                return MergeVerdict.Deny;
            }
            if (ConsumeJudgeFault(FaultProtection))
            { WarnPolicyFault("mp.fault.container", true); return MergeVerdict.Deny; }   // 保护故障 → 拦截

            // ── 1.5) ★ 1.0.0 状态一致性（数据保护）【不可豁免的最终硬闸门】──
            //   两件物品的 GameItem.state（TagSystem）必须完全相等才允许合并。
            //   ★ 位置：必须早于「允许」出口，且在白名单 / 物品名单【之前】——
            //     状态不一致是物理事实，任何名单都不能放行（核桃定稿：不可豁免）。
            //   ★ 故障语义（2026-10-13 更正）：StatesEqual 自身【不置故障位】—— 它比较抛异常时
            //     直接冒泡给本方法外层 catch，由那里统一判 Deny（保守）。下面这次 ConsumeJudgeFault
            //     的作用是【清掉可能残留的故障位】，防止其污染后续判据（与 0.5)/1)/1.6) 同构）。
            bool statesEqual = StatesEqual(ctx.target, ctx.source);
            if (ConsumeJudgeFault(FaultProtection))
            { WarnPolicyFault("mp.fault.statecmp", true); return MergeVerdict.Deny; }
            if (!statesEqual)
            {
                reason = "状态不同（电量/纯度/品质/磨损等不一致）";
                return MergeVerdict.Deny;
            }

            // ── 1.6) ★ 1.2.0 tag 值域校验（数据保护）【状态相等的补充闸门】──
            //   背景（2026-10-12 核桃反馈 + 反汇编取证）：
            //   GetSplitItem 拆分时【不用原物品 tag】，而是 DirectoryMaster.Item(identifier)
            //   重新生成默认物品 ⇒ 拆出来的物品带【默认 tag】（满电/满耐久/默认纯度/默认品质）。
            //   ⇒ 「状态相同但偏离默认」的物品（如两件 50% 电量电池）堆叠后，拆分会污染成满电。
            //   ★ 本判据要求：关键 tag（电量/耐久/纯度/品质/液体/次数等，见 ValueGate.cs）的
            //     当前值必须【等于该 identifier 的默认模板值】才可堆，从根上杜绝拆分污染。
            //   ★ 0.14.0：零值 tag 变体（Constants.ValueGateZeroTags，如赃物热度 STOLEN_VALUE_INT）
            //     同在本判据内：要求当前值缺省或 == 0（0 热度可堆 / 非 0 不可堆）——
            //     填补「热度相同但非 0 也能合并」的缺口（模板法对模板上没有的 tag 会 fail-open）。
            //   ★ 故障语义：比较抛异常 = 数据保护故障 ⇒ Deny（宁可拦不可丢）。
            if (ValueGateEnabled)
            {
                bool atDefault = AtDefaultState(ctx.target, ctx.source);
                // ★ P1 修复（2026-10-13）：先消费故障位、再判结果 —— 与 0.5)/1) 两处同构。
                //   原写法把 ConsumeJudgeFault 放在 `!AtDefaultState` 分支【内】：一旦判据
                //   「标记了故障位却返回 true」（值域模板获取失败的 fail-open 路径），
                //   该位会残留下来污染【下一个】判据（误报 mp.fault.blacklist 并 Deny）。
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
            // ★ 阶段2：框架自身异常（非判据内部）—— 保守判 Deny（数据安全优先）。
            LogOnce("mergepolicy", Level.Warn, $"合并决策框架异常，本次拦截（保守）: {e.Message}");
            reason = "决策框架异常";
            return MergeVerdict.Deny;
        }
    }

    // 判据故障告警（一次性，避免刷屏）。
    //   ★ §七 2.2（0.11.0）：去掉本类独立的 `_mpFaultWarned` 去重集合，直接交给 `WarnOnce`
    //     去重（key 沿用 `mp.fault.*`）：同一 key 仍只告警一次。
    //   ★ 分组由调用点【显式传入】protective，不再靠 key 子串猜测 —— 旧实现把
    //     `mp.fault.statecmp`（实际 return Deny）误判为「便利类·本次放行」，文案与行为相反。
    //   ★ M2（2026-10-12）：原名 WarnOnceOnce（WarnOnce 再包一层 Only Once）语义混乱，
    //     按职责改名 WarnPolicyFault = 「合并策略判据故障」告警。
    private static void WarnPolicyFault(string key, bool protective)
    {
        string group = protective ? "数据保护类" : "便利类";
        string action = protective ? "本次拦截（宁可拦不可丢）" : "本次放行（不影响游玩）";
        LogOnce(key, Level.Warn, $"[合并决策] {group}判据异常，{action}（后续同类故障不再重复告警，详见 [防护降级]）");
    }

    // 便捷封装：只取布尔「是否允许合并」。Unknown 暂按 Allow（fail-safe 放行，与 0.9.9 一致）。
    //   ★ 阶段2 会把 Unknown 的落点按「数据保护类 = Deny」细化，届时改这一处即可。
    internal static bool EvaluateAllow(in MergeContext ctx, out string reason)
    {
        var v = Evaluate(ctx, out reason);
        return v != MergeVerdict.Deny;
    }

    // 回滚开关（cfg z9_UseMergePolicy，默认 true）。
    //   false ⇒ 三挂点退回 0.9.9 各自内联判据的老路径（保留旧代码不删，可二态切换）。
    //   ★ 注意：老路径【不含状态一致性闸门 StatesEqual】（该方法 1.0.0 才引入）——
    //     关闭本开关 = 连状态闸门一起绕过，会放行状态不同的物品合并，故【会改变功能行为】。
    //   ★★ 2026-10-13 补充：【机器容器 Deny】（0.5 判据，0.13.0 引入）也只在【本条新路径】里，
    //     老路径（LegacyMergeGateReason / LegacyMayStack / LegacyGetStackSlot）没有该判据。
    //     而容量层的 Patch_MaxNumAdd 机器钳 1 是【独立补丁、不受本开关控制】。
    //     ⇒ z9_UseMergePolicy=false 会形成「半开状态」：机器容器内【不再禁并】（老路径放过合并），
    //        但每格仍只放 1 件 —— 合并到已有堆的路径会重新打开（bottle_printer 等在白名单里的
    //        机器会重新触发 DestroyFix 修过的崩坏）。本开关仅供排障，切勿长期关闭。
    internal static bool UseMergePolicy => _useMergePolicy == null || _useMergePolicy.Value;
}


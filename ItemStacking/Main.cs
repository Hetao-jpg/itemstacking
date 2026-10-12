using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Preferences;
using UnityEngine;

namespace ItemStacking;

public partial class Main : MelonMod
{
    // 静态日志句柄（嵌套补丁类是 static，用不了实例 LoggerInstance）。
    internal static MelonLogger.Instance Log = null!;

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;
        CreateConfigEntries();

        // ★ 0.11.2：旧中文 slot: token → 游戏原生 id 的一次性迁移（核桃 2026-10-11 定）。
        //   0.11.1 及以前 slot: 后面写的是中文槽位名，0.11.2 起改游戏原生 id → 老 cfg 里的
        //   中文 token 会静默匹配不上（堆叠在那些容器里失效）。这里做确定性替换并落盘，
        //   避免「升级后功能悄悄没了」。纯字符串替换，无游戏对象访问，失败也不影响启动。
        MigrateLegacyChineseSlotTokens();

        // ★ 0.12.2：RemoveLegacyConfigEntries / RemoveCfgKeys（废弃键文本清理）已整体删除。
        //   理由（2026-10-12 核桃定）：该功能为「cfg 整洁」而生，但实测失效（旧键仍残留）、
        //   且引入了「手写 TOML 解析 + 注释回溯删除 + 启动期整份 cfg 写盘」三个隐性隐患。
        //   残留死文字无功能影响（MelonLoader 忽略未注册键），靠「手动删一次」即可根治，
        //   不值得常驻一段有 bug 的脆弱逻辑。

        // ★ 0.9.9：MelonLoader 已在 HarmonyInit 阶段按类型容错自动 PatchAll 一次，
        //   这里是历史保留的第二遍（补丁实现均幂等，重复挂载无副作用）。
        //   必须包 try/catch：0.9.8 实机里 Patch_FurnaceConsume 解析失败抛异常，
        //   直接把 OnInitializeMelon 打断 —— ValidatePatches 和启动汇总日志全没跑。
        try
        {
            HarmonyInstance.PatchAll();
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            Log.Error("PatchAll 有补丁挂载失败（HarmonyInit 阶段已按类型容错挂过一遍，" +
                      "其余补丁不受影响）：" + e);
        }
        // ★ 0.10.0（阶段5）：加载内置黑白名单（代码默认 + 可选外部 JSON 覆盖）。
        //   必须在 ValidatePatches() 之前 —— 自检要核对名单 id / 打印采用来源。
        LoadBuiltinBlacklist();
        // ★ 1.0.0：忽略名单与内置黑白名单共用同一个 JSON 文件（item_blacklist.json），
        //   必须在 LoadBuiltinBlacklist() 之后（复用其查找/解析结果语义）。
        LoadIgnoredStateTags();
        ValidatePatches();
        LoggerInstance.Msg(
            $"Item Stacking 已加载：拖放堆叠 = {_enableStacking.Value}，堆叠上限 = {StackCap}，" +
            $"Ctrl+拖动整堆 = {_ctrlWholeStack.Value}，诊断日志 = {_diagLog.Value}。");
        LoggerInstance.Msg(
            $"  保护总闸 = {_stackBlacklist.Value}。" +
            $"容器白名单 = {(_containerWhitelist.Value == "" ? "【空 = 全不生效】" : _containerWhitelist.Value)}；" +
            $"容器黑名单 = {(_containerBlacklist.Value == "" ? "空" : _containerBlacklist.Value)}；" +
            $"柜台禁堆 = {_blockCounterStack.Value}。");
        // ★ 白名单 = 安全边界，空 = 全不生效（mod 等于未启用）。
        //   2026-10-12 起默认已含常用容器；走到这里说明用户手动清空过，醒目提示 + 给恢复串。
        if (string.IsNullOrWhiteSpace(_containerWhitelist.Value))
        {
            LoggerInstance.Warning(
                "[安全边界] ContainerWhitelist 为空 ⇒ 堆叠【全不生效】。" +
                "要恢复默认（常用容器集合），删除 cfg 中的 ContainerWhitelist 行后重启，" +
                "或直接填入下面这串（含逗号【必须用引号包起来】）：");
            LoggerInstance.Warning(
                "[安全边界] 默认串：" +
                "ContainerWhitelist = \"" + DefaultContainerWhitelist + "\"");
            LoggerInstance.Warning(
                "[安全边界] 机器容器（熔炉/解序器/其他 mod 自定义机器）请【不要】加入白名单 —— " +
                "它们假设「一槽=一实例」，堆叠会导致物品丢失。");
        }
        LoggerInstance.Msg(
            $"  物品黑名单·自定义 = {(_itemBlacklist.Value == "" ? "空" : _itemBlacklist.Value)}。");
        // ★ 2026-10-12：内置 id 黑名单恢复启用（用户要求）；类判据维持停用。
        LoggerInstance.Msg(
            "  ★★ 类判据【已停用】（状态一致性闸门已覆盖）；" +
            "内置 id 黑名单【已恢复启用】（node/module/keycard/文书等重新拦截）；" +
            "自定义 ItemBlacklist 仍生效。若要停用：把 StackGates.cs 里 GetBlacklistReason " +
            "的内置段 #if true 改回 #if false 重新编译。");
        LoggerInstance.Msg(
            $"  ★ 状态一致性闸门 = 恒定开启（见上行）+ 忽略名单 {IgnoredStateTags.Length} 项[{_ignoredTagsSource}]。");
        // ★ 0.12.0（§四·2）：修饰键快移「1 总闸 + 3 小开关」的启动汇总。
        LoggerInstance.Msg(
            $"  ★ 修饰键快移：总开关 = {_modifierKeysEnabled.Value}；" +
            $"Alt 快移 = {_altClickTransfer.Value}；Alt 配对 = {_altPairEnable.Value}；" +
            $"Shift 配对 = {_shiftPairEnable.Value}；每次件数 = {AltQuickTransferAmount}。");
        LoggerInstance.Msg(
            $"  ★ Alt 配对快移（§四）= 模式「{_altPairMode.Value}」" +
            (AltPairActive
                ? $"（玩家柜台 ↔ 主仓库双向，合并 = {_altPairMerge.Value}）"
                : "（未生效：交还原生『主仓库/后仓/柜台前格 → 展示柜』）"));
        LoggerInstance.Msg(
            $"  ★ Shift 配对快移（§四·2）= {(ShiftPairActive ? "开" : "关")}" +
            (ShiftPairActive
                ? $"（展示柜 ↔ 主仓库双向，与 Alt 共用合并开关 = {_altPairMerge.Value}）"
                : "（交还原生：Shift+左键单向「导向展示柜」）"));
        // ★ 未知值告警：AltPairMode 用区分大小写的精确匹配，填错会【静默】退回原生。
        //   启动时显式提示，避免「以为开了却没生效」的静默故障。
        if (_altPairMode != null
            && _altPairMode.Value != "counter_inv"
            && _altPairMode.Value != "native")
        {
            LoggerInstance.Warning(
                $"  ★ AltPairMode 值「{_altPairMode.Value}」无法识别（仅接受 \"counter_inv\" / \"native\"，" +
                "区分大小写），已按退回原版处理。");
        }
        LoggerInstance.Msg(
            $"  ★ 合并判定 = {(UseMergePolicy ? "新版单一源 MergePolicy" : "旧版 0.9.9 内联判据（已失去状态一致性保护！）")}" +
            "（cfg z9_UseMergePolicy；普通玩家请保持 true）");
        LoggerInstance.Msg(
            $"  ★ tag 值域校验闸门 = {(ValueGateEnabled ? "开" : "关（已失去防拆分污染保护）")}" +
            "（cfg ValueGate；普通玩家请保持 true）");
        LoggerInstance.Msg(
            $"  ★ 整堆计价 = {_stackValueTotal.Value}" +
            (_stackValueTotal.Value
                ? "（GetNegociatedValue × unitCount ⇒ 卖出/买入/批发/估价/盗窃/犯罪价值 全部按整堆计；" +
                  "首次开启请先用 ×2~×3 小堆在柜台实测确认扣减语义）"
                : "（关闭：交易系统仍按单件计价，堆叠物品卖出只按 1 件结账）"));
    }

    internal static void Diag(string message)
    {
        if (DiagOn) Log.Msg("[Diag] " + message);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  诊断日志限流（2026-10-12，实测前收敛刷屏）
    //
    //  背景：MayStack / GetStackSlot / MaxNumAdd / MaxNumRemove 等补丁在【每次拖放】
    //  都会逐条输出；一次拖放会高频触发这些钩子（同名堆合并循环 + 数量重算），
    //  DiagLog=true 时瞬间刷出几十上百行，反而淹没真正要看的「拦截原因」。
    //
    //  策略：给「成功/放行」类日志加【去重限流】（同一 key 只打一次），
    //  「拦截/异常」类日志【始终输出】—— 实测要抓的正是拦截原因，不能被限掉。
    //  全局上限 MAX_DIAG_ONCE 兜底，防止 key 组合爆炸仍刷屏。
    // ══════════════════════════════════════════════════════════════════════
    private const int MaxDiagOnce = 200;   // 限流日志的全局条数上限（超出后静默丢弃）
    private static readonly HashSet<string> _diagOnce = new HashSet<string>(StringComparer.Ordinal);

    // 同一 key 只输出一次（热路径放行/成功类日志用）。仍受 DiagOn 总闸控制。
    internal static void DiagOnce(string key, string message)
    {
        if (!DiagOn) return;
        if (_diagOnce.Count >= MaxDiagOnce) return;
        if (!_diagOnce.Add(key)) return;
        Log.Msg("[Diag·限流] " + message);
    }

    // 同一 key 最多输出 maxTimes 次（介于"每次"与"仅一次"之间）。仍受 DiagOn 总闸控制。
    // 用 key → 已输出次数 计数；超出上限后不再输出、也不再新增 key（避免无限增长）。
    private static readonly Dictionary<string, int> _diagCount = new Dictionary<string, int>(StringComparer.Ordinal);
    internal static void DiagRate(string key, int maxTimes, string message)
    {
        if (!DiagOn) return;
        int n;
        _diagCount.TryGetValue(key, out n);
        if (n >= maxTimes) return;
        _diagCount[key] = n + 1;
        Log.Msg("[Diag·限流] " + message);
    }

    // ── 0.6.2：防护降级「一次性告警」──
    // 各防护判据（黑名单 / 作用域 / 排除容器 / 容器判定 / 耐久 / 柜台探测）在异常时都走
    // fail-safe（返回 null/false = 放行），保证游戏能继续玩；但默认 DiagLog=false 时
    // 用户【完全无感】—— 防护会静默消失，这正是本项目最痛的一类故障。
    // 这里对「每类判据」只告警一次（HashSet 去重防刷屏），走 Log.Warning 无条件输出。
    private static readonly HashSet<string> _warnedOnce = new HashSet<string>(StringComparer.Ordinal);
    private static void WarnOnce(string key, string msg)
    {
        if (_warnedOnce.Add(key))
        {
            // ★ 0.9.9：MelonLoader 的 HarmonyInit 自动 PatchAll 早于 OnInitializeMelon，
            //   该阶段 Log 还是 null（0.9.8 实机首崩 NRE 根因），退回全局日志。
            if (Log != null) Log.Warning("[防护降级] " + msg);
            else MelonLogger.Warning("[防护降级] " + msg);
        }
    }


    // ── 生命周期：柜台容器 id 一次性打印 ──
    public override void OnUpdate()
    {
        try
        {
            CounterIdProbeTick();   // 0.6.6：进交易场景后一次性打印柜台容器 id（配白名单用）
            ModifierKeyProbeTick(); // 0.12.0：进场景后一次性打印三个快移 handler 的实际键位（配按键分派）
            ReportStateCmpTiming(); // 1.0.0：状态比较耗时统计（仅 DiagLog=true 时输出）
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
        }
    }
}

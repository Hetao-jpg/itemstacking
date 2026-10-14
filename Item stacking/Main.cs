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

        // 旧中文 slot: token → 游戏原生 id 的一次性迁移（0.11.2）。
        // 纯字符串替换，无游戏对象访问，失败也不影响启动。
        MigrateLegacyChineseSlotTokens();

        // ★★★ 删除手动 PatchAll —— 补丁被挂了两遍 ★★★
        //
        // ── 根因（0.14.3 修复）──
        //   MelonLoader 的 HarmonyInit 阶段已【按类型容错】自动 PatchAll 一次；
        //   旧版本这里又手动跑一次 ⇒ 每个 [HarmonyPatch] 类挂载两遍。
        //   对【带状态】的 Postfix（如 GetNegociatedValue 的 `__result = __result * count`）
        //   会双重放大：2 件 25 单价 → 25 × 2 × 2 = 100（应为 50）；
        //   9 件 20 单价 → 20 × 9 × 9 = 1620（应为 180）。
        //
        // ── 补丁挂载失败怎么办 ──
        //   `ValidatePatches()`（下方几行后）逐一自检全部补丁目标；缺失会打 Warning。
        //   这条自检路径已覆盖「补丁没挂上」的告警需求，不需要手动 PatchAll 兜底。
        //
        // ── 回滚 ──
        //   若未来某版 MelonLoader 不再自动 PatchAll，可恢复本段（但要重新评估所有
        //   带状态 Postfix 的双重挂载风险，可能要改成 conditional patch）。

        // 加载内置黑白名单（代码默认 + 可选外部 JSON 覆盖）。
        // 必须在 ValidatePatches() 之前 —— 自检要核对名单 id / 打印采用来源。
        LoadBuiltinBlacklist();
        // 忽略名单与内置黑白名单共用同一个 JSON 文件（item_blacklist.json），
        // 必须在 LoadBuiltinBlacklist() 之后。
        LoadIgnoredStateTags();
        ValidatePatches();

        // ── 启动汇总日志（VerboseStartup 可关闭逐项明细） ──
        //   ★ 两类 Warning 不受 VerboseStartup 影响、始终输出：
        //     白名单为空（安全边界）、AltPairMode 未知值（配置错误）。
        bool verboseStartup = _verboseStartup == null || _verboseStartup.Value;
        LoggerInstance.Msg(
            $"Item Stacking 已加载：拖放堆叠 = {_enableStacking.Value}，堆叠上限 = {StackCap}，" +
            $"Ctrl+拖动整堆 = {_ctrlWholeStack.Value}，诊断日志 = {_diagLog.Value}。");
        if (!verboseStartup)
        {
            LoggerInstance.Msg(
                "  （详细启动日志已关闭：把 cfg 的 VerboseStartup 改为 true 可恢复逐项明细。）");
        }
        if (verboseStartup)
        {
            LoggerInstance.Msg(
                $"  保护总闸 = {_stackBlacklist.Value}。" +
                $"容器白名单 = {(_containerWhitelist.Value == "" ? "【空 = 全不生效】" : _containerWhitelist.Value)}；" +
                $"容器黑名单 = {(_containerBlacklist.Value == "" ? "空" : _containerBlacklist.Value)}。");
        }
        // ★ 白名单 = 安全边界，空 = 全不生效（mod 等于未启用）。
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
                "[安全边界] 机器容器（熔炉/解序器/水瓶打印机等）已由【机器容器名单】单独拦截：" +
                "合并被拒 + 每格只放 1 件（cfg/JSON 的 machineContainers，见 BuiltinBlacklist）—— " +
                "即使把它们写进白名单也不会在机器里堆叠。若你装了【本作之外的自定义机器】，" +
                "请先用 [作用域探针] 确认其容器 id，再决定是否把 owner:xxx 加入 machineContainers。");
        }
        if (verboseStartup)
        {
            LoggerInstance.Msg(
                $"  物品黑名单·内置 = {_builtinIdBlacklist.Value}（cfg BuiltinIdBlacklist）；" +
                $"自定义 = {(_itemBlacklist.Value == "" ? "空" : _itemBlacklist.Value)}。");
            LoggerInstance.Msg(
                $"  ★★ 类判据【已停用】（状态一致性闸门已覆盖）；" +
                $"内置 id 黑名单 = {_builtinIdBlacklist.Value}" +
                (_builtinIdBlacklist.Value
                    ? "（node/module/keycard/文书等拦截中；要停用就把 cfg 的 BuiltinIdBlacklist 改成 false，无需重编译）"
                    : "（已关闭：仅 cfg 自定义 ItemBlacklist 生效）") +
                "。");
            LoggerInstance.Msg(
                $"  ★ 堆叠上限硬收紧 = {(StrictStackCap ? "开" : "关（仅探针 maxnumadd.clamp.*）")}" +
                "（cfg StrictStackCap；详见 Config.cs 该项说明）。");
            LoggerInstance.Msg(
                $"  ★ 机器容器每格 1 件（禁并）= {(MachineOnePerSlot ? "开" : "关（机器里可能再次出现堆叠）")}" +
                "（cfg MachineOnePerSlot；覆盖拖放/Ctrl拖动/快移全部入口）。");
            LoggerInstance.Msg(
                $"  ★ 状态一致性闸门 = 恒定开启（见上行）+ 忽略名单 {IgnoredStateTags.Length} 项[{_ignoredTagsSource}]。");
            LoggerInstance.Msg(
                $"  ★ 修饰键快移：总开关 = {_modifierKeysEnabled.Value}；" +
                $"Alt 快移 = {_altTransferEnable.Value}；Alt 配对 = {_altPairEnable.Value}；" +
                $"Shift 配对 = {_shiftPairEnable.Value}；每次件数 = {AltQuickTransferAmount}。");
            LoggerInstance.Msg(
                $"  ★ Alt 快移（§四）= 模式「{_altPairMode.Value}」" +
                (AltRelayToCtrl
                    ? $"（★ 与 Ctrl+点击同一条逻辑；数量上限 = {AltQuickTransferAmount}，余量归还源容器）"
                    : AltPairActive
                        ? $"（玩家柜台 ↔ 主仓库双向，合并 = {_altPairMerge.Value}）"
                        : "（native：交还原生『主仓库/后仓/柜台前格 → 展示柜』）"));
            LoggerInstance.Msg(
                $"  ★ Shift 配对快移（§四·2）= {(ShiftPairActive ? "开" : "关")}" +
                (ShiftPairActive
                    ? $"（展示柜 ↔ 主仓库双向，与 Alt 共用合并开关 = {_altPairMerge.Value}）"
                    : "（交还原生：Shift+左键单向「导向展示柜」）"));
        }
        // 未知值告警：AltPairMode 用区分大小写的精确匹配，填错会【静默】退回原生。
        if (_altPairMode != null
            && _altPairMode.Value != Constants.AltPairModeCtrl
            && _altPairMode.Value != Constants.AltPairModeCounterInv
            && _altPairMode.Value != Constants.AltPairModeNative)
        {
            LoggerInstance.Warning(
                $"  ★ AltPairMode 值「{_altPairMode.Value}」无法识别（仅接受 \"{Constants.AltPairModeCtrl}\" / " +
                $"\"{Constants.AltPairModeCounterInv}\" / \"{Constants.AltPairModeNative}\"，" +
                "区分大小写），已按退回原版处理。");
        }
        if (verboseStartup)
        {
            LoggerInstance.Msg(
                $"  ★ 合并判定 = {(UseMergePolicy ? "新版单一源 MergePolicy" : "旧版 0.9.9 内联判据（已失去状态一致性保护！）")}" +
                "（cfg UseMergePolicy；普通玩家请保持 true）");
            LoggerInstance.Msg(
                $"  ★ tag 值域校验闸门 = {(ValueGateEnabled ? "开" : "关（已失去防拆分污染保护）")}" +
                (ValueGateEnabled ? "（含 0.14.0 非0热度赃物拦截）" : "") +
                "（cfg ValueGate；普通玩家请保持 true）");
            LoggerInstance.Msg(
                $"  ★ 整堆计价 = {_stackValueTotal.Value}" +
                (_stackValueTotal.Value
                    ? "（GetNegociatedValue × unitCount ⇒ 卖出/买入/批发/估价/盗窃/犯罪价值 全部按整堆计；" +
                      "首次开启请先用 ×2~×3 小堆在柜台实测确认扣减语义）"
                    : "（关闭：交易系统仍按单件计价，堆叠物品卖出只按 1 件结账）"));
        }
    }

    internal static void Diag(string message)
    {
        if (DiagOn) Log.Msg("[Diag] " + message);
    }

    // ══════════════════════════════════════════════════════════════════════
    // 诊断日志限流
    //
    // ★ 背景：MayStack / GetStackSlot / MaxNumAdd 等补丁在【每次拖放】都高频触发，
    //   DiagLog=true 时瞬间刷出几十上百行，淹没真正要看的「拦截原因」。
    //
    // ★ 策略：按 key 限流（同一 key 最多 N 次） + 全局上限兜底。
    //   · Warn —— 不受 DiagOn 总闸管（防护降级必须出现）、独立上限 MaxWarnOnce；
    //   · Diag —— 受 DiagOn 总闸管、共用 MaxDiagOnce 上限；
    //   · 上限只拦【新增 key】，已登记的老 key 剩余次数照常打。
    //   key 用 "W:" / "D:" 前缀分开存，避免 Warn 与 Diag 撞 key 互相顶掉。
    // ══════════════════════════════════════════════════════════════════════
    private const int MaxDiagOnce = 200;

    // Warn 级别的独立 key 上限：组合爆炸场景（如 StateConsistency.DiagStateDiff 的
    // "state.diff.<idA>/<idB>|<detail>"）会把字典撑到无限 ⇒ 长会话内存缓慢泄漏。
    // 500 为经验值：正常会话 Warn 种类 < 30，留足余量。
    private const int MaxWarnOnce = 500;

    private static readonly Dictionary<string, int> _logOnce = new Dictionary<string, int>(StringComparer.Ordinal);
    // 两个级别各自独立计数、互不挤占额度。
    private static int _diagKeys;
    private static int _warnKeys;

    private enum Level { Warn, Diag }

    // 限流日志唯一入口：
    //   LogOnce("key", Level.Warn, "msg");           // 一次性告警（不看总闸、至多 MaxWarnOnce 个 key）
    //   LogOnce("key", Level.Diag, "msg");           // 一次性诊断（看总闸，至多 1 次）
    //   LogOnce("key", Level.Diag, "msg", max: 3);   // 至多 3 次
    private static void LogOnce(string key, Level level, string message, int max = 1)
    {
        bool warn = level == Level.Warn;
        if (!warn && !DiagOn) return;   // 总闸只管 Diag（Warn 无条件输出）

        string bagKey = (warn ? "W:" : "D:") + key;
        int n;
        if (_logOnce.TryGetValue(bagKey, out n))
        {
            if (n >= max) return;
            _logOnce[bagKey] = n + 1;
        }
        else
        {
            // 全局条数上限：只拦【新增 key】，已登记且还有剩余次数的老 key 不被静默。
            if (warn)
            {
                if (_warnKeys >= MaxWarnOnce) return;
                _warnKeys++;
            }
            else
            {
                if (_diagKeys >= MaxDiagOnce) return;
                _diagKeys++;
            }
            _logOnce[bagKey] = 1;
        }

        if (warn)
        {
            // ── 防护降级一次性告警 ──
            // 各判据异常时都走 fail-safe（放行），保证游戏能继续玩；但默认 DiagLog=false 时
            // 用户【完全无感】—— 防护会静默消失，这是本项目最痛的一类故障。
            // ⇒ 每类判据只告警一次（去重防刷屏），走 Log.Warning 无条件输出。
            //   ★ HarmonyInit 阶段自动 PatchAll 早于 OnInitializeMelon，该阶段 Log 还是 null
            //     ⇒ 退回全局日志。
            if (Log != null) Log.Warning("[防护降级] " + message);
            else MelonLogger.Warning("[防护降级] " + message);
        }
        else Log.Msg("[Diag·限流] " + message);
    }

    // ── 生命周期：一次性探针 ──
    public override void OnUpdate()
    {
        try
        {
            CounterIdProbeTick();   // 进交易场景后一次性打印柜台容器 id（配白名单用）
            ModifierKeyProbeTick(); // 进场景后一次性打印三个快移 handler 的实际键位（配按键分派）
            ReportStateCmpTiming(); // 状态比较耗时统计（仅 DiagLog=true 时输出）
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
        }
    }
}
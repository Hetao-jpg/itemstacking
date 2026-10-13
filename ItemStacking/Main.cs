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
        // ── L2（2026-10-12 计划·可选阶段）：启动日志分级 ──
        //   VerboseStartup=true（默认）= 逐项明细全打（输出与分级前逐字节一致）；
        //   VerboseStartup=false       = 只打「核心加载行 + 关闭提示」。
        //   ★ 两类 Warning 不受本项影响、始终输出：白名单为空（安全边界）、
        //     AltPairMode 未知值（配置错误）—— 故障/配置错误不许静默。
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
                $"容器黑名单 = {(_containerBlacklist.Value == "" ? "空" : _containerBlacklist.Value)}；" +
                $"柜台禁堆 = {_blockCounterStack.Value}。");
        }
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
                "[安全边界] 机器容器（熔炉/解序器/水瓶打印机等）已由【机器容器名单】单独拦截：" +
                "合并被拒 + 每格只放 1 件（cfg/JSON 的 machineContainers，见 BuiltinBlacklist）—— " +
                "即使把它们写进白名单也不会在机器里堆叠。若你装了【本作之外的自定义机器】，" +
                "请先用 [作用域探针] 确认其容器 id，再决定是否把 owner:xxx 加入 machineContainers。");
        }
        if (verboseStartup)
        {
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
        // ★ 未知值告警：AltPairMode 用区分大小写的精确匹配，填错会【静默】退回原生。
        //   启动时显式提示，避免「以为开了却没生效」的静默故障。
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
                "（cfg z9_UseMergePolicy；普通玩家请保持 true）");
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
    //  诊断日志限流（2026-10-12，实测前收敛刷屏）
    //
    //  背景：MayStack / GetStackSlot / MaxNumAdd / MaxNumRemove 等补丁在【每次拖放】
    //  都会逐条输出；一次拖放会高频触发这些钩子（同名堆合并循环 + 数量重算），
    //  DiagLog=true 时瞬间刷出几十上百行，反而淹没真正要看的「拦截原因」。
    //
    //  策略：给「成功/放行」类日志加【去重限流】（同一 key 只打一次）；
    //  「拦截/异常」类 —— ★ 0.14.0（用户定稿）起【同样限流】：按「物品对 + 原因」
    //  LogOnce max 3（原生每次悬停调 2 次 MayStack，裸 Diag 会成对刷屏）。
    //  3 次额度覆盖双调用且保留可观察性；换原因 / 换物品对重新计数。
    //  全局上限 MAX_DIAG_ONCE 兜底，防止 key 组合爆炸仍刷屏。
    //
    //  T3.1（2026-10-12）：原来的 WarnOnce / DiagOnce / DiagRate 三个方法 + 三个静态集合
    //  （_warnedOnce / _diagOnce / _diagCount）合并成一个 LogOnce + 一个字典。
    //  合并最要命的就是「门控搞混」，三种口径必须各自保住：
    //    · Warn —— 不受 DiagOn 总闸管（防护降级必须出现）、独立上限 MaxWarnOnce（见下 H1）；
    //    · Diag —— 受 DiagOn 总闸管、共用 MaxDiagOnce 上限；
    //    · 上限 —— 只拦【新增 key】，已登记的老 key 剩余次数照常打。
    //  key 用 "W:" / "D:" 前缀分开存，避免 Warn 与 Diag 撞 key 互相顶掉。
    //  这几条分支由 _tests/ItemStacking.Tests/LogGateTests.cs 逐条钉死，改这里先跑测试。
    // ══════════════════════════════════════════════════════════════════════
    private const int MaxDiagOnce = 200;   // Diag 限流日志的条数上限（超出后静默丢弃）

    // ★ H1（2026-10-12）：Warn 级别的独立 key 上限。
    //   原先 Warn 完全不设上限（防「防护降级告警被限死」），但 key 组合可爆炸：
    //   StateConsistency.DiagStateDiff 的 key = "state.diff." + idA + "/" + idB + "|" + detail
    //   ⇒ 480 物品 × 480 物品 × N tag 会把 _logOnce 撑到无限增长（长会话内存缓慢泄漏）。
    //   500 为经验值：正常会话 Warn 种类 < 30，留足余量；组合爆炸最多登记 500 条即停。
    private const int MaxWarnOnce = 500;

    // 「key → 已输出次数」，Warn / Diag 都进这一个字典，靠前缀区分（见上）。
    private static readonly Dictionary<string, int> _logOnce = new Dictionary<string, int>(StringComparer.Ordinal);
    // ★ 两个级别各自独立计数、互不挤占额度：Warn 一多不能把 Diag 提前限死，反之亦然。
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
        if (!warn && !DiagOn) return;   // 总闸只管 Diag（0.6.2：Warn 无条件输出）

        string bagKey = (warn ? "W:" : "D:") + key;
        int n;
        if (_logOnce.TryGetValue(bagKey, out n))
        {
            if (n >= max) return;
            _logOnce[bagKey] = n + 1;
        }
        else
        {
            // ★ 全局条数上限：_logOnce 是「key → 次数」的字典，key 组合
            //   （state.diff / maystack.diff / observe.* 拼上 SafeId）可能随会话无限增长 ⇒
            //   长会话内存缓慢上涨。只拦【新增 key】：已经登记、且还有剩余次数的老 key
            //   不能被一起静默。
            //   ★ H1（2026-10-12）：Warn 与 Diag 各自独立计数（_warnKeys / _diagKeys），
            //     互不挤占额度 —— 但 Warn 也不再无限，防止 key 组合爆炸拖垮长会话。
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
            // ── 0.6.2：防护降级「一次性告警」──
            // 各防护判据（黑名单 / 作用域 / 排除容器 / 容器判定 / 耐久 / 柜台探测）在异常时都走
            // fail-safe（返回 null/false = 放行），保证游戏能继续玩；但默认 DiagLog=false 时
            // 用户【完全无感】—— 防护会静默消失，这正是本项目最痛的一类故障。
            // 这里对「每类判据」只告警一次（去重防刷屏），走 Log.Warning 无条件输出。
            // ★ 0.9.9：MelonLoader 的 HarmonyInit 自动 PatchAll 早于 OnInitializeMelon，
            //   该阶段 Log 还是 null（0.9.8 实机首崩 NRE 根因），退回全局日志。
            if (Log != null) Log.Warning("[防护降级] " + message);
            else MelonLogger.Warning("[防护降级] " + message);
        }
        else Log.Msg("[Diag·限流] " + message);
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

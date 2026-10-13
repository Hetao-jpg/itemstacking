using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;

namespace ItemStacking.Tests;

/// <summary>
/// 测试宿主：把「跑在 MelonLoader 里才成立」的三件事在普通 xUnit 进程里补齐。
///
/// ① 日志管线：MelonLogger.*Msg/Warning 最终都调 BootstrapInterop.Library.LogMsg(...)，
///    而 Library 是 native 引导库句柄 —— 测试进程里为 null ⇒ 一发日志就 NRE。
///    这里反射塞一个托管 BootstrapLibrary + 自己的 LogMsgFn 记录器，把管线接活，
///    于是 Main.Log.Msg/Warning 能真正跑完，且能拿到逐条原文。
/// ② 日志句柄：Main.Log = new MelonLogger.Instance("测试名")。
/// ③ 诊断总闸：Main.DiagOn 读 _diagLog 配置项 —— 测试里直接 new 一个 Entry 设值。
///
/// 另外每次测试前清空 _diagOnce / _diagCount / _warnedOnce 三个静态集合，
/// 否则测试之间会互相污染（它们都是 static）。
/// </summary>
internal static unsafe class ModLogTestHost
{
    private const BindingFlags F = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

    private static readonly List<string> _raw = new();      // 原始日志：section|msg|stripped
    private static readonly List<string> _msgCb = new();    // MsgDrawingCallbackHandler 捕获
    private static readonly List<string> _warnCb = new();   // WarningCallbackHandler 捕获
    private static readonly List<string> _errCb = new();    // ErrorCallbackHandler 捕获
    private static bool _installed;

    public static IReadOnlyList<string> Raw { get { lock (_raw) return _raw.ToArray(); } }
    public static IReadOnlyList<string> MsgCallbacks { get { lock (_msgCb) return _msgCb.ToArray(); } }
    public static IReadOnlyList<string> WarningCallbacks { get { lock (_warnCb) return _warnCb.ToArray(); } }
    public static IReadOnlyList<string> ErrorCallbacks { get { lock (_errCb) return _errCb.ToArray(); } }

    /// <summary>装好日志管线 + 挂上回调；整个测试进程只做一次。</summary>
    public static void Install()
    {
        if (_installed) return;
        _installed = true;

        // ── ① 托管的 LogMsg 记录器 ──
        Type bootLib = Type.GetType("MelonLoader.InternalUtils.BootstrapLibrary, MelonLoader", true);
        object lib = Activator.CreateInstance(bootLib);          // public 无参构造
        Type logMsgFn = Type.GetType("MelonLoader.Bootstrap.LogMsgFn, MelonLoader", true);
        Delegate logMsg = Delegate.CreateDelegate(logMsgFn,
            typeof(ModLogTestHost).GetMethod(nameof(OnLogMsg), BindingFlags.NonPublic | BindingFlags.Static));
        bootLib.GetField("<LogMsg>k__BackingField", F).SetValue(lib, logMsg);
        // ★ Warning/Error 走的是 PassLogError → Library.LogError（另一条管线），不设一样 NRE。
        Type logErrFn = Type.GetType("MelonLoader.Bootstrap.LogErrorFn, MelonLoader", true);
        Delegate logErr = Delegate.CreateDelegate(logErrFn,
            typeof(ModLogTestHost).GetMethod(nameof(OnLogError), BindingFlags.NonPublic | BindingFlags.Static));
        bootLib.GetField("<LogError>k__BackingField", F).SetValue(lib, logErr);
        Type bootInterop = Type.GetType("MelonLoader.InternalUtils.BootstrapInterop, MelonLoader", true);
        bootInterop.GetField("<Library>k__BackingField", F).SetValue(null, lib);

        // ── ② 日志句柄 ──
        RestoreLog();

        // ── ③ 回调：断言用（比解析原始行更稳） ──
        MelonLoader.MelonLogger.MsgDrawingCallbackHandler += (a, b, name, msg) => { lock (_msgCb) _msgCb.Add(msg); };
        MelonLoader.MelonLogger.WarningCallbackHandler += (name, msg) => { lock (_warnCb) _warnCb.Add(msg); };
        MelonLoader.MelonLogger.ErrorCallbackHandler += (name, msg) => { lock (_errCb) _errCb.Add(msg); };
    }

    private static void RestoreLog()
        => typeof(Main).GetField("Log", F).SetValue(null, new MelonLoader.MelonLogger.Instance("ItemStacking.Tests"));

    /// <summary>验 0.9.9 回退分支：Log==null 时 WarnOnce 应退回全局 MelonLogger.Warning 而非 NRE。</summary>
    public static void SetLogNull()
    {
        Install();
        typeof(Main).GetField("Log", F).SetValue(null, null);
    }

    // LogMsgFn 签名带 ColorARGB*（native 侧要指针），这里一个都不读，只留文本。
    private static void OnLogMsg(MelonLoader.Logging.ColorARGB* msgColor, string msg, int msgLength,
                                 MelonLoader.Logging.ColorARGB* sectionColor, string section, int sectionLength,
                                 string strippedMsg, int strippedMsgLength)
    {
        lock (_raw) _raw.Add(section + "|" + msg + "|" + strippedMsg);
    }

    // LogErrorFn：warning=true 即 WarnOnce 走的那条，加前缀便于断言区分。
    private static void OnLogError(string msg, int msgLength, string section, int sectionLength, bool warning)
    {
        lock (_raw) _raw.Add((warning ? "WARN" : "ERROR") + "|" + section + "|" + msg);
    }

    /// <summary>每个测试前调用：清空捕获、清空三个去重集合、按需切换 DiagOn、恢复日志句柄。</summary>
    public static void Reset(bool diagOn)
    {
        Install();
        RestoreLog();          // 上一个用例若把 Log 置 null，这里兜回来

        lock (_raw) _raw.Clear();
        lock (_msgCb) _msgCb.Clear();
        lock (_warnCb) _warnCb.Clear();
        lock (_errCb) _errCb.Clear();

        // T3.1 后三个容器合并成 _logOnce（key 带 "W:"/"D:" 前缀）+ _diagKeys 计数。
        // H1 后 Warn 另有独立计数 _warnKeys（各 500 / 200 上限互不挤占）。
        // 按实际类型调 Clear，不硬转接口，免得以后有人换集合时测试炸出 InvalidCastException。
        var main = typeof(Main);
        object bag = main.GetField("_logOnce", F).GetValue(null);
        bag.GetType().GetMethod("Clear", Type.EmptyTypes).Invoke(bag, null);
        main.GetField("_diagKeys", F).SetValue(null, 0);
        main.GetField("_warnKeys", F).SetValue(null, 0);

        // DiagOn => _diagLog != null && _diagLog.Value
        object entry = Activator.CreateInstance(main.GetField("_diagLog", F).FieldType);
        main.GetField("_diagLog", F).SetValue(null, entry);
        main.GetField("_diagLog", F).FieldType.GetProperty("Value").SetValue(entry, diagOn);
    }

    /// <summary>MaxDiagOnce 是 private const，从元数据读，避免测试里写死 200。</summary>
    public static int MaxDiagOnce
        => (int)typeof(Main).GetField("MaxDiagOnce", F).GetRawConstantValue();

    /// <summary>MaxWarnOnce（H1：Warn 独立 key 上限）同样是 private const，从元数据读。</summary>
    public static int MaxWarnOnce
        => (int)typeof(Main).GetField("MaxWarnOnce", F).GetRawConstantValue();

    /// <summary>已登记的 key 数 —— 断言「超出上限后不新增 key」用（光看日志数看不出来）。
    /// T3.1 后 Warn / Diag 共用一个 _logOnce，各用例只用其中一种级别，故总数即该级别数。</summary>
    public static int RegisteredCount()
    {
        object bag = typeof(Main).GetField("_logOnce", F).GetValue(null);
        return (int)bag.GetType().GetProperty("Count").GetValue(bag, null);
    }

    /// <summary>测试按「原来那三个方法」的语义调用，内部统一走 LogOnce —— 这样断言写的是行为，
    /// 不是实现细节；把三个方法合回一个也不会让测试失去意义。</summary>
    public static void WarnOnce(string key, string msg) => InvokeLogOnce(key, "Warn", msg, 1);
    public static void DiagOnce(string key, string msg) => InvokeLogOnce(key, "Diag", msg, 1);
    public static void DiagRate(string key, int maxTimes, string msg) => InvokeLogOnce(key, "Diag", msg, maxTimes);
    public static void Diag(string msg) => Invoke("Diag", msg);

    private static void InvokeLogOnce(string key, string levelName, string msg, int max)
    {
        // Level 是 Main 的 private nested enum，测试侧只能反射拿到再 Enum.Parse。
        var lvlType = typeof(Main).GetNestedType("Level", BindingFlags.NonPublic);
        if (lvlType == null) throw new InvalidOperationException("Main.Level 不存在（改名了？测试要跟着改）");
        object lvl = Enum.Parse(lvlType, levelName);
        Invoke("LogOnce", key, lvl, msg, max);
    }

    private static void Invoke(string name, params object[] args)
    {
        var m = typeof(Main).GetMethod(name, F);
        if (m == null) throw new InvalidOperationException($"Main.{name} 不存在（改名了？测试要跟着改）");
        m.Invoke(null, args);
    }

    /// <summary>反射调用 private 成员时，异常会被包一层 TargetInvocationException，这里剥回原始异常。</summary>
    public static Exception Unwrap(Exception e)
        => e is TargetInvocationException { InnerException: not null } t ? t.InnerException : e;
}

/// <summary>每个测试类用：xUnit 不保证用例顺序，也不保证静态状态干净，进每个用例先 Reset。</summary>
public abstract class ModTestBase
{
    protected readonly ITestOutputHelper Out;
    protected ModTestBase(ITestOutputHelper o) => Out = o;
}

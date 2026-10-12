using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ItemStacking.Tests;

/// <summary>
/// DiagOnce / DiagRate / WarnOnce / Diag 的**门控**特性测试（characterization tests）。
///
/// 为什么先测这些：phase_3 的 T3.1 要把 WarnOnce / DiagOnce / DiagRate 三个方法合并成
/// 一套带来源标记的限流器 —— 这三个方法的分支条件各不相同（总闸、全局上限、
/// 新老 key 区分、WarnOnce 不受总闸管），合并极易改变行为。
/// 所以先用这组测试把当前行为钉死，再动手改；改完测试必须原样全绿。
///
/// 观测手段：ModLogTestHost 把 MelonLogger 的 native 管线换成托管记录器，
/// 于是「有没有输出」= Raw 里有没有对应文本；
/// 「有没有登记 key」= BagCount 能看到（光看日志数看不出来，尤其是上限分支）。
/// </summary>
public class LogGateTests
{
    private readonly ITestOutputHelper _out;
    public LogGateTests(ITestOutputHelper o) => _out = o;

    private static int Emitted(string needle) => ModLogTestHost.Raw.Count(s => s.Contains(needle));

    private void Dump()
    {
        foreach (var s in ModLogTestHost.Raw) _out.WriteLine("raw: " + s);
    }

    // ───────────────────────────── Diag（裸总闸） ─────────────────────────────

    [Fact]
    public void Diag_RespectsMasterSwitch()
    {
        ModLogTestHost.Reset(diagOn: false);
        ModLogTestHost.Diag("DIAG-OFF");
        Assert.Empty(ModLogTestHost.Raw);

        ModLogTestHost.Reset(diagOn: true);
        ModLogTestHost.Diag("DIAG-ON");
        Dump();
        Assert.Single(ModLogTestHost.Raw, s => s.Contains("[Diag] DIAG-ON"));
    }

    // ───────────────────────────── DiagOnce ─────────────────────────────

    [Fact]
    public void DiagOnce_OffGate_LogsNothing_AndRegistersNoKey()
    {
        ModLogTestHost.Reset(diagOn: false);
        ModLogTestHost.DiagOnce("k1", "D1");
        ModLogTestHost.DiagOnce("k1", "D1");

        Dump();
        Assert.Empty(ModLogTestHost.Raw);
        // 总闸关 ⇒ 连登记都没发生（不是「登记了但没打」）。
        Assert.Equal(0, ModLogTestHost.RegisteredCount());
    }

    [Fact]
    public void DiagOnce_SameKey_EmitsExactlyOnce_DifferentKeysIndependent()
    {
        ModLogTestHost.Reset(diagOn: true);
        ModLogTestHost.DiagOnce("k1", "DA");
        ModLogTestHost.DiagOnce("k1", "DA");
        ModLogTestHost.DiagOnce("k1", "DA");

        Dump();
        Assert.Single(ModLogTestHost.Raw, s => s.Contains("DA"));

        ModLogTestHost.DiagOnce("k2", "DB");
        Assert.Single(ModLogTestHost.Raw, s => s.Contains("DB"));
        Assert.Equal(2, ModLogTestHost.RegisteredCount());
    }

    [Fact]
    public void DiagOnce_BelowGlobalCap_StillEmits()
    {
        var cap = ModLogTestHost.MaxDiagOnce;
        ModLogTestHost.Reset(diagOn: true);
        for (var i = 0; i < cap - 1; i++)
            ModLogTestHost.DiagOnce("fill-" + i, "FILL");
        Assert.Equal(cap - 1, ModLogTestHost.Raw.Count);

        ModLogTestHost.DiagOnce("near-cap", "NEARCAP");
        Dump();
        Assert.Equal(cap, ModLogTestHost.Raw.Count);
        Assert.Single(ModLogTestHost.Raw, s => s.Contains("NEARCAP"));
        Assert.Equal(cap, ModLogTestHost.RegisteredCount());
    }

    [Fact]
    public void DiagOnce_AtGlobalCap_NewKeyIsSilentlyDropped()
    {
        var cap = ModLogTestHost.MaxDiagOnce;
        ModLogTestHost.Reset(diagOn: true);
        for (var i = 0; i < cap; i++)
            ModLogTestHost.DiagOnce("fill-" + i, "FILL");
        Assert.Equal(cap, ModLogTestHost.Raw.Count);
        Assert.Equal(cap, ModLogTestHost.RegisteredCount());

        ModLogTestHost.DiagOnce("overflow", "OVERFLOW-MARKER");

        Dump();
        Assert.Equal(cap, ModLogTestHost.Raw.Count);                     // 没多打
        Assert.DoesNotContain(ModLogTestHost.Raw, s => s.Contains("OVERFLOW-MARKER"));
        Assert.Equal(cap, ModLogTestHost.RegisteredCount());          // 也没登记（否则上限会被这个 key 永久占掉）
    }

    [Fact]
    public void DiagOnce_AtGlobalCap_DiagStillWorks()
    {
        // 上限只管 DiagOnce 的去重集，不该把裸 Diag / DiagRate 的总闸一起带瞎。
        var cap = ModLogTestHost.MaxDiagOnce;
        ModLogTestHost.Reset(diagOn: true);
        for (var i = 0; i < cap; i++)
            ModLogTestHost.DiagOnce("fill-" + i, "FILL");

        ModLogTestHost.Diag("AFTER-CAP");
        Dump();
        Assert.Contains(ModLogTestHost.Raw, s => s.Contains("[Diag] AFTER-CAP"));
    }

    [Fact]
    public void DiagCapCountsOnlyDiagKeys_WarnKeysDoNotShrinkIt()
    {
        // 上限必须只数 Diag 级别的 key。Warn 先登记一批的话，若把 Warn 也算进总额度，
        // Diag 的 200 个名额就会被挤掉 —— 实机上 Warn 有 50+ 个静态 key，正好会踩到。
        var cap = ModLogTestHost.MaxDiagOnce;
        ModLogTestHost.Reset(diagOn: true);
        for (var i = 0; i < 50; i++)
            ModLogTestHost.WarnOnce("wfill-" + i, "WFILL");
        Assert.Equal(50, ModLogTestHost.RegisteredCount());

        for (var i = 0; i < cap; i++)
            ModLogTestHost.DiagOnce("dfill-" + i, "DFILL");

        Dump();
        Assert.Equal(cap, ModLogTestHost.Raw.Count(s => s.Contains("DFILL")));   // 200 条一条都不能少
        Assert.Equal(cap + 50, ModLogTestHost.RegisteredCount());
    }

    // ───────────────────────────── DiagRate ─────────────────────────────

    [Fact]
    public void DiagRate_OffGate_LogsNothing_AndRegistersNoKey()
    {
        ModLogTestHost.Reset(diagOn: false);
        ModLogTestHost.DiagRate("k1", 3, "R1");

        Dump();
        Assert.Empty(ModLogTestHost.Raw);
        Assert.Equal(0, ModLogTestHost.RegisteredCount());
    }

    [Fact]
    public void DiagRate_EmitsUpToMaxTimes_ThenStops()
    {
        ModLogTestHost.Reset(diagOn: true);
        for (var i = 0; i < 3; i++)
            ModLogTestHost.DiagRate("rk", 3, "RATE");

        Dump();
        Assert.Equal(3, ModLogTestHost.Raw.Count(s => s.Contains("RATE")));

        ModLogTestHost.DiagRate("rk", 3, "RATE");   // 第 4 次
        Assert.Equal(3, ModLogTestHost.Raw.Count(s => s.Contains("RATE")));

        // 另一个 key 各有各的额度，互不占用。
        ModLogTestHost.DiagRate("rk2", 1, "RATE2");
        Assert.Single(ModLogTestHost.Raw, s => s.Contains("RATE2"));
        Assert.Equal(2, ModLogTestHost.RegisteredCount());
    }

    [Fact]
    public void DiagRate_AtGlobalCap_NewKeyIsNotRegistered()
    {
        var cap = ModLogTestHost.MaxDiagOnce;
        ModLogTestHost.Reset(diagOn: true);
        for (var i = 0; i < cap; i++)
            ModLogTestHost.DiagRate("fill-" + i, 1, "FILL");
        Assert.Equal(cap, ModLogTestHost.RegisteredCount());

        var before = ModLogTestHost.Raw.Count;
        ModLogTestHost.DiagRate("brand-new", 5, "BRANDNEW-MARKER");

        Dump();
        Assert.Equal(before, ModLogTestHost.Raw.Count);
        Assert.DoesNotContain(ModLogTestHost.Raw, s => s.Contains("BRANDNEW-MARKER"));
        Assert.Equal(cap, ModLogTestHost.RegisteredCount());   // 超上限的新 key 不占位
    }

    [Fact]
    public void DiagRate_AtGlobalCap_RegisteredKeyWithBudgetStillEmits()
    {
        // ★ 这正是 T3.3 与 DiagOnce 写法不同之处（Main.cs 里有注释）：
        //   DiagRate 在满额时只拦【新 key】，已登记的老 key 剩余次数必须继续打。
        //   若有人把两处上限写法合并成一样的，这条会红。
        var cap = ModLogTestHost.MaxDiagOnce;
        ModLogTestHost.Reset(diagOn: true);
        ModLogTestHost.DiagRate("keeper", 5, "KEEPER-1");       // 登记
        for (var i = 1; i < cap; i++)
            ModLogTestHost.DiagRate("fill-" + i, 1, "FILL");    // 填满到 200
        Assert.Equal(cap, ModLogTestHost.RegisteredCount());

        ModLogTestHost.DiagRate("keeper", 5, "KEEPER-2");       // 满额下仍要打出来

        Dump();
        Assert.Contains(ModLogTestHost.Raw, s => s.Contains("KEEPER-2"));
    }

    // ───────────────────────────── WarnOnce ─────────────────────────────

    [Fact]
    public void WarnOnce_EmitsEvenWhenDiagGateIsOff()
    {
        // F3 相关语义：防护降级告警必须无条件出现（DiagLog=false 时用户全靠它）。
        ModLogTestHost.Reset(diagOn: false);
        ModLogTestHost.WarnOnce("wk", "WARN-WHEN-OFF");

        Dump();
        Assert.Contains(ModLogTestHost.Raw, s => s.Contains("WARN-WHEN-OFF"));
        Assert.Contains(ModLogTestHost.WarningCallbacks, s => s.Contains("WARN-WHEN-OFF"));
    }

    [Fact]
    public void WarnOnce_SameKeyEmitsOnce_DifferentKeysIndependent()
    {
        ModLogTestHost.Reset(diagOn: true);
        ModLogTestHost.WarnOnce("wk1", "WA");
        ModLogTestHost.WarnOnce("wk1", "WA");
        ModLogTestHost.WarnOnce("wk1", "WA");

        Dump();
        Assert.Single(ModLogTestHost.Raw, s => s.Contains("WA"));

        ModLogTestHost.WarnOnce("wk2", "WB");
        Assert.Single(ModLogTestHost.Raw, s => s.Contains("WB"));
        Assert.Equal(2, ModLogTestHost.RegisteredCount());
    }

    [Fact]
    public void WarnOnce_NotBlockedByDiagCap_AndDoesNotEatDiagBudget()
    {
        // T3.1 把 Warn / Diag 合进同一个字典后，上限仍然只能对 Diag 计数，两条都要成立：
        // ① Diag 打满 MaxDiagOnce 也挡不住 Warn；
        // ② Warn 自己的登记不能挤占 Diag 名额（否则 Warn 一多，Diag 会被提前限死）。
        var cap = ModLogTestHost.MaxDiagOnce;
        ModLogTestHost.Reset(diagOn: true);
        for (var i = 0; i < cap; i++)
            ModLogTestHost.DiagOnce("fill-" + i, "FILL");
        Assert.Equal(cap, ModLogTestHost.RegisteredCount());

        ModLogTestHost.WarnOnce("warn-late", "WARN-AFTER-CAP");
        Dump();
        Assert.Contains(ModLogTestHost.Raw, s => s.Contains("WARN-AFTER-CAP"));

        // ② 多登记了一个 Warn key 之后，Diag 名额仍是满的 ⇒ 新 Diag key 照样被静默丢弃。
        ModLogTestHost.DiagOnce("diag-after-warn", "DIAG-AFTER-WARN");
        Assert.DoesNotContain(ModLogTestHost.Raw, s => s.Contains("DIAG-AFTER-WARN"));
    }

    [Fact]
    public void WarnOnce_WithLogNull_FallsBackToGlobalLogger()
    {
        // 0.9.9 防回归：HarmonyInit 阶段 Log 还是 null，退回全局 MelonLogger，
        // 不能变成 NRE（0.9.8 实机首崩的根因）。
        ModLogTestHost.Reset(diagOn: true);
        ModLogTestHost.SetLogNull();

        var ex = Record.Exception(() => ModLogTestHost.WarnOnce("wk-null", "WARN-NULL-LOG"));

        Dump();
        Assert.Null(ex);
        Assert.Contains(ModLogTestHost.Raw, s => s.Contains("WARN-NULL-LOG"));
        Assert.Contains(ModLogTestHost.WarningCallbacks, s => s.Contains("WARN-NULL-LOG"));
    }
}

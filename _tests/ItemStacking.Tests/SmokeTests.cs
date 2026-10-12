using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;
using Xunit;
using Xunit.Abstractions;

namespace ItemStacking.Tests;

// 冒烟：确认测试宿主的前置条件都成立。这几件任何一件不成立，
// 后面的特性测试就无从谈起 —— 所以先在这里把它验掉，而不是在业务测试里绕。
public class SmokeTests
{
    private readonly ITestOutputHelper _out;
    public SmokeTests(ITestOutputHelper o) => _out = o;

    [Fact]
    public void CanLoadModType()
    {
        var t = typeof(Main);
        _out.WriteLine("type = " + t.FullName + " base = " + t.BaseType?.FullName);
        Assert.Equal("ItemStacking.Main", t.FullName);
        Assert.NotNull(t.BaseType);
    }

    [Fact]
    public void CanConstructPreferenceEntry()
    {
        var e = new MelonPreferences_Entry<bool> { Value = true };
        Assert.True(e.Value);
        e.Value = false;
        Assert.False(e.Value);
    }

    [Fact]
    public void CanReadPrivateStaticsOfMain()
    {
        const BindingFlags F = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public;
        Assert.NotNull(typeof(Main).GetField("_logOnce", F));
        Assert.NotNull(typeof(Main).GetField("_diagKeys", F));
        Assert.NotNull(typeof(Main).GetField("Log", F));
        Assert.NotNull(typeof(Main).GetField("_diagLog", F));
        Assert.NotNull(typeof(Main).GetMethod("LogOnce", F));
        Assert.NotNull(typeof(Main).GetMethod("Diag", F));
    }

    // ── 宿主本身：日志管线被接活后，Msg / Warning 都要能真正跑完并被捕获 ──

    [Fact]
    public void HostCanCaptureMsg()
    {
        ModLogTestHost.Reset(diagOn: true);
        new MelonLogger.Instance("Smoke").Msg("smoke-msg-42");
        foreach (var s in ModLogTestHost.Raw) _out.WriteLine("raw: " + s);
        foreach (var s in ModLogTestHost.MsgCallbacks) _out.WriteLine("cb : " + s);
        Assert.Contains(ModLogTestHost.Raw, s => s.Contains("smoke-msg-42"));
        Assert.Contains(ModLogTestHost.MsgCallbacks, s => s.Contains("smoke-msg-42"));
    }

    [Fact]
    public void HostCanCaptureWarning()
    {
        ModLogTestHost.Reset(diagOn: true);
        new MelonLogger.Instance("Smoke").Warning("smoke-warn-43");
        foreach (var s in ModLogTestHost.Raw) _out.WriteLine("raw: " + s);
        foreach (var s in ModLogTestHost.WarningCallbacks) _out.WriteLine("cb : " + s);
        Assert.Contains(ModLogTestHost.Raw, s => s.Contains("smoke-warn-43"));
        Assert.Contains(ModLogTestHost.WarningCallbacks, s => s.Contains("smoke-warn-43"));
    }

    [Fact]
    public void ResetSwitchesDiagGateAndClearsBags()
    {
        ModLogTestHost.Reset(diagOn: false);
        ModLogTestHost.DiagOnce("k1", "m1");          // 总闸关 ⇒ 应当一条都不出
        Assert.Empty(ModLogTestHost.Raw);

        ModLogTestHost.Reset(diagOn: true);
        ModLogTestHost.DiagOnce("k1", "m1");
        Assert.Single(ModLogTestHost.Raw);

        ModLogTestHost.Reset(diagOn: true);           // 清空后同 key 可以再打一次
        ModLogTestHost.DiagOnce("k1", "m1");
        Assert.Single(ModLogTestHost.Raw);
    }

    [Fact]
    public void MaxDiagOnceReadableFromMetadata()
    {
        Assert.Equal(200, ModLogTestHost.MaxDiagOnce);
    }
}

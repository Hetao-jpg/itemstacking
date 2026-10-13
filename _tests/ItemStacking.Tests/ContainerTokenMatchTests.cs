using System;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace ItemStacking.Tests;

// ══════════════════════════════════════════════════════════════════════════
//  ★ 2026-10-13 回归测试：容器 token 三路匹配（裸 id / slot: / owner:）
//
//  背景（真实故障，用户实机日志已证 —— 见《堆叠mod机器拦截方案v3.yaml》§十一/§十二）：
//    一次「热路径惰性取值」优化把「需要哪几路取值」当成了「是否命中」的判据，写成：
//        if (!needId && !needSlot) return false;
//    ⇒ 纯 owner: 名单（= DefaultMachineContainerTokens 全部 13 项）恒判「不命中」
//    ⇒ 机器容器拦截（MergePolicy Deny + MaxNumAdd 钳 1）**整体静默失效**
//    ⇒ Ctrl 快移 / Ctrl 拖动把整堆塞进熔炉同一个格子（日志：numTransfer=10 = StackCap）。
//    ⇒ 只填 owner: 的自定义 ContainerWhitelist 也会「全不命中」，导致那些容器里堆叠整体失效。
//
//  本文件钉死两件事：
//    ① ScanTokenNeeds 只决定「取值」，返回 (false,false) 是合法且常见的（纯 owner: / 空名单）；
//    ② AnyTokenMatches 必须**逐条评估** owner: 路 —— 纯 owner: 列表 + owner 命中 ⇒ true。
//  ⚠️ 之所以把 ownerMatch 做成注入参数，就是为了让这条回归用例能【离线】跑起来。
// ══════════════════════════════════════════════════════════════════════════
public class ContainerTokenMatchTests
{
    private const BindingFlags F = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public;
    private readonly ITestOutputHelper _out;
    public ContainerTokenMatchTests(ITestOutputHelper o) => _out = o;

    private static readonly MethodInfo Scan = typeof(Main).GetMethod("ScanTokenNeeds", F);
    private static readonly MethodInfo Any  = typeof(Main).GetMethod("AnyTokenMatches", F);

    private static (bool needId, bool needSlot) ScanNeeds(params string[] tokens)
    {
        Assert.NotNull(Scan);
        var args = new object[] { tokens, null, null };
        Scan!.Invoke(null, args);
        return ((bool)args[1], (bool)args[2]);
    }

    private static bool Match(string[] tokens, string id, string slot, Func<string, bool> owner)
    {
        Assert.NotNull(Any);
        return (bool)Any!.Invoke(null, new object[] { tokens, id, slot, owner })!;
    }

    // ───────────────────────── ScanTokenNeeds：只决定「取值」 ─────────────────────────

    [Fact]
    public void Scan_AllOwnerTokens_NeedsNeitherValueRoute()
    {
        var r = ScanNeeds("owner:furnace", "owner:desequencer", "owner:bottle_printer");
        Assert.False(r.needId);
        Assert.False(r.needSlot);
        // ★ 注意：这一档【不是】"不命中"，只是"不必取 id/slot" —— 见下 Any_ 的回归用例。
    }

    [Fact]
    public void Scan_DetectsEachRoute()
    {
        Assert.Equal((false, true), ScanNeeds("slot:save_bag"));
        Assert.Equal((true, false), ScanNeeds("save_bag"));
        Assert.Equal((false, true), ScanNeeds("owner:a", "slot:b"));
        Assert.Equal((true, false), ScanNeeds("owner:a", "backInv"));
        Assert.Equal((true, true), ScanNeeds("slot:b", "backInv"));
    }

    [Fact]
    public void Scan_EmptyAndNull_AreBothFalse()
    {
        Assert.Equal((false, false), ScanNeeds());

        Assert.NotNull(Scan);
        var args = new object[] { null, null, null };
        Scan!.Invoke(null, args);
        Assert.False((bool)args[1]);
        Assert.False((bool)args[2]);
    }

    [Fact]
    public void Scan_IgnoresBlankEntries()
    {
        Assert.Equal((false, false), ScanNeeds("", "   ", "owner:x"));
        Assert.Equal((true, false), ScanNeeds("", "backInv"));
    }

    // ───────────────────────── AnyTokenMatches：三路命中 + 回归 ─────────────────────────

    [Fact]
    public void Any_OwnerRoute_MatchesViaOwnerChain()
    {
        // ★★ 本 bug 的核心回归用例：纯 owner: 列表 + owner 链命中 ⇒ 必须 true。
        //    旧实现在调用方按 needId/needSlot 提前 return false ⇒ 此处失败。
        bool m = Match(new[] { "owner:furnace", "owner:desequencer" },
                       id: null, slot: null,
                       owner: w => w == "furnace");
        Assert.True(m);
    }

    [Fact]
    public void Any_OwnerRoute_NoMatchWhenChainMisses()
    {
        Assert.False(Match(new[] { "owner:furnace" }, null, null, w => w == "fridge"));
    }

    [Fact]
    public void Any_RealMachineRoster_MatchesFurnace_WithNullIdAndSlot()
    {
        // 拿【真实默认机器名单】跑实机路径：13 项全是 owner:，id/slot 传 null
        //（= 取值被懒跳过的那一档）—— 必须命中 furnace。
        var roster = (string[])typeof(Main)
            .GetField("DefaultMachineContainerTokens", F)!.GetValue(null)!;
        _out.WriteLine("roster = " + string.Join(",", roster));

        Assert.All(roster, t => Assert.StartsWith("owner:", t, StringComparison.Ordinal));
        Assert.True(Match(roster, id: null, slot: null, owner: w => w == "furnace"));
        Assert.True(Match(roster, id: null, slot: null, owner: w => w == "bottle_printer"));
        Assert.False(Match(roster, id: null, slot: null, owner: w => w == "fridge")); // 有意排除项
    }

    [Fact]
    public void Any_SlotRoute()
    {
        Assert.True(Match(new[] { "slot:save_bag" }, null, "save_bag", _ => false));
        Assert.False(Match(new[] { "slot:save_bag" }, null, null, _ => false));
        Assert.False(Match(new[] { "slot:save_bag" }, null, "backInv", _ => false));
    }

    [Fact]
    public void Any_BareIdRoute_IsOrdinalCaseSensitive()
    {
        Assert.True(Match(new[] { "backInv" }, "backInv", null, _ => false));
        Assert.False(Match(new[] { "backInv" }, null, null, _ => false));
        Assert.False(Match(new[] { "backInv" }, "BACKINV", null, _ => false)); // 与实现一致：Ordinal
    }

    [Fact]
    public void Any_BlankWantAndNulls_AreSafe()
    {
        Assert.False(Match(new[] { "owner:" }, null, null, _ => true));      // want 为空 ⇒ 不命中
        Assert.False(Match(new[] { "slot:" }, null, "save_bag", _ => false)); // want 为空 ⇒ 不命中
        Assert.False(Match(new[] { "", "   " }, "x", "y", _ => true));
        Assert.False(Match(null, "x", "y", _ => true));
        Assert.False(Match(new[] { "owner:furnace" }, null, null, null));     // ownerMatch=null 防御
    }

    [Fact]
    public void Any_WhitespaceAroundToken_IsTrimmed()
    {
        Assert.True(Match(new[] { "  owner:furnace  " }, null, null, w => w == "furnace"));
        Assert.True(Match(new[] { " slot:save_bag " }, null, "save_bag", _ => false));
        Assert.True(Match(new[] { " backInv " }, "backInv", null, _ => false));
    }

    // ───────────────────── 源码形状守门（防「提前 return」回归） ─────────────────────
    //  上面的纯函数测试覆盖不到**调用方** `IsContainerInTokens` —— 它要 il2cpp 的
    //  GameInventory，离线构造不出来。而本次故障恰恰出在调用方那句
    //      if (!needId && !needSlot) return false;
    //  故直接对源码做一次形状断言：该模式永不允许再出现，且必须把 token 交给 AnyTokenMatches。
    //  （允许任意空白/换行差异：先把源码压平再查子串。）
    [Fact]
    public void StackGates_TokenMatch_MustNotShortCircuitOwnerRoute()
    {
        string path = FindRepoFile("ItemStacking", "StackGates.cs");
        Assert.True(path != null, "找不到 StackGates.cs（源码形状守门无法执行）");

        string src = System.IO.File.ReadAllText(path!);

        // 先剥注释再压平 —— 否则本 bug 的"罪证注释"（我们刻意在源码里复述了那句错误写法）
        // 会把守门自己触发成误报。
        string code = System.Text.RegularExpressions.Regex.Replace(
            src, @"/\*.*?\*/", "", System.Text.RegularExpressions.RegexOptions.Singleline);
        code = System.Text.RegularExpressions.Regex.Replace(code, @"//[^\n]*", "");
        string flat = System.Text.RegularExpressions.Regex.Replace(code, @"\s+", "");

        Assert.DoesNotContain("!needId&&!needSlot)returnfalse", flat);
        Assert.DoesNotContain("!needSlot&&!needId)returnfalse", flat);
        Assert.Contains("AnyTokenMatches(rawTokens,id,slot", flat);
    }

    // ── 第二道源码守门：Alt→Ctrl 转发【必须】把 keysReleased 传进去 ──
    //   反汇编实证（`ItemQuickTransferHandler::OnEventRelease` 0x7E2090）：
    //       test rbx, rbx → je 退出                      （rbx = keysReleased 形参）
    //       keysReleased.Contains(this.key@0x20) 假 → 退出
    //   ⇒ 调用 `ctrl.OnEventRelease()` 【不带参数】在编译期就过不去（形参非可选），
    //     但带 null 或"另造一个空集合"能编译通过、运行期却是【静默空转】（物品不动、无日志）。
    //     这类故障正是本项目最忌讳的静默失效面 ⇒ 钉死形状：必须传 keys 且 null 要告警。
    [Fact]
    public void AltRelay_MustForwardKeysReleased()
    {
        string path = FindRepoFile("ItemStacking", "ModifierPairTransfer.cs");
        Assert.True(path != null, "找不到 ModifierPairTransfer.cs（源码形状守门无法执行）");

        string src = System.IO.File.ReadAllText(path!);
        string code = System.Text.RegularExpressions.Regex.Replace(
            src, @"/\*.*?\*/", "", System.Text.RegularExpressions.RegexOptions.Singleline);
        code = System.Text.RegularExpressions.Regex.Replace(code, @"//[^\n]*", "");
        string flat = System.Text.RegularExpressions.Regex.Replace(code, @"\s+", "");

        // ① 必须是带集合的调用，绝不能退回无参调用（编译期拦不住 null，故这里拦形状）
        Assert.Contains("OnEventRelease(keys)", flat);
        // ② keys 必须来自 Alt handler 收到的那份实参（object[] __args 取参，与形参名无关）
        Assert.Contains("_relayKeys=(__args!=null&&__args.Length>0)?__args[0]:null", flat);
        // ③ 取不到集合时必须显式告警并交还原生，不留静默空转
        Assert.Contains("relay-keys", flat);
    }

    private static string FindRepoFile(params string[] parts)
    {
        var dir = new System.IO.DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var p = System.IO.Path.Combine(dir.FullName, System.IO.Path.Combine(parts));
            if (System.IO.File.Exists(p)) return p;
            dir = dir.Parent;
        }
        return null;
    }
}

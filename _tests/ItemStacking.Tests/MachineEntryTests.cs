using Xunit;

namespace ItemStacking.Tests;

// ══════════════════════════════════════════════════════════════════════════
//  ★ 2026-10-14 回归守门：机器容器「入口」与「余量归还」的源码形状
//
//  背景（用户 2026-10-14 实机反馈 + 当日反汇编/日志取证）：
//    A. 机器里有同名物品时 Ctrl/Alt 快移【彻底进不去机器】。
//       根因：`TryFillEmptySlots` 第一行 `origin.inventory as GameGridInventory` ——
//       熔炉等机器内库是**裸 GameInventory**（实机日志：`类=GameInventory … 所属=furnace`），
//       强转恒 null ⇒ 机器分支永远返回 0 ⇒ 只能硬拒。
//    B. 余量归还（DestroyFix）同样因 `sourceInv as GameGridInventory` 跳过源容器，
//       并且旧代码拿 `GraphUtils.TryAcceptAllMid` 的返回值当「已接受量」，判据恒真 ⇒ 静默丢件。
//    C. 柜台禁堆（BlockCounterStack）已于 2026-10-14 整体删除 —— 不许回魂。
//
//  ⚠️ 这些都是**调用方/类型耦合**类故障，纯函数测不到 ⇒ 只能对源码做形状断言。
//     （先剥注释再压平，避免我们刻意复述的"罪证写法"把守门自己触发成误报。）
// ══════════════════════════════════════════════════════════════════════════
public class MachineEntryTests
{
    private static string Strip(string src)
    {
        string code = System.Text.RegularExpressions.Regex.Replace(
            src, @"/\*.*?\*/", "", System.Text.RegularExpressions.RegexOptions.Singleline);
        code = System.Text.RegularExpressions.Regex.Replace(code, @"//[^\n]*", "");
        return System.Text.RegularExpressions.Regex.Replace(code, @"\s+", "");
    }

    private static string ReadFlattened(params string[] parts)
    {
        string path = FindRepoFile(parts);
        Assert.True(path != null, $"找不到 {string.Join("/", parts)}（源码形状守门无法执行）");
        return Strip(System.IO.File.ReadAllText(path!));
    }

    private static string FindRepoFile(params string[] parts)
    {
        var dir = new System.IO.DirectoryInfo(System.AppContext.BaseDirectory);
        while (dir != null)
        {
            var p = System.IO.Path.Combine(dir.FullName, System.IO.Path.Combine(parts));
            if (System.IO.File.Exists(p)) return p;
            dir = dir.Parent;
        }
        return null;
    }

    // ── 守门 1：机器改投空槽不许再强转 GameGridInventory ──
    [Fact]
    public void TryFillEmptySlots_MustUseBaseGameInventory()
    {
        string flat = ReadFlattened("ItemStacking", "StackPatches.Merge.cs");

        Assert.DoesNotContain("origin.inventoryasGameGridInventory", flat);
        Assert.DoesNotContain("asGameGridInventory", flat);              // 本文件不该再有这种强转
        Assert.Contains("inv.TryFindOneValidInventorySlot(source)", flat); // 必须走基类虚分发
        Assert.Contains("_selfAcceptDepth++", flat);                     // 自搬运必须防递归
    }

    // ── 守门 2：机器「每格 1 件」必须落在公共瓶颈，且能读到本次实参 ──
    [Fact]
    public void MachineGate_MustBeAtTryAcceptOnceAndReadAmount()
    {
        string flat = ReadFlattened("ItemStacking", "StackPatches.Merge.cs");

        // 必须在 SlotMarker.TryAcceptOnce 的 Prefix 上，且带 __args（读本次 amount 的唯一鲁棒方式）
        Assert.Contains("Prefix(SlotMarker__instance,object[]__args,refint__result)", flat);
        Assert.Contains("if(StackEnabled&&MachineOnePerSlot)", flat);
        Assert.Contains("ReadAmountArg(__args)", flat);
        // 数量钳 1 必须真写回 __args（否则钳制只是"打了个日志"）
        Assert.Contains("__args[0]=1;", flat);
        // 合并候选必须改投空槽 / 无空槽才拒
        Assert.Contains("TryFillEmptySlots(__instance,msrc)", flat);
        Assert.Contains("__result=0;", flat);
    }

    [Fact]
    public void MachineOnePerSlot_ConfigExists_DefaultTrue()
    {
        string flat = ReadFlattened("ItemStacking", "Config.cs");
        Assert.Contains("_machineOnePerSlot=cat.CreateEntry<bool>(\"MachineOnePerSlot\",true,", flat);
        Assert.Contains("internalstaticboolMachineOnePerSlot=>_machineOnePerSlot==null||_machineOnePerSlot.Value;", flat);
    }

    // ── 守门 3：余量归还不得依赖 GraphUtils 返回值语义，且必须支持非 Grid 容器 ──
    [Fact]
    public void ReturnRemainder_MustNotUseTryAcceptAllMidReturnValue()
    {
        string flat = ReadFlattened("ItemStacking", "DestroyFix.cs");

        Assert.DoesNotContain("TryAcceptAllMid(grid,remainder", flat);
        Assert.DoesNotContain("TryAcceptAllMid(backpack,remainder", flat);
        Assert.DoesNotContain("sourceInvasGameGridInventory", flat);
        Assert.Contains("TryReturnIntoInventory(sourceInv,remainder)", flat);
        Assert.Contains("TryReturnIntoInventory(backpack,remainder)", flat);
        Assert.Contains("IsPlacedInContainer(remainder)", flat);   // 是否归位只看 parentInventory
        Assert.Contains("inv.TryFindOneValidInventorySlot(item)", flat);
    }

    // ── 守门 4：机器容量层必须「只降不升」──
    //   旧写法 `if (__result != 1) __result = 1;` 会把原生算出的【0】也改成 1 ——
    //   而 0 的语义是「这一格放不进更多」（= 已占用槽 / 该物品 cap 已满），
    //   被改掉之后 finder 会把「已占用的同名堆」当成合法落点反复返回，
    //   于是 TryFillEmptySlots 永远找不到空槽 ⇒ 机器里有同名物品就再也放不进东西。
    //   （用户 2026-10-14 实机日志：`机器容器内 MaxNumAdd 钳到 1：common_ore（原 0）`）
    [Fact]
    public void MachineMaxNumAdd_MustOnlyLower_NotRaise()
    {
        string flat = ReadFlattened("ItemStacking", "StackPatches.Value.cs");
        Assert.DoesNotContain("__result!=1)", flat);   // 不许再把 0 改写成 1
        Assert.Contains("__result>1)", flat);          // 只压到 1（原生 0 / 1 都保持原样）
    }

    // ── 守门 5：柜台禁堆（BlockCounterStack）已删除，不许回魂 ──
    [Fact]
    public void CounterStack_IsFullyRemoved()
    {
        foreach (var f in new[] { "Config.cs", "StackGates.cs", "MergeSteerer.cs", "Main.cs", "MergePolicy.Legacy.cs" })
        {
            string flat = ReadFlattened("ItemStacking", f);
            Assert.DoesNotContain("BlockCounterStack", flat);
            Assert.DoesNotContain("_blockCounterStack", flat);
            Assert.DoesNotContain("IsCounterSlot", flat);
            Assert.DoesNotContain("IsCounterInventory", flat);
        }

        foreach (var r in new[] { "README.md.txt", "README_mobile.txt" })
        {
            string path = FindRepoFile("ItemStacking", r);
            Assert.True(path != null, $"找不到 {r}");
            Assert.DoesNotContain("BlockCounterStack", System.IO.File.ReadAllText(path!));
            Assert.DoesNotContain("柜台禁堆", System.IO.File.ReadAllText(path!));
        }
    }
}

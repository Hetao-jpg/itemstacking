using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace ItemStacking.Tests;

// 方案 v2「机器容器拦截」· 数据层单测：
// 默认名单健全性 / 生效名单初值镜像 / IsMachineContainer 的 null fail-safe。
// 消费点（MergePolicy.Evaluate 的 Deny、Patch_MaxNumAdd 的钳 1）依赖 il2cpp 运行时，
// 归实机用例 TC1-TC14 验证（见 堆叠mod机器拦截方案v2.yaml §八）。
public class MachineContainerTests
{
    private const BindingFlags F = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public;
    private readonly ITestOutputHelper _out;
    public MachineContainerTests(ITestOutputHelper o) => _out = o;

    private static string[] GetStrings(string field)
        => (string[])typeof(Main).GetField(field, F)!.GetValue(null)!;

    [Fact]
    public void DefaultMachineContainerTokens_Sane()
    {
        var arr = GetStrings("DefaultMachineContainerTokens");
        Assert.NotNull(arr);
        Assert.NotEmpty(arr);
        _out.WriteLine("count = " + arr.Length);
        foreach (var t in arr) _out.WriteLine("  " + t);

        // 全部为 owner: token（机器格沿所属物品链命中）且互不重复
        Assert.All(arr, t => Assert.StartsWith("owner:", t, StringComparison.Ordinal));
        Assert.All(arr, t => Assert.False(string.IsNullOrWhiteSpace(t)));
        Assert.Equal(arr.Length, arr.Distinct(StringComparer.Ordinal).Count());

        // 反汇编实证（DestroyFix 已修崩坏）的机器必须在默认名单 —— 防回归
        foreach (var must in new[] { "owner:furnace", "owner:desequencer", "owner:bottle_printer" })
            Assert.Contains(must, arr);

        // 储物型（默认白名单在用、无崩坏证据）不得进默认名单 —— R8 回归防护
        foreach (var never in new[] { "owner:fridge", "owner:recharger", "owner:machine_bay" })
            Assert.DoesNotContain(never, arr);

        // v2 方案的编造占位符不得写入（目录核对不存在）
        foreach (var fake in new[] { "owner:brewing_barrel", "owner:incubator",
                                     "owner:concentrator", "owner:crafting_table" })
            Assert.DoesNotContain(fake, arr);
    }

    [Fact]
    public void MachineContainerTokens_InitiallyMirrorsDefault()
    {
        var def = GetStrings("DefaultMachineContainerTokens");
        var live = GetStrings("MachineContainerTokens");
        // 未加载外部 JSON 前（测试宿主不调 LoadBuiltinBlacklist）两者必须逐项一致
        Assert.Equal(def, live);
        // 来源标签初值
        var src = typeof(Main).GetField("_machineSource", F)!.GetValue(null);
        Assert.Equal("代码默认", src);
    }

    [Fact]
    public void IsMachineContainer_NullInventory_ReturnsFalse()
    {
        var m = typeof(Main).GetMethod("IsMachineContainer", F);
        Assert.NotNull(m);
        // null 容器 / 名单为空两路都必须安全返回 false（fail-safe，不抛异常）
        Assert.False((bool)m!.Invoke(null, new object[] { null })!);
    }
}

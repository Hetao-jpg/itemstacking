using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace ItemStacking.Tests;

// ══════════════════════════════════════════════════════════════════════════
//  0.14.0 回归测试：赃物热度零值闸门的数据层（Constants.ValueGateZeroTags）
//
//  背景：STOLEN_VALUE_INT 的模板法会 fail-open（模板是全新物品、不带赃物热度，
//  GetDefaultState 只登记模板上存在的 tag），故 0.14.0 引入「零值变体」清单：
//  AtDefaultState 内要求这些 tag 当前值【缺省或 == 0】才可堆。
//  消费点（AtDefaultState 的零值检查）需要 il2cpp GameItem 运行时，归实机验证；
//  本文件钉死清单本身的健全性（防误删 / 防与模板清单混淆）。
// ══════════════════════════════════════════════════════════════════════════
public class ValueGateZeroTagsTests
{
    private const BindingFlags F = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public;
    private readonly ITestOutputHelper _out;
    public ValueGateZeroTagsTests(ITestOutputHelper o) => _out = o;

    // Constants 是 internal static class —— 无 InternalsVisibleTo，经程序集反射取。
    private static string[] GetList(string name)
    {
        var constants = typeof(Main).Assembly.GetType("ItemStacking.Constants");
        Assert.NotNull(constants);
        var field = constants!.GetField(name, F);
        Assert.NotNull(field);
        return (string[])field!.GetValue(null)!;
    }

    [Fact]
    public void ValueGateZeroTags_Sane()
    {
        var arr = GetList("ValueGateZeroTags");
        Assert.NotNull(arr);
        Assert.NotEmpty(arr);
        _out.WriteLine("count = " + arr.Length);
        foreach (var t in arr) _out.WriteLine("  " + t);

        Assert.All(arr, t => Assert.False(string.IsNullOrWhiteSpace(t)));
        Assert.Equal(arr.Length, arr.Distinct(StringComparer.Ordinal).Count());

        // 0.14.0 核心回归：赃物热度必须在零值清单（防误删 —— 删了 = 非0热度可堆）
        Assert.Contains("STOLEN_VALUE_INT", arr);
    }

    [Fact]
    public void ValueGateZeroTags_DisjointFromTemplateTags()
    {
        var zero = GetList("ValueGateZeroTags");
        var tpl = GetList("ValueGateTags");
        // 两清单语义不同（零值「缺省或==0」 vs 模板比对「==默认模板值」）；
        // 同名 tag 同时出现在两处 = 维护事故（零值检查会被模板早退/缺省语义搅浑）。
        Assert.Empty(zero.Intersect(tpl, StringComparer.Ordinal));
    }
}

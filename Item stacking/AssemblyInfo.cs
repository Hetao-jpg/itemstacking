using MelonLoader;

// 公司名须经游戏自身文件核实（*_Data/app.info）。
[assembly: MelonInfo(typeof(ItemStacking.Main), "Item Stacking", ItemStacking.Main.ModVersion, "核桃")]
[assembly: MelonGame("Questing Goose Studio", "Probably Stolen")]

[assembly: System.Reflection.AssemblyVersion(ItemStacking.Main.ModVersion)]
[assembly: System.Reflection.AssemblyFileVersion(ItemStacking.Main.ModVersion)]

namespace ItemStacking
{
    // 版本号单一源：程序集特性与后续引用统一取这里，避免多处手改漂移。
    //   注意：代码注释里的「1.0.0 / 1.2.0」是【内部特性批次代号】，与此处程序集版本不是一回事。
    //
    // 完整版本历史见 README.md「版本历史（简）」章节。
    //
    // 最近变更：
    //   0.14.3：修复 PatchAll 双重挂载（整堆计价 n² 放大）
    //   0.14.4：Config 键名重命名 + 移除白名单里 bottle_printer + AltPairEnable 门控修复
    public partial class Main { public const string ModVersion = "0.14.4"; }
}
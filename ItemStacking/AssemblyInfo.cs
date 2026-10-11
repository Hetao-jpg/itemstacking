using MelonLoader;

// 公司名须经游戏自身文件核实（*_Data/app.info）。
// 本游戏（Probably Stolen）公司名 = "Questing Goose Studio"（非 "Kinda Brave"）。
[assembly: MelonInfo(typeof(ItemStacking.Main), "Item Stacking", ItemStacking.Main.ModVersion, "核桃")]
[assembly: MelonGame("Questing Goose Studio", "Probably Stolen")]

[assembly: System.Reflection.AssemblyVersion(ItemStacking.Main.ModVersion)]
[assembly: System.Reflection.AssemblyFileVersion(ItemStacking.Main.ModVersion)]

namespace ItemStacking
{
    // ★ 版本号单一源：程序集特性与后续引用统一取这里，避免多处手改漂移。
    //   注意：代码注释里的「1.0.0」是【内部特性批次代号】，与此处程序集版本不是一回事。
    public partial class Main { public const string ModVersion = "0.12.1"; }
}

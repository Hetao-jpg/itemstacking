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
    //   0.13.0（2026-10-13）：机器容器拦截（自动拆堆）随本版发布 ——
    //     见《堆叠mod机器拦截方案v3.yaml》。此前 0.12.1 尚无该功能，故不再是等价版本。
    //   0.14.0（2026-10-13）：赃物热度零值闸门（ValueGateZeroTags / STOLEN_VALUE_INT）——
    //     0 热度可堆 / 非 0 不可堆，填补「热度相同非 0 也能合并」的缺口。
    public partial class Main { public const string ModVersion = "0.14.0"; }
}

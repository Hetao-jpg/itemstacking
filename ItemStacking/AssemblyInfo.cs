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
    //   0.14.1（2026-10-14）：三处修正 ——
    //     ① P0-3：TryAcceptAllMid 返回值语义用反（「剩余量」被当「已接受量」）导致
    //        余量归还判据恒真、部分成功时静默丢件 + 口袋转移误判 —— 已按反汇编实证修正；
    //     ② P1-5：内置 id 黑名单从编译期 #if 改为 cfg「BuiltinIdBlacklist」（默认开）；
    //     ③ P0-2：MaxNumAdd 新增 maxnumadd.clamp.* 探针 + cfg「StrictStackCap」（默认关）。
    //   0.14.2（2026-10-14 晚）：机器容器三处修正（实机日志驱动）——
    //     ① 机器容量层由「无条件钳到 1」改为「★ 只降不升」：旧写法把原生算出的 0
    //        （= 已占用槽放不进更多，是游戏自己寻路依赖的信号）改写成 1，
    //        导致「机器里有同名物品后，堆叠/单件/快移/拖动全都放不进去」；
    //     ② TryFillEmptySlots 改用基类 GameInventory.TryFindOneValidInventorySlot
    //        （机器内库是裸 GameInventory，旧强转 GameGridInventory 恒 null）；
    //     ③ 机器「禁并 + 每格 1 件」下沉到 SlotMarker.TryAcceptOnce 公共瓶颈（cfg MachineOnePerSlot）。
    public partial class Main { public const string ModVersion = "0.14.2"; }
}

using System;
using System.Collections.Generic;
using MelonLoader;
using MelonLoader.Preferences;

namespace ItemStacking;

// ══════════════════════════════════════════════════════════════════════════
//  配置（MelonPreferences）：条目声明 + 建表 + 读取属性 + cfg 迁移
//  2026-10-12 从 Main.cs 抽出（T2.2）—— Main.cs 只留生命周期与日志。
// ══════════════════════════════════════════════════════════════════════════
public partial class Main
{
    // ───────────────────────────────────────────────────────────────────────
    // 配置项（MelonPreferences 段名 = AssemblyName = "ItemStacking"）
    // ───────────────────────────────────────────────────────────────────────
    private static MelonPreferences_Entry<bool> _enableStacking = null!;
    private static MelonPreferences_Entry<int> _newStackCap = null!;
    private static MelonPreferences_Entry<bool> _ctrlWholeStack = null!;
    // ★ 0.12.1：Ctrl+左键点击快移的合并闸门（问题1）。
    //   true（默认）= Ctrl 快移的合并也走 MergePolicy（白/黑名单 + 状态闸门），不通过则导流为空位放置。
    //   false       = 退回改前行为（Ctrl 快移合并无视名单）。
    private static MelonPreferences_Entry<bool> _ctrlTransferMergeGate = null!;
    private static MelonPreferences_Entry<bool> _diagLog = null!;

    // ══════════════════════════════════════════════════════════════════════
    // ★ 0.11.x：黑白名单「四列表」架构（★ 与 0.8.0~0.10 相比已变动，见下）
    //   ┌ 容器白名单 ContainerWhitelist  —— 非空 ⇒ 仅名单内容器可堆叠（★ 唯一作用域开关）
    //   ├ 容器黑名单 ContainerBlacklist  —— 名单内容器禁止堆叠（双端判定）
    //   ├ 物品白名单 ItemWhitelist       —— ★ 已删除（2026-10-12：原本就无实际作用）
    //   └ 物品黑名单 ItemBlacklist       —— 按 id 禁止堆叠（自定义；内置名单已停用，见 StackGates）
    //
    //   ★★ 当前（0.12.x）真正生效的堆叠拦截只有两类：
    //     ① 状态一致性闸门（恒定开启，无开关）：两件物品的完整状态必须【完全相等】才可合并；
    //     ② 按 id 的自定义黑名单（受 cfg「StackBlacklist」总闸控制）。
    //        ★ 内置 id 黑名单（node/module/keycard/文书…）已于 2026-10-12 注释停用。
    //   ★ 旧版（≤0.7.1）的 9 条「类判据」（可鉴定 / 赃物 / 未拥有 / 电量不同 / 动物 / 鼠 /
    //     酒瓶 / 耐久 / 液体容器）自 0.11.0 起【永久停用】—— 它们是①的真子集，
    //     且其实现代码已在 2026-10-12 重构时删除（原 StackGates.cs 的 `#if false` 块，
    //     见 git 历史 dc3d11a；留档注释见 StackGates.GetBlacklistReason 方法头）。
    // ══════════════════════════════════════════════════════════════════════

    // 总闸：一键关闭全部黑名单保护（保留原 StackBlacklist 键名，向后兼容旧 cfg）。
    private static MelonPreferences_Entry<bool> _stackBlacklist = null!;

    // ── ① 容器白名单（合并了旧 StackScope + StrictWhitelist）──
    //   语义：留空 = 不限容器（全部允许）；非空 = **只有名单内容器可堆叠**。
    //   （旧版「留空 = 不限 / 填了才限」的宽松语义已废弃 —— 严格模式才是用户真正要的。）
    private static MelonPreferences_Entry<string> _containerWhitelist = null!;

    // 容器白名单的代码默认串（★ 类级常量：CreateEntry 与空值回填迁移共用同一份，避免漂移）。
    //   0.12.0 语义反转为「留空=全禁」，故必须给默认常用容器集合，否则旧 cfg 空值会让堆叠全失效。
    private const string DefaultContainerWhitelist =
        "owner:storage_bay,owner:storage_bay_large,owner:smuggler_bay,owner:smuggler_bay_mod," +
        "owner:smuggler_bay_mini,owner:machine_bay,owner:machine_bay_ext," +
        "owner:backpack_small,owner:backpack_medium,owner:backpack_large,owner:satchel,owner:fanny_pack," +
        "owner:animal_cage,owner:fridge,owner:recharger,owner:vacuum_robot,owner:bottle_printer," +
        "slot:save_bag,slot:showcase_save_bag,slot:hidden_save_bag,slot:back_inv_save_bag";

    // ── ② 容器黑名单（原 StackExclude 改名）──
    //   语义：逗号分隔容器 id，双端判定（被叠物品所在容器 / 拖起方所在容器）命中即拦截。
    private static MelonPreferences_Entry<string> _containerBlacklist = null!;

    // ── ③ 物品黑名单（原 BlockItemIds 改名）──
    //   语义：逗号分隔物品 id，禁止堆叠（自定义项；内置名单见 BuiltinBlock* 常量）。
    private static MelonPreferences_Entry<string> _itemBlacklist = null!;

    // ── 柜台禁堆（容器黑名单的「运行时实例版」，不靠 id）──
    //   判据 = EmporiumEntry 三个柜台格实例指针比对：
    //     frontInvinvElement(0x88) / backInvinvElement(0x98) / backInvinvElementCounter(0xA0)。
    private static MelonPreferences_Entry<bool> _blockCounterStack = null!;

    // ★ 0.10.0（阶段5）：内置黑白名单已外置到 `BuiltinBlacklist.cs`
    //   —— 代码默认值（DefaultBlock*）+ 可选外部 JSON 覆盖，读取点为 static 字段
    //   `BuiltinBlockItemIds` / `BuiltinBlockIdPrefixes` / `BuiltinBlockIdContains` /
    //   `BuiltinBlockIdSuffixes` / `BuiltinBlockSuffixExempt`（定义见该文件）。
    //   历史沿革（0.6.6 起不放 cfg、0.7.0 加模组类、0.7.1 加后缀类、0.8.1 改子串）见该文件注释。

    // ── 修饰键快移（总闸 + 三个功能小开关）──
    //   ★ 0.12.0（§四·2）：把原先散落的修饰键开关收敛为「1 总闸 + 3 小开关」：
    //     · ModifierKeysEnabled      ：★ 总开关。关 ⇒ 本 mod 全部修饰键快移（Alt/Shift）整体退回原生。
    //     · AltClickTransferToCounter：Alt 快移开关（关 ⇒ Alt 快移退回原生）。
    //     · AltPairEnable            ：Alt 配对快移开关（原 AltClickOnClickMode 收编而来）。
    //     · ShiftPairEnable          ：Shift 快移配对开关（展示柜 ↔ 主仓库双向）。
    //   ⇒ Alt 配对生效 = 总闸 && AltClickTransferToCounter && AltPairEnable && AltPairMode=="counter_inv"。
    //   ⇒ Shift 配对生效 = 总闸 && ShiftPairEnable。
    //   ★ 目标容器（搬到哪）由 §四 的模式/开关决定，小开关本身不描述具体目标。
    private static MelonPreferences_Entry<bool> _modifierKeysEnabled = null!;
    private static MelonPreferences_Entry<bool> _altClickTransfer = null!;
    private static MelonPreferences_Entry<bool> _altPairEnable = null!;
    private static MelonPreferences_Entry<bool> _shiftPairEnable = null!;
    // Alt 快移「每次转移几个」。原生是整堆（GraphUtils 里 amount=-1 展开成 unitCount）。
    private static MelonPreferences_Entry<int> _altQuickTransferAmount = null!;

    // ★ 0.11.0（§四）：Alt 快移【配对模式】。
    //   "counter_inv"（默认）= 玩家柜台(backInvinvElement 0x98) ↔ 主仓库(invElement 0x30) 双向
    //                          （本 mod 自算目标容器，复用原生 TryAcceptOnce）。
    //   "native"            = 交还原生硬编码配对（主仓库/后仓/柜台前格 → 展示柜）。
    private static MelonPreferences_Entry<string> _altPairMode = null!;

    // ★ 0.11.0（§四）：Alt 配对快移时是否允许【合并】同类堆。
    //   true（默认）= 走正常合并（受 MergePolicy 全链路约束，含状态比较）。
    //   false       = 强制逐件 + 找空位；目标装不下则本次不搬（不丢件）。
    private static MelonPreferences_Entry<bool> _altPairMerge = null!;

    // ★ 0.9.0：交易系统按【整堆总价值】计价。
    private static MelonPreferences_Entry<bool> _stackValueTotal = null!;

    // ★ 0.10.0（重构·阶段1 回滚开关）：合并决策是否走【单一源 MergePolicy】。
    //   true（默认）= 三个挂点统一委托 MergePolicy.Evaluate，判据只在一处定序。
    //   false       = 退回 0.9.9 各自内联判据的老路径（旧代码保留未删，可二态切换）。
    private static MelonPreferences_Entry<bool> _useMergePolicy = null!;

    // ★ 1.2.0：tag 值域校验闸门（ValueGate，默认开启）。
    //   true（默认）= 状态相等后，再校验「关键 tag 是否等于默认模板值」，偏离默认即拦截。
    //   目的：GetSplitItem 拆分用 DirectoryMaster.Item 重建默认物品 ⇒ 偏离默认的物品
    //   堆叠后拆分会污染成默认（满电/满耐久/默认纯度/默认品质）。本闸门从根上杜绝。
    //   false       = 关闭值域校验（退回只比状态，可能放行「偏离默认」的堆叠）。
    private static MelonPreferences_Entry<bool> _valueGate = null!;

    // ── 建表（原 OnInitializeMelon 内联段，2026-10-12 抽出；调用点在 Main.cs）──
    //   原顺序：CreateCategory → Log=LoggerInstance → CreateEntry… → 白名单空值迁移 → SaveToFile。
    //   现 Log= 在 OnInitializeMelon 里、CreateCategory 之前赋值（CreateCategory 不用 Log，等价）。
    private static void CreateConfigEntries()
    {
        var cat = MelonPreferences.CreateCategory("ItemStacking", "物品堆叠增强");
        _enableStacking = cat.CreateEntry<bool>(
            "EnableStacking", true,
            "启用拖放堆叠",
            "把一个物品拖到同名物品上即合并堆叠（0.2.0 核心功能）。");
        _newStackCap = cat.CreateEntry<int>(
            "NewStackCap", 10,
            "堆叠上限",
            "单个槽位允许的最大堆叠数量。");
        _ctrlWholeStack = cat.CreateEntry<bool>(
            "CtrlDragWholeStack", true,
            "Ctrl+拖动 = 整堆",
            "按住 Ctrl 开始拖动 = 拿起整堆；不按 = 只拿 1 个。" +
            "原生 Shift（10%）/ Alt（1 个）/ 拖起后滚轮微调数量不受影响。");
        // ★ 0.12.1：Ctrl+左键【点击快移】的合并闸门（问题1）。
        _ctrlTransferMergeGate = cat.CreateEntry<bool>(
            "CtrlTransferMergeGate", true,
            "Ctrl 点击快移 · 合并闸门",
            "0.12.1 修复（问题1）：Ctrl+鼠标左键【点击快移】物品时，" +
            "若落点容器里已有同类堆、原生会直接把整堆【合并】进去 —— 这条路径" +
            "不经过普通拖放的堆叠闸门，因此会【无视容器白/黑名单】。" +
            "本项开启后，Ctrl 快移的【合并】也会走同一套判定（白名单 / 黑名单 / 状态一致性）；" +
            "判定不通过时【拒绝本次合并】，物品留在原处（不丢、不覆盖）。" +
            "★ 只拦【合并】；放进空位（放置）一律放行，符合原版行为。" +
            "★ 关闭 = 退回改前行为（Ctrl 快移可无视名单直接合并）。");
        _diagLog = cat.CreateEntry<bool>(
            "DiagLog", false,
            "诊断日志（总开关）",
            "0.9.0 起：控制【本 mod 全部诊断输出】，不再只是 [Diag] 前缀那部分。" +
            "受控的探针日志：[Diag] / [Alt 落点] / [Alt 数量钳制] / [转场探针] / " +
            "[落点抬升] / [整堆计价] / [柜台探针] / [柜台禁堆] / [柜台id] / [作用域探针] / [修饰键] / " +
            "[Ctrl 快移·拦截]。" +
            "★ 不受本项影响、始终输出的只有两类：" +
            "① 启动自检（加载行 + 补丁目标自检结果）—— 否则无法判断 mod 有没有装上；" +
            "② 故障告警（[防护降级] / Log.Error / 自检失败）—— 否则故障会静默。" +
            "默认关闭；排查问题时再开，正常游戏请保持关闭避免刷屏。");
        _stackBlacklist = cat.CreateEntry<bool>(
            "StackBlacklist", true,
            "堆叠保护总闸",
            "0.5.0 起：阻止「有独立状态」的物品被堆叠，避免丢失鉴定/所有权/进度等属性。\n" +
            "★ 当前（0.12.x）真正生效的拦截有两道：\n" +
            "  ① 状态一致性闸门（恒定开启，不被本项关闭）：两件物品的完整状态必须【完全相等】才可合并；\n" +
            "  ② 按 id 自定义黑名单（ItemBlacklist，受本项控制）：内置名单已停用（2026-10-12）。\n" +
            "关闭本项 = 关掉 ② 的自定义物品黑名单；① 与容器白/黑名单不受影响。\n" +
            "★ 历史：0.8.0 前还有 9 条「类判据」（可鉴定/赃物/未拥有/电量/动物/鼠/酒瓶/耐久/液体容器），" +
            "已被状态一致性闸门完全覆盖（它们是状态比较的真子集），自 0.11.0 起【永久停用、不再有开关】。");

        // ── ① 容器白名单（★ 唯一作用域开关 = 安全边界）──
        //   2026-10-12：默认值从「空（全不生效）」改为用户指定的常用容器集合。
        //   ★ 必须写 owner:/slot: token 而非裸 id —— 0.9.1 实机证实 identifier 几乎全空，
        //     裸 id 名单在本作匹配不上（见 StackGates.IsContainerInList 头注释）。
        //   ★ 映射规则：随身容器/船舱/机器/笼子（有归属物品）→ owner:物品id；
        //     固定 UI 窗格（主仓库/展示柜/暗格/柜台）→ slot:槽位 id。
        _containerWhitelist = cat.CreateEntry<string>(
            "ContainerWhitelist", DefaultContainerWhitelist,
            "容器白名单（唯一作用域开关，空 = 全不生效）",
            "★ 只有名单内的容器允许堆叠，其余一律禁止。\n" +
            "★ 2026-10-12 起代码默认值已含常用容器：" +
            "储藏区/走私暗格/机器区（含扩建）、小/中/大背包/挎包/腰包、动物笼、冰箱、充能器、" +
            "清洁机器人、水瓶打印机、主仓库/展示柜/海报暗格/玩家柜台。\n" +
            "★ 留空 = 【任何容器都不堆叠】（mod 等于未启用）。\n" +
            "★ 注意：机器容器（熔炉/解序器等）内部假设「一槽=一实例」，堆叠会丢物品，" +
            "默认名单刻意不含它们；bottle_printer 等如遇异常请从名单移除。\n" +
            "支持三种 token（实测大多数容器 identifier 为空，裸 id 无法区分）：" +
            "① 裸 id（如 backInv）= 按 identifier 精确匹配（仅少数容器 id 非空）；" +
            "② slot:槽位 id（如 slot:save_bag=主仓库 / slot:showcase_save_bag=展示柜 / " +
            "slot:back_inv_save_bag=玩家柜台 / slot:hidden_save_bag=海报暗格）= 按 EmporiumEntry 槽位匹配；" +
            "③ owner:物品id（如 owner:backpack_medium）= 按容器所属物品链匹配（背包/货仓/笼子等内部库存）。" +
            "用 [作用域探针] 日志里的 槽位=/所属= 抄写 token。" +
            "★ 只看落点（目标容器）在不在名单里，不看拖起方。" +
            "★ 与 ContainerBlacklist 同时命中时【黑名单优先】。" +
            "★ 值含逗号时请【用引号包起来】，否则会产生非法 TOML。" +
            "★ 已有 cfg 文件里的旧值（含空值）优先于本默认值；要采用新默认请删除 cfg 中的 " +
            "ContainerWhitelist 行后重启。");

        // ── ② 容器黑名单（原 StackExclude 改名）──
        _containerBlacklist = cat.CreateEntry<string>(
            "ContainerBlacklist", "",
            "容器黑名单（名单内容器禁止堆叠）",
            "0.9.3：逗号分隔的容器匹配 token，与白名单同一套判据：" +
            "① 裸 id（如 backInv）= 按 identifier 精确匹配；" +
            "② slot:槽位 id（★ 0.11.2 起为游戏原生 id；如 slot:back_inv_save_bag=玩家柜台 / " +
            "slot:bazarInv=黑市左侧）= 按 EmporiumEntry 槽位匹配；" +
            "③ owner:物品id（如 owner:smuggler_bay_mod）= 按容器所属物品链匹配。" +
            "留空 = 不排除任何容器。双端判定：被叠物品所在容器 或 拖起方所在容器 命中即拦截。" +
            "★ 严格语义：命中容器内禁止堆叠（合并）；拖放 / Alt 快移 / Ctrl 快移的合并路径都被拦截。" +
            "  空槽放置（移动）不受限 —— 物品仍可移入/移出，只是不能堆成堆。" +
            "★ 与白名单同时命中时【黑名单优先】（更严格的禁止覆盖白名单的允许）。" +
            "对所有合并路径生效（含拖放 / Alt 快移 / Ctrl 快移）。" +
            "★ 值含逗号时请【用引号包起来】：ContainerBlacklist = \"a,b\"。");

        // ── ③ 物品黑名单（原 BlockItemIds 改名）──
        _itemBlacklist = cat.CreateEntry<string>(
            "ItemBlacklist", "",
            "物品黑名单（按 id 禁止堆叠）",
            "0.8.0：逗号分隔的物品 identifier，这些物品禁止被堆叠（用于无通用 tag 的特定物品）。" +
            "★ 内置名单已硬编码在代码里（净水药片 / 能量电池 / 钥匙卡 / 莓果 / 能量阵列 / 模组 / 节点 …），" +
            "见启动日志的「物品黑名单·内置」行；本项只用于额外自定义。" +
            "留空 = 不额外拦截（内置项仍生效）。" +
            "★ 值含逗号时请【用引号包起来】：ItemBlacklist = \"foo,bar\"。");
        // ── 柜台禁堆（0.6.5）──
        //   ★ 修复：本项此前只有字段声明、漏了 CreateEntry，导致 OnInitializeMelon 读
        //     `_blockCounterStack.Value` 时 NRE（启动即中止，「柜台禁堆」长期静默失效）。
        _blockCounterStack = cat.CreateEntry<bool>(
            "BlockCounterStack", false,
            "柜台禁堆",
            "0.9.0 起【默认关闭】。开启后，交易柜台容器内【任何方式】都不允许堆叠合并 —— 包括鼠标拖放、" +
            "Alt 快移、LeftCtrl 快移。柜台里同名物品会各自占一格，不再并成一堆。\n" +
            "判据用游戏自己的 EmporiumEntry 三个柜台格实例（frontInvinvElement / backInvinvElement / " +
            "backInvinvElementCounter），不靠硬编码 id。\n" +
            "★ 为什么默认关：0.9.0 补上整堆计价（StackValueTotal）后，柜台合并不再亏钱 —— " +
            "卖出一堆会按整堆总价值结账，所以当初为规避「柜台亏钱」而加的禁堆闸门已无必要，" +
            "反而妨碍玩家把同种商品并成一堆、腾出柜台格位。\n" +
            "★ 若你担心柜台里同种商品被误合并造成难以分拣，可手动改回 true。");
        // ── 修饰键快移总闸（0.12.0，§四·2）──
        _modifierKeysEnabled = cat.CreateEntry<bool>(
            "ModifierKeysEnabled", true,
            "修饰键快移（总开关）",
            "★ 0.12.0（§四·2）新增的【修饰键快移总开关】：\n" +
            "  关闭后，本 mod 全部修饰键快移功能（Alt 快移 / Alt 配对 / Shift 配对）一次性失效，" +
            "LeftAlt / LeftShift + 点击/拖拽 完全交还原版。\n" +
            "  等价于同时关掉下面三个小开关（AltClickTransferToCounter / AltPairEnable / ShiftPairEnable）。\n" +
            "  仅在你想「临时整体停用修饰键功能、但保留各自的配置值」时用；平时保持开启。");
        _altClickTransfer = cat.CreateEntry<bool>(
            "AltClickTransferToCounter", true,
            "Alt 快移（总闸）",
            "★ 本项是【LeftAlt 快移功能的总开关】（0.6.1 起）：\n" +
            "  开启后，在交易界面按住 LeftAlt 对物品点击/拖拽，即把物品快移（搬去哪个容器由 " +
            "「AltPairMode」决定）。\n" +
            "  关闭后整个 Alt 快移功能失效，LeftAlt+点击/拖拽 完全交还原版。" +
            "（柜台不存在 / 非交易场景时也会自动退化为原生行为。）\n" +
            "★ 需与总开关 ModifierKeysEnabled 同时开启，Alt 快移才生效。\n" +
            "★ 与 ContainerSuite 的 LeftAlt 升级拖拽共用 Alt 键，已确认可共存。");
        // ── Alt 配对开关（0.12.0 由旧「AltClickOnClickMode」收编而来）──
        _altPairEnable = cat.CreateEntry<bool>(
            "AltPairEnable", true,
            "Alt 配对快移（玩家柜台 ↔ 主仓库）",
            "★ 本项控制【Alt 快移是否走本 mod 的配对搬运】（0.6.4 起原名 AltClickOnClickMode，0.12.0 改名收编）：\n" +
            "  · true（默认）= 本 mod 接管：Alt+点击也能快移，且【搬到哪】由「AltPairMode」决定\n" +
            "    （默认 = 玩家柜台 ↔ 主仓库 双向配对）。\n" +
            "  · false = 完全交还原生：Alt+点击/拖拽 = 原版行为（把整堆移进落点窗口对应的容器，\n" +
            "    例如 主仓库/后仓/柜台前格 → 展示柜）。\n" +
            "★ 需与总开关 ModifierKeysEnabled、以及「AltClickTransferToCounter」同时开启。\n" +
            "★ 改名说明：旧键 AltClickOnClickMode 已废弃并从配置中清除，行为由本项承接。");
        // ── Shift 配对开关（0.12.0，§四·2 新增）──
        _shiftPairEnable = cat.CreateEntry<bool>(
            "ShiftPairEnable", true,
            "Shift 快移配对（展示柜 ↔ 主仓库）",
            "★ 0.12.0（§四·2）新增：把原版「Shift+左键 = 单向导向展示柜」改为【展示柜 ↔ 主仓库 双向配对】：\n" +
            "  · Shift 点【主仓库】里的物品 → 搬进【展示柜】(showcaseElement 0x40，槽 id = showcase_save_bag)；\n" +
            "  · Shift 点【展示柜】里的物品 → 搬回【主仓库】(invElement 0x30，槽 id = save_bag)；\n" +
            "  · Shift 点【既不在展示柜也不在主仓库】的物品 → 交还原生（不擅自另找落点）。\n" +
            "  · false = 完全交还原生：Shift+左键 = 原版单向「导向展示柜」。\n" +
            "★ 需与总开关 ModifierKeysEnabled 同时开启。\n" +
            "★ 数量由 AltQuickTransferAmount 决定；是否允许合并由 AltPairMerge 决定（两套配对共用）。\n" +
            "★ 只搬运【你自己拥有】的物品（非本人所有的不搬，与原生归属门一致）。");
        _stackValueTotal = cat.CreateEntry<bool>(
            "StackValueTotal", false,
            "交易系统按整堆总价值计价",
            "★ 0.10.0 起【默认关闭】。原生 GameItem.GetNegociatedValue() 只算【单件】价值（不乘数量），" +
            "所以把一个 ×N 的堆卖给客户 / 买入 / 批发 / 估价 / 偷窃，只会按 1 件结账，其余价值蒸发。" +
            "开启后：GetNegociatedValue 的返回值 × unitCount ⇒ 所有走该方法的交易口径统一按整堆计价。" +
            "★ 为什么默认关（0.10.0）：本项与其它『协商价值/加价』类 mod 存在【顺序冲突】——" +
            "若对方在原生内部链或字段层抬高单件价，我们再把结果 ×N 会把该加价放大 N 倍" +
            "（堆 10 件 = 加价 ×10，表现为『价格异常过高』）。设 Priority.First 只能保证" +
            "同方法 Postfix 里我们最前，治不了『内部链劫持』。⇒ 默认关，避免悄悄改变交易体验；" +
            "确需整堆计价、且未装加价 mod 时再显式开启。开启后请用 DiagLog 看 [整堆计价·链] 探针。" +
            "★ 连带影响（同一方法也用于）：盗窃/检查损失价值、赃物犯罪价值(Contraband/脏水/濒危物品)、" +
            "保险理赔、SecData 热度。这些也会同步 ×N，属预期但会改变游戏难度曲线。" +
            "★ 关闭则完全交还原生：交易系统退回按单件计价，堆叠物品卖出只按 1 件结账。" +
            "★ 与加价 mod 共存：两者只能活一个；若要共存，请保持本项关闭，或先关对方 mod。");

        // ── 0.10.0（重构）：合并决策单一源（回滚开关）──
        _useMergePolicy = cat.CreateEntry<bool>(
            "z9_UseMergePolicy", true,
            "合并判定用哪套代码（普通玩家请保持开）",
            "【这个开关是干嘛的】\n" +
            "本 mod 判断「两件物品能不能合并成一堆」时，有两条代码路径：\n" +
            "  · true（默认，新版）= MergePolicy 单一源：拖放 / Alt 快移 / Ctrl 快移\n" +
            "    三条路径都调用同一个中心判定函数，规则顺序固定（状态一致性 → 物品黑名单 →\n" +
            "    容器黑名单 → 容器白名单），改规则只改一处、三条路径同时生效。\n" +
            "  · false（旧版 0.9.9）= 各条路径各判各的：三处代码各自内联一套判据，\n" +
            "    顺序还不一致 —— 这是重构前的历史状态，仅作为【紧急回滚】保留。\n" +
            "【为什么平时不要关】\n" +
            "旧路径【没有状态一致性保护】：电量/纯度/品质/磨损不同的物品也会被放行合并，\n" +
            "合并后除第一件外全部属性丢失。这不是等价切换，关掉 = 主动拆掉最重要的保护。\n" +
            "★ 普通玩家：永远保持 true，不要动。\n" +
            "★ 只有排障（怀疑新判定本身有 bug）时才临时关，查明后立刻改回。");

        // ── 1.2.0：tag 值域校验闸门（默认开启）──
        _valueGate = cat.CreateEntry<bool>(
            "ValueGate", true,
            "tag 值域校验闸门（防拆分后状态污染）",
            "★ 1.2.0 新增（默认开启）：状态相等后，再校验「关键 tag 是否等于该物品的默认值」。\n" +
            "【为什么需要】\n" +
            "游戏拆分（GetSplitItem）不用原物品的 tag，而是用 DirectoryMaster.Item(identifier)\n" +
            "重新生成一个【默认状态】的物品。于是「状态相同但偏离默认」的物品（如两件 50% 电量\n" +
            "的电池、两件非满耐久工具、非默认纯度金属锭、非默认品质莓果）堆叠后，拆分出来会\n" +
            "【恢复默认】（满电/满耐久/默认纯度/默认品质）—— 这就是「拆分后状态污染」。\n" +
            "本闸门要求：电量/耐久/纯度/品质/液体/使用次数等【关键 tag 必须等于默认值】才可堆，\n" +
            "从根上杜绝污染。\n" +
            "【判定颗粒度】\n" +
            "满电可堆 / 非满不可堆；满耐久可堆 / 非满不可堆；默认纯度可堆 / 非默认不可堆；\n" +
            "默认品质可堆 / 非默认不可堆。关键 tag 清单见 ValueGate.cs 的 ValueGateTags。\n" +
            "【什么时候关】\n" +
            "· false = 关闭值域校验（退回只比状态，可能放行「偏离默认」的堆叠，拆分后污染）。\n" +
            "★ 普通玩家：保持 true，不要动。");

        _altQuickTransferAmount = cat.CreateEntry<int>(
            "AltQuickTransferAmount", 1,
            "Alt 快移件数",
            "0.6.4：按住 LeftAlt 对物品点击/拖拽时，一次转移【几个】。" +
            "原生行为是整堆（内部把 -1 展开成 unitCount）。本 mod 在原生流程内把源堆预先裁剪成" +
            "这个数量，因此【只移动这么多个，其余留在原容器】。" +
            "1 = 逐件上架（默认，配交易柜台最常用）；设成很大的数（如 9999）= 等价原生整堆。" +
            "仅在总开关『ModifierKeysEnabled』开启、且本次确实是 Alt/Shift 快移时生效。");

        // ── 0.11.0（§四）：Alt 配对模式 + 合并开关 ──
        _altPairMode = cat.CreateEntry<string>(
            "AltPairMode", "counter_inv",
            "Alt 快移配对模式",
            "0.11.0（§四）：按住 LeftAlt 点击（或拖拽）物品时，物品【往哪个容器搬】。两个可选值：\n" +
            "  · \"counter_inv\"（默认）= 玩家柜台 ↔ 主仓库【双向】：\n" +
            "      · Alt 点【主仓库】里的物品 → 搬进【玩家柜台】(backInvinvElement 0x98，槽 id = back_inv_save_bag)；\n" +
            "      · Alt 点【玩家柜台】里的物品 → 搬回【主仓库】(invElement 0x30，槽 id = save_bag)。\n" +
            "      也就是「把货摆上柜台 / 把货收回仓库」两键完成。\n" +
            "  · \"native\" = 交还原版：Alt 点主仓库/后仓/柜台前格的物品 → 一律搬进【展示柜】。\n" +
            "★ 反汇编依据：原版 ItemQuickTransferAltHandler 是硬编码派发器，只做" +
            "『主仓库/后仓/柜台前格 → 展示柜』，根本没有『柜台 ↔ 主仓库』配对；本项即为此新增。\n" +
            "★ 搬运复用游戏自身的 GraphUtils.TryAcceptOnce（内部就是逐格填充循环），不自写循环。\n" +
            "★ 只搬运【你自己拥有】的物品（非本人所有的不搬，与原生归属门一致）。\n" +
            "★ 数量由 AltQuickTransferAmount 决定（§四搬运同样受它约束）。\n" +
            "★ 请照抄双引号内的字符串（含引号）；此值区分大小写，填错 = 退回原版行为。");
        _altPairMerge = cat.CreateEntry<bool>(
            "AltPairMerge", true,
            "配对快移时允许合并（Alt/Shift 共用）",
            "0.11.0（§四）起：控制配对快移时，目标容器里若有【状态完全相同的同类堆】，要不要并进去。\n" +
            "  · true（默认）= 允许合并：能合就合，省格位。\n" +
            "  · false       = 禁止合并：强制搬到【空位】；目标容器没有空位则本次不搬（绝不丢件）。\n" +
            "★ 0.12.0 起 Alt 配对与 Shift 配对【共用本开关】。\n" +
            "★ 只影响配对快移（AltPairEnable / ShiftPairEnable 打开时）；普通鼠标拖放 / Ctrl 快移不受影响。");

        // ★ 0.12.2：容器白名单「空值自动回填默认串」迁移（2026-10-12 定）。
        //   背景：ContainerWhitelist 语义在 0.12.0 反转（旧版留空=不限，新版留空=全禁），
        //   而 MelonPreferences 的 CreateEntry 默认值【对 cfg 已存在的 key 不生效】——
        //   旧 cfg 里遗留的空值 "" 会覆盖代码默认串，导致「白名单空=全禁=堆叠整体失效」，
        //   且 SaveToFile 只回写内存值、永不回填默认（实机 2026-10-12 踩坑）。
        //   故在 SaveToFile 前显式检测：值为空白时回填代码默认常用容器串，一劳永逸。
        MigrateEmptyContainerWhitelist();

        cat.SaveToFile();
    }

    internal static bool StackEnabled => _enableStacking.Value;

    // 1.1.0：价值一致性闸门（cfg ValueGate，默认 true）。
    //   null 兜底 true（尚未初始化时视为开启，保守）。
    internal static bool ValueGateEnabled => _valueGate == null || _valueGate.Value;

    // P2：堆叠上限下限钳制（用户误设 0 / 负数时不出错，至少保留 1）。
    internal static int StackCap => Math.Max(1, _newStackCap.Value);

    // 0.6.4：Alt 快移件数（下限钳到 1；上限给个合理ceil，防止极端值把 GetSplitItem 参数搞坏）。
    internal static int AltQuickTransferAmount
        => _altQuickTransferAmount == null ? 1 : Math.Clamp(_altQuickTransferAmount.Value, 1, 9999);

    // ★ 0.12.0（§四·2）：修饰键快移【总开关】。
    //   关 ⇒ 本 mod 全部修饰键快移（Alt 快移 / Alt 配对 / Shift 配对）整体退回原生。
    internal static bool ModifierKeysEnabled
        => _modifierKeysEnabled == null || _modifierKeysEnabled.Value;

    // 0.6.4：Alt 快移是否需要本 mod 接管（总闸 + AltClickTransferToCounter 都开）。
    //   ★ 0.12.0：原「AltClickOnClickMode」已收编为 AltPairEnable（见 AltPairActive），
    //     本属性只表示「Alt 快移功能本身是否启用」。
    internal static bool AltTransferTakeover
        => ModifierKeysEnabled
        && _altClickTransfer != null && _altClickTransfer.Value;

    // ★ 0.9.0：整堆计价总闸（交易系统识别堆叠物品总价值）。
    //   判据取反：GetNegociatedValue 单件计价 ⇒ 必须显式开启才乘数量。
    internal static bool StackValueTotalOn => _stackValueTotal != null && _stackValueTotal.Value;

    // 诊断日志开关（3.4 新增，默认关闭）：开启后各补丁在关键决策点输出 [Diag] 日志。
    // 用属性实时读 _diagLog.Value，改 MelonPreferences.cfg 后无需重启即可在下一次调用生效。
    internal static bool DiagOn => _diagLog != null && _diagLog.Value;

    // ── 0.11.2：旧中文 slot: token → 游戏原生 id 的一次性迁移 ──
    //   旧（0.11.1-）：slot:背包(主仓库) / slot:展示柜 / slot:柜台前格 / slot:柜台后格 /
    //                 slot:柜台三格 / slot:已售格 / slot:海报暗格 / slot:黑市左侧
    //   新（0.11.2+）：slot:save_bag / slot:showcase_save_bag / slot:frontInv /
    //                 slot:back_inv_save_bag / slot:backInvCounter / slot:sold_save_bag /
    //                 slot:hidden_save_bag / slot:bazarInv
    //   替换只针对 `slot:` 前缀后的完整 token（用逗号分隔后按 token 精确比对），
    //   不会误伤 owner:/裸 id 里的同名子串。命中才写盘（无变更不触发 SaveToFile）。
    private static void MigrateLegacyChineseSlotTokens()
    {
        try
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "slot:背包(主仓库)", "slot:" + SlotMainInv },
                { "slot:展示柜",       "slot:" + SlotShowcase },
                { "slot:柜台前格",     "slot:" + SlotFrontCounter },
                { "slot:柜台后格",     "slot:" + SlotBackCounter },
                { "slot:柜台三格",     "slot:" + SlotCounterExtra },
                { "slot:已售格",       "slot:" + SlotSold },
                { "slot:海报暗格",     "slot:" + SlotHidden },
                { "slot:黑市左侧",     "slot:" + SlotBazar },
            };

            MigrateOneEntry(_containerWhitelist, map, "ContainerWhitelist");
            MigrateOneEntry(_containerBlacklist, map, "ContainerBlacklist");
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("slotmigrate", Level.Warn, $"slot token 迁移失败（忽略，行为等同未迁移）: {e.Message}");
        }
    }

    // 对单个 cfg 项做 token 级替换；有变更才落盘 + 打一行日志。
    private static void MigrateOneEntry(
        MelonPreferences_Entry<string> entry,
        Dictionary<string, string> map, string name)
    {
        if (entry == null) return;
        string cur = entry.Value;
        if (string.IsNullOrWhiteSpace(cur)) return;

        var parts = cur.Split(',');
        bool changed = false;
        for (int i = 0; i < parts.Length; i++)
        {
            var t = parts[i].Trim();
            if (map.TryGetValue(t, out var repl))
            {
                parts[i] = repl;
                changed = true;
            }
            else
            {
                parts[i] = t;   // 统一 Trim（原值可能带空格）
            }
        }
        if (!changed) return;

        string migrated = string.Join(",", parts);
        entry.Value = migrated;
        MelonPreferences.Save();
        if (Log != null)
            Log.Msg($"[slot 迁移] {name} 旧中文槽位 token → 游戏原生 id：\"{cur}\" ⇒ \"{migrated}\"");
    }

    // ── 0.12.2：容器白名单「空值自动回填默认串」迁移 ──
    //   背景见 OnInitializeMelon 里的调用点注释。核心：MelonPreferences 的 CreateEntry
    //   默认值对「cfg 已存在的 key」不生效，旧 cfg 遗留的空值会覆盖代码默认 → 白名单空 = 全禁。
    //   这里在 SaveToFile 前显式检测：值为空白则回填 DefaultContainerWhitelist 并落盘，
    //   等价于既有的 MigrateLegacyChineseSlotTokens「一次性迁移」模式。
    private static void MigrateEmptyContainerWhitelist()
    {
        try
        {
            if (_containerWhitelist == null) return;
            string cur = _containerWhitelist.Value;
            if (!string.IsNullOrWhiteSpace(cur)) return;   // 已配置 → 不干预

            _containerWhitelist.Value = DefaultContainerWhitelist;
            if (Log != null)
                Log.Msg($"[白名单迁移] ContainerWhitelist 为空（旧 cfg 遗留），已回填默认常用容器串（{DefaultContainerWhitelist.Length} 字符）。");
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("wl-migrate", Level.Warn, $"白名单空值迁移失败（忽略，堆叠可能仍全禁）: {e.Message}");
        }
    }
}

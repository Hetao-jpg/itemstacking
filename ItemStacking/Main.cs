using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Preferences;
using UnityEngine;

namespace ItemStacking;

public partial class Main : MelonMod
{
    // ───────────────────────────────────────────────────────────────────────
    // 配置项（MelonPreferences 段名 = AssemblyName = "ItemStacking"）
    // ───────────────────────────────────────────────────────────────────────
    private static MelonPreferences_Entry<bool> _enableStacking = null!;
    private static MelonPreferences_Entry<int> _newStackCap = null!;
    private static MelonPreferences_Entry<bool> _fillToCapOnLoad = null!;
    private static MelonPreferences_Entry<int> _oldStackCap = null!;
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
    //   ├ 物品白名单 ItemWhitelist       —— ★ 已【无实际作用】（仅保留以兼容旧 cfg，见其说明）
    //   └ 物品黑名单 ItemBlacklist       —— 按 id 禁止堆叠（自定义；内置名单见 BuiltinBlock*）
    //
    //   ★★ 当前（0.11.x）真正生效的堆叠拦截只有两类：
    //     ① 状态一致性闸门（恒定开启，无开关）：两件物品的完整状态必须【完全相等】才可合并；
    //     ② 按 id 的内置 + 自定义黑名单（受 cfg「StackBlacklist」总闸控制）。
    //   ★ 旧版（≤0.7.1）的 9 条「类判据」（可鉴定 / 赃物 / 未拥有 / 电量不同 / 动物 / 鼠 /
    //     酒瓶 / 耐久 / 液体容器）自 0.11.0 起【永久停用】—— 它们是①的真子集，
    //     且其实现代码仍留在 StackGates.cs 的 `#if false` 块里（历史留档，不再编译生效）。
    //     ⇒ 因此「用白名单豁免类判据」这条路也随之失效（没有类判据可豁免了）。
    // ══════════════════════════════════════════════════════════════════════

    // 总闸：一键关闭全部黑名单保护（保留原 StackBlacklist 键名，向后兼容旧 cfg）。
    private static MelonPreferences_Entry<bool> _stackBlacklist = null!;

    // ── ① 容器白名单（合并了旧 StackScope + StrictWhitelist）──
    //   语义：留空 = 不限容器（全部允许）；非空 = **只有名单内容器可堆叠**。
    //   （旧版「留空 = 不限 / 填了才限」的宽松语义已废弃 —— 严格模式才是用户真正要的。）
    private static MelonPreferences_Entry<string> _containerWhitelist = null!;

    // ── ② 容器黑名单（原 StackExclude 改名）──
    //   语义：逗号分隔容器 id，双端判定（被叠物品所在容器 / 拖起方所在容器）命中即拦截。
    private static MelonPreferences_Entry<string> _containerBlacklist = null!;

    // ── ③ 物品白名单（★ 0.11.x：无实际作用，仅保留兼容旧 cfg）──
    //   历史语义：命中白名单的物品可跳过「9 条类判据」的整体拦截（= 显式声明"确认可堆叠"）。
    //   ★ 但 0.11.0 起那 9 条类判据已全部停用（被状态一致性闸门取代），本项随之失效。
    //   ★ 本项【不豁免】物品黑名单 —— id 级黑名单是该物品的死命令，白名单从来不能翻案。
    private static MelonPreferences_Entry<string> _itemWhitelist = null!;

    // ── ④ 物品黑名单（原 BlockItemIds 改名）──
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

    // 静态日志句柄（嵌套补丁类是 static，用不了实例 LoggerInstance）。
    internal static MelonLogger.Instance Log = null!;

    public override void OnInitializeMelon()
    {
        var cat = MelonPreferences.CreateCategory("ItemStacking", "物品堆叠增强");
        Log = LoggerInstance;

        _enableStacking = cat.CreateEntry<bool>(
            "EnableStacking", true,
            "启用拖放堆叠",
            "把一个物品拖到同名物品上即合并堆叠（0.2.0 核心功能）。");
        _newStackCap = cat.CreateEntry<int>(
            "NewStackCap", 10,
            "堆叠上限",
            "单个槽位允许的最大堆叠数量。");
        _fillToCapOnLoad = cat.CreateEntry<bool>(
            "FillToCapOnLoad", true,
            "读档补满",
            "读档时，把原本恰好叠满旧上限的存量物品补满到新上限。");
        _oldStackCap = cat.CreateEntry<int>(
            "OldStackCap", 10,
            "旧堆叠上限",
            "打本 mod 之前游戏的默认堆叠上限，用于识别『之前已叠满』的物品。");
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
            "★ 当前（0.11.x）真正生效的拦截有两道：\n" +
            "  ① 状态一致性闸门（恒定开启，不被本项关闭）：两件物品的完整状态必须【完全相等】才可合并；\n" +
            "  ② 按 id 内置黑名单（受本项控制）：node* / module / neural_core / *_keycard / 文书类等" +
            "「状态可能相同、但各自有独立内容」的物品，按 id 拦截。\n" +
            "关闭本项 = 关掉 ② 的 id 黑名单（以及自定义物品黑名单）；① 与容器白/黑名单不受影响。\n" +
            "★ 历史：0.8.0 前还有 9 条「类判据」（可鉴定/赃物/未拥有/电量/动物/鼠/酒瓶/耐久/液体容器），" +
            "已被状态一致性闸门完全覆盖（它们是状态比较的真子集），自 0.11.0 起【永久停用、不再有开关】。");

        // ── ① 容器白名单（★ 1.0.0：唯一作用域开关 = 安全边界）──
        _containerWhitelist = cat.CreateEntry<string>(
            "ContainerWhitelist", "",
            "容器白名单（★ 1.0.0：唯一作用域开关，空 = 全不生效）",
            "★★ 1.0.0 行为变更（与 0.9.x 相反，务必注意）：" +
            "★ 留空 = 【任何容器都不堆叠】（mod 等于未启用，这是安全默认值）；" +
            "★ 非空 = 【只有名单内的容器】允许堆叠，其余一律禁止。" +
            "为什么要默认全禁：机器容器（熔炉/解序器/其他 mod 自定义机器）内部假设「一槽=一实例」，" +
            "堆叠会打破它导致物品丢失；默认全开会永远被未知容器咬，默认全禁则免疫。" +
            "0.9.2 起支持三种 token（因为实测大多数容器 identifier 为空，纯 id 无法区分）：" +
            "① 裸 id（如 backInv）= 按 identifier 精确匹配（旧行为）；" +
            "② slot:槽位 id（★ 0.11.2 起为游戏原生 id，全部 ASCII；如 slot:save_bag=主仓库 / " +
            "slot:showcase_save_bag=展示柜 / slot:back_inv_save_bag=玩家柜台 / slot:sold_save_bag=已售格 / " +
            "slot:hidden_save_bag=海报暗格 / slot:frontInv=柜台前格 / slot:backInvCounter=柜台三格 / " +
            "slot:bazarInv=黑市左侧）= 按 EmporiumEntry 槽位匹配；" +
            "③ owner:物品id（如 owner:backpack_medium）= 按容器所属物品链匹配（背包/货仓等内部库存）。" +
            "用 [作用域探针] 日志里的 槽位=/所属= 抄写 token；也可运行 mod 目录的 find_whitelist.ps1 自动生成推荐串。" +
            "★ 只看落点（目标容器）在不在名单里，不看拖起方。" +
            "★ 与 ContainerBlacklist 同时命中时【黑名单优先】。" +
            "★ 值含逗号时请【用引号包起来】：ContainerWhitelist = \"a,b\"，否则会产生非法 TOML。");

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

        // ── ③ 物品白名单（★ 0.11.x：当前无实际作用，见说明）──
        _itemWhitelist = cat.CreateEntry<string>(
            "ItemWhitelist", "",
            "物品白名单（★ 当前无效，保留以兼容旧配置）",
            "★ 0.11.0 起本项【无实际作用】：本项原本用于「豁免 9 条类判据」，" +
            "而那些类判据已被状态一致性闸门取代并永久停用（见 StackBlacklist 说明），" +
            "所以现在填了也不改变任何行为。\n" +
            "保留本项只是为了让旧版 cfg 文件不报错；可留空，无需填写。\n" +
            "★ 想放行某个原本被 id 黑名单拦截的物品：只能从黑名单侧处理" +
            "（Mods/item_blacklist.json 里调整 blockItemIds / blockIdPrefixes 等），白名单不能翻案。");

        // ── ④ 物品黑名单（原 BlockItemIds 改名）──
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
            "合并决策单一源（重构·阶段1）",
            "0.10.0 重构开关：true = 拖放/构造落点/快移三条路径的【合并决策】统一走单一源 " +
            "MergePolicy（判据只在一处定序，消除三处顺序漂移）；false = 退回 0.9.9 各自内联判据。" +
            "★ 仅用于重构期做 A/B 对照与紧急回滚；正常游玩保持 true。" +
            "★ 警告：false 会退回 0.9.9 老路径，而老路径【不含状态一致性闸门（StatesEqual）】——" +
            "状态不同的物品（电量/纯度/品质/磨损等）会被放行合并，可能造成属性丢失。" +
            "故本项【会改变功能行为】，并非等价切换；除排障外请勿关闭。");

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

        cat.SaveToFile();

        // ★ 0.11.2：旧中文 slot: token → 游戏原生 id 的一次性迁移（核桃 2026-10-11 定）。
        //   0.11.1 及以前 slot: 后面写的是中文槽位名，0.11.2 起改游戏原生 id → 老 cfg 里的
        //   中文 token 会静默匹配不上（堆叠在那些容器里失效）。这里做确定性替换并落盘，
        //   避免「升级后功能悄悄没了」。纯字符串替换，无游戏对象访问，失败也不影响启动。
        MigrateLegacyChineseSlotTokens();

        // ★ 0.12.0：清理已废弃的旧键（AltClickOnClickMode → 收编为 AltPairEnable）。
        //   MelonPreferences 不会删除「已不再注册」的键 —— 旧键会以孤儿形式永久留在
        //   [ItemStacking] 段里（死文字）。这里做一次性文本级清理，确保旧键及其说明被完整移除。
        RemoveLegacyModifierKeyEntry();

        // ★ 0.9.9：MelonLoader 已在 HarmonyInit 阶段按类型容错自动 PatchAll 一次，
        //   这里是历史保留的第二遍（补丁实现均幂等，重复挂载无副作用）。
        //   必须包 try/catch：0.9.8 实机里 Patch_FurnaceConsume 解析失败抛异常，
        //   直接把 OnInitializeMelon 打断 —— ValidatePatches 和启动汇总日志全没跑。
        try
        {
            HarmonyInstance.PatchAll();
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            Log.Error("PatchAll 有补丁挂载失败（HarmonyInit 阶段已按类型容错挂过一遍，" +
                      "其余补丁不受影响）：" + e);
        }
        // ★ 0.10.0（阶段5）：加载内置黑白名单（代码默认 + 可选外部 JSON 覆盖）。
        //   必须在 ValidatePatches() 之前 —— 自检要核对名单 id / 打印采用来源。
        LoadBuiltinBlacklist();
        // ★ 1.0.0：忽略名单与内置黑白名单共用同一个 JSON 文件（item_blacklist.json），
        //   必须在 LoadBuiltinBlacklist() 之后（复用其查找/解析结果语义）。
        LoadIgnoredStateTags();
        ValidatePatches();
        LoggerInstance.Msg(
            $"Item Stacking 已加载：拖放堆叠 = {_enableStacking.Value}，堆叠上限 = {StackCap}，" +
            $"读档补满 = {_fillToCapOnLoad.Value}（旧上限 {_oldStackCap.Value}），" +
            $"Ctrl+拖动整堆 = {_ctrlWholeStack.Value}，诊断日志 = {_diagLog.Value}。");
        LoggerInstance.Msg(
            $"  保护总闸 = {_stackBlacklist.Value}。" +
            $"容器白名单 = {(_containerWhitelist.Value == "" ? "【空 = 全不生效】" : _containerWhitelist.Value)}；" +
            $"容器黑名单 = {(_containerBlacklist.Value == "" ? "空" : _containerBlacklist.Value)}；" +
            $"柜台禁堆 = {_blockCounterStack.Value}。");
        // ★ 1.0.0：白名单 = 安全边界，空 = 全不生效（mod 等于未启用）。
        //   开箱即用体验为零，必须醒目提示 + 给推荐串，否则玩家以为 mod 坏了。
        if (string.IsNullOrWhiteSpace(_containerWhitelist.Value))
        {
            LoggerInstance.Warning(
                "[安全边界] ContainerWhitelist 为空 ⇒ 堆叠【全不生效】（这是 1.0.0 的安全默认值）。" +
                "要启用堆叠，请在 UserData/MelonPreferences.cfg 的 [ItemStacking] 段填入要允许堆叠的容器 token。");
            LoggerInstance.Warning(
                "[安全边界] 推荐起步（随身背包 + 船舱储藏 + 主仓库/展示柜，整串含逗号【必须用引号包起来】）：" +
                "ContainerWhitelist = \"owner:backpack_small,owner:backpack_medium,owner:backpack_large," +
                "owner:satchel,owner:fanny_pack,owner:storage_bay,owner:storage_bay_large,owner:smuggler_bay," +
                "owner:smuggler_bay_mod,owner:machine_bay,owner:machine_bay_ext,slot:save_bag,slot:showcase_save_bag\"");
            LoggerInstance.Warning(
                "[安全边界] 也可运行 mod 目录下 find_whitelist.ps1（开 DiagLog 后跑一圈场景）自动生成推荐串。" +
                "注意：机器容器（熔炉/解序器/其他 mod 自定义机器）请【不要】加入白名单 —— " +
                "它们假设「一槽=一实例」，堆叠会导致物品丢失。");
        }
        LoggerInstance.Msg(
            $"  物品白名单（★ 已无实际作用）= {(_itemWhitelist.Value == "" ? "空" : _itemWhitelist.Value)}；" +
            $"物品黑名单·自定义 = {(_itemBlacklist.Value == "" ? "空" : _itemBlacklist.Value)}。");
        // ★ 1.0.0 对比实验【定稿】（核桃 2026-10-11）：类判据停用、内置 id 黑名单【恢复】。
        //   见 StackGates.GetBlacklistReason 头注释。判据分工：
        //     · 类判据（9 条）→ 作废（状态一致性闸门的真子集）。
        //     · id 黑名单    → 生效（同态但内容独立的物品：node*/module*/neural_core/*_keycard/文书）。
        LoggerInstance.Msg(
            "  ★★ 类判据【已停用】（状态一致性闸门已覆盖，见 StackGates 方法头①）；" +
            "内置 id 黑名单【已恢复生效】（见方法头②）。");
        LoggerInstance.Msg(
            "  ★ id 黑名单覆盖：内置 id + 前缀 node* + 子串 module/neural_core + 后缀 *_keycard + 自定义黑名单。" +
            "回滚类判据：把 StackGates.cs 里 GetBlacklistReason 的 #if false 改成 #if true 重新编译。" +
            "回滚 id 黑名单：cfg 关掉 StackBlacklist。");
        LoggerInstance.Msg(
            $"  ★ 状态一致性闸门 = 恒定开启（见上行）+ 忽略名单 {IgnoredStateTags.Length} 项[{_ignoredTagsSource}]。");
        // ★ 0.12.0（§四·2）：修饰键快移「1 总闸 + 3 小开关」的启动汇总。
        LoggerInstance.Msg(
            $"  ★ 修饰键快移：总开关 = {_modifierKeysEnabled.Value}；" +
            $"Alt 快移 = {_altClickTransfer.Value}；Alt 配对 = {_altPairEnable.Value}；" +
            $"Shift 配对 = {_shiftPairEnable.Value}；每次件数 = {AltQuickTransferAmount}。");
        LoggerInstance.Msg(
            $"  ★ Alt 配对快移（§四）= 模式「{_altPairMode.Value}」" +
            (AltPairActive
                ? $"（玩家柜台 ↔ 主仓库双向，合并 = {_altPairMerge.Value}）"
                : "（未生效：交还原生『主仓库/后仓/柜台前格 → 展示柜』）"));
        LoggerInstance.Msg(
            $"  ★ Shift 配对快移（§四·2）= {(ShiftPairActive ? "开" : "关")}" +
            (ShiftPairActive
                ? $"（展示柜 ↔ 主仓库双向，与 Alt 共用合并开关 = {_altPairMerge.Value}）"
                : "（交还原生：Shift+左键单向「导向展示柜」）"));
        // ★ 未知值告警：AltPairMode 用区分大小写的精确匹配，填错会【静默】退回原生。
        //   启动时显式提示，避免「以为开了却没生效」的静默故障。
        if (_altPairMode != null
            && _altPairMode.Value != "counter_inv"
            && _altPairMode.Value != "native")
        {
            LoggerInstance.Warning(
                $"  ★ AltPairMode 值「{_altPairMode.Value}」无法识别（仅接受 \"counter_inv\" / \"native\"，" +
                "区分大小写），已按退回原版处理。");
        }
        LoggerInstance.Msg(
            $"  ★ 合并决策单一源（0.10.0 重构）= {UseMergePolicy}" +
            (UseMergePolicy
                ? "（拖放/落点/快移三路径统一走 MergePolicy，判据只在一处定序）"
                : "（已回退到 0.9.9 各自内联判据的老路径）"));
        LoggerInstance.Msg(
            $"  ★ 整堆计价 = {_stackValueTotal.Value}" +
            (_stackValueTotal.Value
                ? "（GetNegociatedValue × unitCount ⇒ 卖出/买入/批发/估价/盗窃/犯罪价值 全部按整堆计；" +
                  "首次开启请先用 ×2~×3 小堆在柜台实测确认扣减语义）"
                : "（关闭：交易系统仍按单件计价，堆叠物品卖出只按 1 件结账）"));
    }
    internal static bool StackEnabled => _enableStacking.Value;

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
    internal static void Diag(string message)
    {
        if (DiagOn) Log.Msg("[Diag] " + message);
    }

    // ── 0.6.2：防护降级「一次性告警」──
    // 各防护判据（黑名单 / 作用域 / 排除容器 / 容器判定 / 耐久 / 柜台探测）在异常时都走
    // fail-safe（返回 null/false = 放行），保证游戏能继续玩；但默认 DiagLog=false 时
    // 用户【完全无感】—— 防护会静默消失，这正是本项目最痛的一类故障。
    // 这里对「每类判据」只告警一次（HashSet 去重防刷屏），走 Log.Warning 无条件输出。
    private static readonly HashSet<string> _warnedOnce = new HashSet<string>(StringComparer.Ordinal);
    private static void WarnOnce(string key, string msg)
    {
        if (_warnedOnce.Add(key))
        {
            // ★ 0.9.9：MelonLoader 的 HarmonyInit 自动 PatchAll 早于 OnInitializeMelon，
            //   该阶段 Log 还是 null（0.9.8 实机首崩 NRE 根因），退回全局日志。
            if (Log != null) Log.Warning("[防护降级] " + msg);
            else MelonLogger.Warning("[防护降级] " + msg);
        }
    }
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
            WarnOnce("slotmigrate", $"slot token 迁移失败（忽略，行为等同未迁移）: {e.Message}");
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

    // ── 0.12.0：从 MelonPreferences.cfg 文本中移除已废弃的旧键条目 ──
    //    MelonPreferences 只写「本次注册」的键，**不会删除**旧版本遗留的孤儿键 ⇒
    //    旧键与其说明注释会永久残留（死文字）。本 mod 的 cfg 段格式（实机核对）：
    //        # 说明注释（可能多行，紧邻紧随）
    //        KeyName = value
    //    故移除一个条目 = 删掉 `KeyName = value` 行 + 其**紧邻上方的连续注释行**。
    //    纯文本操作，无游戏对象访问；命中才写盘，失败一律忽略（不影响启动）。
    private static void RemoveLegacyModifierKeyEntry()
        => RemoveCfgKeys(new[] { "AltClickOnClickMode" });   // 已收编为 AltPairEnable

    private static void RemoveCfgKeys(string[] keys)
    {
        try
        {
            string path = System.IO.Path.Combine(
                MelonLoader.Utils.MelonEnvironment.UserDataDirectory, "MelonPreferences.cfg");
            if (!System.IO.File.Exists(path)) return;

            var enc = new System.Text.UTF8Encoding(false);          // 与 MelonLoader 写盘一致（无 BOM）
            var lines = System.IO.File.ReadAllLines(path, enc);
            var keep = new List<string>(lines.Length);
            var removed = new List<string>();
            bool inSection = false;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();

                if (trimmed.Length > 1 && trimmed[0] == '[' && trimmed[trimmed.Length - 1] == ']')
                {
                    inSection = trimmed == "[ItemStacking]";
                    keep.Add(line);
                    continue;
                }

                if (inSection && trimmed.Length > 0 && trimmed[0] != '#')
                {
                    int eq = trimmed.IndexOf('=');
                    string key = eq > 0 ? trimmed.Substring(0, eq).Trim() : null;
                    if (key != null && Array.IndexOf(keys, key) >= 0)
                    {
                        // 回退删除紧随其上的说明注释块（直到段头或非注释行为止）
                        while (keep.Count > 0 && keep[keep.Count - 1].TrimStart().StartsWith("#"))
                            keep.RemoveAt(keep.Count - 1);
                        removed.Add(key);
                        continue;                                   // 丢弃 `key = value` 行
                    }
                }

                keep.Add(line);
            }

            if (removed.Count == 0) return;                         // 无旧键 → 不写盘（避免无谓改动）
            System.IO.File.WriteAllLines(path, keep, enc);
            if (Log != null)
                Log.Msg($"[配置清理] 已从 MelonPreferences.cfg 移除废弃键：{string.Join(", ", removed)}");
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            WarnOnce("cfgclean", $"废弃键清理失败（忽略，仅影响 cfg 整洁）: {e.Message}");
        }
    }
    // ── 生命周期：柜台容器 id 一次性打印 ──
    public override void OnUpdate()
    {
        try
        {
            CounterIdProbeTick();   // 0.6.6：进交易场景后一次性打印柜台容器 id（配白名单用）
            ModifierKeyProbeTick(); // 0.12.0：进场景后一次性打印三个快移 handler 的实际键位（配按键分派）
            ReportStateCmpTiming(); // 1.0.0：状态比较耗时统计（仅 DiagLog=true 时输出）
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
        }
    }
}

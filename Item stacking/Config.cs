using System;
using System.Collections.Generic;
using MelonLoader;
using MelonLoader.Preferences;

namespace ItemStacking;

// 配置（MelonPreferences）：条目声明 + 建表 + 读取属性 + cfg 迁移
//
// ★ cfg 段名 = AssemblyName = "ItemStacking"。
// ★ 每个 CreateEntry 的 description 会被 MelonLoader 写进 cfg 文件（玩家看到的注释）。
//   故 description 面向玩家：简洁 + 关键风险提示，不写版本沿革。
// ★ 改 cfg 后【需重启游戏】生效（MelonLoader 不监听文件变化）；
//   唯一例外是 ModManager 面板改（它直接写 Entry.Value）。
// ★ 逻辑设计详见 README.md；本文件只做声明。
public partial class Main
{
    // ── 配置项 ──
    private static MelonPreferences_Entry<bool> _enableStacking = null!;
    private static MelonPreferences_Entry<int> _newStackCap = null!;
    private static MelonPreferences_Entry<bool> _ctrlWholeStack = null!;
    private static MelonPreferences_Entry<bool> _ctrlTransferMergeGate = null!;
    private static MelonPreferences_Entry<bool> _diagLog = null!;

    // ── 黑白名单「四列表」架构 ──
    //   ContainerWhitelist  —— 非空 ⇒ 仅名单内容器可堆叠（唯一作用域开关）
    //   ContainerBlacklist  —— 名单内容器禁止堆叠（双端判定）
    //   ItemBlacklist       —— 按 id 禁止堆叠（自定义；内置名单在代码里）
    //   StackBlacklist      —— 总闸（关 = 内置 + 自定义全关）
    private static MelonPreferences_Entry<bool> _stackBlacklist = null!;
    private static MelonPreferences_Entry<bool> _builtinIdBlacklist = null!;

    // ★ 容器白名单的代码默认串（CreateEntry 与迁移共用同一份，避免漂移）。
    //   注意：机器容器（熔炉/解序器/水瓶打印机等）不在白名单里 ——
    //   它们由 machineContainers 名单单独拦截（禁止合并 + 每格 1 件）。
    private const string DefaultContainerWhitelist =
        "owner:storage_bay,owner:storage_bay_large,owner:smuggler_bay,owner:smuggler_bay_mod," +
        "owner:smuggler_bay_mini,owner:machine_bay,owner:machine_bay_ext," +
        "owner:backpack_small,owner:backpack_medium,owner:backpack_large,owner:satchel,owner:fanny_pack," +
        "owner:animal_cage,owner:fridge,owner:recharger,owner:vacuum_robot," +
        "slot:save_bag,slot:showcase_save_bag,slot:hidden_save_bag,slot:back_inv_save_bag";

    private static MelonPreferences_Entry<string> _containerWhitelist = null!;
    private static MelonPreferences_Entry<string> _containerBlacklist = null!;
    private static MelonPreferences_Entry<string> _itemBlacklist = null!;

    // ── 修饰键快移（总闸 + 三小开关） ──
    private static MelonPreferences_Entry<bool> _modifierKeysEnabled = null!;
    private static MelonPreferences_Entry<bool> _altTransferEnable = null!;
    private static MelonPreferences_Entry<bool> _altPairEnable = null!;
    private static MelonPreferences_Entry<bool> _shiftPairEnable = null!;
    private static MelonPreferences_Entry<int> _altQuickTransferAmount = null!;
    private static MelonPreferences_Entry<string> _altPairMode = null!;
    private static MelonPreferences_Entry<bool> _altPairMerge = null!;

    // ── 交易 / 状态 / 合并 ──
    private static MelonPreferences_Entry<bool> _stackValueTotal = null!;
    private static MelonPreferences_Entry<bool> _useMergePolicy = null!;
    private static MelonPreferences_Entry<bool> _valueGate = null!;
    private static MelonPreferences_Entry<bool> _strictStackCap = null!;
    private static MelonPreferences_Entry<bool> _machineOnePerSlot = null!;
    private static MelonPreferences_Entry<bool> _verboseStartup = null!;

    // ── 建表 ──
    private static void CreateConfigEntries()
    {
        var cat = MelonPreferences.CreateCategory("ItemStacking", "物品堆叠增强");

        _enableStacking = cat.CreateEntry<bool>(
            "EnableStacking", true,
            "启用拖放堆叠",
            "把一个物品拖到同名物品上即合并堆叠。");

        _newStackCap = cat.CreateEntry<int>(
            "NewStackCap", 10,
            "堆叠上限",
            "单个槽位允许的最大堆叠数量。");

        _ctrlWholeStack = cat.CreateEntry<bool>(
            "CtrlDragWholeStack", true,
            "Ctrl+拖动 = 整堆",
            "按住 Ctrl 开始拖动 = 拿起整堆。\n" +
            "不按修饰键 / Shift / Alt 均交还原版：无修饰键=整堆，Shift=10%，Alt=1 个。");

        _ctrlTransferMergeGate = cat.CreateEntry<bool>(
            "CtrlTransferMergeGate", true,
            "Ctrl 点击快移 · 合并闸门",
            "Ctrl+左键【点击快移】的合并闸门。\n" +
            "开 = Ctrl 快移的合并也走判定：状态不一致 / 物品黑名单 / 容器含内部物品 → 拒绝合并；" +
            "容器白/黑名单不通过 → 改投空槽（不合并）。\n" +
            "关 = 退回改前行为（Ctrl 快移可无视名单直接合并）。");

        _diagLog = cat.CreateEntry<bool>(
            "DiagLog", false,
            "诊断日志（总开关）",
            "开 = 输出 [Diag] / [整堆计价] / [作用域探针] 等排查日志。\n" +
            "始终输出（不受本项影响）：启动自检 + 故障告警。\n" +
            "默认关闭；排查问题时再开。改后需重启游戏。");

        _verboseStartup = cat.CreateEntry<bool>(
            "VerboseStartup", true,
            "启动详细日志",
            "开（默认）= 逐项打印白/黑名单、修饰键、闸门状态等明细。\n" +
            "关 = 只输出 1-2 行核心汇总。\n" +
            "安全告警与配置错误告警不受本项影响，始终输出。");

        _stackBlacklist = cat.CreateEntry<bool>(
            "StackBlacklist", true,
            "堆叠保护总闸",
            "关 = 关掉【内置 id 黑名单 + 自定义 ItemBlacklist】。\n" +
            "不受本项影响：状态一致性闸门（恒定开启）、容器白/黑名单。");

        _builtinIdBlacklist = cat.CreateEntry<bool>(
            "BuiltinIdBlacklist", true,
            "内置 id 黑名单",
            "★ 仅当 StackBlacklist = true 时本项才生效。\n" +
            "开（默认）= 拦截 node/module/keycard/文书/名片/刮刮乐/指南手册等。\n" +
            "关 = 仅 cfg 自定义 ItemBlacklist 生效。\n" +
            "备注：能量电池 / 金属锭当前不在启用名单内，需拦截请填 ItemBlacklist。");

        _containerWhitelist = cat.CreateEntry<string>(
            "ContainerWhitelist", DefaultContainerWhitelist,
            "容器白名单（唯一作用域开关，空 = 全不生效）",
            "只有名单内的容器允许堆叠，其余一律禁止。★ 空 = 全不生效（mod 等于未启用）。\n" +
            "token 三种：① 裸 id（如 backInv）；② slot:槽位id（save_bag=主仓库 / showcase_save_bag=展示柜 / " +
            "back_inv_save_bag=玩家柜台 / hidden_save_bag=海报暗格 / sold_save_bag=已售格 / " +
            "frontInv=柜台前格 / backInvCounter=柜台三格 / bazarInv=黑市左侧）；" +
            "③ owner:物品id（如 owner:backpack_medium）。\n" +
            "只看落点容器；与黑名单同时命中时黑名单优先；含逗号请用引号包起来。\n" +
            "机器容器（熔炉/解序器/水瓶打印机等）不在本名单里 —— 由机器名单单独拦截（禁并 + 每格 1 件）。\n" +
            "★ 已有 cfg 里的旧值优先于代码默认值；要采用新默认请删除本行后重启。");

        _containerBlacklist = cat.CreateEntry<string>(
            "ContainerBlacklist", "",
            "容器黑名单（名单内容器禁止堆叠）",
            "token 与白名单同一套（裸 id / slot:槽位id / owner:物品id）。\n" +
            "留空 = 不排除任何容器。双端判定：被叠/拖起任一方命中即拦。\n" +
            "空槽放置不受限；与白名单同时命中时黑名单优先。");

        _itemBlacklist = cat.CreateEntry<string>(
            "ItemBlacklist", "",
            "物品黑名单（按 id 禁止堆叠）",
            "在内置名单之外【额外】补充的自定义拦截（逗号分隔的 identifier）。\n" +
            "留空 = 不额外拦截（内置项仍生效，受 StackBlacklist + BuiltinIdBlacklist 控制）。\n" +
            "含逗号请用引号包起来。");

        _machineOnePerSlot = cat.CreateEntry<bool>(
            "MachineOnePerSlot", true,
            "机器容器每格 1 件（禁并）",
            "开（默认）= 机器容器（熔炉/解序器/水瓶打印机等）内不允许合并，且每格只放 1 件。\n" +
            "关 = 退回旧行为（机器里可能一次塞进多件物品，有丢件风险）。\n" +
            "覆盖拖放 / Ctrl 拖动 / Ctrl 点击快移 / Alt 快移全部入口。");

        _modifierKeysEnabled = cat.CreateEntry<bool>(
            "ModifierKeysEnabled", true,
            "修饰键快移（总开关）",
            "关 = Alt/Shift 快移全部退回原版。\n" +
            "等价于同时关掉 AltQuickTransferEnable / AltPairEnable / ShiftPairEnable。");

        _altTransferEnable = cat.CreateEntry<bool>(
            "AltQuickTransferEnable", true,
            "Alt 快移（总闸）",
            "开 = Alt+点击/拖拽走本 mod 快移逻辑（目标由 AltPairMode 决定）。\n" +
            "关 = 完全交还原版。需与 ModifierKeysEnabled 同时开启。");

        _altPairEnable = cat.CreateEntry<bool>(
            "AltPairEnable", true,
            "Alt 走本 mod 逻辑",
            "开 = 走本 mod 接管（目标由 AltPairMode 决定，默认 \"ctrl\"）。\n" +
            "关 = 完全交还原版：Alt+点击/拖拽 = 原版行为（本 mod 不再改写落点/数量）。\n" +
            "需与 ModifierKeysEnabled + AltQuickTransferEnable 同时开启。");

        _shiftPairEnable = cat.CreateEntry<bool>(
            "ShiftPairEnable", false,
            "Shift 配对快移（展示柜 ↔ 主仓库）",
            "开 = 展示柜 ↔ 主仓库双向配对。\n" +
            "关（默认）= 交还原版单向「Shift 点主仓库 → 展示柜」。\n" +
            "需与 ModifierKeysEnabled 同时开启。");

        _altQuickTransferAmount = cat.CreateEntry<int>(
            "AltQuickTransferAmount", 1,
            "Alt 快移件数",
            "Alt / Shift 快移每次转移几个（默认 1）。\n" +
            "1 = 每次 1 件；设成很大值（如 9999）= 等价原版整堆。");

        _altPairMode = cat.CreateEntry<string>(
            "AltPairMode", Constants.AltPairModeCtrl,
            "Alt 快移目标模式",
            "可选值（区分大小写）：\n" +
            "  \"ctrl\"（默认）  = 与 Ctrl+点击同一条逻辑（目标判定交原生）。\n" +
            "  \"counter_inv\"   = 玩家柜台 ↔ 主仓库 双向配对。\n" +
            "  \"native\"        = 原版：主仓库/后仓/柜台前格 → 展示柜。\n" +
            "★ 数量由 AltQuickTransferAmount 决定；只搬运本人拥有的物品。");

        _altPairMerge = cat.CreateEntry<bool>(
            "AltPairMerge", true,
            "配对快移时允许合并（Alt/Shift 共用）",
            "开（默认）= 能合就合，省格位。\n" +
            "关 = 强制搬到空位；目标无空位则本次不搬（绝不丢件）。\n" +
            "只影响配对快移（AltPairEnable / ShiftPairEnable 打开时）。");

        _stackValueTotal = cat.CreateEntry<bool>(
            "StackValueTotal", false,
            "整堆计价",
            "开 = 交易系统按整堆总价值结账（卖出/买入/批发/估价/盗窃/犯罪价值全部 ×unitCount）。\n" +
            "★ 与加价 mod 存在顺序冲突：对方先抬单件价、我们再 ×N 会放大 N 倍。\n" +
            "★ 默认关，避免悄悄改变交易体验。");

        _useMergePolicy = cat.CreateEntry<bool>(
            "UseMergePolicy", true,
            "合并判定用哪套代码（普通玩家请保持开）",
            "true（默认）= 单一源 MergePolicy：拖放 / Alt / Ctrl 三条路径调用同一个中心判定，\n" +
            "  含状态一致性 + 机器容器禁并。\n" +
            "false = 0.9.9 旧内联判据（回滚用）。会失去状态一致性保护与机器容器禁并。");

        _valueGate = cat.CreateEntry<bool>(
            "ValueGate", true,
            "tag 值域校验闸门（防拆分后状态污染）",
            "开（默认）= 状态相等后再校验关键 tag 是否等于默认模板值\n" +
            "  （电量/耐久/纯度/品质/液体/次数/赃物热度等），防拆分后状态污染。\n" +
            "关 = 退回只比状态（可能放行偏离默认的堆叠）。\n" +
            "★ 普通玩家保持开。");

        _strictStackCap = cat.CreateEntry<bool>(
            "StrictStackCap", false,
            "堆叠上限硬收紧（探针恒开）",
            "关（默认）= 只输出 maxnumadd.clamp.* 探针日志。\n" +
            "开 = 强制把堆叠上限收紧到 NewStackCap（防外部 mod 注入超大值绕过上限）。\n" +
            "下限恒保 1（返 0 会打断原生逐格填充循环）。");

        // ── 旧键迁移（重命名的两个 cfg 项） ──
        //   原则：cfg 里有【旧键】、无【新键】→ 把旧值复制到新键，让老玩家升级后不丢设置。
        //   新键已存在 / 都无 → 不动（新键用默认值）。
        MigrateRenamedCfgKey(cat, "z9_UseMergePolicy", "UseMergePolicy");
        MigrateRenamedCfgKey(cat, "AltClickTransferToCounter", "AltQuickTransferEnable");

        // 说明：MelonLoader 会忽略 cfg 里的未注册键（旧键仍留在文本里但不再被读）。
        // 玩家可手动删掉旧键行保持整洁，不删也无功能影响。

        cat.SaveToFile();
    }

    // ───────────────────────────────────────────────────────────────────────
    // 属性访问器（cfg 值 → 内部读取；未初始化时给保守默认）
    //   ★ null 兜底：MelonLoader 的 HarmonyInit 阶段会先自动 PatchAll 一次，
    //     那时本 mod 的 cfg 尚未建表（entry 仍为 null），补丁 Prefix 已可能读这些属性。
    //     ⇒ 所有属性都要在 null 时给「保守 = 与代码默认一致」的值。
    // ───────────────────────────────────────────────────────────────────────

    internal static bool StackEnabled => _enableStacking == null || _enableStacking.Value;
    internal static bool BuiltinIdBlacklist => _builtinIdBlacklist == null || _builtinIdBlacklist.Value;
    internal static bool StrictStackCap => _strictStackCap != null && _strictStackCap.Value;
    internal static bool MachineOnePerSlot => _machineOnePerSlot == null || _machineOnePerSlot.Value;
    internal static bool ValueGateEnabled => _valueGate == null || _valueGate.Value;

    // 堆叠上限下限钳制（用户误设 0 / 负数时不出错，至少保留 1）。
    internal static int StackCap => Math.Max(1, _newStackCap.Value);

    // Alt 快移件数（下限钳到 1；上限 9999，防极端值把 GetSplitItem 参数搞坏）。
    internal static int AltQuickTransferAmount
        => _altQuickTransferAmount == null ? 1 : Math.Clamp(_altQuickTransferAmount.Value, 1, 9999);

    internal static bool ModifierKeysEnabled
        => _modifierKeysEnabled == null || _modifierKeysEnabled.Value;

    // Alt 快移是否需要本 mod 接管（总闸 + AltQuickTransferEnable 都开）。
    internal static bool AltTransferTakeover
        => ModifierKeysEnabled
        && _altTransferEnable != null && _altTransferEnable.Value;

    internal static bool StackValueTotalOn => _stackValueTotal != null && _stackValueTotal.Value;

    // 诊断日志开关：实时读 cfg（ModManager 面板改立即生效；外部改文件需重启）。
    internal static bool DiagOn => _diagLog != null && _diagLog.Value;

    // ───────────────────────────────────────────────────────────────────────
    // 旧中文 slot: token → 游戏原生 id 的一次性迁移（0.11.2）
    //   旧（0.11.1-）：slot:背包(主仓库) / slot:展示柜 / slot:柜台前格 / …
    //   新（0.11.2+）：slot:save_bag / slot:showcase_save_bag / slot:frontInv / …
    //   替换只针对 `slot:` 前缀后的完整 token（逗号分隔后按 token 精确比对），
    //   不会误伤 owner:/裸 id 里的同名子串。
    // ───────────────────────────────────────────────────────────────────────
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
                parts[i] = t;   // 统一 Trim
            }
        }
        if (!changed) return;

        string migrated = string.Join(",", parts);
        entry.Value = migrated;
        MelonPreferences.Save();
        if (Log != null)
            Log.Msg($"[slot 迁移] {name} 旧中文槽位 token → 游戏原生 id：\"{cur}\" ⇒ \"{migrated}\"");
    }

    // ───────────────────────────────────────────────────────────────────────
    // cfg 键重命名迁移
    //   cfg 里有【旧键】、无【新键】→ 把旧值复制到新键（老玩家升级后不丢设置）。
    //   做法：读 cfg 文件文本按行扫，精确匹配 `<key> = <value>` 形式的旧键值。
    //   ★ 为什么不用 MelonPreferences API：MelonLoader 未暴露「遍历 cfg 里未注册键」的接口。
    //   ★ 不删除旧键行（MelonLoader 会忽略它，留着无功能影响；避免手写 TOML 解析）。
    // ───────────────────────────────────────────────────────────────────────
    private static void MigrateRenamedCfgKey(MelonPreferences_Category cat, string oldKey, string newKey)
    {
        try
        {
            string path = System.IO.Path.Combine(
                MelonLoader.Utils.MelonEnvironment.UserDataDirectory,
                "MelonPreferences.cfg");
            if (!System.IO.File.Exists(path)) return;

            string rawOld = ReadCfgKeyValue(path, oldKey);
            if (rawOld == null) return;                    // 无旧键 → 不需要迁移
            if (ReadCfgKeyValue(path, newKey) != null) return;  // 已有新键 → 用户已手动改过，不覆盖

            // 用 entry API 写入（避免手写 TOML）
            var entry = cat.GetEntry<bool>(newKey);
            if (entry == null) return;
            if (!bool.TryParse(rawOld, out var bv)) return;
            entry.Value = bv;
            if (Log != null)
                Log.Msg($"[cfg 迁移] 键重命名：{oldKey} → {newKey} = {bv}");
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("cfg-rename", Level.Warn, $"旧键迁移失败（忽略，行为等同未迁移）: {e.Message}");
        }
    }

    // 从 cfg 文本里读 `<key> = <value>` 的原始值（不含段头行、跳过注释）。
    //   匹配规则：行首空白 + 完全等于 key + 等号 + 值；忽略以 # 或 [ 开头的行。
    private static string ReadCfgKeyValue(string cfgPath, string key)
    {
        try
        {
            foreach (var line in System.IO.File.ReadAllLines(cfgPath))
            {
                var trimmed = line.TrimStart();
                if (trimmed.Length == 0) continue;
                if (trimmed[0] == '#' || trimmed[0] == '[') continue;
                int eq = trimmed.IndexOf('=');
                if (eq <= 0) continue;
                string k = trimmed.Substring(0, eq).Trim();
                if (k != key) continue;
                return trimmed.Substring(eq + 1).Trim();
            }
        }
        catch { /* 读取失败 → 视为没有该键，迁移跳过 */ }
        return null;
    }
}
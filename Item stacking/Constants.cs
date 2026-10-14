namespace ItemStacking;

// 魔法常量集中（单一维护点，避免各文件手写副本拼写漂移）
internal static class Constants
{
    // ── 容器列表 token 前缀（cfg ContainerWhitelist/Blacklist 列表值） ──
    //   解析点：StackGates.AnyTokenMatches（前缀分支 + Substring 用 .Length，
    //   前缀改了长度自动跟上）。
    internal const string TokenSlotPrefix = "slot:";
    internal const string TokenOwnerPrefix = "owner:";

    // ── AltPairMode 可识别值（cfg 字符串枚举，区分大小写精确匹配） ──
    //   消费点：Main 启动未知值告警、ModifierPairTransfer.AltPairActive 判定。
    internal const string AltPairModeCounterInv = "counter_inv";
    internal const string AltPairModeNative = "native";
    // Alt 转发给 Ctrl 快移的原生逻辑（目标判定全部由游戏完成）。
    internal const string AltPairModeCtrl = "ctrl";

    // ── 后缀黑名单的豁免表条目（后缀命中但放行）——空白钥匙卡 ──
    //   消费点：BuiltinBlacklist.DefaultBlockSuffixExempt。
    internal const string SuffixExemptBlankKeycard = "blank_keycard";

    // ── 关键 tag 清单：会影响「拆分后状态」的 tag ──
    //   只收「拆分后会被 DirectoryMaster.Item 重置为默认值」的【物品属性】tag。
    //   不收机器运行时状态（已在 StateConsistency 忽略名单里，收进来只会误伤）。
    //   ★ 取值：TagState 是 7 值字段联合体，统一用「字符串化」比较（见 ValueGate.TagFingerprint）。
    internal static readonly string[] ValueGateTags =
    {
        // 电量
        "power_source_item_energy",     // 电池/能量物品当前电量（int）
        "CURRENT_CHARGE_INT",           // 充电类物品当前电量（int）
        // 耐久
        "DURABILITY_TAG",               // 当前耐久（int）
        "DURABILITY_FAKE_TAG",          // 未鉴定的耐久（int）
        // 使用次数
        "CURRENT_USE_COUNT_INT",        // 工具剩余使用次数（int）
        // 液体
        "LIQUID_CONTAINER_CURRENT",     // 液体容器当前量（int）
        // 金属锭纯度
        "INGOT_PURITY_TAG",             // 纯度（string：INGOT_PURITY_*）
        "INGOT_PURITY_FAKE_TAG",        // 未鉴定纯度（string）
        // 莓果 / 食物品质
        "BERRY_CONDITION_INT",          // 莓果状态（int：-2 发霉 / 1/2 新鲜）
        // 酒
        "WINE_SCORE_INT",               // 酒评分（int）
    };

    // ── 零值 tag 清单：这些 tag 必须【缺省或 == 0】才允许堆叠 ──
    //   为什么不能并入 ValueGateTags（模板法）：模板是全新物品，【没有】STOLEN_VALUE_INT；
    //   直接放进 ValueGateTags 会静默跳过 = fail-open，无效。
    //   语义（0.14.0 定稿）：0 热度可堆 / 非 0 不可堆（填补「热度相同但非 0 也能合并」的缺口）。
    internal static readonly string[] ValueGateZeroTags =
    {
        // 赃物热度（int；缺省或 0 = 干净/无热度）
        "STOLEN_VALUE_INT",
    };
}
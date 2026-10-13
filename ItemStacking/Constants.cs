namespace ItemStacking;

// ══════════════════════════════════════════════════════════════════════════
// M3 · 魔法常量集中（2026-10-12 计划任务）
//   把散落在 StackGates / ModifierPairTransfer / ValueGate / BuiltinBlacklist /
//   Main / Config 里的「同一含义字符串字面量」收到这里，单一维护点，
//   避免各文件手写副本拼写漂移。移动前后运行时值完全一致，行为无变化。
// ══════════════════════════════════════════════════════════════════════════
internal static class Constants
{
    // ── 容器列表 token 前缀（cfg ContainerWhitelist/Blacklist 列表值）──
    //   解析点：StackGates.AnyTokenMatches（前缀分支 + Substring 用 .Length，
    //   不再硬写 5 / 6，前缀改了长度自动跟上）。
    internal const string TokenSlotPrefix  = "slot:";
    internal const string TokenOwnerPrefix = "owner:";

    // ── AltPairMode 可识别值（cfg 字符串枚举，区分大小写精确匹配）──
    //   消费点：Main 启动未知值告警、Main.AltPairActive 判定（ModifierPairTransfer.cs）。
    internal const string AltPairModeCounterInv = "counter_inv";
    internal const string AltPairModeNative     = "native";
    // ★ 2026-10-13 新增（用户定稿）：Alt 不再用"固定配对"，改为【转发给 Ctrl 快移的原生逻辑】——
    //   目标判定（raycast → 父窗口 → 活动容器）全部由游戏完成，与 Ctrl+点击同源；
    //   本 mod 只保留"数量可配（默认 1）"与"机器容器改投空槽"两件事。
    internal const string AltPairModeCtrl       = "ctrl";

    // ── 后缀黑名单的豁免表条目（后缀命中但放行）——空白钥匙卡 ──
    //   消费点：BuiltinBlacklist.DefaultBlockSuffixExempt。
    internal const string SuffixExemptBlankKeycard = "blank_keycard";

    // ★ 关键 tag 清单：会影响「拆分后状态」的 tag（★ 维护入口，需要增删时改这里）。
    //   原位置 ValueGate.cs（M3 迁入，语义与内容未动）。
    //   ── 解决什么问题 ──
    //   每个条目 = 一个「会影响拆分后状态的 tag」。判断 = 该 tag 的当前值必须等于模板值。
    //   取「值」用 TagState.GetInt()/GetString()（TagState 是 7 值字段联合体，
    //   实际存哪个字段由 tag 类型决定；统一用「字符串化」比较，见 ValueGate.TagFingerprint）。
    //   ★ 只收「拆分后会被 DirectoryMaster.Item 重置为默认值」的【物品属性】tag。
    //     不收机器运行时状态（MACHINE_*/PROGRESS_*/CURRENT_PLAYING_* 等已在状态忽略名单里，
    //     它们不随拆分污染，收进来只会误伤机器/加工中的物品）。
    internal static readonly string[] ValueGateTags =
    {
        // — 电量 —
        "power_source_item_energy",     // 电池/能量物品当前电量（int）
        "CURRENT_CHARGE_INT",           // 充电类物品当前电量（int）
        // — 耐久 —
        "DURABILITY_TAG",               // 当前耐久（int）
        "DURABILITY_FAKE_TAG",          // 未鉴定的耐久（int）
        // — 使用次数 —
        "CURRENT_USE_COUNT_INT",        // 工具剩余使用次数（int）
        // — 液体 —
        "LIQUID_CONTAINER_CURRENT",     // 液体容器当前量（int）
        // — 金属锭纯度 —
        "INGOT_PURITY_TAG",             // 纯度（string：INGOT_PURITY_*）
        "INGOT_PURITY_FAKE_TAG",        // 未鉴定纯度（string）
        // — 莓果 / 食物品质 —
        "BERRY_CONDITION_INT",          // 莓果状态（int：-2 发霉 / 1/2 新鲜）
        // — 酒 —
        "WINE_SCORE_INT",               // 酒评分（int）
    };

    // ★ 零值 tag 清单：这些 tag 必须【缺省或 == 0】才允许堆叠（★ 维护入口，需要增删时改这里）。
    //   ── 为什么不能并入 ValueGateTags（模板法） ──
    //   ValueGateTags 的基准是 DirectoryMaster.Item(id) 模板值；模板是全新物品，【没有】
    //   STOLEN_VALUE_INT（赃物热度是游戏运行时施加的），而 GetDefaultState 只登记模板上
    //   存在的 tag（fp != null 才入表）⇒ 直接放进 ValueGateTags 会静默跳过 = fail-open，无效。
    //   故采用「零值变体」：不比模板，直接要求当前值缺省或为 0。
    //   ── 语义（0.14.0，用户定稿） ──
    //   0 热度可堆 / 非 0 不可堆（按字面规则：STOLEN_TAG 存在但热度=0/缺省 → 放行，
    //   由状态一致性闸门继续管其余差异）。填补的是「热度相同但非 0 也能合并」的缺口。
    internal static readonly string[] ValueGateZeroTags =
    {
        // — 赃物热度 —
        "STOLEN_VALUE_INT",             // 赃物热度值（int；缺省或 0 = 干净/无热度）
    };
}

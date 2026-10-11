using System;
using System.Collections.Generic;
using System.Diagnostics;
using Il2Cpp;
using MelonLoader;

namespace ItemStacking;

// ══════════════════════════════════════════════════════════════════════════
//  1.0.0 · 批次2：状态一致性判定（State Consistency Gate）
//
//  ── 核心规则 ──
//  两件物品的 GameItem.state（TagSystem）必须【完全相等】才允许合并。
//  这是【不可豁免的最终硬闸门】：白名单 / 物品名单都不能放行状态不一致。
//
//  ── 为什么用 state 全比，而不存快照 ──
//  v0.1 原方案 #12「合并时保存逐件元数据快照」已作废 —— 核桃的方案更简单也更安全：
//  状态不一致就不许合，自然无需存快照 ⇒ 存档负担为零、无指针映射、无内存泄漏，
//  原方案「元数据一致性 / 指针失联 / 内存泄漏」三大风险全部消除。
//
//  ── 数据模型（反汇编取证，2026-10-11）──
//    GameItem.state        = TagSystem（字段 0x1B0，_state_k__BackingField 直读，非 invoke）
//    TagSystem.dict        = Il2CppSystem.Collections.Generic.Dictionary<string, TagState>（0x10）
//    TagState  值字段      = valueEnabled(0x20) / internalValueString(0x28) / valueInt(0x30)
//                            / valueFloat(0x34) / valueLong(0x38) / valueDouble(0x40) / valueBool(0x48)
//  三处均为 interop 直接内存读写（见 Il2CppInterop 反编译），【零 il2cpp_runtime_invoke】，
//  无异常风险、极快。
//
//  ── ★ 比较口径 = 游戏自己的口径（TagState.Clone 0x7B1F70 逐字段取证）──
//  TagState.Clone 恰好拷贝上面 7 个值字段（+ identifier / identifierName 两个身份字段）。
//  即「游戏认为两个 TagState 怎样才算不一样」= 这 7 个值字段任一不同。
//  ★ 本类照抄该口径（借鉴官方实现，不自创）。
//
//  ── unitCount 为什么不参与 ──
//  [取证] unitCount 是 GameItem 自身字段（+0x18），【不在 TagSystem 之内】——
//  只要比的是 state，unitCount 结构上就不可能被比到（核桃要求「绝对不参与比较」自动满足）。
//
//  ── 忽略名单（Ignored Tags）──
//  内置默认（黑名单式：列出的 tag 不参与比较）+ item_blacklist.json 的 ignoreTags 可覆盖。
//  理由：这些 tag 描述「系统对它做了什么」，而非「物品本身是什么」。
//  两件同样磨损的刀，不该因为一把「正被 UI 选中」就被判为状态不同。
// ══════════════════════════════════════════════════════════════════════════

public partial class Main
{
    // ──────────────────────────────────────────────────────────────────────
    // 忽略名单：代码默认值（永不失守的兜底；JSON 可覆盖）
    // ──────────────────────────────────────────────────────────────────────
    private static readonly string[] DefaultIgnoredStateTags =
    {
        // — 系统状态 / 锁定 / 可见性（纯运行时）—
        "LOCKED_TAG", "LOCKED_CONTAINER_TAG", "LOCKED_WORKING_TAG",
        "ITEM_HIDDEN_TAG", "ITEM_BEHIND_LOCK_TAG", "IS_TOOLTIP_DISABLED_TAG",
        "HAND_TAG", "IMPORTANT_TAG", "IS_CRISIS_TAG", "GCI_SEATED_IN_SHIP_TAG",
        // — UI / 预览 / 选中（瞬时）—
        "CHOICE_PREVIEW_TAG", "IS_ITEM_TOGGLE_ON_TAG", "SELECTED_CHEM_ID_TAG",
        "TOGGLE_APPEARANCE_TAG", "LAMP_TARGET_TAG", "TRANSMITTER_TARGET_TAG",
        "LAST_SCAN_RESULT_TAG", "LOOT_CRATE_SCANNED_TAG", "AUG_SCANNER_TAG", "ALREADY_CONFRONTED_TAG",
        // — 机器 / 进度（随时间变，非物品属性）—
        "MACHINE_STATE_TAG", "MACHINE_PROGRESS_CURRENT_TAG", "MACHINE_PROGRESS_TARGET_TAG",
        "PROGRESS_ITEM_TAG", "PROGRESS_ITEM_TARGET_TAG", "PROGRESS_ITEM_TARGET_ITEM_COUNT_TAG",
        "PROGRESS_ITEM_TARGET_ITEM_ID_TAG", "PROGRESS_ITEM_REQUIRED_MACHINE_TAG",
        "PROGRESS_TYPE_MACHINE_TAG",
        "DESEQUENCER_TARGET_ID_TAG", "DESEQUQUENCER_PROGRESS_TAG",
        "CURRENT_PLAYING_CASSETTE_INDEX_TAG", "CURRENT_PLAYING_TRACK_ID_TAG",
        "CONTAINER_TEMP_TAG", "HEATING_TAG",
        // — 教程 / 引导（会自行消失）—
        "TUTORIAL_PAPER_TAG", "GUIDE_PAPER_TAG",
        // — 价格 / 成本（核桃明确：进货价不影响卖出价）—
        "BOUGHT_PRICE_TAG",
    };

    // 生效忽略名单（由 LoadIgnoredStateTags() 在启动时填充；未加载前 = 代码默认值）。
    private static string[] IgnoredStateTags = DefaultIgnoredStateTags;
    private static string _ignoredTagsSource = "代码默认";

    // 快查集合：判据在超热路径上，用 HashSet 而非数组线性扫（约 42 项，线性也可，但集合更稳）。
    private static HashSet<string> _ignoredTagSet;

    // ──────────────────────────────────────────────────────────────────────
    // 加载入口：在 LoadBuiltinBlacklist() 之后调用一次（复用同一个 JSON 文件）。
    // ──────────────────────────────────────────────────────────────────────
    internal static void LoadIgnoredStateTags()
    {
        try
        {
            // 先按默认建集合（保证任何路径下 _ignoredTagSet 都非 null）。
            RebuildIgnoredTagSet();

            var path = FindBlacklistJson();
            if (path == null) return;   // 无外部文件 = 正常情况，用默认

            string text;
            try { text = System.IO.File.ReadAllText(path); }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                WarnOnce("ist.read", $"读取 {BlacklistJsonName} 失败（忽略名单），改用代码默认（防护不受影响）：{e.Message}");
                return;
            }

            var doc = MiniJson.ParseObject(text);
            if (doc == null) return;    // 格式非法：LoadBuiltinBlacklist 已告警，这里静默用默认

            string[] overridden = null;
            string src = "代码默认";
            if (TryOverride(doc, "ignoreTags", ref overridden, ref src))
            {
                IgnoredStateTags = overridden;
                _ignoredTagsSource = src;
                RebuildIgnoredTagSet();
                Log?.Msg($"[忽略名单] 已加载外部 {BlacklistJsonName} 的 ignoreTags：" +
                         $"{IgnoredStateTags.Length} 项[{_ignoredTagsSource}]。");
            }
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            // ★ 兜底：任何意外都保证集合非 null（退回默认，防护不丢）。
            try { RebuildIgnoredTagSet(); } catch { }
            WarnOnce("ist.fatal", $"加载忽略名单异常，改用代码默认（防护不受影响）：{e.Message}");
        }
    }

    private static void RebuildIgnoredTagSet()
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        if (IgnoredStateTags != null)
            foreach (var t in IgnoredStateTags)
                if (!string.IsNullOrEmpty(t)) set.Add(t);
        _ignoredTagSet = set;
    }

    private static bool IsStateTagIgnored(string key)
        => _ignoredTagSet != null && _ignoredTagSet.Contains(key);

    // ──────────────────────────────────────────────────────────────────────
    // ★ 核心：两件物品的状态是否完全相等
    //   返回 true = 相等（可合）；false = 不等（不可合）。
    //   异常 → 抛给调用方（Evaluate 会 MarkJudgeFault + Deny）。本方法自身不吞异常，
    //   以便「比较过程抛异常 ⇒ 判不等」的保守语义由故障位机制统一处理。
    // ──────────────────────────────────────────────────────────────────────
    internal static bool StatesEqual(GameItem a, GameItem b)
    {
        long t0 = 0;
        bool timing = _stateCmpTimingOn;
        if (timing) t0 = Stopwatch.GetTimestamp();

        try
        {
            var sa = a == null ? null : a._state_k__BackingField;
            var sb = b == null ? null : b._state_k__BackingField;

            // 同一个 TagSystem 实例（极罕见，物品共享状态）→ 恒等。
            if (ReferenceEquals(sa, sb)) return true;
            // 一方无状态：另一方也无状态才算相等。
            if (sa == null || sb == null) return sa == null && sb == null;

            var da = sa.dict;
            var db = sb.dict;
            if (da == null || db == null) return da == null && db == null;

            // ★ 快筛：key 数量。仅在【忽略名单为空】时 Count 相等才是必要条件。
            //   ⚠️ 坑（坏样本自检抓出）：忽略名单非空时，两方「被忽略的 key 数量」可以不同，
            //   此时 Count 不等也可能是相等状态（如 a={E}，b={E,UI_SEL_TAG}）⇒ 不能提前 return false。
            if (da.Count != db.Count && (_ignoredTagSet == null || _ignoredTagSet.Count == 0))
                return false;

            // 正向：a 的每个【未被忽略】key，b 必须也有且 TagState 相等。
            foreach (var kv in da)
            {
                string key = kv.key;
                if (key == null) continue;               // 理论不该发生；跳过（双方都会跳过，不影响对称性）
                if (IsStateTagIgnored(key)) continue;

                // 严格相等：a 有的 key，b 必须也有（否则判不等）。
                if (!db.TryGetValue(key, out var vb)) return false;
                if (!TagStateEqual(kv.value, vb)) return false;
            }

            // 反向：b 的每个【未被忽略】key，a 必须也有。
            //   ⇒ 双向核对保证「非忽略 key 集合」严格相同（仅正向不够：b 可能多出 a 没有的 key）。
            foreach (var kv in db)
            {
                string key = kv.key;
                if (key == null) continue;
                if (IsStateTagIgnored(key)) continue;
                if (!da.ContainsKey(key)) return false;
            }

            // 被忽略的 key 不参与相等语义：一侧被 UI 选中、另一侧没有，仍视为状态相同。
            //   ⇒ 【不比较两侧忽略项数量】（坏样本自检已证：强制相等是错的）。
            return true;
        }
        finally
        {
            if (timing)
            {
                long dt = Stopwatch.GetTimestamp() - t0;
                _stateCmpCalls++;
                _stateCmpTicks += dt;
            }
        }
    }

    // 单个 TagState 的严格相等（7 个值字段 + 7 字段两两比）。
    //   ★ 照抄 TagState.Clone（0x7B1F70）的字段口径。
    //   ★ 如实说明：valueFloat / valueDouble 实际用 C# 数值比较（`!=`）——NaN 判【不等】、
    //     ±0.0 判【相等】；与 Clone 的逐字节口径仅在上述极端值上不同（tag 浮点极罕见）。
    //     采用数值比较是刻意选择：行为更宽松可预期、无浮点位级依赖；若需严格字节口径，
    //     可改为 BitConverter.SingleToInt32Bits / DoubleToInt64Bits 比较。
    private static bool TagStateEqual(TagState x, TagState y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;

        if (x.valueEnabled != y.valueEnabled) return false;
        if (x.valueInt != y.valueInt) return false;
        if (x.valueLong != y.valueLong) return false;
        if (x.valueBool != y.valueBool) return false;
        if (x.valueFloat != y.valueFloat) return false;
        if (x.valueDouble != y.valueDouble) return false;
        if (!string.Equals(x.internalValueString, y.internalValueString, StringComparison.Ordinal))
            return false;
        return true;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  耗时统计（§三 3.5）
    //   DiagLog=true 时每 5 秒打印「状态比较调用 X 次 / 累计 Y ms / 平均 Z µs」。
    //   使「是否卡」有数据而非感觉。
    // ══════════════════════════════════════════════════════════════════════
    private static long _stateCmpCalls;
    private static long _stateCmpTicks;
    private static float _stateCmpLastReport;

    // 只在 DiagLog 开着时才计时（避免正常游玩时白付 Stopwatch 成本）。
    private static bool _stateCmpTimingOn => DiagOn;

    private static void ReportStateCmpTiming()
    {
        if (!DiagOn) return;
        float now = UnityEngine.Time.realtimeSinceStartup;
        if (_stateCmpLastReport <= 0f) { _stateCmpLastReport = now; return; }
        if (now - _stateCmpLastReport < 5f) return;
        _stateCmpLastReport = now;

        long calls = _stateCmpCalls;
        long ticks = _stateCmpTicks;
        _stateCmpCalls = 0;
        _stateCmpTicks = 0;
        if (calls == 0) return;

        double ms = ticks * 1000.0 / Stopwatch.Frequency;
        double us = ms * 1000.0 / calls;
        Log?.Msg($"[状态比较·耗时] 最近 5 秒：调用 {calls} 次 / 累计 {ms:F2} ms / 平均 {us:F2} µs。");
    }
}

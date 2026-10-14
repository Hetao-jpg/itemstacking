using System;
using System.Collections.Generic;
using System.Diagnostics;
using Il2Cpp;
using MelonLoader;

namespace ItemStacking;

// 状态一致性判定（State Consistency Gate）
//
// ★ 核心规则
//   两件物品的 GameItem.state（TagSystem）必须【完全相等】才允许合并。
//   这是【不可豁免的最终硬闸门】：白名单 / 物品名单都不能放行状态不一致。
//
// ★ 为什么用 state 全比，而不存快照
//   状态不一致就不许合，自然无需存快照 ⇒ 存档负担为零、无指针映射、无内存泄漏。
//
// ★ 数据模型（反汇编取证）
//   GameItem.state  = TagSystem（字段 0x1B0，_state__BackingField 直读，非 invoke）
//   TagSystem.dict  = Dictionary<string, TagState>（0x10）
//   TagState 值字段 = valueEnabled / internalValueString / valueInt / valueFloat /
//                     valueLong / valueDouble / valueBool
//   三处均为 interop 直接内存读写，【零 il2cpp_runtime_invoke】，无异常风险、极快。
//
// ★ 比较口径 = 游戏自己的口径（TagState.Clone 逐字段取证）
//   Clone 恰好拷贝上面 7 个值字段 ⇒ 「游戏认为两个 TagState 怎样才算不一样」= 这 7 个任一不同。
//   本类照抄该口径。
//
// ★ 忽略名单（Ignored Tags）
//   这些 tag 描述「系统对它做了什么」，而非「物品本身是什么」：
//   两件同样磨损的刀，不该因为一把「正被 UI 选中」就被判状态不同。
//   ★ 内置默认 + item_blacklist.json 的 ignoreTags 可覆盖。
public partial class Main
{
    // ── 忽略名单：代码默认值（永不失守的兜底；JSON 可覆盖） ──
    private static readonly string[] DefaultIgnoredStateTags =
    {
        // 系统状态 / 锁定 / 可见性（纯运行时）
        "LOCKED_TAG", "LOCKED_CONTAINER_TAG", "LOCKED_WORKING_TAG",
        "ITEM_HIDDEN_TAG", "ITEM_BEHIND_LOCK_TAG", "IS_TOOLTIP_DISABLED_TAG",
        "HAND_TAG", "IMPORTANT_TAG", "IS_CRISIS_TAG", "GCI_SEATED_IN_SHIP_TAG",
        // UI / 预览 / 选中（瞬时）
        "CHOICE_PREVIEW_TAG", "IS_ITEM_TOGGLE_ON_TAG", "SELECTED_CHEM_ID_TAG",
        "TOGGLE_APPEARANCE_TAG", "LAMP_TARGET_TAG", "TRANSMITTER_TARGET_TAG",
        "LAST_SCAN_RESULT_TAG", "LOOT_CRATE_SCANNED_TAG", "AUG_SCANNER_TAG", "ALREADY_CONFRONTED_TAG",
        // 机器 / 进度（随时间变，非物品属性）
        "MACHINE_STATE_TAG", "MACHINE_PROGRESS_CURRENT_TAG", "MACHINE_PROGRESS_TARGET_TAG",
        "PROGRESS_ITEM_TAG", "PROGRESS_ITEM_TARGET_TAG", "PROGRESS_ITEM_TARGET_ITEM_COUNT_TAG",
        "PROGRESS_ITEM_TARGET_ITEM_ID_TAG", "PROGRESS_ITEM_REQUIRED_MACHINE_TAG",
        "PROGRESS_TYPE_MACHINE_TAG",
        "DESEQUENCER_TARGET_ID_TAG", "DESEQUQUENCER_PROGRESS_TAG",
        "CURRENT_PLAYING_CASSETTE_INDEX_TAG", "CURRENT_PLAYING_TRACK_ID_TAG",
        "CONTAINER_TEMP_TAG", "HEATING_TAG",
        // 教程 / 引导（会自行消失）
        "TUTORIAL_PAPER_TAG", "GUIDE_PAPER_TAG",
        // 价格 / 成本（进货价不影响卖出价）
        "BOUGHT_PRICE_TAG",
    };

    private static string[] IgnoredStateTags = DefaultIgnoredStateTags;
    private static string _ignoredTagsSource = "代码默认";

    // 快查集合：判据在超热路径上，用 HashSet 而非数组线性扫。
    private static HashSet<string> _ignoredTagSet;

    // ── 加载入口：在 LoadBuiltinBlacklist() 之后调用一次（复用同一个 JSON 文件） ──
    internal static void LoadIgnoredStateTags()
    {
        try
        {
            // 先按默认建集合（保证任何路径下 _ignoredTagSet 都非 null）。
            RebuildIgnoredTagSet();

            var path = FindBlacklistJson();
            if (path == null) return;

            string text;
            try { text = System.IO.File.ReadAllText(path); }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                LogOnce("ist.read", Level.Warn, $"读取 {BlacklistJsonName} 失败（忽略名单），改用代码默认：{e.Message}");
                return;
            }

            var doc = MiniJson.ParseObject(text);
            if (doc == null) return;   // 格式非法：LoadBuiltinBlacklist 已告警，这里静默用默认

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
            // 兜底：任何意外都保证集合非 null（退回默认，防护不丢）。
            try { RebuildIgnoredTagSet(); } catch { }
            LogOnce("ist.fatal", Level.Warn, $"加载忽略名单异常，改用代码默认（防护不受影响）：{e.Message}");
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

    // ── 核心：两件物品的状态是否完全相等 ──
    //   返回 true = 相等（可合）；false = 不等（不可合）。
    //   异常 → 抛给调用方（Evaluate 会 MarkJudgeFault + Deny）。
    //   ★ 本方法自身不吞异常 —— 便于「比较过程抛异常 ⇒ 判不等」的保守语义由故障位机制统一处理。
    internal static bool StatesEqual(GameItem a, GameItem b)
    {
        long t0 = 0;
        bool timing = _stateCmpTimingOn;
        if (timing) t0 = Stopwatch.GetTimestamp();

        try
        {
            var sa = a == null ? null : a._state_k__BackingField;
            var sb = b == null ? null : b._state_k__BackingField;

            // 同一个 TagSystem 实例（极罕见）→ 恒等。
            if (ReferenceEquals(sa, sb)) return true;
            if (sa == null || sb == null) return sa == null && sb == null;

            var da = sa.dict;
            var db = sb.dict;
            if (da == null || db == null) return da == null && db == null;

            // ★ 快筛：key 数量。仅在【忽略名单为空】时 Count 相等才是必要条件。
            //   ⚠️ 忽略名单非空时，两方「被忽略的 key 数量」可以不同 ⇒ 不能提前 return false。
            if (da.Count != db.Count && (_ignoredTagSet == null || _ignoredTagSet.Count == 0))
            {
                DiagStateDiff(a, b, "(key 数量不同) da=" + da.Count + " db=" + db.Count);
                return false;
            }

            // 正向：a 的每个【未被忽略】key，b 必须也有且 TagState 相等。
            foreach (var kv in da)
            {
                string key = kv.key;
                if (key == null) continue;
                if (IsStateTagIgnored(key)) continue;

                if (!db.TryGetValue(key, out var vb))
                {
                    DiagStateDiff(a, b, "b 缺少 tag「" + key + "」");
                    return false;
                }
                if (!TagStateEqual(kv.value, vb))
                {
                    DiagStateDiff(a, b, "tag「" + key + "」值不同：" + DescribeTagState(kv.value) + " vs " + DescribeTagState(vb));
                    return false;
                }
            }

            // 反向：b 的每个【未被忽略】key，a 必须也有。
            //   ⇒ 双向核对保证「非忽略 key 集合」严格相同（仅正向不够：b 可能多出 a 没有的 key）。
            foreach (var kv in db)
            {
                string key = kv.key;
                if (key == null) continue;
                if (IsStateTagIgnored(key)) continue;
                if (!da.ContainsKey(key))
                {
                    DiagStateDiff(a, b, "a 缺少 tag「" + key + "」");
                    return false;
                }
            }

            // 被忽略的 key 不参与相等语义：【不比较两侧忽略项数量】。
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

    // 状态差异定位诊断（判不等时打印具体差异 tag）。
    //   受 DiagOn 总闸控制，按「物品 id 对 + 差异 key」限流（同一差异只打一次）。
    private static void DiagStateDiff(GameItem a, GameItem b, string detail)
    {
        if (!DiagOn) return;
        string idA = null, idB = null;
        try { idA = a?.identifier; } catch { }
        try { idB = b?.identifier; } catch { }
        LogOnce("state.diff." + idA + "/" + idB + "|" + detail, Level.Diag,
            $"[状态差异] {idA} / {idB}：{detail}", max: 3);
    }

    // 单个 TagState 的可读描述（差异定位用；7 个值字段里挑"非默认"的打出来）。
    private static string DescribeTagState(TagState x)
    {
        if (x == null) return "<null>";
        var parts = new System.Text.StringBuilder();
        if (x.valueEnabled) parts.Append("enabled");
        if (x.valueInt != 0) parts.Append(parts.Length > 0 ? "," : "").Append("int=").Append(x.valueInt);
        if (x.valueLong != 0) parts.Append(parts.Length > 0 ? "," : "").Append("long=").Append(x.valueLong);
        if (x.valueBool) parts.Append(parts.Length > 0 ? "," : "").Append("bool=true");
        if (x.valueFloat != 0f) parts.Append(parts.Length > 0 ? "," : "").Append("float=").Append(x.valueFloat);
        if (x.valueDouble != 0d) parts.Append(parts.Length > 0 ? "," : "").Append("double=").Append(x.valueDouble);
        if (!string.IsNullOrEmpty(x.internalValueString)) parts.Append(parts.Length > 0 ? "," : "").Append("str=").Append(x.internalValueString);
        return parts.Length == 0 ? "<空>" : parts.ToString();
    }

    // 单个 TagState 的严格相等（照抄 TagState.Clone 的 7 字段口径）。
    //   ★ valueFloat / valueDouble 用 C# 数值比较（NaN 判【不等】、±0.0 判【相等】）；
    //     与 Clone 的逐字节口径仅在极端值上不同（tag 浮点极罕见）。
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
}
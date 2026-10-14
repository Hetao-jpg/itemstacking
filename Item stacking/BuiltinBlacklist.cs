using System;
using System.Collections.Generic;
using System.IO;
using Il2Cpp;
using MelonLoader;

namespace ItemStacking;

// 内置黑白名单「代码默认 + 可选外部 JSON」
//
// ★ 设计原则：绝不能让「文件缺失/损坏」变成「防护消失」。
//   双层结构：
//     · JSON 不存在 / 解析失败 / 字段缺失 → 一律退回代码默认值（防护永不丢）。
//     · JSON 存在且某组给出 → 该组以 JSON 为准，其余组仍用默认。
//     · JSON 里某组显式给空数组 → 视为「该组清空」（玩家的显式意图，尊重）。
//   判据：把「是否提供了这一组」和「这一组是不是空」区分开 —— null（未提供）vs 空数组（清空）。
//
// ★ 文件位置：Mods/item_blacklist.json（DLL 同目录优先，其次 UserData）。
//
// ★ 自检：启动时把「实际采用来源（默认/JSON）」与各组条目数打进日志，
//   玩家一眼看出 JSON 到底生效了没有（避免「改了文件但没生效」）。
public partial class Main
{
    // ── 代码默认值（永不失守的兜底） ──
    //   id 名单来自 items_catalog.md；游戏若改名相关拦截会【静默失效】，
    //   用 DiagLog 可观测是否命中。
    private static readonly string[] DefaultBlockItemIds =
    {
        // 水质检测试纸 / 净水药片（含容器）
        "water_test_strip", "water_tablet", "water_tablet_container",
        // 莓果类（果实 / 种子 / 果酿）
        "stardust_berry_fruit", "bloomberry_seed",
        "wine_bloomberry", "wine_gloomberry",
        // 能量阵列（PowerBloc，独立能量状态）
        "powerblock",
        // 文书/纸质类（每件内容独立，合并后除第一件外不可见）
        // 便签/单据/档案/报纸/合同/证书/教程
        "handnote", "postit", "slip", "procurement_slip", "sec_slip",
        "rev_codeword", "exchange_directive", "intel_sec", "wanted_paper",
        "newspaper", "expedition_log", "business_permit", "mentor_contract",
        "cert1", "tutorial_book", "tutorial_page",
        // 鉴定指南/使用说明/手册
        "aug_id_guide", "cigarette_guide", "fuel_guide", "hand_guide",
        "stamp_guide", "water_guide", "ribwich_guide", "injector_guide",
        "logo_checker", "ingot_book", "wine_book", "be_brewing_manual",
        "be_barrel_usage_guide", "be_concentrator_usage_guide",
        "be_incubator_usage_guide",
        // 名片
        "card_chem", "card_mining", "card_pharma", "card_rev",
        "joe_card", "card_brewer", "be_nini_card",
        // 食品券/刮刮乐/酿酒证书
        "food_stamp", "scratcher", "scratcher_stack",
        "be_basic_cert", "be_mid_cert", "be_high_cert",
        // 酿酒配方手札
        "be_recipe_1", "be_recipe_2", "be_recipe_3", "be_recipe_4",
        "be_recipe_5", "be_recipe_6", "be_recipe_7", "be_recipe_8",
    };

    // 按 id 前缀拦截的整类（节点 node*，独立进度/归属）。
    private static readonly string[] DefaultBlockIdPrefixes = { "node" };

    // 按 id 子串拦截（module 在 system_module_*/farm_module_*/… 的中间/末尾，
    // 前缀匹配抓不到；neural_core 完全不含 module 也要单独子串）。
    private static readonly string[] DefaultBlockIdContains = { "module", "neural_core" };

    // 按 id 后缀拦截（*_keycard：med/eng/cmd/sci/sec/ser/sup 7 种功能卡）。
    private static readonly string[] DefaultBlockIdSuffixes = { "_keycard" };
    private static readonly string[] DefaultBlockSuffixExempt = { Constants.SuffixExemptBlankKeycard };

    // ── 机器容器名单 ──
    //   机器内不允许堆叠合并 + 每格 1 件（详见 README.md「机器容器保护」）。
    //   token 复用 IsContainerInList 三路匹配（裸 id / slot: / owner:）。
    //   有意排除：fridge / recharger / cassette_player / machine_bay* 等储物型（无崩坏证据）；
    //             security_alarm / mirage_projector / deal_maker 等无物品输入格的机器。
    private static readonly string[] DefaultMachineContainerTokens =
    {
        "owner:furnace",
        "owner:desequencer",
        "owner:bottle_printer",
        "owner:box_dispenser",
        "owner:moisture_farm",
        "owner:water_purifier",
        "owner:wine_rack",
        "owner:livestock_feed_dispenser",
        "owner:blender",
        "owner:chem_finisher",
        "owner:heating_plate",
        "owner:hydroponic",
        "owner:3d_printer",
    };

    // 生效名单（由 LoadBuiltinBlacklist 在启动时填充；未加载前 = 代码默认）。
    private static string[] MachineContainerTokens = DefaultMachineContainerTokens;
    private static string _machineSource = "代码默认";

    // ── 机器容器识别（供 MergePolicy / Patch_MaxNumAdd 调用） ──
    //   异常时 fail-safe 返回 false，但先置 FaultProtection 故障位 ——
    //   MergePolicy 据此走 Deny 保守路径（宁可拦不可丢）。
    internal static bool IsMachineContainer(GameInventory inv)
    {
        if (inv == null) return false;
        var tokens = MachineContainerTokens;
        if (tokens == null || tokens.Length == 0)
            return false;   // 显式清空（JSON []）= 玩家选择关闭机器拦截
        try
        {
            // 直传 string[]（复用数组重载）—— 省掉 string.Join + Split 的分配。
            return IsContainerInList(inv, tokens);
        }
        catch
        {
            MarkJudgeFault(FaultProtection);
            return false;
        }
    }

    // ── 生效名单（由 LoadBuiltinBlacklist 填充；未加载前 = 代码默认） ──
    //   static 字段而非 readonly —— 才能被 JSON 覆盖。
    private static string[] BuiltinBlockItemIds = DefaultBlockItemIds;
    private static string[] BuiltinBlockIdPrefixes = DefaultBlockIdPrefixes;
    private static string[] BuiltinBlockIdContains = DefaultBlockIdContains;
    private static string[] BuiltinBlockIdSuffixes = DefaultBlockIdSuffixes;
    private static string[] BuiltinBlockSuffixExempt = DefaultBlockSuffixExempt;

    // 来源标签（写日志用）："代码默认" / "外部JSON"（逐组）。
    private static string _blSourceItemIds = "代码默认";
    private static string _blSourcePrefixes = "代码默认";
    private static string _blSourceContains = "代码默认";
    private static string _blSourceSuffixes = "代码默认";
    private static string _blSourceExempt = "代码默认";

    private const string BlacklistJsonName = "item_blacklist.json";

    // ── 加载入口（在 OnInitializeMelon 里、ValidatePatches 之前调用一次） ──
    internal static void LoadBuiltinBlacklist()
    {
        try
        {
            var path = FindBlacklistJson();
            if (path == null)
            {
                // 无外置文件 = 正常情况（绝大多数玩家），静默用代码默认值。
                Log?.Msg($"[名单] 无外部 {BlacklistJsonName}，使用代码默认内置名单（" +
                         $"id {BuiltinBlockItemIds.Length} / 前缀 {BuiltinBlockIdPrefixes.Length} / " +
                         $"子串 {BuiltinBlockIdContains.Length} / 后缀 {BuiltinBlockIdSuffixes.Length} / " +
                         $"机器 {MachineContainerTokens.Length}）。");
                return;
            }

            string text;
            try { text = File.ReadAllText(path); }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                LogOnce("bl.read", Level.Warn, $"读取 {BlacklistJsonName} 失败，改用代码默认名单（防护不受影响）：{e.Message}");
                return;
            }

            var doc = MiniJson.ParseObject(text);
            if (doc == null)
            {
                LogOnce("bl.parse", Level.Warn, $"{BlacklistJsonName} 格式非法（应为 JSON 对象），改用代码默认名单（防护不受影响）。");
                return;
            }

            // 逐组判断：字段【不存在】= 未提供（用默认）；字段【是空数组】= 显式清空。
            bool anyOverride = false;
            anyOverride |= TryOverride(doc, "blockItemIds", ref BuiltinBlockItemIds, ref _blSourceItemIds);
            anyOverride |= TryOverride(doc, "blockIdPrefixes", ref BuiltinBlockIdPrefixes, ref _blSourcePrefixes);
            anyOverride |= TryOverride(doc, "blockIdContains", ref BuiltinBlockIdContains, ref _blSourceContains);
            anyOverride |= TryOverride(doc, "blockIdSuffixes", ref BuiltinBlockIdSuffixes, ref _blSourceSuffixes);
            anyOverride |= TryOverride(doc, "blockSuffixExempt", ref BuiltinBlockSuffixExempt, ref _blSourceExempt);
            anyOverride |= TryOverride(doc, "machineContainers", ref MachineContainerTokens, ref _machineSource);

            if (anyOverride)
            {
                Log?.Msg($"[名单] 已加载外部 {BlacklistJsonName}（{path}）：" +
                         $"id={BuiltinBlockItemIds.Length}[{_blSourceItemIds}] / " +
                         $"前缀={BuiltinBlockIdPrefixes.Length}[{_blSourcePrefixes}] / " +
                         $"子串={BuiltinBlockIdContains.Length}[{_blSourceContains}] / " +
                         $"后缀={BuiltinBlockIdSuffixes.Length}[{_blSourceSuffixes}] / " +
                         $"豁免={BuiltinBlockSuffixExempt.Length}[{_blSourceExempt}] / " +
                         $"机器={MachineContainerTokens.Length}[{_machineSource}]。");
            }
            else
            {
                Log?.Msg($"[名单] 外部 {BlacklistJsonName} 存在但未提供任何可识别字段，使用代码默认名单。");
            }
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("bl.fatal", Level.Warn, $"加载 {BlacklistJsonName} 异常，使用代码默认名单（防护不受影响）：{e.Message}");
        }
    }

    // 定位 JSON：DLL 同目录优先，其次 MelonLoader UserData 目录。
    private static string FindBlacklistJson()
    {
        try
        {
            var dllDir = Path.GetDirectoryName(typeof(Main).Assembly.Location);
            if (!string.IsNullOrEmpty(dllDir))
            {
                var p1 = Path.Combine(dllDir, BlacklistJsonName);
                if (File.Exists(p1)) return p1;
            }
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
        }
        try
        {
            var p2 = Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, BlacklistJsonName);
            if (File.Exists(p2)) return p2;
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
        }
        return null;
    }

    // 若 doc 提供 key（值为字符串数组），则覆盖 target 并记来源；返回是否发生覆盖。
    private static bool TryOverride(IDictionary<string, object> doc, string key,
                                    ref string[] target, ref string source)
    {
        if (doc == null || !doc.TryGetValue(key, out var raw) || raw == null) return false;
        if (raw is not List<object> list) return false;   // 类型不对 = 不覆盖（保默认）

        var cleaned = new List<string>(list.Count);
        foreach (var item in list)
        {
            var s = item as string ?? (item?.ToString() ?? "");
            if (!string.IsNullOrWhiteSpace(s)) cleaned.Add(s.Trim());
        }
        target = cleaned.ToArray();
        source = "外部JSON";
        return true;
    }
}

// ══════════════════════════════════════════════════════════════════════════
// 极简 JSON 解析器（只够解析「对象 + 字符串数组」这一种结构）
//
// ★ 为什么不用 System.Text.Json / Newtonsoft：net6.0 目标需额外引用；
//   本项目坚持「单 DLL 交付、零外部依赖」。
// ★ 只做「尽力解析」；出现任何意外 → 返回 null → 用默认名单。
// ══════════════════════════════════════════════════════════════════════════
internal static class MiniJson
{
    // 解析顶层对象为字典；失败返回 null。
    public static Dictionary<string, object> ParseObject(string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            int i = 0;
            return ParseObjectAt(text, ref i);
        }
        catch { return null; }
    }

    // 扫描式提取：在任意嵌套层级找出所有名为 key 的字符串值 / 字符串数组元素。
    //   用于从 item_catalog.json 里捞出所有 id / aliases。
    //   不要求结构合法（文件里有不认识的字段也照样能捞出目标键）。
    public static HashSet<string> ExtractAllStringsByKey(string text, string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrEmpty(key)) return null;
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int i = 0;
            int n = text.Length;
            while (i < n)
            {
                while (i < n && text[i] != '"') i++;
                if (i >= n) break;
                int save = i;
                string k = ParseString(text, ref i);
                if (k == null) { i = save + 1; continue; }

                int j = i;
                while (j < n && char.IsWhiteSpace(text[j])) j++;
                if (j >= n || text[j] != ':') { i = save + 1; continue; }

                if (!string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                {
                    i = save + 1;
                    continue;
                }

                j++;
                while (j < n && char.IsWhiteSpace(text[j])) j++;
                if (j >= n) break;
                if (text[j] == '"')
                {
                    int k2 = j;
                    string v = ParseString(text, ref k2);
                    if (v != null) { set.Add(v); i = k2; }
                    else i = save + 1;
                }
                else if (text[j] == '[')
                {
                    int k2 = j;
                    var arr = ParseArray(text, ref k2);
                    if (arr != null)
                    {
                        foreach (var el in arr)
                            if (el is string sv && !string.IsNullOrWhiteSpace(sv)) set.Add(sv);
                        i = k2;
                    }
                    else i = save + 1;
                }
                else i = save + 1;
            }
            return set;
        }
        catch { return null; }
    }

    private static Dictionary<string, object> ParseObjectAt(string s, ref int i)
    {
        SkipWs(s, ref i);
        if (i >= s.Length || s[i] != '{') return null;
        i++;
        var map = new Dictionary<string, object>(StringComparer.Ordinal);
        SkipWs(s, ref i);
        if (i < s.Length && s[i] == '}') { i++; return map; }

        while (i < s.Length)
        {
            SkipWs(s, ref i);
            if (i >= s.Length || s[i] != '"') return null;
            string key = ParseString(s, ref i);
            if (key == null) return null;
            SkipWs(s, ref i);
            if (i >= s.Length || s[i] != ':') return null;
            i++;
            SkipWs(s, ref i);

            object value = ParseValue(s, ref i);
            if (value == null) return null;
            map[key] = value;

            SkipWs(s, ref i);
            if (i < s.Length && s[i] == ',') { i++; continue; }
            if (i < s.Length && s[i] == '}') { i++; return map; }
            return null;
        }
        return null;
    }

    private static object ParseValue(string s, ref int i)
    {
        SkipWs(s, ref i);
        if (i >= s.Length) return null;
        char c = s[i];
        if (c == '[') return ParseArray(s, ref i);
        if (c == '"') return ParseString(s, ref i);
        if (c == '{') return ParseObjectAt(s, ref i);
        if (c == 't') { if (Match(s, ref i, "true")) return true; return null; }
        if (c == 'f') { if (Match(s, ref i, "false")) return false; return null; }
        if (c == 'n') { if (Match(s, ref i, "null")) return null; return null; }
        return ParseNumber(s, ref i);
    }

    private static List<object> ParseArray(string s, ref int i)
    {
        if (s[i] != '[') return null;
        i++;
        var list = new List<object>();
        SkipWs(s, ref i);
        if (i < s.Length && s[i] == ']') { i++; return list; }

        while (i < s.Length)
        {
            SkipWs(s, ref i);
            object v = ParseValue(s, ref i);
            if (v == null && (i >= s.Length || s[i] != 'n')) return null;
            list.Add(v);
            SkipWs(s, ref i);
            if (i < s.Length && s[i] == ',') { i++; continue; }
            if (i < s.Length && s[i] == ']') { i++; return list; }
            return null;
        }
        return null;
    }

    private static string ParseString(string s, ref int i)
    {
        if (i >= s.Length || s[i] != '"') return null;
        i++;
        var sb = new System.Text.StringBuilder();
        while (i < s.Length)
        {
            char c = s[i++];
            if (c == '"') return sb.ToString();
            if (c == '\\' && i < s.Length)
            {
                char e = s[i++];
                switch (e)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        if (i + 4 <= s.Length)
                        {
                            var hex = s.Substring(i, 4);
                            if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber,
                                             System.Globalization.CultureInfo.InvariantCulture, out var code))
                            {
                                sb.Append((char)code);
                                i += 4;
                            }
                        }
                        break;
                    default: sb.Append(e); break;
                }
            }
            else sb.Append(c);
        }
        return null;
    }

    private static object ParseNumber(string s, ref int i)
    {
        int start = i;
        while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '-' || s[i] == '+' ||
                                s[i] == '.' || s[i] == 'e' || s[i] == 'E'))
            i++;
        if (i == start) return null;
        var sub = s.Substring(start, i - start);
        if (double.TryParse(sub, System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out var d))
            return d;
        return null;
    }

    private static void SkipWs(string s, ref int i)
    {
        while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\r' || s[i] == '\n'))
            i++;
    }

    private static bool Match(string s, ref int i, string word)
    {
        if (i + word.Length > s.Length) return false;
        if (string.CompareOrdinal(s, i, word, 0, word.Length) != 0) return false;
        i += word.Length;
        return true;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader;

namespace ItemStacking;

// ══════════════════════════════════════════════════════════════════════════
//  0.10.0 · 阶段5：内置黑白名单「硬编码 → 代码默认值 + 可选外置 JSON」
//
//  ── 为什么要外置 ──
//  内置 id 名单（53+ 项文书类 / 前缀 / 子串 / 后缀）硬编码在 Main.cs 里，
//  游戏每次新增一类物品（新模组 id、新文书、新钥匙卡）都要【改代码 + 重新编译 + 重新发 DLL】，
//  分享给其他玩家时更无法按自己的存档/版本微调 → 名单永远滞后于游戏。
//
//  ── 为什么不直接改成「读文件为准」 ──
//  ★ 铁律（本项目踩过）：绝不能让「文件缺失/损坏」变成「防护消失」。
//  因此本类是【代码默认值 + 可选覆盖】的双层结构：
//    · JSON 不存在 / 解析失败 / 字段缺失 → 一律退回下方代码默认值（防护永不丢）。
//    · JSON 存在且某组给出 → 该组以 JSON 为准（可增可减），其余组仍用默认。
//    · JSON 里某组显式给空数组 → 视为「该组清空」（这是玩家的显式意图，需尊重）。
//  判据：把「是否提供了这一组」和「这一组是不是空」区分开 —— 用 null（未提供）vs 空数组（显式清空）。
//
//  ── 文件位置 ──
//  与 mod DLL 同目录：`Mods\item_blacklist.json`（不存在则无感，用默认值）。
//  同理兼容 MelonLoader 的 UserData 目录（两处任一存在即用，DLL 目录优先）。
//
//  ── 自检 ──
//  启动时把「实际采用来源（默认/JSON）」与各组条目数打进日志（无条件），
//  玩家一眼能看出自己的 JSON 到底生效了没有（避免"改了文件但没生效"的静默故障）。
// ══════════════════════════════════════════════════════════════════════════

public partial class Main
{
    // ──────────────────────────────────────────────────────────────────────
    // 代码默认值（与 0.9.5 硬编码版本逐条一致，作为「永不失守的兜底」）
    // ──────────────────────────────────────────────────────────────────────

    // ★ 0.6.6：内置按-id 拦截名单。
    //   历史原因：MelonLoader 把含逗号的裸字符串写成非法 TOML，会让整份 MelonPreferences.cfg
    //   载入失败（所有 mod 配置无法保存），故名单不放 cfg 默认值而放代码里。
    // ★ 0.7.0：新增「模组类 / 节点类」——模组带独立装配数据、节点带独立进度，合并会丢状态。
    private static readonly string[] DefaultBlockItemIds =
    {
        "water_test_strip", "water_tablet",                 // 0.6.6 水质检测试纸 / 净水药片
        "water_tablet_container",                           // 0.7.1 一瓶净水药片（容器，内含独立药片数）
        // ★ 0.7.1「能量电池」系列（用户："空电池加入黑名单"）。
        //   目录里没有叫"空电池"的物品：满电/空电共用同一 id，靠 power_source_item_energy tag 区分。
        //   ⇒ 整类拦（不分满电空电），最可靠。三个 id 覆盖全部能量电池。
        "energy_credit", "energy_credit_ext", "energy_credit_breeder",
        // ★ 0.7.1「莓果」（用户：果实 + 种子 + 果酿）。
        "wine_berry",                                       // 荧光莓（果实）
        "stardust_berry_fruit",                             // 星尘莓（果实）
        "bloomberry_seed",                                  // 花莓种子
        "wine_bloomberry", "wine_gloomberry",               // 荧光莓果酿 / 阴光莓果酿
        // ★ 0.7.1「能量阵列」（PowerBloc，独立能量状态）。
        "powerblock",
        // ★「金属锭」（用户："金属锭加入堆叠黑名单"）。
        //   资料库 items_catalog.md 里带"锭"字 / ingot 的物品只有两个：
        //     metal_ingot（金属锭，本项要拦的）/ ingot_book（冶金手册，说明书，无独立状态，不拦）。
        //   ⇒ 用精确 id 而非子串 "ingot"，避免把冶金手册一起误伤。
        //   拦的理由：金属锭带 INGOT_PURITY_TAG / INGOT_PURITY_*（纯度分
        //   PERFECT/VERY_HIGH/HIGH/FAIR/LOW/VERY_LOW/INCONCLUSIVE/FAKE），
        //   每块锭的纯度是各自独立状态，合并后除第一块外全部按第一块结算。
        "metal_ingot",
        // ★ 0.9.5：文档/纸质类整类拦截（用户 2026-10-10 要求，便于分享 DLL 时默认生效）。
        //   拦的理由：文档/指南/名片/证书/彩票/配方手札等每件是独立内容（编号/图案/可刮开），
        //   合并后除第一件外其余内容不可见，且交易/使用时按同一种结算，属误合并高发区。
        //   名单来自资料库 items_catalog.md 文书/杂项分类筛选（53 项）。
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

    // ★ 0.8.1：按【id 前缀】拦截的整类名单（identifier.StartsWith，忽略大小写）。
    //   "node" 覆盖 node_small / node_medium 等（节点类，独立进度/归属数据）。
    //   ⚠️ module 不走前缀 —— 见下方 Contains（0.8.1 实测修正）。
    private static readonly string[] DefaultBlockIdPrefixes = { "node" };

    // ★ 0.8.1：按【id 子串】拦截的整类名单（identifier.Contains，忽略大小写）。
    //   ★★ module 从「前缀」改成「子串」（用户实测：模组仍能堆叠）：
    //      游戏里绝大多数模组 id 是 `system_module_*` / `farm_module_*` / `blank_module` …
    //      （module 在中间/末尾），前缀匹配只命中 `module_extractor*` 等三个 → 大量漏拦。
    //   "neural_core" 覆盖神经核心模组（id 不含 "module"，需单独子串）。
    private static readonly string[] DefaultBlockIdContains = { "module", "neural_core" };

    // ★ 0.7.1：按【id 后缀】拦截的整类名单（identifier.EndsWith，忽略大小写）。
    //   "_keycard" 覆盖一次性钥匙卡系列：med/eng/cmd/sci/sec/ser/sup（7 种功能卡）。
    private static readonly string[] DefaultBlockIdSuffixes = { "_keycard" };

    // 后缀黑名单的豁免表（后缀命中但放行）——空白钥匙卡 blank_keycard。
    private static readonly string[] DefaultBlockSuffixExempt = { "blank_keycard" };

    // ──────────────────────────────────────────────────────────────────────
    // 生效名单（由 LoadBuiltinBlacklist() 在启动时填充；未加载前 = 代码默认值）
    // ★ 用 static 字段而非 readonly 常量，才能被 JSON 覆盖。
    //   所有读取点（StackGates.GetBlacklistReason / IsSuffixExempt）保持不变。
    // ──────────────────────────────────────────────────────────────────────
    private static string[] BuiltinBlockItemIds = DefaultBlockItemIds;
    private static string[] BuiltinBlockIdPrefixes = DefaultBlockIdPrefixes;
    private static string[] BuiltinBlockIdContains = DefaultBlockIdContains;
    private static string[] BuiltinBlockIdSuffixes = DefaultBlockIdSuffixes;
    private static string[] BuiltinBlockSuffixExempt = DefaultBlockSuffixExempt;

    // 名单来源标签（写日志用）："代码默认" / "外部 JSON"（逐组）。
    private static string _blSourceItemIds = "代码默认";
    private static string _blSourcePrefixes = "代码默认";
    private static string _blSourceContains = "代码默认";
    private static string _blSourceSuffixes = "代码默认";
    private static string _blSourceExempt = "代码默认";

    private const string BlacklistJsonName = "item_blacklist.json";

    // ──────────────────────────────────────────────────────────────────────
    // 加载入口：在 OnInitializeMelon 里、ValidatePatches() 之前调用一次。
    // ──────────────────────────────────────────────────────────────────────
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
                         $"子串 {BuiltinBlockIdContains.Length} / 后缀 {BuiltinBlockIdSuffixes.Length}）。");
                return;
            }

            string text;
            try { text = File.ReadAllText(path); }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                WarnOnce("bl.read", $"读取 {BlacklistJsonName} 失败，改用代码默认名单（防护不受影响）：{e.Message}");
                return;
            }

            var doc = MiniJson.ParseObject(text);
            if (doc == null)
            {
                WarnOnce("bl.parse", $"{BlacklistJsonName} 格式非法（应为 JSON 对象），改用代码默认名单（防护不受影响）。");
                return;
            }

            // ★ 逐组判断：字段【不存在】= 未提供（用默认）；字段【是空数组】= 显式清空（尊重意图）。
            bool anyOverride = false;
            anyOverride |= TryOverride(doc, "blockItemIds",     ref BuiltinBlockItemIds,     ref _blSourceItemIds);
            anyOverride |= TryOverride(doc, "blockIdPrefixes",  ref BuiltinBlockIdPrefixes,  ref _blSourcePrefixes);
            anyOverride |= TryOverride(doc, "blockIdContains",  ref BuiltinBlockIdContains,  ref _blSourceContains);
            anyOverride |= TryOverride(doc, "blockIdSuffixes",  ref BuiltinBlockIdSuffixes,  ref _blSourceSuffixes);
            anyOverride |= TryOverride(doc, "blockSuffixExempt",ref BuiltinBlockSuffixExempt,ref _blSourceExempt);

            if (anyOverride)
            {
                Log?.Msg($"[名单] 已加载外部 {BlacklistJsonName}（{path}）：" +
                         $"id={BuiltinBlockItemIds.Length}[{_blSourceItemIds}] / " +
                         $"前缀={BuiltinBlockIdPrefixes.Length}[{_blSourcePrefixes}] / " +
                         $"子串={BuiltinBlockIdContains.Length}[{_blSourceContains}] / " +
                         $"后缀={BuiltinBlockIdSuffixes.Length}[{_blSourceSuffixes}] / " +
                         $"豁免={BuiltinBlockSuffixExempt.Length}[{_blSourceExempt}]。");
            }
            else
            {
                Log?.Msg($"[名单] 外部 {BlacklistJsonName} 存在但未提供任何可识别字段，使用代码默认名单。");
            }
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            // ★ 兜底：任何意外都退回默认值（字段初值即默认，无需回滚）。
            WarnOnce("bl.fatal", $"加载 {BlacklistJsonName} 异常，使用代码默认名单（防护不受影响）：{e.Message}");
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
        if (raw is not List<object> list) return false;      // 类型不对 = 不覆盖（保默认）

        var arr = new string[list.Count];
        for (int i = 0; i < list.Count; i++)
            arr[i] = list[i] as string ?? (list[i]?.ToString() ?? "");
        // 去掉空串项（JSON 里手滑多写逗号会产生 ""）。
        var cleaned = new List<string>(arr.Length);
        foreach (var s in arr)
            if (!string.IsNullOrWhiteSpace(s)) cleaned.Add(s.Trim());
        target = cleaned.ToArray();
        source = "外部JSON";
        return true;
    }

    // 写日志用：把当前采用来源拼成一行（供启动汇总日志调用）。
    private static string BlacklistSourceSummary()
        => $"id={BuiltinBlockItemIds.Length}({_blSourceItemIds}) 前缀={BuiltinBlockIdPrefixes.Length}({_blSourcePrefixes}) " +
           $"子串={BuiltinBlockIdContains.Length}({_blSourceContains}) 后缀={BuiltinBlockIdSuffixes.Length}({_blSourceSuffixes}) " +
           $"豁免={BuiltinBlockSuffixExempt.Length}({_blSourceExempt})";
}

// ══════════════════════════════════════════════════════════════════════════
//  极简 JSON 解析器（只够解析「对象 + 字符串数组」这一种结构）
//
//  ★ 为什么不用 System.Text.Json / Newtonsoft：
//    · net6.0 目标里 System.Text.Json 需额外引用，且 IL2CPP 环境下 NuGet 依赖
//      会被打进 Mods 目录（本项目历史上坚持「单 DLL 交付、零外部依赖」）。
//    · 本需求只需读「{"blockItemIds":["a","b"],...}」这种扁平结构，
//      手写 100 行的容错解析器即可，且解析失败一律退回默认值（安全侧）。
//  ★ 本解析器只做「尽力解析」，不保证 HTTP/转义完备；出现任何意外 → 返回 null → 用默认名单。
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
            var result = ParseObjectAt(text, ref i);
            return result;
        }
        catch
        {
            return null;
        }
    }

    // ★ 扫描式提取：在任意嵌套层级找出所有名为 key 的字符串值/字符串数组元素。
    //   用于「从 item_catalog.json 里捞出所有 id / aliases」这类只读核对，
    //   不要求结构合法（即使文件里有我们不认识的字段也照样能捞出目标键）。
    //   返回 HashSet（忽略大小写）便于存在性判断；解析异常返回 null。
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
                // 找下一个 '"'
                while (i < n && text[i] != '"') i++;
                if (i >= n) break;
                int save = i;
                string k = ParseString(text, ref i);
                if (k == null) { i = save + 1; continue; }
                // 跳过空白找 ':'
                int j = i;
                while (j < n && char.IsWhiteSpace(text[j])) j++;
                if (j >= n || text[j] != ':') { i = save + 1; continue; }

                if (!string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                {
                    i = save + 1;               // 不是目标键，继续往后扫（仍会扫到值里的字符串，无妨）
                    continue;
                }

                j++;                            // 跳过 ':'
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
                else
                {
                    i = save + 1;
                }
            }
            return set;
        }
        catch
        {
            return null;
        }
    }

    private static Dictionary<string, object> ParseObjectAt(string s, ref int i)
    {
        SkipWs(s, ref i);
        if (i >= s.Length || s[i] != '{') return null;
        i++;                              // 跳过 '{'
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
        if (c == '{') return ParseObjectAt(s, ref i);   // 嵌套对象（本用途用不到，容错支持）
        if (c == 't') { if (Match(s, ref i, "true")) return true; return null; }
        if (c == 'f') { if (Match(s, ref i, "false")) return false; return null; }
        if (c == 'n') { if (Match(s, ref i, "null")) return null; return null; }
        return ParseNumber(s, ref i);
    }

    private static List<object> ParseArray(string s, ref int i)
    {
        if (s[i] != '[') return null;
        i++;                              // 跳过 '['
        var list = new List<object>();
        SkipWs(s, ref i);
        if (i < s.Length && s[i] == ']') { i++; return list; }

        while (i < s.Length)
        {
            SkipWs(s, ref i);
            object v = ParseValue(s, ref i);
            if (v == null && (i >= s.Length || s[i] != 'n')) return null;  // null 值容错
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
        return null;                      // 未闭合字符串
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

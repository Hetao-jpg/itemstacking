using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Preferences;
using UnityEngine;

namespace ItemStacking;

// 堆叠闸门判据（容器 / 物品黑名单 / 容器白名单 / 容器黑名单 / slot: token 匹配）
//
// ★ 阶段 2：fail-safe【三态语义】的故障标志机制
//   判据异常时不再一律 fail-safe「放行」—— 对【数据保护类】判据（容器 / 物品类）而言，
//   异常放行 = 保护静默消失，可能造成不可逆丢失。
//   机制：判据 catch 里置【故障位】；MergePolicy.Evaluate 在调用前后清读该位：
//     · 数据保护组（容器 / 物品黑名单）异常 → Deny（宁可拦不可丢）
//     · 便利组（容器白/黑名单）异常       → Allow（不影响游玩）
//   [ThreadStatic] 位标志：挂点均在主线程，判据可能嵌套（GetMergeGateReason→Evaluate），
//   位是「是否出现过故障」的累积 OR，读后清零即可。
//
// ★ 详细设计（判定顺序、slot token 映射、白名单语义等）见 README.md「合并决策单一源」。
public partial class Main
{
    [ThreadStatic] private static int _judgeFaultBits;
    private const int FaultProtection = 1;   // 数据保护组判据异常
    private const int FaultConvenience = 2;  // 便利组判据异常

    private static void MarkJudgeFault(int bit)
    {
        _judgeFaultBits |= bit;
    }

    private static bool ConsumeJudgeFault(int bit)
    {
        bool hit = (_judgeFaultBits & bit) != 0;
        if (hit) _judgeFaultBits &= ~bit;
        return hit;
    }

    // ── P0：容器检测 ──
    //   装了东西的容器禁止堆叠：原生 StackItemUnchecked 合并时只搬 unitCount，
    //   不迁移子物品 ⇒ 合并即造成内部物品永久丢失。
    //   空容器返回空列表 → 允许堆叠（无内容可丢）。
    private static bool IsContainer(GameItem it)
    {
        try
        {
            var children = it?.FindAllChildItems();
            bool isContainer = children != null && children.Count > 0;
            // 探针实参延后（Diag 形参急切求值）；只打 True 行（False 是噪音，一次悬停十几条）。
            if (DiagOn && isContainer)
                Diag($"IsContainer id={it?.identifier} childCount={children?.Count ?? -1} => True");
            return isContainer;
        }
        catch (Exception e)
        {
            // 收窄为 catch(Exception) 并显式重抛致命异常（catch{} 也捕 OOM，故仅靠前者无法达成不吞 OOM）。
            if (e is OutOfMemoryException || e is StackOverflowException)
                throw;
            MarkJudgeFault(FaultProtection);
            LogOnce("container", Level.Warn, $"容器判定异常，本次按『非容器』放行（容器内物品丢失防护暂时失效）: {e.Message}");
            return false;
        }
    }

    // ── 堆叠黑名单 ──
    //   本 mod 放开堆叠后，游戏原版的 tag 黑名单（mayStackItemFunc）从未设置过，
    //   导致有独立状态的物品也被合并（鉴定物品/赃物/未拥有/电量不同/动物/鼠/酒瓶/耐久/液体容器）。
    //
    //   ★ 1.0.0 定稿：【类判据（9 条）已永久停用】—— 状态一致性闸门（StateConsistency.cs）
    //     更严格也更通用：两件物品 state 完全相等才可合。9 条类判据是其真子集。
    //   ★ 内置 id 黑名单：启用中，由 cfg BuiltinIdBlacklist 控制（默认开）。
    //   ★ 自定义 ItemBlacklist：仍生效。
    //   ★ 物品白名单（ItemWhitelist）：已删除（原本无实际作用）。
    //
    //   返回「被拦截原因」，null = 允许堆叠。
    private static string GetBlacklistReason(GameItem a, GameItem b)
    {
        try
        {
            if (_stackBlacklist != null && !_stackBlacklist.Value) return null;   // 总闸关闭

            // 类判据已永久停用（状态闸门承担）。历史实现见 git 历史。

            if (BuiltinIdBlacklist)
            {
                // 硬编码 id 的脆弱性：游戏若改名，相关拦截会【静默失效】（无报错）；
                // 用 DiagLog 可观测是否命中。
                var pat = MatchIdPair(a, b, BuiltinBlockItemIds, IdMatch.Exact);
                if (pat != null) return "按 id 黑名单·内置（" + pat + "）";

                pat = MatchIdPair(a, b, BuiltinBlockIdPrefixes, IdMatch.Prefix);
                if (pat != null) return "按 id 前缀黑名单·内置（" + pat + "*）";

                pat = MatchIdPair(a, b, BuiltinBlockIdContains, IdMatch.Contains);
                if (pat != null) return "按 id 子串黑名单·内置（*" + pat + "*）";

                pat = MatchIdPair(a, b, BuiltinBlockIdSuffixes, IdMatch.Suffix, IsSuffixExempt);
                if (pat != null) return "按 id 后缀黑名单·内置（*" + pat + "）";
            }

            if (_itemBlacklist != null && !string.IsNullOrWhiteSpace(_itemBlacklist.Value))
            {
                foreach (var raw in _itemBlacklist.Value.Split(','))
                {
                    var id = raw.Trim();
                    if (id.Length == 0) continue;
                    if (string.Equals(a.identifier, id, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(b.identifier, id, StringComparison.OrdinalIgnoreCase))
                        return "按 id 黑名单·自定义（" + id + "）";
                }
            }

            return null;
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            MarkJudgeFault(FaultProtection);
            LogOnce("blacklist", Level.Warn, $"黑名单判据异常，本次放行堆叠（按 id 防护暂时失效）: {e.Message}");
            return null;
        }
    }

    // 后缀黑名单的豁免判定（如 blank_keycard 命中 *_keycard 但放行）。
    private static bool IsSuffixExempt(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        foreach (var ex in BuiltinBlockSuffixExempt)
            if (string.Equals(id, ex, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    // ── id 匹配模式（Exact / Prefix / Contains / Suffix） ──
    private enum IdMatch { Exact, Prefix, Contains, Suffix }

    // 双端匹配：a、b 任一 identifier 命中模式串即命中，返回【命中的那个模式串】。
    //   exempt = 按 identifier 豁免（后缀表用：blank_keycard 命中 *_keycard 但仍放行）。
    //   遍历顺序：模式串外层 → 先 a 后 b → 命中即返回。
    //   identifier 是 il2cpp getter，外提到 foreach 之前（每条名单只取 2 次，与名单长度无关）。
    private static string MatchIdPair(GameItem a, GameItem b, string[] list,
                                      IdMatch kind, Func<string, bool> exempt = null)
    {
        if (list == null) return null;
        var ia = a?.identifier;
        var ib = b?.identifier;
        foreach (var pat in list)
        {
            if (MatchesId(ia, pat, kind) && (exempt == null || !exempt(ia))) return pat;
            if (MatchesId(ib, pat, kind) && (exempt == null || !exempt(ib))) return pat;
        }
        return null;
    }

    private static bool MatchesId(string id, string pat, IdMatch kind)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pat)) return false;
        switch (kind)
        {
            case IdMatch.Exact: return string.Equals(id, pat, StringComparison.OrdinalIgnoreCase);
            case IdMatch.Prefix: return id.StartsWith(pat, StringComparison.OrdinalIgnoreCase);
            case IdMatch.Contains: return id.IndexOf(pat, StringComparison.OrdinalIgnoreCase) >= 0;
            case IdMatch.Suffix: return id.EndsWith(pat, StringComparison.OrdinalIgnoreCase);
            default: return false;
        }
    }

    // ── 只读 tag 工具（不碰委托字段，纯查询无副作用） ──
    private static bool IsTagSafe(GameItem it, string tag)
    {
        try { return it != null && it.IsTag(tag); }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            return false;
        }
    }

    private static int? GetTagIntSafe(GameItem it, string tag)
    {
        try
        {
            if (it == null) return null;
            var ts = it.GetTagReadonly(tag);
            return ts == null ? (int?)null : ts.GetInt();
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            return null;
        }
    }

    private static string GetTagStringSafe(GameItem it, string tag)
    {
        try
        {
            if (it == null) return null;
            var ts = it.GetTagReadonly(tag);
            return ts == null ? null : ts.GetString();
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            return null;
        }
    }

    // 物种判定：ANIMAL_SPECIES_TAG 的 str 值 == speciesId（如 "ANIMAL_RAT"）。
    private static bool IsSpecies(GameItem it, string speciesId)
    {
        var s = GetTagStringSafe(it, "ANIMAL_SPECIES_TAG");
        return !string.IsNullOrEmpty(s) &&
               string.Equals(s, speciesId, StringComparison.OrdinalIgnoreCase);
    }

    // ── 历史类判据（保留代码，永久停用，不再被 GetBlacklistReason 调用） ──
    private static bool IsWineBottle(GameItem it)
    {
        try { return it != null && string.Equals(it.identifier, "wine_bottle", StringComparison.Ordinal); }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("wine", Level.Warn, $"酒瓶判据异常，本次放行: {e.Message}");
            return false;
        }
    }

    private static bool IsDurable(GameItem it)
    {
        try { return it != null && DurabilityHelper.GetMaxDurability(it) > 0; }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("durable", Level.Warn, $"耐久判据异常，本次按『非耐久』放行: {e.Message}");
            return false;
        }
    }

    private static bool IsLiquidContainer(GameItem it)
        => IsTagSafe(it, "LIQUID_CONTAINER_TAG");

    // ── 容器白名单（堆叠作用域） ──
    //   ★ 1.0.0 定稿：白名单 = 【安全边界】。
    //     · ContainerWhitelist 留空 → 【任何容器都不生效】（返回拒绝原因）。
    //     · 非空 → 只有名单内的容器允许堆叠，其余一律拒绝。
    //   ⇒ 与 0.8.0~0.9.x「留空 = 不限容器」完全相反。理由：机器容器（熔炉/解序器等）
    //     内部假设「一槽 = 一实例」，堆叠会丢物品；默认全开 ⇒ 永远被未知容器咬。
    //
    //   判据三路（0.9.2 起）：
    //     parentInventory → ① identifier 精确匹配
    //                       ② slot: 槽位名（EmporiumEntry 指针比对）
    //                       ③ owner: 所属物品链（GetParentItem 上溯 ≤3 层）
    private static string GetScopeReason(GameItem target)
    {
        try
        {
            var inv = target?.parentInventory;
            // 探针放在判定之前：白名单为空（=全禁）时也要能打出「落点堆所在」。
            ProbeScopeInventory(inv, "落点堆所在");

            if (_containerWhitelist == null || string.IsNullOrWhiteSpace(_containerWhitelist.Value))
                return "容器白名单为空：堆叠未生效（请在 ContainerWhitelist 填入要允许堆叠的容器）";

            // 不再因「id 为空」提前拦截 —— 空 id 时仍可能命中 slot:/owner: token。
            if (IsContainerWhitelisted(inv)) return null;

            return "容器白名单：容器不在名单内（" + BriefContainer(inv) + "）";
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            MarkJudgeFault(FaultConvenience);
            LogOnce("scope", Level.Warn, $"堆叠作用域判据异常，本次不阻断: {e.Message}");
            return null;
        }
    }

    private static bool IsContainerWhitelisted(GameInventory inv)
    {
        if (_containerWhitelist == null || string.IsNullOrWhiteSpace(_containerWhitelist.Value))
            return false;
        if (inv == null) return false;
        return IsContainerInList(inv, _containerWhitelist.Value);
    }

    // ── 容器列表匹配公共实现（白名单 / 黑名单 / 机器容器共用） ──
    //   token 三种形态，逗号分隔 + Trim，Ordinal（大小写敏感）精确比对：
    //     · 裸 token（无前缀）      = GameInventory.identifier 精确匹配
    //     · slot:save_bag 等         = MatchEmporiumSlot 槽位 id 精确匹配
    //     · owner:backpack_medium 等 = 所属物品链任一层 identifier 精确匹配
    //   为什么要 slot:/owner:：实机证实 identifier 几乎全空，纯 id 名单在本作无法区分容器。
    private static bool IsContainerInList(GameInventory inv, string listValue)
    {
        if (inv == null || string.IsNullOrWhiteSpace(listValue)) return false;
        return IsContainerInTokens(inv, listValue.Split(','));
    }

    // string[] 重载 —— 供【代码内置名单】（已是数组，如机器容器名单）直传。
    private static bool IsContainerInList(GameInventory inv, string[] tokens)
    {
        if (inv == null || tokens == null || tokens.Length == 0) return false;
        return IsContainerInTokens(inv, tokens);
    }

    //   ★ 关键教训：只允许把「取值」变懒，**绝不允许裁剪「评估」**。
    //     原实现在 needId/needSlot 都为假时提前 return false，把【纯 owner: 名单整条裁掉】
    //     ⇒ 机器容器名单（13 项全是 owner:）恒判不命中 ⇒ 机器 Deny + 每格 1 件双双静默失效。
    private static bool IsContainerInTokens(GameInventory inv, string[] rawTokens)
    {
        if (inv == null || rawTokens == null || rawTokens.Length == 0) return false;

        ScanTokenNeeds(rawTokens, out bool needId, out bool needSlot);

        string id = null;
        if (needId) { try { id = inv.identifier; } catch { } }
        string slot = null;
        if (needSlot) { try { slot = MatchEmporiumSlot(inv); } catch { } }

        return AnyTokenMatches(rawTokens, id, slot, want => OwnerChainMatches(inv, want, 3));
    }

    // ★ 纯函数（可单测）—— 全工程唯一的容器 token 命中判定。
    //   独立成纯函数的原因就是上面那个 bug：「判定」和「il2cpp 取值」揉在一起时，
    //   一旦有人按「本次是否需要取值」提前返回，owner: 路就被静默裁掉，且没有测试能拦住。
    internal static bool AnyTokenMatches(string[] rawTokens, string id, string slot, Func<string, bool> ownerMatch)
    {
        if (rawTokens == null) return false;
        foreach (var raw in rawTokens)
        {
            var token = raw?.Trim();
            if (string.IsNullOrEmpty(token)) continue;

            if (token.StartsWith(Constants.TokenSlotPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var want = token.Substring(Constants.TokenSlotPrefix.Length).Trim();
                if (want.Length > 0 && slot != null && string.Equals(slot, want, StringComparison.Ordinal))
                    return true;
            }
            else if (token.StartsWith(Constants.TokenOwnerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var want = token.Substring(Constants.TokenOwnerPrefix.Length).Trim();
                if (want.Length > 0 && ownerMatch != null && ownerMatch(want))
                    return true;
            }
            else if (id != null && string.Equals(token, id, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    // ★ 纯函数（可单测）：扫一遍 token，判定本次需要哪几路「取值」（裸 id / slot:）。
    //   ⚠️ 它【只决定取值，不决定命中】。返回 (false, false) 时（空列表 / 纯 owner:）
    //      仍必须把 token 交给 AnyTokenMatches 评估 —— owner: 路正落在这一档。
    internal static void ScanTokenNeeds(string[] rawTokens, out bool needId, out bool needSlot)
    {
        needId = false;
        needSlot = false;
        if (rawTokens == null) return;
        foreach (var raw in rawTokens)
        {
            var token = raw?.Trim();
            if (string.IsNullOrEmpty(token)) continue;
            if (token.StartsWith(Constants.TokenSlotPrefix, StringComparison.OrdinalIgnoreCase))
                needSlot = true;
            else if (!token.StartsWith(Constants.TokenOwnerPrefix, StringComparison.OrdinalIgnoreCase))
                needId = true;
            if (needId && needSlot) return;
        }
    }

    // 所属物品链任一层是否等于 want（与 DescribeOwnerChain 同一上溯逻辑，逐层比对而非拼串）。
    private static bool OwnerChainMatches(GameInventory inv, string want, int depth)
    {
        var cur = inv;
        for (int i = 0; i < depth && cur != null; i++)
        {
            GameItem item;
            try { item = cur.GetParentItem(); } catch { break; }
            if (item == null) break;
            string iid;
            try { iid = item.identifier; } catch { iid = null; }
            if (iid != null && string.Equals(iid, want, StringComparison.Ordinal))
                return true;
            try { cur = item.parentInventory; } catch { break; }
        }
        return false;
    }

    // 拒绝原因里的简短容器描述（完整版在 DescribeContainer，日志用）。
    private static string BriefContainer(GameInventory inv)
    {
        if (inv == null) return "null";
        string id = null; try { id = inv.identifier; } catch { }
        string slot = null; try { slot = MatchEmporiumSlot(inv); } catch { }
        string owner = null; try { owner = DescribeOwnerChain(inv, 3); } catch { }
        return $"id={(string.IsNullOrEmpty(id) ? "<空>" : id)} 槽位={slot} 所属={owner}";
    }

    // ── 容器完整描述（探针输出用） ──
    //   identifier 常为空（运行时创建的柜台/背包格），单靠 id 无法区分，故补三路：
    //     ① EmporiumEntry 槽位名（指针比对）；② 所属物品链；③ 内容指纹。
    private static string DescribeContainer(GameInventory inv)
    {
        if (inv == null) return "null";
        long key = Ptr(inv);

        string id, cls;
        try { id = inv.identifier; } catch { id = null; }
        try { cls = inv.GetType().Name; } catch { cls = "?"; }

        string slot = "<未知槽位>";
        try { slot = MatchEmporiumSlot(inv); } catch { }

        string owner;
        try { owner = DescribeOwnerChain(inv, 3); } catch { owner = "?"; }

        string content;
        try { content = DescribeContent(inv, 5); } catch { content = "?"; }

        return $"id={(string.IsNullOrEmpty(id) ? "<空id>" : id)}" +
               $"  类={cls}  @{key:X}" +
               $"  槽位={slot}" +
               $"  所属={owner}" +
               $"  内含={content}";
    }

    // ── 指针比对 EmporiumEntry 的 8 个已知容器 ──
    //   每帧实时取，不缓存（换场景可能重建）。
    //   entry 预取一次 + 逐属性直读（每个 getter 各自 try/catch，异常 → null）。
    //
    // ★ slot: token 用【游戏原生 id】（不再硬编码中文）：
    //     主仓库 (invElement 0x30)               → save_bag
    //     展示柜 (showcaseElement 0x40)          → showcase_save_bag
    //     柜台前格 (frontInvinvElement 0x88)     → frontInv
    //     玩家柜台 (backInvinvElement 0x98)      → back_inv_save_bag
    //     柜台三格 (backInvinvElementCounter 0xA0)→ backInvCounter
    //     已售格 (soldElement 0x108)             → sold_save_bag
    //     海报暗格 (hiddenElement 0xC0)          → hidden_save_bag
    //     黑市左侧 (bazarLeftinvElement 0xB0)    → bazarInv
    private static string MatchEmporiumSlot(GameInventory inv)
    {
        EmporiumEntry entry;
        try { entry = EmporiumEntry.Instance; } catch { return "<不在EmporiumEntry内>"; }
        if (entry == null) return "非交易场景";

        GameGridInventory eInv = null, eShow = null, eFront = null, eBack = null;
        GameGridInventory eBackC = null, eSold = null, eHidden = null, eBazar = null;
        try { eInv = entry.invElement; } catch { }
        try { eShow = entry.showcaseElement; } catch { }
        try { eFront = entry.frontInvinvElement; } catch { }
        try { eBack = entry.backInvinvElement; } catch { }
        try { eBackC = entry.backInvinvElementCounter; } catch { }
        try { eSold = entry.soldElement; } catch { }
        try { eHidden = entry.hiddenElement; } catch { }
        try { eBazar = entry.bazarLeftinvElement; } catch { }

        if (SameNativePointer(inv, eInv)) return SlotMainInv;
        if (SameNativePointer(inv, eShow)) return SlotShowcase;
        if (SameNativePointer(inv, eFront)) return SlotFrontCounter;
        if (SameNativePointer(inv, eBack)) return SlotBackCounter;
        if (SameNativePointer(inv, eBackC)) return SlotCounterExtra;
        if (SameNativePointer(inv, eSold)) return SlotSold;
        if (SameNativePointer(inv, eHidden)) return SlotHidden;
        if (SameNativePointer(inv, eBazar)) return SlotBazar;
        return "<不在EmporiumEntry内>";
    }

    // slot: token 常量（游戏原生 id，全部 ASCII）。
    //   前 5 个 = 游戏存档袋物品 id（PlayerStore.saveBags 键）；
    //   后 3 个 = EmporiumEntry 的 C# 成员名（这 3 个槽游戏没有存档袋，也没有 identifier）。
    internal const string SlotMainInv = "save_bag";
    internal const string SlotShowcase = "showcase_save_bag";
    internal const string SlotFrontCounter = "frontInv";
    internal const string SlotBackCounter = "back_inv_save_bag";
    internal const string SlotCounterExtra = "backInvCounter";
    internal const string SlotSold = "sold_save_bag";
    internal const string SlotHidden = "hidden_save_bag";
    internal const string SlotBazar = "bazarInv";

    // 所属物品链：inv.GetParentItem() = 这个容器是谁（背包物品/箱子物品）的内部库存。
    // 上溯方式：item.parentInventory（外层容器）→ 再 GetParentItem，最多 depth 层。
    private static string DescribeOwnerChain(GameInventory inv, int depth)
    {
        var parts = new System.Text.StringBuilder();
        var cur = inv;
        for (int i = 0; i < depth && cur != null; i++)
        {
            GameItem item;
            try { item = cur.GetParentItem(); } catch { break; }
            if (item == null) break;
            string iid;
            try { iid = item.identifier; } catch { iid = "?"; }
            if (parts.Length > 0) parts.Append(" < ");
            parts.Append(string.IsNullOrEmpty(iid) ? "<无id物品>" : iid);
            try { cur = item.parentInventory; } catch { break; }
        }
        return parts.Length == 0 ? "<顶层容器>" : parts.ToString();
    }

    // 内容指纹：childItems 前 maxCount 个物品的 id×数量。
    private static string DescribeContent(GameInventory inv, int maxCount)
    {
        var list = inv.childItems;
        if (list == null) return "?";
        int n = 0;
        var parts = new System.Text.StringBuilder();
        for (int i = 0; i < list.Count && n < maxCount; i++, n++)
        {
            var it = list[i];
            string iid; int cnt;
            try { iid = it.identifier; } catch { iid = "?"; }
            try { cnt = it.unitCount; } catch { cnt = 1; }
            if (parts.Length > 0) parts.Append(", ");
            parts.Append(string.IsNullOrEmpty(iid) ? "<无id>" : iid);
            if (cnt > 1) parts.Append("×").Append(cnt);
        }
        int total;
        try { total = list.Count; } catch { total = n; }
        if (total > n) parts.Append(", …共").Append(total).Append("种");
        return total == 0 ? "<空>" : parts.ToString();
    }

    // ── 容器黑名单 ──
    //   token 与白名单共用（IsContainerInList）。双端判定：被叠 / 拖起 任一方命中即拦。
    //   只拦「合并」；空槽放置（移动）不受限。
    //   与白名单同时命中时【黑名单优先】。
    private static string GetExcludeReason(GameItem target, GameItem source)
    {
        try
        {
            if (_containerBlacklist == null || string.IsNullOrWhiteSpace(_containerBlacklist.Value))
                return null;
            var tinv = target?.parentInventory;
            var sinv = source?.parentInventory;
            if (IsContainerInList(tinv, _containerBlacklist.Value))
                return "容器黑名单（" + BriefContainer(tinv) + "）";
            if (IsContainerInList(sinv, _containerBlacklist.Value))
                return "容器黑名单（" + BriefContainer(sinv) + "）";
            return null;
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            MarkJudgeFault(FaultConvenience);
            LogOnce("exclude", Level.Warn, $"容器排除判据异常，本次不阻断: {e.Message}");
            return null;
        }
    }

    // ── 合并闸门的统一判定：返回「拒绝原因」，null = 允许 ──
    //   target = 被并进的那一格上的物品；source = 要并进去的物品。
    //   slot = 落点（提供 inventory 归属，快移路径下这才是可靠的容器来源）。
    //
    //   ★ 本方法改为【单一源 MergePolicy】的薄封装 —— 探针保留，决策委托给
    //     MergePolicy.Evaluate（固定顺序）。这样 TryAcceptOnce / MayStack / GetStackSlot
    //     三处结论必然一致。回滚：cfg z9_UseMergePolicy=false 时走 0.9.9 旧顺序。
    private static string GetMergeGateReason(SlotMarker slot, GameItem target, GameItem source)
    {
        // 探针：整块包进 if (DiagOn) —— 探针函数的实参是【调用前急切求值】的。
        if (DiagOn)
        {
            DumpCounterProbeOnce(slot.inventory);
            ProbeScopeInventory(slot?.inventory, "落点槽");
            ProbeScopeInventory(TryGet(() => source?.parentInventory), "拖起方");
        }

        if (UseMergePolicy)
        {
            var ctx = new MergeContext(slot?.inventory, target, source);
            var verdict = Evaluate(ctx, out var reason);
            return verdict == MergeVerdict.Deny ? reason : null;
        }

        // 回滚路径（cfg z9_UseMergePolicy=false）。
        return LegacyMergeGateReason(slot, target, source);
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Preferences;
using UnityEngine;

namespace ItemStacking;

public partial class Main
{
    // ══════════════════════════════════════════════════════════════════════
    //  0.10.0 · 阶段2：fail-safe【三态语义】的故障标志机制
    //
    //  ── 问题 ──
    //  0.9.9 所有判据异常时一律 fail-safe「放行」（return false/null）—— 对
    //  【数据保护类】判据（容器 / 物品类判据）而言，异常放行 = 保护静默消失，
    //  可能造成不可逆丢失（物品被误合并、容器内物品蒸发）。
    //
    //  ── 机制 ──
    //  判据函数在 catch 里置对应【故障位】（不改签名，旧调用点零改动）；
    //  MergePolicy.Evaluate 在调用「数据保护组 / 便利组」前后清读该位，按级别决定落点：
    //    · 数据保护组（容器 / 物品黑名单·类判据）异常 → Deny（宁可拦不可丢）
    //    · 便利组（柜台 / 容器黑名单 / 容器白名单）异常 → Allow（不影响游玩）
    //  用 [ThreadStatic] 位标志：挂点均在主线程，且判据可能嵌套（GetMergeGateReason→Evaluate），
    //  位标志是「是否出现过故障」的累积 OR，读后清零即可，无竞态。
    // ══════════════════════════════════════════════════════════════════════
    [ThreadStatic] private static int _judgeFaultBits;
    private const int FaultProtection = 1;   // 数据保护组判据异常
    private const int FaultConvenience = 2;  // 便利组判据异常

    // 判据 catch 里调用：标记「本组判据刚发生异常」。
    private static void MarkJudgeFault(int bit)
    {
        _judgeFaultBits |= bit;
    }

    // 供 Evaluate 使用：读取并清零指定组的故障位。
    private static bool ConsumeJudgeFault(int bit)
    {
        bool hit = (_judgeFaultBits & bit) != 0;
        if (hit) _judgeFaultBits &= ~bit;
        return hit;
    }

    // ── P0：容器检测 ──
    // 装了东西的容器禁止堆叠：原生 StackItemUnchecked 合并时只搬 unitCount，
    // 不迁移子物品（容器内物品），合并即造成内部物品永久丢失。
    // 用 FindAllChildItems() 取全部后代（interop 已确认该实例方法存在）；
    // 空容器返回空列表，允许堆叠（无内容可丢）。
    private static bool IsContainer(GameItem it)
    {
        try
        {
            var children = it?.FindAllChildItems();
            bool isContainer = children != null && children.Count > 0;
            // ★ 探针实参延后：Diag 的形参在调用前【急切求值】，故用 if(DiagOn) 守卫 ——
            //   避免默认（DiagLog 关）时仍构造插值串并调用 it.identifier（il2cpp getter）。
            //   IsContainer 每次合并决策被调 ≥2 次，属热点。
            if (DiagOn) Diag($"IsContainer id={it?.identifier} childCount={children?.Count ?? -1} => {isContainer}");
            return isContainer;
        }
        catch (Exception e)
        {
            // 3.4 #1：收窄为 catch(Exception) 并显式重抛「致命异常」，避免静默吞掉 OOM / 栈溢出
            // （catch {} 与 catch(Exception) 都会捕获 OOM，故仅靠前者无法达成「不吞 OOM」的意图；
            //  这里显式重抛，让致命异常冒泡到外层补丁的 catch 并记录，而不是在本层被静默掩盖）。
            if (e is OutOfMemoryException || e is StackOverflowException)
                throw;
            MarkJudgeFault(FaultProtection);   // 阶段2：数据保护类异常 → Evaluate 将判 Deny
            LogOnce("container", Level.Warn, $"容器判定异常，本次按『非容器』放行（容器内物品丢失防护暂时失效）: {e.Message}");
            return false;   // 读取失败时不阻断正常堆叠（★ 注意：MergePolicy 会据故障位置 Deny）
        }
    }

    // ── P3（0.5.0）：堆叠黑名单 ──
    // 用户实机反馈 bug，根因都是「本 mod 放开堆叠后，游戏原版的 tag 黑名单从未设置过
    // （mayStackItemFunc 恒空）」，导致有独立状态的物品也被合并：
    //   ① 鉴定物品（等离子/手/三明治）自动堆叠 → 拖出来变默认样子和价值（独立鉴定结果丢失）；
    //   ② 商人商品（未付款）堆进已拥有堆 → 除第一个外变「已拥有」→ 免费拿走（盗窃）；
    //   ③ 赃物（有热度值）堆叠 → 污染所有权 / 警察检查判定；
    //   ④ 电量不同的电池堆叠 → 除第一个外丢失剩余电量；
    //   ⑤ 活体动物堆叠 → 每只的年龄/体重/健康/饥饿/性别/怀孕/基因全丢（一只鼠 34 个 tag，探针实证）；
    //   ⑥ 鼠堆叠 → 同上，且污染饲养 / 繁殖 / 过度拥挤判定。
    // 判据全部用游戏官方只读口 GameItem.IsTag(string) / GetTagReadonly(string)
    // （反汇编确认：IsItemOwned 内部就是 IsTag("IS_OWNED_TAG")；电池电量存
    // power_source_item_energy 这个 int tag），不碰任何委托字段。
    //
    // 返回「被拦截原因」，null = 允许堆叠。
    // ══════════════════════════════════════════════════════════════════════
    //  ★★★ 1.0.0 对比实验【定稿】（核桃 2026-10-11 19:0x 决策）
    //
    //  ── 实验结论 ──
    //   ① 【类判据（9 条）确认作废】：状态一致性闸门（MergePolicy 1.5，见 StateConsistency.cs）
    //      已实机验证通过，它比类判据更严格也更通用：两件物品 state 完全相等才可合。
    //      反汇编已证实【类判据涉及的状态全部存在 state tag 里】：
    //       · 耐久   = DEFAULT_DURABILITY_TAG（上限）/ DURABILITY_TAG（当前）  → state int tag
    //       · 液体   = LIQUID_CONTAINER_CURRENT / LIQUID_CONTAINER_MAX          → state int tag
    //       · 酒     = WINE_SCORE_INT / WINE_CONTAINER_STATE / WINE_CONTAINER_TAG → state tag
    //       · 电量   = power_source_item_energy                                  → state int tag
    //       · 鉴定/赃物/未拥有/动物/鼠 均为 state tag
    //      ⇒ 9 条类判据是状态比较的【真子集】⇒ 永久停用（本方法不再调用它们）。
    //
    //   ② 【按 id 黑名单 ★ 2026-10-12 注释停用】：用户要求停用内置 id 黑名单
    //      （node*/module*/neural_core/*_keycard/文书类等）。停用后仅 cfg 自定义
    //      ItemBlacklist 仍生效。恢复方法：把下方内置段的 #if false 改回 #if true。
    //
    //  ── 现状 ──
    //   · 类判据：停用（状态闸门承担）。
    //   · 内置 id 黑名单：★ 停用（2026-10-12，见方法头②）。
    //   · 自定义 ItemBlacklist：仍生效。
    //   · 物品白名单（ItemWhitelist）：★ 已删除（2026-10-12，原本就无实际作用）。
    // ══════════════════════════════════════════════════════════════════════
    private static string GetBlacklistReason(GameItem a, GameItem b)
    {
        try
        {
            if (_stackBlacklist != null && !_stackBlacklist.Value) return null;   // 总闸关闭

            // ══════════ 类判据：永久停用（状态闸门已覆盖，见方法头①）══════════
            // 历史（≤0.10）：9 条类判据（可鉴定/赃物/未拥有/电量/动物/鼠/酒瓶/耐久/液体容器）
            // 已于 1.0.0 永久停用 —— 它们是状态一致性闸门的真子集，见 MergePolicy.Evaluate 1.5。
            // 若需回滚，见 git 历史：HEAD dc3d11a 版 StackGates.cs 的 #if false 块。
            // ══════════════════════════════════════════════════════════════════

            // ══════════ 内置 id 黑名单：2026-10-12 用户要求【恢复启用】═══════════
            //   恢复模组/节点/钥匙卡/文书类等按 id 拦截（尤其 module/node/neural_core 模组类）。
            //   若需再次停用：把 #if true 改回 #if false 重新编译。
#if true
            // 0.6.1：按 id 黑名单（特定物品）。
            // ⚠️ 硬编码 id 的脆弱性：identifier 由配置提供、来源为游戏 0.46D 物品目录。
            //    游戏若改名，相关拦截会【静默失效】（无报错）；用 DiagLog 可观测是否命中。
            //
            // ★ 0.6.6：内置默认名单从 cfg 移到代码常量。
            //   原因：MelonLoader 的 cfg 写入会把「不含引号且含逗号的字符串」原样裸写，
            //   产生非法 TOML（`BlockItemIds = a,b`）→ 下次 Load 抛 TomlInvalidValueException
            //   → 整份 MelonPreferences.cfg 进入 Fallback 模式 → 所有 mod 的配置全部无法保存。
            //
            // ★ T3.2（2026-10-12）：下面 4 段【同构 foreach + a/b 双端判定】收敛为
            //   MatchIdPair(...) 一行一段（helper 见本方法下方 IdMatch / MatchIdPair / MatchesId）。
            //   等价性：仍是「按模式串外层循环 → 先 a 后 b → 命中即返回」，且把
            //   【命中的模式串】塞回原因文案 ⇒ 日志格式与合并前逐字节一致。
            var pat = MatchIdPair(a, b, BuiltinBlockItemIds, IdMatch.Exact);
            if (pat != null) return "按 id 黑名单·内置（" + pat + "）";

            // ★ 0.7.0：按 id 前缀拦截整类（节点 node*）。
            //   判据用 identifier.StartsWith（忽略大小写），null 安全交给 string.IsNullOrEmpty 前置判断。
            pat = MatchIdPair(a, b, BuiltinBlockIdPrefixes, IdMatch.Prefix);
            if (pat != null) return "按 id 前缀黑名单·内置（" + pat + "*）";

            // ★ 0.8.1：按 id 子串拦截整类（模组 module / 神经核心 neural_core）。
            //   判据用 identifier.Contains（忽略大小写）—— module 在 system_module_*/farm_module_*/… 的
            //   中间/末尾，前缀匹配抓不到；neural_core 完全不含 module，也要单独子串。
            pat = MatchIdPair(a, b, BuiltinBlockIdContains, IdMatch.Contains);
            if (pat != null) return "按 id 子串黑名单·内置（*" + pat + "*）";

            // ★ 0.7.1：按 id 后缀拦截整类（钥匙卡 *_keycard），带豁免表（blank_keycard 放行）。
            pat = MatchIdPair(a, b, BuiltinBlockIdSuffixes, IdMatch.Suffix, IsSuffixExempt);
            if (pat != null) return "按 id 后缀黑名单·内置（*" + pat + "）";
#endif
            // ══════════════════════════════════════════════════════════════════

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
            if (e is OutOfMemoryException || e is StackOverflowException)
                throw;
            MarkJudgeFault(FaultProtection);   // 阶段2：数据保护类异常 → Evaluate 将判 Deny
            LogOnce("blacklist", Level.Warn, $"黑名单判据异常，本次放行堆叠（按 id 防护暂时失效）: {e.Message}");
            return null;   // 黑名单读取失败时不阻断正常堆叠（★ MergePolicy 会据故障位 Deny）
        }
    }

    // ★ 0.7.1：后缀黑名单的豁免判定（如 blank_keycard 是 "_keycard" 后缀但用户要求放行）。
    //   纯字符串比较，异常不可能发生，无需 try。
    private static bool IsSuffixExempt(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        foreach (var ex in BuiltinBlockSuffixExempt)
            if (string.Equals(id, ex, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  T3.2 · id 匹配模式（2026-10-12）
    //  把 GetBlacklistReason 里 4 段同构的「foreach + a/b 双端 + 忽略大小写」收敛到一处。
    //  新增一种匹配方式（如正则）只需在 MatchesId 加一个 case，四个名单自动生效。
    // ══════════════════════════════════════════════════════════════════════
    private enum IdMatch { Exact, Prefix, Contains, Suffix }

    // 双端匹配：a、b 任一 identifier 命中模式串即命中，返回【命中的那个模式串】，
    //   一个都没命中返回 null（调用点据此拼日志原因，保住模式原文）。
    // exempt = 按【identifier】豁免（后缀表用：blank_keycard 命中 *_keycard 但仍放行）。
    // 遍历顺序与合并前的 4 段 foreach 逐字一致：模式串外层 → 先 a 后 b → 命中即返回。
    private static string MatchIdPair(GameItem a, GameItem b, string[] list,
                                      IdMatch kind, Func<string, bool> exempt = null)
    {
        if (list == null) return null;
        foreach (var pat in list)
        {
            var ia = a?.identifier;
            if (MatchesId(ia, pat, kind) && (exempt == null || !exempt(ia))) return pat;
            var ib = b?.identifier;
            if (MatchesId(ib, pat, kind) && (exempt == null || !exempt(ib))) return pat;
        }
        return null;
    }

    // 单串匹配（统一 OrdinalIgnoreCase + null/空安全）。
    //   ★ 空串一律 false —— 与原 foreach 版等价：模式串来自代码常量（非空），
    //     故「identifier 为空」在新旧两版里都命中不了。
    private static bool MatchesId(string id, string pat, IdMatch kind)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pat)) return false;
        switch (kind)
        {
            case IdMatch.Exact:    return string.Equals(id, pat, StringComparison.OrdinalIgnoreCase);
            case IdMatch.Prefix:   return id.StartsWith(pat, StringComparison.OrdinalIgnoreCase);
            case IdMatch.Contains: return id.IndexOf(pat, StringComparison.OrdinalIgnoreCase) >= 0;
            case IdMatch.Suffix:   return id.EndsWith(pat, StringComparison.OrdinalIgnoreCase);
            default:               return false;
        }
    }

    // （原「物品白名单 IsWhitelisted」已随 ItemWhitelist 配置项一并删除，2026-10-12；
    //   其唯一调用点是上方已删除的 #if false 类判据停用块，见 git 历史 dc3d11a。）

    // 只读判 tag 是否存在（官方 IsTag，纯查询、不创建、无副作用）。null 视为 false。
    private static bool IsTagSafe(GameItem it, string tag)
    {
        try { return it != null && it.IsTag(tag); }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            return false;
        }
    }

    // 只读取 int 型 tag 的值（官方 GetTagReadonly + GetInt，无副作用）。无此 tag 返回 null。
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

    // 只读取 string 型 tag 的值（官方 GetTagReadonly + GetString，无副作用）。无此 tag 返回 null。
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
    // 与官方 HusbandryHelper.GetSpecies(item).speciesID 同源（探针实证一致），
    // 但不依赖 HusbandryHelper/AnimalSpecies 类型，少一层跨类型风险。
    private static bool IsSpecies(GameItem it, string speciesId)
    {
        var s = GetTagStringSafe(it, "ANIMAL_SPECIES_TAG");
        return !string.IsNullOrEmpty(s) &&
               string.Equals(s, speciesId, StringComparison.OrdinalIgnoreCase);
    }

    // ── 0.6.0：整类物品判据（方向2/3）──
    // 方向2：酒瓶（wine_bottle）整类拦截。
    // ⚠️ 硬编码 identifier「wine_bottle」：来自游戏 0.46D 的物品目录。
    //    若游戏更新改名，此项会【静默失效】（无报错、只是不再拦截）——
    //    届时改 cfg 或在此更新常量；详见 README「硬编码参数」。
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

    // 方向3a：有耐久度的物品（过滤芯、带耐久的工具等）。
    // 判据用游戏官方 DurabilityHelper.GetMaxDurability（静态，>0 = 有耐久），
    // 通用覆盖全部耐久物品，不依赖枚举 id。
    private static bool IsDurable(GameItem it)
    {
        try { return it != null && DurabilityHelper.GetMaxDurability(it) > 0; }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("durable", Level.Warn, $"耐久判据异常，本次按『非耐久』放行: {e.Message}");
            return false;   // 读取失败不阻断（fail-safe）
        }
    }

    // 方向3b：液体容器（水瓶/药瓶/血袋/洗发水/酒瓶等）。
    // 判据用游戏官方只读 tag LIQUID_CONTAINER_TAG（stringliteral 已确认存在），
    // 空瓶与满瓶都拦，通用覆盖全部液体容器（将来新增也自动生效）。
    private static bool IsLiquidContainer(GameItem it)
        => IsTagSafe(it, "LIQUID_CONTAINER_TAG");

    // ── 需求3（0.5.1）：堆叠作用域（容器白名单）──
    // ★ 1.0.0 定稿（核桃 2026-10-11）：白名单 = 【安全边界】。语义翻转为：
    //   · ContainerWhitelist 留空 → 【任何容器都不生效】（返回拒绝原因）。
    //   · ContainerWhitelist 非空 → 只有名单内的容器允许堆叠，其余一律拒绝。
    //   ⇒ 与 0.8.0~0.9.x 的「留空 = 不限容器」【完全相反】，这是刻意的行为变更：
    //     默认必须是安全的（默认什么都不堆），玩家逐个填要开的容器。
    //     理由：机器容器（熔炉/解序器/其他 mod 自定义机器）内部假设「一槽=一实例」，
    //     堆叠会打破它导致丢失；默认全开 ⇒ 永远被未知容器咬。默认全禁 ⇒ 免疫。
    // 判据（0.9.2 起三路，0.8.x 只有 identifier 一路）：
    //   GameItem.parentInventory → ① identifier 精确匹配
    //                             ② slot: 槽位名（EmporiumEntry 指针比对）
    //                             ③ owner: 所属物品链（GetParentItem 上溯 ≤3 层）
    // 返回「被拒绝原因」，null = 允许。
    private static string GetScopeReason(GameItem target)
    {
        try
        {
            var inv = target?.parentInventory;
            // 探针放在白名单判定之前：即使白名单为空（=全禁），也要能打出「落点堆所在」，
            // 玩家靠它摸清容器后再抄 token。探针内部受 DiagOn 门控，无副作用。
            ProbeScopeInventory(inv, "落点堆所在");

            // ★ 1.0.0：白名单留空 → 【全禁】（安全边界，未配置 = 不生效）。
            if (_containerWhitelist == null || string.IsNullOrWhiteSpace(_containerWhitelist.Value))
                return "容器白名单为空：堆叠未生效（请在 ContainerWhitelist 填入要允许堆叠的容器）";

            // 0.9.2：不再因「id 为空」提前拦截 —— 实测（0.9.1 日志）绝大多数容器 id 为空，
            // 空 id 时仍可能命中 slot:/owner: token；三路全落空才拒绝。
            if (IsContainerWhitelisted(inv)) return null;

            return "容器白名单：容器不在名单内（" + BriefContainer(inv) + "）";
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            MarkJudgeFault(FaultConvenience);   // 阶段2：便利类异常 → Evaluate 将判 Allow
            LogOnce("scope", Level.Warn, $"堆叠作用域判据异常，本次不阻断: {e.Message}");
            return null;   // 作用域判定失败时不阻断
        }
    }

    // 白名单判定（GetScopeReason 与作用域探针共用，避免两处规则漂移）。
    // 0.9.3：token 匹配抽到 MatchesContainerToken，与黑名单共用一套规则。
    // ★ 1.0.0：留空 = 【全禁】= 不命中任何容器（与 GetScopeReason 的语义严格一致）。
    //   旧版此处留空返回 true（=不限），会让作用域探针误报「白名单内=是」；已随语义翻转同改。
    private static bool IsContainerWhitelisted(GameInventory inv)
    {
        if (_containerWhitelist == null || string.IsNullOrWhiteSpace(_containerWhitelist.Value))
            return false;
        if (inv == null) return false;
        return IsContainerInList(inv, _containerWhitelist.Value);
    }

    // 0.9.3：容器列表匹配公共实现（白名单 / 黑名单共用）。
    // token 三种形态，逗号分隔 + Trim，每种 Ordinal（大小写敏感）精确比对：
    //   · 裸 token（无前缀）      = GameInventory.identifier 精确匹配（0.8.0 旧行为，向后兼容）
    //   · slot:save_bag 等         = MatchEmporiumSlot 槽位 id 精确匹配（★ 0.11.2 起为游戏原生 id，
    //                               不再用中文；映射表见 MatchEmporiumSlot 头注释）
    //   · owner:backpack_medium 等 = 所属物品链任一层 identifier 精确匹配
    // 为什么要 slot:/owner:：0.9.1 实机日志证实 identifier 几乎全空（仅 backInv / smuggler_bay
    //   等例外），纯 id 名单在本作无法区分容器；槽位与所属物品 id 才是可靠判据。
    private static bool IsContainerInList(GameInventory inv, string listValue)
    {
        if (inv == null || string.IsNullOrWhiteSpace(listValue)) return false;

        string id = null;
        try { id = inv.identifier; } catch { }
        string slot = null;
        try { slot = MatchEmporiumSlot(inv); } catch { }

        foreach (var raw in listValue.Split(','))
        {
            var p = raw.Trim();
            if (p.Length == 0) continue;
            if (MatchesContainerToken(p, inv, id, slot))
                return true;
        }
        return false;
    }

    // 单 token 匹配（id/slot 由调用方预取，owner 按需上溯 —— 多数 token 不走 owner 分支）。
    private static bool MatchesContainerToken(string token, GameInventory inv, string id, string slot)
    {
        if (token.StartsWith("slot:", StringComparison.OrdinalIgnoreCase))
        {
            var want = token.Substring(5).Trim();
            return want.Length > 0 && slot != null && string.Equals(slot, want, StringComparison.Ordinal);
        }
        if (token.StartsWith("owner:", StringComparison.OrdinalIgnoreCase))
        {
            var want = token.Substring(6).Trim();
            return want.Length > 0 && OwnerChainMatches(inv, want, 3);
        }
        return id != null && string.Equals(token, id, StringComparison.Ordinal);
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

    // 拒绝原因里的简短容器描述（完整版在 DescribeContainer，日志用；这里只要能对照抄 token 即可）。
    private static string BriefContainer(GameInventory inv)
    {
        if (inv == null) return "null";
        string id = null; try { id = inv.identifier; } catch { }
        string slot = null; try { slot = MatchEmporiumSlot(inv); } catch { }
        string owner = null; try { owner = DescribeOwnerChain(inv, 3); } catch { }
        return $"id={(string.IsNullOrEmpty(id) ? "<空>" : id)} 槽位={slot} 所属={owner}";
    }

    // 0.9.1：容器完整描述 —— identifier 常为空（运行时创建的柜台/背包格），
    // 单靠 id 无法区分容器，所以补三路判据：
    //   ① EmporiumEntry 槽位名（指针比对，交易场景 8 个已知容器）；
    //   ② 所属物品链（GameInventory.GetParentItem → 该容器是谁肚子里的，最多上溯 3 层）；
    //   ③ 内容指纹（childItems 前 5 个物品 id×数量 —— 「携带背包选择」窗口这类
    //      没有 EmporiumEntry 对照的容器，靠内含物品一眼认出）。
    // 每个 GameInventory 实例只打一次（_scopeProbed 去重），单次上溯 ≤3 层，开销可忽略。
    private static string DescribeContainer(GameInventory inv)
    {
        if (inv == null) return "null";
        long key = Ptr(inv);

        string id, cls;
        try { id = inv.identifier; } catch { id = null; }
        try { cls = inv.GetType().Name; } catch { cls = "?"; }

        string slot = "<未知槽位>";
        try { slot = MatchEmporiumSlot(inv); } catch { /* 指针比对失败不影响其余输出 */ }

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

    // 指针比对 EmporiumEntry 的 8 个已知容器（每帧实时取，不缓存 —— 换场景可能重建）。
    //   ★ §七 1.2（0.11.0）：原实现每个容器一个 `TryGet(() => entry.xxx)` 闭包 —— 一次调用
    //     分配 8 个委托对象 + 8 次 il2cpp invoke。改为 `entry` 预取一次 + 逐属性直读
    //     （每个 getter 各自 try/catch，异常语义与原 `TryGet` 完全一致：异常 → null）。
    //   ★ 语义不变：任一步失败只跳过该项（原 TryGet 同）；仅在【配了容器名单】时才被调用
    //     （IsContainerWhitelisted / IsContainerInList 均空串早退），默认配置下零开销。
    //
    // ══ ★★ 0.11.2：slot: token 全面改用【游戏原生 id】（不再硬编码中文，核桃 2026-10-11 定）══
    //   背景：旧版返回中文槽位名（"背包(主仓库)"/"展示柜"/…），cfg 里必须手打中文 → 易错、
    //     且违背「任何标识都不能凭记忆写、一律以游戏文件为准」。
    //   取证（反汇编 GameAssembly.dll + Il2CppDumper 产物，见 _probe/）：
    //     · EmporiumEntry 8 个容器里，运行时**只有 backInvinvElement(0x98) 有 identifier = `backInv`**；
    //       其余 7 个 identifier 实测全空（`[柜台id]` 探针实锤，README §第五轮）。
    //     · 但游戏有对应的【存档袋物品 id】——`ContainerItemDirectory` 工厂方法 + `PlayerStore.saveBags`
    //       字典（`GetSaveBag(string id)` 0x58D240，`Dictionary<string,GameItem>` @0x360）证明这些
    //       id 是游戏真实使用的（`items_catalog.md` 里全部收录）：
    //         save_bag / back_inv_save_bag / showcase_save_bag / sold_save_bag / hidden_save_bag
    //     · 无存档袋的 3 个槽（柜台前格 / 柜台三格 / 黑市左侧）→ 用游戏自己的 C# 成员名作 token
    //       （同为「游戏原生标识」，非自造中文）。
    //   映射表（中文注释仅供人读；cfg 里写 token 列，全部 ASCII）：
    //     主仓库 (invElement 0x30)               → save_bag
    //     展示柜 (showcaseElement 0x40)          → showcase_save_bag
    //     柜台前格 (frontInvinvElement 0x88)     → frontInv
    //     玩家柜台 (backInvinvElement 0x98)      → back_inv_save_bag   （= 游戏 identifier backInv）
    //     柜台三格 (backInvinvElementCounter 0xA0)→ backInvCounter
    //     已售格 (soldElement 0x108)             → sold_save_bag
    //     海报暗格 (hiddenElement 0xC0)          → hidden_save_bag
    //     黑市左侧 (bazarLeftinvElement 0xB0)    → bazarInv
    //   ★ 旧的裸 id token（如 `backInv`）仍可用：那是 identifier 精确匹配路径（IsContainerInList
    //     的 return 分支），与 slot: 是两条独立判据，互不影响。
    private static string MatchEmporiumSlot(GameInventory inv)
    {
        EmporiumEntry entry;
        try { entry = EmporiumEntry.Instance; } catch { return "<不在EmporiumEntry内>"; }
        if (entry == null) return "非交易场景";

        // 逐项直读：每个 getter 独立 try/catch（对应原 TryGet 的容错边界）。
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

    // slot: token 常量（游戏原生 id，全部 ASCII）。集中定义，改一处即全工程同步。
    //   前 5 个 = 游戏存档袋物品 id（items_catalog.md 收录，PlayerStore.saveBags 键）；
    //   后 3 个 = EmporiumEntry 的 C# 成员名（这 3 个槽游戏没有存档袋，也没有 identifier）。
    internal const string SlotMainInv      = "save_bag";            // 主仓库 invElement 0x30
    internal const string SlotShowcase     = "showcase_save_bag";   // 展示柜 showcaseElement 0x40
    internal const string SlotFrontCounter = "frontInv";            // 柜台前格 frontInvinvElement 0x88
    internal const string SlotBackCounter  = "back_inv_save_bag";   // 玩家柜台 backInvinvElement 0x98
    internal const string SlotCounterExtra = "backInvCounter";      // 柜台三格 backInvinvElementCounter 0xA0
    internal const string SlotSold         = "sold_save_bag";       // 已售格 soldElement 0x108
    internal const string SlotHidden       = "hidden_save_bag";     // 海报暗格 hiddenElement 0xC0
    internal const string SlotBazar        = "bazarInv";            // 黑市左侧 bazarLeftinvElement 0xB0

    // 所属物品链：inv.GetParentItem() = 这个容器是谁（背包物品/箱子物品）的内部库存。
    // 例：携带背包选择窗口 → storage_bay；嵌套背包 → 外层背包物品 id。
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

    // ── 0.6.0：容器黑名单（原 StackExclude 改名，0.8.0）──
    // 与白名单相反：这些容器里禁止堆叠（手动配置，用于个别容器需要独占地识别单件的场景）。
    // 双端判定：被叠物品 OR 拖起方 任一方容器在黑名单 → 拦截。
    //
    // ★ 0.9.3：token 匹配与白名单共用 IsContainerInList（裸 id / slot: / owner:）。
    //   实机日志证实 identifier 几乎全空，纯 id 黑名单同样匹配不上；
    //   现在可写 slot:back_inv_save_bag（玩家柜台）或 owner:smuggler_bay 等（★ 0.11.2 起为游戏原生 id）。
    //
    // ★ 0.6.3（需求1）：**移除**了「默认自动把交易柜台加入排除」的运行时探测。
    //   旧版（0.6.0~0.6.2）在留空时会运行时探 EmporiumEntry.Instance 的
    //   backInvinvElement / backInvinvElementCounter 并自动排除，目的是规避「柜台只识别 1 个
    //   堆叠商品」的亏钱 bug。但实测与反汇编复核后确认：
    //     · 该自动排除只影响 MayStack/GetStackSlot 的**堆叠合并**判定，属于「按容器一刀切」，
    //       玩家在柜台整理背包也会被误伤；
    //     · 柜台亏钱问题的正解是「Alt 快移逐件上架」（见文件末尾 AltQuickTransferPrefix），
    //       不应靠全局排除柜台容器来规避。
    //   ⇒ 现在 ContainerBlacklist 只按 cfg 手动生效；留空 = 不排除任何容器。黑名单机制完整保留。
    private static string GetExcludeReason(GameItem target, GameItem source)
    {
        try
        {
            if (_containerBlacklist == null || string.IsNullOrWhiteSpace(_containerBlacklist.Value))
                return null;
            var tinv = target?.parentInventory;
            var sinv = source?.parentInventory;
            // 双端：任一端容器命中黑名单即拦（0.9.3 起走 token 匹配）。
            if (IsContainerInList(tinv, _containerBlacklist.Value))
                return "容器黑名单（" + BriefContainer(tinv) + "）";
            if (IsContainerInList(sinv, _containerBlacklist.Value))
                return "容器黑名单（" + BriefContainer(sinv) + "）";
            return null;
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            MarkJudgeFault(FaultConvenience);   // 阶段2：便利类异常 → Evaluate 将判 Allow
            LogOnce("exclude", Level.Warn, $"容器排除判据异常，本次不阻断: {e.Message}");
            return null;   // 判定失败不阻断
        }
    }

    // ── 0.6.5：交易柜台内禁止堆叠（覆盖【所有】合并路径）──
    //
    // 为什么需要这一层（根因取证）：
    //   本 mod 原有的堆叠闸门只挂在 `GameItem.MayStack` / `GameItem.GetStackSlot` 上。
    //   但反汇编实锤，键位快移走的是完全不同的另一条路：
    //     ItemQuickTransferAltHandler(1).OnEventRelease / ItemQuickTransferHandler.OnEventRelease
    //       → GraphUtils.TryAcceptOnce(GraphNodeStorage, GameItem, int)（0x766730）
    //       → SlotMarker.TryAcceptOnce(int)（0x7B0020）
    //            · targetItem == null（空槽）→ GameInventory.UncheckedAccept（Slot 19）← 纯「移动」
    //            · targetItem != null（有堆）→ SlotMarker.StackItemUnchecked（0x7AFDF0）
    //   `SlotMarker.StackItemUnchecked` 就是真正的合并实现，汇编只有两行算术：
    //       0x7AFE41  add dword ptr [rdi + 0x1F8], esi     ; targetItem.unitCount += n
    //       0x7AFE50  sub dword ptr [rbx + 0x1F8], esi     ; sourceItem.unitCount -= n
    //   **它一个 gate 都不查** —— 没有 MayStack、没有 GetStackSlot、没有任何委托回调。
    //   ⇒ 所以 Alt/Ctrl 快移能绕过全部黑名单/作用域判定，把第 2 件并进柜台已有的堆。
    //
    // 对策：把闸门下沉到这条公共瓶颈 `SlotMarker.TryAcceptOnce`。
    //   · 只在【合并】（targetItem != null）时判定；空槽放置是「移动」不是「堆叠」，一律放行。
    //   · 落点容器从 `SlotMarker.inventory`（0x18，GameInventory）取 —— 对快移路径就是
    //     「鼠标落点窗口里第一个有效槽所在的容器」，语义与拖放一致。
    //   · 判定复用既有两条规则：GetScopeReason（白名单）+ GetExcludeReason（黑名单）
    //     + IsCounterSlot（柜台），保证「拖放拦的，快移也拦」。
    //
    // ★ 0.6.6 修正柜台判据（0.6.5 认错了容器，实机无效）：
    //   0.6.5 只用 `EmporiumEntry.backInvinvElementCounter`(0xA0) 一个容器比对 —— 反汇编复核后确认这是**错的**：
    //     · `EmporiumEntry.GetCountersItems`(0x7CE010) 读的是 frontInvinvElement(0x88) **Concat** backInvinvElement(0x98)
    //       → 「柜台」在游戏眼里 = front + back 两个格。
    //     · `EmporiumEntry.Start`(0x7D49A0) 里三个格的构造尺寸：
    //           [rdi+0x88] frontInvinvElement        18×4   ← 柜台正面
    //           [rdi+0x98] backInvinvElement         18×4   ← 柜台背面
    //           [rdi+0xa0] backInvinvElementCounter  18×4   ← 第三个格（影子/另一处柜台）
    //     · 形状改写只发生在 0x88：`ResetFrontCounterGrid`(0x7D38B0) / `ExpandFrontCounterGrid`(0x7CB3B0)
    //       都 `mov rcx,[rcx+0x88]` → SetShape；`SwapInv`(0x7D7BB0) 交换的也是 0x88 ↔ 0x98。
    //     · `backInvinvElementCounter`(0xA0) 全库只有 3 处引用（Start 构造 + GetAllOwnedItems(Shallow) 读），
    //       玩家拖放/快移**永远不会**落在它上面 ⇒ 0.6.5 的判据永远返回 false，功能静默失效。
    //   ⇒ 正解：把「柜台容器集合」定义为 front(0x88) + back(0x98) + counter(0xA0)，任一命中即视为柜台。
    //
    // 判据实现：不硬编码 identifier（柜台容器运行时创建，identifier 可能为空），
    //   直接用 EmporiumEntry.Instance 上的这三个容器引用做指针比对（廉价、稳定）。
    //   每次实时取，不缓存 —— 换场景/开关窗口可能重建容器，缓存旧引用会让判据失效。
    private static bool IsCounterSlot(SlotMarker slot)
    {
        try
        {
            if (_blockCounterStack == null || !_blockCounterStack.Value) return false;
            return IsCounterInventory(slot == null ? null : slot.inventory);
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("counterslot", Level.Warn, $"柜台判定异常，本次不阻断: {e.Message}");
            return false;
        }
    }

    // ★ 0.7.1：同一个判据的「按容器」入口（给 MayStack 用 —— 那条路径只有 GameItem，没有 SlotMarker）。
    //   front(0x88) / back(0x98) / counter(0xA0) 三格任一命中即视为柜台。
    internal static bool IsCounterInventory(GameInventory inv)
    {
        try
        {
            if (_blockCounterStack == null || !_blockCounterStack.Value) return false;
            if (inv == null) return false;

            var entry = EmporiumEntry.Instance;
            if (entry == null) return false;      // 不在交易场景 → 不可能命中柜台

            // ★ 直读三个柜台格（与 MatchEmporiumSlot 同款：entry 预取 + 逐属性 try/catch）——
            //   不再逐属性分配 `TryGet(()=>entry.xxx)` 闭包（原实现一次调用 3 个委托 + 3 次 invoke）。
            //   异常语义与原 TryGet 一致（异常 → 该项视为 null，跳过）。
            GameGridInventory eFront = null, eBack = null, eBackC = null;
            try { eFront = entry.frontInvinvElement; } catch { }
            try { eBack = entry.backInvinvElement; } catch { }
            try { eBackC = entry.backInvinvElementCounter; } catch { }

            if (SameNativePointer(inv, eFront)) return true;
            if (SameNativePointer(inv, eBack)) return true;
            if (SameNativePointer(inv, eBackC)) return true;
            return false;
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            MarkJudgeFault(FaultConvenience);   // 阶段2：便利类异常 → Evaluate 将判 Allow
            LogOnce("counterinv", Level.Warn, $"柜台容器判定异常，本次不阻断: {e.Message}");
            return false;
        }
    }

    // （TryGet / Ptr / SafeId / SameNativePointer / Describe 已抽出至 Il2CppHelpers.cs，
    //   2026-10-12；原 SameInventory 包装已删除，调用点全部直接改调 SameNativePointer。）

    // 合并闸门的统一判定：返回「拒绝原因」，null = 允许。
    // target = 被并进的那一格上的物品（目标堆），source = 要并进去的物品。
    // slot = 落点（提供 inventory 归属，快移路径下这才是可靠的容器来源）。
    //
    // ★ 0.10.0（阶段1）：本方法改为【单一源 MergePolicy】的薄封装 ——
    //   探针保留，决策委托给 MergePolicy.Evaluate（固定顺序：柜台→容器→黑名单→排除→作用域）。
    //   这样 TryAcceptOnce（快移/全路径）、MayStack、GetStackSlot 三处结论必然一致。
    //   回滚：z9_UseMergePolicy=false 时走下方 0.9.9 旧顺序（保留未删）。
    private static string GetMergeGateReason(SlotMarker slot, GameItem target, GameItem source)
    {
        // 0) 探针：任何一次「真实合并」都记录落点容器（每容器只打一次）。
        //   ★ §七 1.3/1.4（0.11.0）：整块包进 `if (DiagOn)` —— 各探针函数内部虽已 `!DiagOn 早退`，
        //     但其**实参**（尤其 `TryGet(() => source?.parentInventory)` 闭包）是【调用前急切求值】的，
        //     故 DiagLog 关闭（默认）时仍会白白分配委托 + il2cpp invoke 一次 getter。
        //     包住后：DiagLog 关 → 零闭包、零 invoke；DiagLog 开 → 行为与原先逐字节一致。
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
            // Deny → 返回原因（拦截）；Allow/Unknown → null（放行）。
            return verdict == MergeVerdict.Deny ? reason : null;
        }

        // ══════════ 0.9.9 旧顺序（回滚用，保留未删）══════════
        //   ★ T2.1 variant B（2026-10-12）：整段原样搬入 MergePolicy.Legacy.cs 的
        //     LegacyMergeGateReason（柜台 → 排除 → 作用域 → 黑名单 → 容器，顺序与文案未改）。
        return LegacyMergeGateReason(slot, target, source);
    }
}

using System;
using System.Collections.Generic;
using Il2Cpp;
using MelonLoader;

namespace ItemStacking;

public partial class Main
{
    // ══════════════════════════════════════════════════════════════════════
    //  1.2.0 · tag 值域校验（Default-State Value Gate）
    //
    //  ── 解决什么问题 ──
    //  游戏 GetSplitItem(0x797500) 拆分堆叠时，拆出来的「新那份」不是复制原物品的 tag，
    //  而是【用 DirectoryMaster.Item(identifier) 重新生成一个默认物品】再设 unitCount。
    //  ⇒ 拆出来的物品带着「该 identifier 的默认 tag」（满电/满耐久/默认纯度/默认品质），
    //     而【不是】原堆里物品的实际 tag。
    //  ⇒ 于是「状态相同但偏离默认」的物品（如两件都是 50% 电量的电池）堆叠后，
    //     拆出来会变成 100% 电量 —— 这就是「拆分后 tag 污染」。
    //
    //  ── 判定规则（核桃定稿 2026-10-12）──
    //  只有「关键 tag 的值 == 该 identifier 的默认模板值」的物品才允许堆叠。
    //  即：电池满电可堆 / 非满不可堆；工具满耐久可堆 / 非满不可堆；
    //      金属锭默认纯度可堆 / 非默认不可堆；莓果默认品质可堆 / 非默认不可堆。
    //  理由：默认状态拆分后不变（DirectoryMaster.Item 造的正是默认状态）⇒ 零污染。
    //
    //  ── 「默认模板值」从哪来 ──
    //  运行时用 DirectoryMaster.Item(identifier) 造一次模板，读它的关键 tag 值作为基准。
    //  ★ 必须缓存（按 identifier）：DirectoryMaster.Item 每次调用都新分配一个物品对象，
    //     热路径上反复调用会内存泄漏 + 卡顿。缓存后每个 identifier 只造一次。
    //
    //  ── 关键 tag 清单（★ 维护入口，需要增删时改这里）──
    //  每个条目 = 一个「会影响拆分后状态的 tag」。判断 = 该 tag 的当前值必须等于模板值。
    //  取「值」用 TagState.GetInt()/GetString()（TagState 是 7 值字段联合体，
    //  实际存哪个字段由 tag 类型决定；这里统一用「字符串化」比较，见 TagFingerprint）。
    // ══════════════════════════════════════════════════════════════════════

    // ★ 关键 tag 清单：会影响「拆分后状态」的 tag。
    //   维护方法：新增一个会随使用/加工/时间变化的【物品持久属性】tag 时，往这里加一行。
    //   ★ 只收「拆分后会被 DirectoryMaster.Item 重置为默认值」的【物品属性】tag。
    //     不收机器运行时状态（MACHINE_*/PROGRESS_*/CURRENT_PLAYING_* 等已在状态忽略名单里，
    //     它们不随拆分污染，收进来只会误伤机器/加工中的物品）。
    private static readonly string[] ValueGateTags =
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

    // 默认模板缓存：identifier → 该物品「关键 tag 的指纹」（字符串化的值）。
    //   key = identifier；value = Dictionary<tagKey, string>（tag 名 → 值指纹）。
    private static readonly Dictionary<string, Dictionary<string, string>> _defaultStateCache
        = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

    // ──────────────────────────────────────────────────────────────────────
    // ★ 核心：两件物品是否都处于「默认状态」（可安全堆叠，拆分无污染）。
    //   返回 true = 关键 tag 都等于默认模板值（可堆）；false = 有偏离（不可堆）。
    //   异常 → 置 FaultProtection 并返回 false（保守：宁可拦不可丢）。
    // ──────────────────────────────────────────────────────────────────────
    internal static bool AtDefaultState(GameItem a, GameItem b)
    {
        try
        {
            if (a == null || b == null) return false;
            string id = a.identifier;
            if (string.IsNullOrEmpty(id)) return false;

            // 两件物品 identifier 必须一致（状态闸门已保证同名，这里再兜底）。
            if (!string.Equals(id, b.identifier, StringComparison.Ordinal)) return false;

            var defaults = GetDefaultState(id);
            // ★ 关键修复（2026-10-12）：该物品【没有任何关键 tag】（如绷带/牙膏/普通食物）
            //   ⇒ 无需值域校验 ⇒ 放行（return true）。只有【有关键 tag】的物品才比，
            //   偏离默认才拦。否则会误伤所有「无电量/耐久/纯度」的普通物品。
            if (defaults == null || defaults.Count == 0) return true;

            // 对每个关键 tag：a、b 的当前值都必须 == 默认模板值。
            foreach (var kv in defaults)
            {
                string tagKey = kv.Key;
                string defVal = kv.Value;
                string va = TagFingerprint(a, tagKey);
                string vb = TagFingerprint(b, tagKey);

                // 一侧缺 tag、或值不等于默认值 → 偏离默认 → 拦截。
                if (va == null || !string.Equals(va, defVal, StringComparison.Ordinal))
                {
                    if (DiagOn)
                        LogOnce("valuegate." + id + "." + tagKey, Level.Diag,
                            $"[值域校验] {id} 的 tag「{tagKey}」偏离默认：当前 {va ?? "<缺>"} ≠ 默认 {defVal} → 拦截", max: 3);
                    return false;
                }
                if (vb == null || !string.Equals(vb, defVal, StringComparison.Ordinal))
                {
                    if (DiagOn)
                        LogOnce("valuegate." + id + "." + tagKey, Level.Diag,
                            $"[值域校验] {id} 的 tag「{tagKey}」偏离默认：当前 {vb ?? "<缺>"} ≠ 默认 {defVal} → 拦截", max: 3);
                    return false;
                }
            }
            return true;
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            MarkJudgeFault(FaultProtection);
            LogOnce("valuegate", Level.Warn, $"值域校验异常，本次按「偏离默认」拦截（保守）: {e.Message}");
            return false;
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    // 取「identifier 的默认模板关键 tag 值」（带缓存）。
    //   首次：DirectoryMaster.Item(id) 造模板 → 逐 tag 取指纹 → 缓存。
    //   之后：直接读缓存。
    //   注意：DirectoryMaster.Item 返回的模板对象【不再持有】——我们只读它的 tag 指纹，
    //         读完即弃（让 GC 回收），只缓存字符串指纹，不缓存对象引用（防泄漏）。
    // ──────────────────────────────────────────────────────────────────────
    private static Dictionary<string, string> GetDefaultState(string identifier)
    {
        if (_defaultStateCache.TryGetValue(identifier, out var cached))
            return cached;

        Dictionary<string, string> defaults = null;
        try
        {
            var template = DirectoryMaster.Item(identifier, true);
            if (template == null) return null;

            defaults = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var tag in ValueGateTags)
            {
                string fp = TagFingerprint(template, tag);
                if (fp != null)
                    defaults[tag] = fp;
            }
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("valuegate.tpl", Level.Warn, $"取默认模板失败（{identifier}），本次按「偏离默认」拦截: {e.Message}");
            return null;
        }

        if (defaults == null || defaults.Count == 0)
            return null;   // 该物品没有任何关键 tag → 不参与值域校验（交给状态闸门）

        _defaultStateCache[identifier] = defaults;
        return defaults;
    }

    // ──────────────────────────────────────────────────────────────────────
    // 取「某个 tag 的值指纹」（字符串化，用于比较）。
    //   TagState 是 7 值字段联合体（valueEnabled/internalValueString/valueInt/
    //   valueFloat/valueLong/valueDouble/valueBool），实际只用一个。这里【直接读字段】
    //   （与 StateConsistency.cs 的 TagStateEqual 同款 interop 字段访问，零 invoke、极快），
    //   不用 GetInt()/GetString()（那俩是 native 方法，慢且语义不明确）。
    //   ★ 字符串化比较：int 型 tag → "i:<值>"；string 型 tag → "s:<值>"。
    //     不同 tag 存不同字段，字符串化能统一比较。
    //   ★ 返回 null = 该物品没有此 tag（不参与该 tag 的校验）。
    // ──────────────────────────────────────────────────────────────────────
    private static string TagFingerprint(GameItem item, string tagKey)
    {
        try
        {
            var ts = item.GetTagReadonly(tagKey);
            if (ts == null) return null;
            if (!ts.valueEnabled) return null;

            // string 型优先（纯度、manufacturer 等）；否则 int（电量、耐久、次数、进度）。
            if (!string.IsNullOrEmpty(ts.internalValueString))
                return "s:" + ts.internalValueString;
            return "i:" + ts.valueInt;
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            return null;   // 读取失败 → 视为无此 tag（不参与该校验项）
        }
    }
}

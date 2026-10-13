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
    //  ── 0.14.0 · 零值 tag 变体（ValueGateZeroTags，见 Constants.cs）──
    //  模板法管不了「模板上根本没有的 tag」（如赃物热度 STOLEN_VALUE_INT——模板是全新物品，
    //  不带赃物状态，登记进 ValueGateTags 会静默跳过 = fail-open）。零值变体不比模板，
    //  直接要求当前值【缺省或 == 0】：0 热度赃物可堆 / 非 0 不可堆（填补「热度相同非 0
    //  也能合并」的缺口）。检查点在 AtDefaultState 内、且必须早于 Count==0 早退。
    //
    //  ── 「默认模板值」从哪来 ──
    //  运行时用 DirectoryMaster.Item(identifier, true) 造一次模板，读它的关键 tag 值作为基准。
    //  ★ 必须缓存（按 identifier）：DirectoryMaster.Item 每次调用都新分配一个物品对象，
    //     热路径上反复调用会内存泄漏 + 卡顿。缓存后每个 identifier 只造一次。
    //
    //  ── H2 取证结论（2026-10-12，反汇编 GameAssembly 0x49a010 + interop 元数据）──
    //  第二个 bool 参数的真名是 isOwned（不是 initTags），语义与全部副作用已钉死：
    //    ① DirectoryMaster.Create<T>(identifier) —— 按目录数据【新建】一个 GameItem 实例；
    //    ② GeneralHelper.SetItemOwned(item, isOwned) —— true 时对【该实例】EnableTag("IS_OWNED_TAG")；
    //    ③ 若 item.ownerId == 0 → ownerId = PlayerStore.Instance.GetUniqueID()（仍只写该实例字段）；
    //    ④ PermitHelper.OnNewItemCreated(item) —— 仅对违禁品做 SetGameItemType("RESTRICTED_*")
    //       类型标注，同为【实例级】写入。
    //  ⇒ 全部作用域 = 新建的临时实例本身：不写存档、不触发成就、不注册进任何容器/全局列表。
    //  ⇒ 副作用「会真的创建物品实例」成立（StackPatches.cs 已注明不可用于探测），但对本方法
    //     是【刻意所需】：GetSplitItem（0x797500）拆分时自己就调 Item(id, isOwned: true)
    //     （0x797601 `mov dl,1` + call 0x49a010 已核实）—— 模板必须与游戏拆分路径同源，
    //     否则基准值偏离拆分实际产物，值域校验就失去意义。故【保持 true，不换替代路径】。
    //  ⇒ 唯一异常面：PlayerStore.Instance 为 null 时 Item 内部抛 NRE —— 本方法 try/catch
    //     已兜住（Warn + 按「偏离默认」拦截），无崩溃风险。
    //
    //  ── 关键 tag 清单（★ 维护入口已迁 Constants.ValueGateTags，M3）──
    //  每个条目 = 一个「会影响拆分后状态的 tag」。判断 = 该 tag 的当前值必须等于模板值。
    //  取「值」用 TagState.GetInt()/GetString()（TagState 是 7 值字段联合体，
    //  实际存哪个字段由 tag 类型决定；这里统一用「字符串化」比较，见 TagFingerprint）。
    // ══════════════════════════════════════════════════════════════════════

    // ★ 关键 tag 清单（ValueGateTags）已迁至 Constants.cs（M3 常量集中）——
    //   维护入口在那边：新增/删除「会影响拆分后状态的 tag」时改 Constants.ValueGateTags，
    //   内容与注释原样搬迁，本文件只读不写（见下方 GetDefaultState 内的 foreach）。

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
            // ★ P1 修复（2026-10-13）：把「模板不可用」与「该物品无关键 tag」严格区分开。
            //   · null  = 模板获取失败（GetDefaultState 已置 FaultProtection）⇒ 判据不可信
            //             ⇒ 返回 false（按「偏离默认」拦截，保守）。绝不能当「无 tag」放行 ——
            //             那是静默 fail-open，与文件头「异常 → 置 FaultProtection 并返回 false」的
            //             承诺相反，会重新引入拆分污染。
            //   · 空字典 = 该物品确实没有任何关键 tag（绷带/牙膏/普通食物）⇒ 无需值域校验 ⇒ 放行。
            //             （原实现把两者都当 null 放行，导致模板取不到时静默跳过整道值域闸门。）
            if (defaults == null) return false;

            // ── 0.14.0 · 零值 tag 检查（★ 必须早于下方 Count==0 早退）──
            //   赃物类物品的模板通常【没有任何关键 tag】（defaults 为空），若放在早退之后
            //   会被 `Count == 0 → 放行` 绕过 = fail-open。故先跑本段，再走模板比对。
            //   规则：Constants.ValueGateZeroTags 里的 tag，当前值必须【缺省或 == 0】
            //   （0 热度可堆 / 非 0 不可堆；TagFingerprint 语义：null=缺省、"i:0"=零值）。
            foreach (var zeroTag in Constants.ValueGateZeroTags)
            {
                string za = TagFingerprint(a, zeroTag);
                if (za != null && !string.Equals(za, "i:0", StringComparison.Ordinal))
                {
                    if (DiagOn)
                        LogOnce("valuegate." + id + "." + zeroTag, Level.Diag,
                            $"[值域校验] {id} 的零值 tag「{zeroTag}」非零：{za} → 拦截", max: 3);
                    return false;
                }
                string zb = TagFingerprint(b, zeroTag);
                if (zb != null && !string.Equals(zb, "i:0", StringComparison.Ordinal))
                {
                    if (DiagOn)
                        LogOnce("valuegate." + b.identifier + "." + zeroTag, Level.Diag,
                            $"[值域校验] {b.identifier} 的零值 tag「{zeroTag}」非零：{zb} → 拦截", max: 3);
                    return false;
                }
            }

            if (defaults.Count == 0) return true;

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
    //   首次：DirectoryMaster.Item(id, isOwned: true) 造模板 → 逐 tag 取指纹 → 缓存。
    //         （isOwned 语义与副作用见文件头 H2 取证结论：与 GetSplitItem 拆分路径同源，
    //           副作用仅作用于新建临时实例，不写存档。true 不可改为 false。）
    //   之后：直接读缓存。
    //   注意：DirectoryMaster.Item 返回的模板对象【不再持有】——我们只读它的 tag 指纹，
    //         读完即弃（让 GC 回收），只缓存字符串指纹，不缓存对象引用（防泄漏）。
    // ──────────────────────────────────────────────────────────────────────
    private static Dictionary<string, string> GetDefaultState(string identifier)
    {
        if (_defaultStateCache.TryGetValue(identifier, out var cached))
            return cached;

        Dictionary<string, string> defaults = null;
        bool failed = false;
        try
        {
            var template = DirectoryMaster.Item(identifier, true);   // true = isOwned（见头注释 H2）
            if (template == null)
            {
                failed = true;
            }
            else
            {
                defaults = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var tag in Constants.ValueGateTags)
                {
                    string fp = TagFingerprint(template, tag);
                    if (fp != null)
                        defaults[tag] = fp;
                }
            }
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("valuegate.tpl", Level.Warn, $"取默认模板失败（{identifier}）: {e.Message}");
            failed = true;
        }

        if (failed)
        {
            // ★ P1 修复（2026-10-13）：模板不可用 = 判据不可信 → 置【数据保护】故障位
            //   （MergePolicy 据此走 [防护降级] + Deny 保守路径），并返回 null。
            //   null 专表「模板不可用」；「该物品无关键 tag」返回【空字典】（二者不再混同）。
            //   ★ 失败结果【不缓存】：万一 PlayerStore.Instance 只是此刻为 null，
            //     下次调用可重试成功（缓存失败会让闸门永久失效）。
            MarkJudgeFault(FaultProtection);
            return null;
        }

        // 「无关键 tag」→ 空字典也缓存（避免每次重复造模板）。
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

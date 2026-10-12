using System;
using System.Collections.Generic;
using System.Diagnostics;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace ItemStacking;

// ══════════════════════════════════════════════════════════════════════════
//  探针 / 诊断输出集中地（2026-10-12 从 StackGates / ModifierPairTransfer /
//  StackPatches.Value / StateConsistency 抽出，T2.4）
//
//  这些方法全部满足三个特征，才被认定为「探针」：
//    ① 只读（不改任何游戏状态 / 不参与合并决策）；
//    ② 第一行就被 DiagOn 总闸拦掉（DiagLog=false 时零开销）；
//    ③ 输出面向「人类排查」，删掉不影响任何功能。
//  ★ 都是 partial class Main 的成员（不能开独立 static class：要复用 Log/DiagOn/
//    WarnOnce/Ptr/SafeId 等既有工具，改动面太大得不偿失）。
//  ★ 各探针的去重集合 / 计数器随方法一起搬过来，字段与方法不再跨文件。
//  ★ CounterIdProbe（柜台 id 打印）已是独立文件，保持不动。
// ══════════════════════════════════════════════════════════════════════════
public partial class Main
{
    // ─── 原出处：StackPatches.Value.cs ───

    // ── 0.9.0 / 0.10.0 价值·链探针：整堆计价的单件价 / 数量 / 总价 + 调用链头部 ──
    //   目的（0.9.0）：实机确认「卖出时柜台到底按什么数量扣物品」。
    //   目的（0.10.0 新增）：**定位是哪个 mod 先抬了单件价** —— 用户机器上出现
    //     「整堆价格异常过高」而本机复现不了，根因是加价顺序（另一 mod 在本方法
    //     Postfix 链或原生内部链更早抬高单件价，我们再 ×N 就放大）。本探针打印
    //     调用栈头部，一眼看出是谁在链上先动的手。
    //   按「物品 id + 数量 + 单价」去重，上限 40 条；**受 DiagLog 总闸控制**。
    private static readonly HashSet<string> _valueProbed = new HashSet<string>(StringComparer.Ordinal);

    private static void ProbeNegociatedValue(GameItem item, long unit, int count, long total)
    {
        try
        {
            if (!DiagOn) return;                             // 0.9.0：诊断总闸
            if (_valueProbed.Count >= 40) return;
            string key = SafeId(item) + "|" + count + "|" + unit;
            if (!_valueProbed.Add(key)) return;
            Log?.Msg($"[整堆计价] {SafeId(item)}：单价 {unit} × {count} 件 = 总价 {total}");

            // ★ 0.10.0：打印调用链头部，用于定位「谁在链上先抬价」。
            //   跳过本 mod 自身的 3 帧（Probe → Postfix → Harmony 蹦床），取之后的调用方。
            try
            {
                var st = new System.Diagnostics.StackTrace(3, false);
                var sb = new System.Text.StringBuilder("[整堆计价·链] 调用链头部：\n");
                int shown = 0;
                for (int i = 0; i < st.FrameCount && shown < 8; i++)
                {
                    var m = st.GetFrame(i)?.GetMethod();
                    if (m == null) continue;
                    var decl = m.DeclaringType;
                    // 跳过本 mod 自身的方法（ItemStacking.*），只留外部调用方。
                    if (decl != null && decl.Namespace != null &&
                        decl.Namespace.StartsWith("ItemStacking", StringComparison.Ordinal))
                        continue;
                    sb.Append("    ").Append(decl?.FullName ?? "?").Append('.').Append(m.Name).Append('\n');
                    shown++;
                }
                Log?.Msg(sb.ToString());
            }
            catch { /* 探针失败不影响计价 */ }
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
        }
    }

    // ─── 原出处：StateConsistency.cs ───

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

    // ─── 原出处：StackGates.cs ───

    // ── 0.7.1 探针：非 Alt 路径的落点快照（定位「转场景回商店只带 1 个」）──
    //
    // 为什么需要：用户的「跨场景转移只搬 1 个」现象有三种可能成因，必须用实测区分：
    //   ① 原生 `TryFindOneValidInventorySlot` 本来就只给 `numTransfer = 1`
    //      （例如目标堆已满 / 委托把上限压成 1）；
    //   ② 原生给了 >1，但下游（`SlotMarker.TryAcceptOnce` 的闸门 / `MaxNumRemove`）把它吃成 1；
    //   ③ 我们的 Alt 钳制泄漏（`AltTransferScope` 意外为真）。
    // 本探针只在**非 Alt** 路径、且「**源堆有多件、但原生只给 1 件**」
    // （`item.unitCount > 1 && numTransfer == 1`）时记录 —— 这正是「只搬 1 个」的特征。
    // 按「容器指针 + numTransfer + item」去重，最多 40 条。
    //   · 命中条目 ⇒ 说明**原生本身就只给 1 件**（问题在抬升点：MaxNumAdd / 委托），
    //     而不是下游闸门吃掉的。
    private static readonly HashSet<string> _nonAltProbed = new HashSet<string>(StringComparer.Ordinal);

    private static void ProbeNonAltMarker(GameItem item, SlotMarker result)
    {
        try
        {
            if (!DiagOn) return;                                  // 0.9.0：诊断总闸
            if (item == null || item.unitCount <= 1) return;      // 只看"本来该搬多件"的
            if (result.numTransfer != 1) return;                  // 只看"只给 1 件"的
            if (_nonAltProbed.Count >= 40) return;

            string key = Ptr(result.inventory).ToString("X") + "|" + SafeId(item);
            if (!_nonAltProbed.Add(key)) return;

            string invId;
            try { invId = SafeId(result.inventory); } catch { invId = "?"; }
            Log?.Msg($"[转场探针] 非Alt落点只给1件：item={SafeId(item)} ×{item.unitCount}，" +
                     $"容器id={invId}，targetItem={SafeId(result.targetItem)}，numTransfer={result.numTransfer}");
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
        }
    }

    // ─── 原出处：StackGates.cs ───

    // ── 作用域探针：把「实际遇到过的容器 id」一次性列出来 ──
    // 为什么需要：0.9.1 实机日志证实 GameInventory.identifier 几乎全空，
    //   用户无法靠 id 配置白名单；探针补 槽位/所属/内含 三路信息，
    //   让用户直接抄成 slot:… / owner:… token（0.9.2）。
    // 本探针把每个**实际出现过**的容器实例去重打一行：
    //   序号 / id / 类名 / 指针 / 槽位 / 所属 / 内含 / 本次角色 /
    //   当前是否落在 ContainerWhitelist 内 ⇒ 走一圈场景就能抄全名单。
    // 开销：DiagLog=false 时第一行就 return（零开销）；开启时每个容器实例只打一次，
    //   达到 40 条上限后停（并提示可能还有容器没记到）。
    private static readonly HashSet<long> _scopeProbed = new HashSet<long>();
    private const int ScopeProbeMax = 64;   // 0.9.1：40 → 64（多 mod 场景容器更多，上限到了会漏）

    private static void ProbeScopeInventory(GameInventory inv, string role)
    {
        try
        {
            if (!DiagOn) return;
            if (inv == null) return;
            long key = Ptr(inv);
            if (key == 0L) return;
            if (_scopeProbed.Count >= ScopeProbeMax)
            {
                if (_scopeProbed.Count == ScopeProbeMax)
                {
                    _scopeProbed.Add(-1L);   // 借一个保留 key 打「已达上限」提示，只打一次
                    Log.Msg($"[作用域探针] 已达 {ScopeProbeMax} 条上限，可能还有容器没记录到。" +
                             "把 ContainerWhitelist 填成「要允许堆叠的那几个」即可，其余一律禁堆。");
                }
                return;
            }
            if (!_scopeProbed.Add(key)) return;

            string id;
            try { id = inv.identifier; } catch { id = null; }
            bool inList = IsContainerWhitelisted(inv);
            bool inBlack = false;
            try
            {
                if (_containerBlacklist != null && !string.IsNullOrWhiteSpace(_containerBlacklist.Value))
                    inBlack = IsContainerInList(inv, _containerBlacklist.Value);
            }
            catch { }
            Log.Msg($"[作用域探针] {_scopeProbed.Count,3} 号容器：{DescribeContainer(inv)}" +
                     $"  角色={role}  白名单内={(inList ? "是" : "否")}  黑名单内={(inBlack ? "是" : "否")}");
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            WarnOnce("scopeprobe", $"[作用域探针] 输出失败: {e.Message}");
        }
    }

    // ─── 原出处：StackGates.cs ───

    // ── 0.6.6 探针：把「本次合并落点容器 vs EmporiumEntry 各容器」打到日志（每容器只打一次）──
    //   为什么保留：柜台容器是运行时创建的，静态分析拿不到 identifier；万一 0.6.6 的
    //   front/back/counter 三格判据仍与实机不符，这一行能一次性给出对齐所需的全部信息
    //   （落点容器 identifier + 指针，与 EmporiumEntry 8 个候选容器的 identifier + 指针并列）。
    //   只在「真实合并」时触发，且同一容器只打一次，对正常游玩无性能影响。
    private static readonly HashSet<long> _counterProbed = new HashSet<long>();

    private static void DumpCounterProbeOnce(GameInventory slotInv)
    {
        try
        {
            if (!DiagOn) return;                       // 0.9.0：诊断总闸
            long key = Ptr(slotInv);
            if (!_counterProbed.Add(key)) return;      // 同一个容器只打一次
            if (_counterProbed.Count > 12) return;     // 防御：最多 12 条

            var entry = EmporiumEntry.Instance;
            if (entry == null) { Log.Msg("[柜台探针] 本次合并落点容器 = " + Describe(slotInv) +
                                         "（EmporiumEntry.Instance = null，非交易场景）"); return; }

            Log.Msg("[柜台探针] 本次合并落点容器 = " + DescribeContainer(slotInv) +
                    "  || EmporiumEntry 容器：" +
                    "invElement="        + DescribeContainer(TryGet(() => entry.invElement)) +
                    " / showcaseElement="+ DescribeContainer(TryGet(() => entry.showcaseElement)) +
                    " / frontInvinv="    + DescribeContainer(TryGet(() => entry.frontInvinvElement)) +
                    " / backInvinv="     + DescribeContainer(TryGet(() => entry.backInvinvElement)) +
                    " / backCounter="    + DescribeContainer(TryGet(() => entry.backInvinvElementCounter)) +
                    " / soldElement="    + DescribeContainer(TryGet(() => entry.soldElement)) +
                    " / hiddenElement="  + DescribeContainer(TryGet(() => entry.hiddenElement)) +
                    " / bazarLeft="      + DescribeContainer(TryGet(() => entry.bazarLeftinvElement)));
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            WarnOnce("counterprobe", $"[柜台探针] 输出失败: {e.Message}");
        }
    }

    // ─── 原出处：ModifierPairTransfer.cs ───

    // ══════════════════════════════════════════════════════════════════════
    //  O6：三个 handler 的实际键位一次性打印（仅 DiagLog 开时）
    //  key / quickTransferKey 是 prefab 的 [SerializeField] 数据，静态不可见
    //  （Awake 只自注册 current —— 已反汇编核实）。分派逻辑按键位路由，
    //  故启动时（进入场景后）打出来，便于确认「哪个类 = Shift / Alt / Ctrl」。
    // ══════════════════════════════════════════════════════════════════════
    private static bool _modifierKeyDumped;

    private static void ModifierKeyProbeTick()
    {
        if (!DiagOn) return;
        if (_modifierKeyDumped) return;
        try
        {
            var q  = TryGet(() => ItemQuickTransferHandler.current);
            var a  = TryGet(() => ItemQuickTransferAltHandler.current);
            var a1 = TryGet(() => ItemQuickTransferAltHandler1.current);
            if (q == null && a == null && a1 == null) return;   // 尚未创建 → 下帧再试
            _modifierKeyDumped = true;

            string sQ  = q  == null ? "缺失" : $"key={q.key}, quickTransferKey={q.quickTransferKey}";
            string sA  = a  == null ? "缺失" : $"key={a.key}, quickTransferKey={a.quickTransferKey}";
            string sA1 = a1 == null ? "缺失" : $"key={a1.key}, quickTransferKey={a1.quickTransferKey}";
            Log.Msg("[修饰键] 键位探针（用于确认分派路由）：" +
                    "ItemQuickTransferHandler(" + sQ + ")，" +
                    "ItemQuickTransferAltHandler(" + sA + ")，" +
                    "ItemQuickTransferAltHandler1(" + sA1 + ")。" +
                    "分派规则：Shift ⇒ 展示柜↔主仓库；Alt ⇒ 玩家柜台↔主仓库；其它 ⇒ 交还原生。");
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            _modifierKeyDumped = true;   // 出错也别每帧重试
            WarnOnce("modifierkey", $"[修饰键] 键位探针输出失败: {e.Message}");
        }
    }
}

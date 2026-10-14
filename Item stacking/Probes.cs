using System;
using System.Collections.Generic;
using System.Diagnostics;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace ItemStacking;

// 探针 / 诊断输出集中地
//
// ★ 探针的三个特征（才被认定为本文件成员）：
//   ① 只读（不改任何游戏状态 / 不参与合并决策）
//   ② 第一行就被 DiagOn 总闸拦掉（DiagLog=false 时零开销）
//   ③ 输出面向「人类排查」，删掉不影响任何功能
//
// ★ 都是 partial class Main 的成员（不开独立 static class —— 要复用 Log/DiagOn/LogOnce/
//   Ptr/SafeId 等既有工具）。
public partial class Main
{
    // ── 整堆计价探针：单件价 / 数量 / 总价 + 调用链头部 ──
    //   调用链头部用于定位「哪个 mod 在链上先抬了单件价」（加价顺序问题）。
    //   按「物品 id + 数量 + 单价」去重，上限 40 条。
    private static readonly HashSet<string> _valueProbed = new HashSet<string>(StringComparer.Ordinal);

    private static void ProbeNegociatedValue(GameItem item, long unit, int count, long total)
    {
        try
        {
            if (!DiagOn) return;
            if (_valueProbed.Count >= 40) return;

            // 本地 HashSet 门控昂贵的 StackTrace 采集（不能只靠 LogOnce）。
            string key = "value." + SafeId(item) + "|" + count + "|" + unit;
            if (!_valueProbed.Add(key)) return;
            LogOnce(key, Level.Diag, $"[整堆计价] {SafeId(item)}：单价 {unit} × {count} 件 = 总价 {total}", max: 1);

            // 调用链头部：跳过本 mod 自身（ItemStacking.*）帧，只留外部调用方。
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
                    if (decl != null && decl.Namespace != null &&
                        decl.Namespace.StartsWith("ItemStacking", StringComparison.Ordinal))
                        continue;
                    sb.Append("    ").Append(decl?.FullName ?? "?").Append('.').Append(m.Name).Append('\n');
                    shown++;
                }
                LogOnce(key + ".chain", Level.Diag, sb.ToString(), max: 1);
            }
            catch { /* 探针失败不影响计价 */ }
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
        }
    }

    // ── 状态比较耗时统计 ──
    //   DiagLog=true 时每 5 秒打印「调用 X 次 / 累计 Y ms / 平均 Z µs」。
    //   使「是否卡」有数据而非感觉。
    private static long _stateCmpCalls;
    private static long _stateCmpTicks;
    private static float _stateCmpLastReport;

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
        // 本条特意不走 LogOnce：语义就是要每 5 秒反复输出。
        Log?.Msg($"[状态比较·耗时] 最近 5 秒：调用 {calls} 次 / 累计 {ms:F2} ms / 平均 {us:F2} µs。");
    }

    // ── 非 Alt 路径「落点只给 1 件」探针 ──
    //   定位「跨场景转移只搬 1 个」的成因（三种可能：原生本就只给 1 / 下游吃成 1 / 我们的 Alt 钳制泄漏）。
    //   只在「源堆有多件、但原生只给 1 件」时记录。
    //   跳过机器容器（每格 1 件是正常，不是缺陷）。
    private static void ProbeNonAltMarker(GameItem item, SlotMarker result)
    {
        try
        {
            if (!DiagOn) return;
            if (item == null || item.unitCount <= 1) return;
            if (result.numTransfer != 1) return;

            try { if (IsMachineContainer(result.inventory)) return; } catch { }

            string invId;
            try { invId = SafeId(result.inventory); } catch { invId = "?"; }
            LogOnce("nonalt." + Ptr(result.inventory).ToString("X") + "|" + SafeId(item), Level.Diag,
                $"[转场探针] 非Alt落点只给1件：item={SafeId(item)} ×{item.unitCount}，" +
                $"容器id={invId}，targetItem={SafeId(result.targetItem)}，numTransfer={result.numTransfer}",
                max: 1);
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
        }
    }

    // ── 作用域探针：列出实际遇到过的容器 ──
    //   补 槽位/所属/内含 三路信息，让用户直接抄成 slot:… / owner:… token。
    //   每个容器实例只打一行；上限 ScopeProbeMax 条。
    private static readonly HashSet<long> _scopeProbed = new HashSet<long>();
    private const int ScopeProbeMax = 64;

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
            LogOnce("scopeprobe", Level.Warn, $"[作用域探针] 输出失败: {e.Message}");
        }
    }

    // ── 柜台探针：把「合并落点容器 vs EmporiumEntry 各容器」并列打出 ──
    //   每个容器只打一次；上限 12 条。
    private static readonly HashSet<long> _counterProbed = new HashSet<long>();

    private static void DumpCounterProbeOnce(GameInventory slotInv)
    {
        try
        {
            if (!DiagOn) return;
            long key = Ptr(slotInv);
            if (_counterProbed.Count >= 12) return;
            if (!_counterProbed.Add(key)) return;

            var entry = EmporiumEntry.Instance;
            if (entry == null)
            {
                Log.Msg("[柜台探针] 本次合并落点容器 = " + Describe(slotInv) +
                        "（EmporiumEntry.Instance = null，非交易场景）");
                return;
            }

            Log.Msg("[柜台探针] 本次合并落点容器 = " + DescribeContainer(slotInv) +
                    "  || EmporiumEntry 容器：" +
                    "invElement=" + DescribeContainer(TryGet(() => entry.invElement)) +
                    " / showcaseElement=" + DescribeContainer(TryGet(() => entry.showcaseElement)) +
                    " / frontInvinv=" + DescribeContainer(TryGet(() => entry.frontInvinvElement)) +
                    " / backInvinv=" + DescribeContainer(TryGet(() => entry.backInvinvElement)) +
                    " / backCounter=" + DescribeContainer(TryGet(() => entry.backInvinvElementCounter)) +
                    " / soldElement=" + DescribeContainer(TryGet(() => entry.soldElement)) +
                    " / hiddenElement=" + DescribeContainer(TryGet(() => entry.hiddenElement)) +
                    " / bazarLeft=" + DescribeContainer(TryGet(() => entry.bazarLeftinvElement)));
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("counterprobe", Level.Warn, $"[柜台探针] 输出失败: {e.Message}");
        }
    }

    // ── 修饰键探针：一次性打印三个 handler 的实际键位 ──
    //   key / quickTransferKey 是 prefab 的 [SerializeField] 数据，静态不可见。
    private static bool _modifierKeyDumped;

    private static void ModifierKeyProbeTick()
    {
        if (!DiagOn) return;
        if (_modifierKeyDumped) return;
        try
        {
            var q = TryGet(() => ItemQuickTransferHandler.current);
            var a = TryGet(() => ItemQuickTransferAltHandler.current);
            var a1 = TryGet(() => ItemQuickTransferAltHandler1.current);
            if (q == null && a == null && a1 == null) return;
            _modifierKeyDumped = true;

            string sQ = q == null ? "缺失" : $"key={q.key}, quickTransferKey={q.quickTransferKey}";
            string sA = a == null ? "缺失" : $"key={a.key}, quickTransferKey={a.quickTransferKey}";
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
            _modifierKeyDumped = true;
            LogOnce("modifierkey", Level.Warn, $"[修饰键] 键位探针输出失败: {e.Message}");
        }
    }
}
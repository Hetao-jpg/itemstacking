using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Preferences;
using UnityEngine;

namespace ItemStacking;

// 合并管线：MayStack / GetStackSlot / TryAcceptOnce 三个挂点
//
// ★ 三挂点的位置关系
//   MayStack       —— 拖放落点判定（原生 EndDrag 堆叠分支的第一道门）
//   GetStackSlot   —— 构造堆叠落点 marker
//   TryAcceptOnce  —— 【所有合并路径的唯一公共瓶颈】
//                     （拖放 / Alt 快移 / Ctrl 快移 / GraphUtils 系列都经此）
//
// ★ 关键机制结论（贯穿本文件）
//   在 TryAcceptOnce 层返 0 会打断 GraphUtils.TryAcceptAll 的逐格填充循环
//   ⇒ 「既不合并、也不再自动找空位」。拦截要尽量落在 MayStack 层（返 false = 导流空槽）。
//
// ★ 详细设计见 README.md「合并决策单一源」「机器容器保护」章节。
public partial class Main
{
    // ── 闸门 1：MayStack 放行同名物品 ──
    //   原生 MayStack 对两件物品的 mayStackItemFunc 做 InvokeAllReduce ——
    //   原版从未设过该委托 ⇒ 恒 false（拖到占用格被拒）。
    //   委托不能注入（il2cpp DynamicCall 会 TargetException）⇒ 直接 Prefix 放行同名。
    [HarmonyPatch(typeof(GameItem), nameof(GameItem.MayStack))]
    private static class Patch_MayStack
    {
        private static bool Prefix(GameItem __instance, GameItem otherItem, ref bool __result)
        {
            try
            {
                if (!StackEnabled)
                {
                    LogOnce("ms.skip.off", Level.Diag, "MayStack 跳过（mod 关闭）", max: 1);
                    return true;
                }
                if (otherItem == null || __instance == otherItem)
                {
                    LogOnce("ms.skip.self", Level.Diag, "MayStack 跳过（null 或同一物品）", max: 3);
                    return true;
                }

                // TryFillEmptySlots 强制执行期间恒返 false —— 让原生搜索跳过「叠加优先」
                // 分支，落到「找空格」分支，真正找到空槽。
                if (ForceNoMerge)
                {
                    __result = false;
                    return false;
                }

                if (UseMergePolicy)
                {
                    var instInvNew = TryGet(() => __instance.parentInventory);
                    if (DiagOn) ProbeScopeInventory(instInvNew, "落点堆所在(MayStack)");
                    var ctxNew = new MergeContext(instInvNew, __instance, otherItem);

                    // ① 导流优先（Alt 快移 / 配对）→ 返回 false 让原生找空格。
                    if (ShouldSteerToEmptySlot(ctxNew))
                    {
                        LogOnce("steer." + SafeId(__instance) + "/" + SafeId(otherItem), Level.Diag,
                            $"[导流] MayStack 返回 false（导流：让原生找空格）：" +
                            $"{__instance.identifier} / {otherItem.identifier}");
                        __result = false;
                        return false;
                    }

                    // ② 决策：MergePolicy 定夺。
                    var verdict = Evaluate(ctxNew, out var reasonNew);
                    if (verdict == MergeVerdict.Deny)
                    {
                        // 按【物品对 + 原因】限流：原生每次悬停调 2 次 MayStack。
                        LogOnce("deny.ms." + SafeId(__instance) + "/" + SafeId(otherItem) + "|" + reasonNew,
                            Level.Diag,
                            $"MayStack 拦截（MergePolicy·{reasonNew}）：{__instance.identifier} / {otherItem.identifier}",
                            max: 3);
                        __result = false;
                        return false;
                    }
                    // Allow → 同名放行。
                    var idAn = __instance.identifier;
                    var idBn = otherItem.identifier;
                    if (!string.IsNullOrEmpty(idAn) &&
                        string.Equals(idAn, idBn, StringComparison.Ordinal))
                    {
                        LogOnce("maystack.ok." + idAn, Level.Diag, $"MayStack 放行（同名）：{idAn}", max: 3);
                        __result = true;
                        return false;
                    }
                    LogOnce("maystack.diff." + SafeId(__instance) + "/" + SafeId(otherItem), Level.Diag,
                        $"MayStack 走原生（不同名）：{idAn} / {idBn}", max: 3);
                    return true;
                }

                // 回滚路径（cfg z9_UseMergePolicy=false）。
                if (ForceNoMerge)
                {
                    __result = false;
                    return false;
                }
                return LegacyMayStack(__instance, otherItem, ref __result);
            }
            catch (Exception e)
            {
                Log.Error($"MayStack prefix 异常: {e}");
                return true;
            }
        }
    }

    // ── 闸门 2：GetStackSlot 托管复刻（跳过 mayStackItemFunc 检查） ──
    //   原生逻辑：m = parentInventory.GetIncompleteItemInventorySlot(dragged)
    //             → m.item = dragged; m.targetItem = this
    //             → m2 = 同上再取一个；m.numTransfer = this.MaxNumAdd(m2)
    [HarmonyPatch(typeof(GameItem), nameof(GameItem.GetStackSlot))]
    private static class Patch_GetStackSlot
    {
        private static bool Prefix(GameItem __instance, GameItem itemToBeStacked, ref SlotMarker __result)
        {
            try
            {
                if (!StackEnabled) return true;
                if (itemToBeStacked == null || __instance == itemToBeStacked) return true;

                // 与 MayStack / TryAcceptOnce 共用同一决策源（纵深防御）。
                if (UseMergePolicy)
                {
                    var slotInvNew = TryGet(() => __instance.parentInventory);
                    if (DiagOn) ProbeScopeInventory(slotInvNew, "落点堆所在(GetStackSlot)");
                    var ctxNew = new MergeContext(slotInvNew, __instance, itemToBeStacked);
                    var verdict = Evaluate(ctxNew, out var reasonNew);
                    if (verdict == MergeVerdict.Deny)
                    {
                        LogOnce("deny.gss." + SafeId(__instance) + "/" + SafeId(itemToBeStacked) + "|" + reasonNew,
                            Level.Diag,
                            $"GetStackSlot 拦截（MergePolicy·{reasonNew}）：{__instance.identifier} / {itemToBeStacked.identifier}",
                            max: 3);
                        __result = null;
                        return false;
                    }
                    var idAn = __instance.identifier;
                    var idBn = itemToBeStacked.identifier;
                    if (!string.IsNullOrEmpty(idAn) &&
                        string.Equals(idAn, idBn, StringComparison.Ordinal))
                    {
                        // 同名 → 落到下方共用段构造 marker。
                    }
                    else
                    {
                        return true;   // 不同名 / 空 id 交给原生（返回 null，不堆叠）
                    }
                }
                else
                {
                    var legacyRet = LegacyGetStackSlot(__instance, itemToBeStacked, ref __result);
                    if (legacyRet.HasValue) return legacyRet.Value;
                }

                // ── 共用段：构造堆叠落点 marker ──
                var inv = __instance.parentInventory;
                ProbeScopeInventory(inv, "落点堆所在(GetStackSlot)");
                if (inv == null) { __result = null; return false; }

                var m = inv.GetIncompleteItemInventorySlot(itemToBeStacked);
                if (m == null) { __result = null; return false; }
                m.item = itemToBeStacked;
                m.targetItem = __instance;

                var m2 = inv.GetIncompleteItemInventorySlot(itemToBeStacked);
                if (m2 != null)
                {
                    m2.targetItem = __instance;
                    m.numTransfer = __instance.MaxNumAdd(m2);   // MaxNumAdd postfix 会抬到新上限
                }
                else
                {
                    m.numTransfer = int.MaxValue;               // 原生同款兜底
                }

                LogOnce("gss.ok." + SafeId(__instance), Level.Diag,
                    $"GetStackSlot {__instance.identifier}：numTransfer={m.numTransfer}（m2={(m2 != null ? "有" : "无")}）", max: 3);
                __result = m;
                return false;
            }
            catch (Exception e)
            {
                Log.Error($"GetStackSlot prefix 异常: {e}");
                return true;
            }
        }
    }

    // ── 闸门 3：TryAcceptOnce（全路径公共瓶颈） ──
    //   执行顺序显式化，分四阶段：
    //     阶段 -1 = Alt→Ctrl 转发（单件 / 禁并 / 优先空槽）
    //     阶段  0 = 机器容器（禁并 + 每格 1 件）
    //     阶段  1 = Ctrl 快移合并闸门（数据保护类硬拒 / 便利类导流空槽）
    //     阶段  2 = 观测直通（仅 DiagLog=true，永不改 __result）
    //
    //   ★ 本补丁【不返 0 打断循环】（0.10.1 结论）：真正的拦截在 MayStack / GetStackSlot 层。
    //     详见 README.md「合并管线」章节。

    // 自搬运重入深度：TryFillEmptySlots 内部自调用 TryAcceptOnce 时防无限递归。
    [ThreadStatic] private static int _selfAcceptDepth;

    // 强制「禁止合并」作用域：让 Patch_MayStack 恒返 false，
    // 使原生 TryFindOneValidInventorySlot 跳过「叠加优先」、落到「找空格」分支。
    [ThreadStatic] private static int _forceNoMergeScope;
    internal static bool ForceNoMerge => _forceNoMergeScope > 0;

    [HarmonyPatch(typeof(SlotMarker), nameof(SlotMarker.TryAcceptOnce), new[] { typeof(int) })]
    private static class Patch_SlotMarkerTryAcceptOnce
    {
        // 便利类（容器白/黑名单）→ 导流空槽；数据保护类 → 硬拒。
        private static bool IsConvenienceReason(string reason)
        {
            if (string.IsNullOrEmpty(reason)) return false;
            return reason.StartsWith("容器白名单", StringComparison.Ordinal)
                || reason.StartsWith("容器黑名单", StringComparison.Ordinal);
        }

        private static bool Prefix(SlotMarker __instance, object[] __args, ref int __result)
        {
            try
            {
                if (_selfAcceptDepth > 0) return true;   // 自搬运 → 直通原生，防递归

                // ══════════════════════════════════════════════════════════
                // 阶段 -1：Alt→Ctrl 转发作用域（只搬 N 件 / 禁并 / 优先空槽）
                //   为什么必须在这一层：原生 Ctrl 落点搜索是「叠加优先」，
                //   目标有同名堆时必然合并，与「只移 1 件、独立放置」相反。
                //   只对 _altRelaySingleScope 生效，普通拖放完全不受影响。
                // ══════════════════════════════════════════════════════════
                if (_altRelaySingleScope > 0 && StackEnabled)
                {
                    var rsrc = __instance.item;
                    int rInv = ReadAmountArg(__args);
                    bool isMachine = false;
                    try { isMachine = IsMachineContainer(__instance.inventory); } catch { }
                    if (!isMachine && rsrc != null)
                    {
                        int want = AltQuickTransferAmount;

                        // ① 合并候选 → 拒绝合并、改投空槽（只铺 want 件）
                        if (__instance.targetItem != null)
                        {
                            int moved = TryFillEmptySlots(__instance, rsrc, want);
                            if (moved > 0)
                            {
                                __result = moved;
                                LogOnce("altrelay.spread." + SafeId(rsrc) + "@" + Ptr(__instance.inventory).ToString("X"),
                                    Level.Diag,
                                    $"[Alt 单件·改投空槽] {SafeId(rsrc)} ×{moved}（请求 {want}，" +
                                    $"已忽略合并候选 {SafeId(__instance.targetItem)}；容器 {SafeId(__instance.inventory)}）",
                                    max: 3);
                                return false;
                            }
                            __result = 0;
                            LogOnce("altrelay.nofree." + SafeId(rsrc) + "@" + Ptr(__instance.inventory).ToString("X"),
                                Level.Diag,
                                $"[Alt 单件·拒绝合并] {SafeId(rsrc)}：目标无空槽且不允许合并 ⇒ 本次拒绝" +
                                $"（物品留在源处；容器 {SafeId(__instance.inventory)}）",
                                max: 3);
                            return false;
                        }

                        // ② 空槽但请求 > want → 钳到 want（原生 GetSplitItem 自己拆）
                        if (rInv < 0 || rInv > want)
                        {
                            if (__args != null && __args.Length > 0)
                                __args[0] = want;
                            LogOnce("altrelay.clamp." + Ptr(__instance.inventory).ToString("X"),
                                Level.Diag,
                                $"[Alt 单件·数量钳制] 请求 {rInv} → {want}（容器 {SafeId(__instance.inventory)}）",
                                max: 3);
                        }
                    }
                }

                // ══════════════════════════════════════════════════════════
                // 阶段 0：机器容器「全程不允许堆叠 + 每格 1 件」
                //   下沉到公共瓶颈，覆盖拖放 / Ctrl 拖动 / Ctrl 点击快移 / Alt 快移全部入口。
                //   cfg MachineOnePerSlot 可整体回滚本层。
                //   详见 README.md「机器容器保护」。
                // ══════════════════════════════════════════════════════════
                if (StackEnabled && MachineOnePerSlot)
                {
                    var minv = __instance.inventory;
                    var msrc = __instance.item;
                    int amount = ReadAmountArg(__args);

                    // 成本守门：只有「合并候选」或「请求 >1 件」时才值得查机器判据。
                    bool worthChecking = __instance.targetItem != null
                                      || amount < 0 || amount > 1
                                      || (msrc != null && msrc.unitCount > 1);

                    if (worthChecking && minv != null && IsMachineContainer(minv))
                    {
                        if (msrc != null)
                        {
                            // ① 合并候选 → 禁并，改投空槽
                            if (__instance.targetItem != null)
                            {
                                int moved = TryFillEmptySlots(__instance, msrc);
                                if (moved > 0)
                                {
                                    __result = moved;
                                    LogOnce("machine.fill." + SafeId(msrc) + "@" + Ptr(minv).ToString("X"),
                                        Level.Diag,
                                        $"[机器容器·禁并改投空槽] {SafeId(msrc)} 放入空槽 ×{moved}" +
                                        $"（已忽略合并候选 {SafeId(__instance.targetItem)}；每格 1 件，容器 {SafeId(minv)}）",
                                        max: 3);
                                    return false;
                                }
                                __result = 0;
                                LogOnce("machine.nofree." + SafeId(msrc) + "@" + Ptr(minv).ToString("X"),
                                    Level.Diag,
                                    $"[机器容器·拒绝放置] {SafeId(msrc)} ×{msrc.unitCount}：" +
                                    $"机器内无空槽且不允许堆叠 ⇒ 本次拒绝（物品留在源处，容器 {SafeId(minv)}）",
                                    max: 3);
                                return false;
                            }

                            // ② 空槽 —— 请求 >1 件时先尝试「逐格铺开」
                            if (amount < 0 || amount > 1)
                            {
                                if (msrc.unitCount > 1)
                                {
                                    int moved = TryFillEmptySlots(__instance, msrc,
                                        amount > 0 ? amount : 0);
                                    if (moved > 0)
                                    {
                                        __result = moved;
                                        LogOnce("machine.spread." + SafeId(msrc) + "@" + Ptr(minv).ToString("X"),
                                            Level.Diag,
                                            $"[机器容器·逐格铺开] {SafeId(msrc)} 放入空槽 ×{moved}" +
                                            $"（每格 1 件，容器 {SafeId(minv)}）", max: 3);
                                        return false;
                                    }
                                    // 一格都没铺进去 → 退回原生（下面把本次数量降到 1）
                                }
                                if (__args != null && __args.Length > 0)
                                {
                                    __args[0] = 1;
                                    LogOnce("machine.amount." + Ptr(minv).ToString("X"), Level.Diag,
                                        $"[机器容器·数量钳 1] 请求 {amount} 件 → 1 件（空槽放置，容器 {SafeId(minv)}）",
                                        max: 3);
                                }
                            }
                        }
                    }
                }

                // ══════════════════════════════════════════════════════════
                // 阶段 1：Ctrl 快移合并闸门（数据保护类硬拒 / 便利类导流空槽）
                //   动因：Ctrl 快移【不走】拖放路径，也【不走】Alt 落点改写，
                //   直接调 GraphUtils.TryAcceptOnce（单次）→ 合并绕过 MergePolicy 全部闸门。
                //   回滚：cfg CtrlTransferMergeGate（默认 true）。
                // ══════════════════════════════════════════════════════════
                if (CtrlTransferScope && StackEnabled)
                {
                    var target = __instance.targetItem;
                    if (target != null)
                    {
                        var source = __instance.item;
                        if (source != null)
                        {
                            var reason = GetMergeGateReason(__instance, target, source);
                            if (reason != null)
                            {
                                // 便利类 → 尝试把物品放进空槽（每格 1 件，不合并）
                                if (IsConvenienceReason(reason))
                                {
                                    int moved = TryFillEmptySlots(__instance, source, 0);
                                    if (moved > 0)
                                    {
                                        __result = moved;
                                        LogOnce("ctrl.steer." + SafeId(source) + "/" + SafeId(target) + "|" + reason,
                                            Level.Diag,
                                            $"[Ctrl 快移·导流空槽] 合并被「{reason}」拒绝，已放入空槽 ×{moved}：" +
                                            $"源 {SafeId(source)} ×{source.unitCount}（容器 {SafeId(__instance.inventory)}）",
                                            max: 3);
                                        return false;
                                    }
                                    // 空槽也放不下 → 返 0
                                }
                                // 数据保护类 或 便利类无空槽 → 返 0 硬拒（物品留源）
                                __result = 0;
                                LogOnce("deny.ctrl." + SafeId(source) + "/" + SafeId(target) + "|" + reason,
                                    Level.Diag,
                                    $"[Ctrl 快移·拦截] 合并候选被判定为「{reason}」→ 拒绝本次合并（物品留在源处）：" +
                                    $"源 {SafeId(source)} ×{source.unitCount} → 目标堆 {SafeId(target)} ×{target.unitCount}" +
                                    $"（容器 {SafeId(__instance.inventory)}）", max: 3);
                                return false;
                            }
                        }
                    }
                    // 允许合并 → 继续阶段 2
                }

                // 阶段 2：观测直通（仅 DiagLog=true，永不改 __result）
                if (DiagOn && StackEnabled)
                {
                    var target = __instance.targetItem;
                    var source = __instance.item;
                    if (source != null)
                    {
                        if (target != null)
                        {
                            var reason = GetMergeGateReason(__instance, target, source);
                            if (reason != null)
                            {
                                LogOnce("observe." + SafeId(source) + "/" + SafeId(target), Level.Diag,
                                    $"[直通观测] 合并候选被判定为「{reason}」——本层不拦截（由 MayStack 导流）：" +
                                    $"源 {SafeId(source)} ×{source.unitCount} → 目标堆 {SafeId(target)} ×{target.unitCount}" +
                                    $"（容器 {SafeId(__instance.inventory)}）", max: 3);
                            }
                        }
                        else if (source.unitCount > 1)
                        {
                            // 空槽【放置】观测：区分「本层收口」vs「绕过本层」。
                            var inv = __instance.inventory;
                            LogOnce("observe.place." + SafeId(source) + "@" + Ptr(inv).ToString("X"), Level.Diag,
                                $"[落点观测·放置] 整堆入【空槽】候选：{SafeId(source)} ×{source.unitCount}" +
                                $"，容器={DescribeContainer(inv)}" +
                                $"，机器判据={IsMachineContainer(inv)}" +
                                $"，marker.numTransfer={__instance.numTransfer}", max: 5);
                        }
                    }
                }

                return true;   // 直通原生
            }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                LogOnce("slotaccept", Level.Warn, $"TryAcceptOnce 补丁异常，本次放行: {e.Message}");
                return true;
            }
        }
    }

    // ── 机器容器「改投空槽」 ──
    //   语义与原生 GraphUtils.InsertToActiveContainer 同构：按件循环 + 每件重新找槽。
    //   找槽经 TryFindOneValidInventorySlot ⇒ 我们的机器 Deny 让它跳过合并候选
    //   ⇒ 天然「每格 1 件 + 逐格铺开」。
    //
    //   ★ 用基类 GameInventory（机器内库可能是裸 GameInventory，不是 GameGridInventory）。
    //   ★ 只投 targetItem == null 的空槽；每次只接受 1 件 ⇒ 机器内不会出现 unitCount > 1。
    //   ★ 连续 8 次「拿到已占用槽」即判定没有新空槽、退出（防 finder 反复返回同一槽）。
    //   ★ 重入：内部自调用 TryAcceptOnce，靠 _selfAcceptDepth 让本补丁 Prefix 直通原生。
    //
    //   maxCount：调用方可指定"最多只铺 N 件"（Alt 转发场景传 AltQuickTransferAmount；
    //             机器容器阶段 0 传本次请求量）。<=0 表示沿用 min(unitCount, 64)。
    private static int TryFillEmptySlots(SlotMarker origin, GameItem source, int maxCount = 0)
    {
        try
        {
            if (origin == null || source == null) return 0;
            var inv = origin.inventory;
            if (inv == null) return 0;

            int moved = 0;
            int want = maxCount > 0
                ? Math.Min(source.unitCount, maxCount)
                : Math.Min(source.unitCount, 64);
            int guard = 0;
            int occupiedSeen = 0;
            int consecutiveOccupied = 0;
            int nullAt = -1;              // 第几次扫描 finder 直接返回 null（= 该容器确无空槽）
            SlotMarker lastOccupied = null;
            _selfAcceptDepth++;
            _forceNoMergeScope++;   // 强制不合并 —— 让 finder 跳过合并候选，真正找到空槽
            try
            {
                while (moved < want && guard++ < 128)
                {
                    SlotMarker m;
                    try { m = inv.TryFindOneValidInventorySlot(source); } catch { break; }
                    if (m == null) { nullAt = guard; break; }

                    if (m.targetItem != null || m.numTransfer < 1)
                    {
                        occupiedSeen++;
                        lastOccupied = m;
                        if (++consecutiveOccupied >= 8) break;
                        continue;
                    }
                    consecutiveOccupied = 0;

                    m.item = source;                   // 原生 GetStackSlot 同款：finder 未必回填 item
                    int n;
                    try { n = m.TryAcceptOnce(1); } catch { break; }
                    if (n > 0) moved += n;
                    else break;
                }
            }
            finally
            {
                _forceNoMergeScope--;
                _selfAcceptDepth--;
            }

            // 探针（DiagLog 门控，只读）：一次调用是「本来就没空槽」还是「找槽失败」，一行区分。
            LogOnce("machine.scan." + Ptr(inv).ToString("X"), Level.Diag,
                $"[机器容器·找空槽] 容器={DescribeContainer(inv)}；请求 {want} 件 → 实移 {moved} 件、" +
                $"扫描 {guard} 次（占用/放不进 {occupiedSeen} 次" +
                (lastOccupied != null
                    ? $"，末次占用槽 targetItem={SafeId(lastOccupied.targetItem)}, numTransfer={lastOccupied.numTransfer}"
                    : "") +
                (nullAt >= 0 ? $"，第 {nullAt} 次 finder 返回 null" : "") + "）", max: 3);

            return moved;
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("fill-slots", Level.Warn, $"[机器容器] 改投空槽异常，本次退回原逻辑: {e.Message}");
            return 0;
        }
    }

    // 读 SlotMarker.TryAcceptOnce(int amount = -1) 的本次实参。
    //   用 object[] __args 而非按名注入 —— 形参名是 interop 生成物、随版本可能变。
    //   取不到返回 -1（原生语义「按 numTransfer 决定」）。
    private static int ReadAmountArg(object[] args)
    {
        try
        {
            if (args == null || args.Length == 0) return -1;
            var v = args[0];
            if (v is int i) return i;
            if (v is short s) return s;
            return -1;
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            return -1;
        }
    }
}
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
    // ══════════════════════════════════════════════════════════════════
    // 合并管线：MayStack / GetStackSlot / TryAcceptOnce 三个挂点
    // ══════════════════════════════════════════════════════════════════
    // ───────────────────────────────────────────────────────────────────────
    // 0.2.0 核心：放开原生拖放堆叠（EndDrag 已有完整合并机器，只是被闸住）
    //
    // 原生取证（GameAssembly 反汇编实锤）：
    //   ItemMouseDragHandler.EndDrag (0x7BA010) 落点分支顺序：
    //     1) 普通放置 TryFindOneValidInventorySlot → TryAcceptOnce
    //     2) ★ 堆叠分支 lastItem.MayStack(dragged)
    //            → lastItem.GetStackSlot(dragged)
    //            → slot.TryAcceptOnce(数量)          ← 整套合并机器都在
    //     3) 物品交互 MayTarget/CanTarget/Target
    //   MayStack (0x799120) = 对两件物品的 mayStackItemFunc(0x20) 做
    //     InvokeAllReduce(OR) —— 原版从没给物品设过这个委托 → 恒 false
    //     → 拖到占用格被拒绝。委托不能注入（mono 委托被 il2cpp 反射
    //       DynamicInvoke 调用会 TargetException，0.1.0 实测），
    //     所以直接 Prefix MayStack 放行同名物品。
    //   GetStackSlot (0x797720) 同样先查两边 mayStackItemFunc → 也得 Prefix，
    //     托管复刻原生逻辑（vtable 校准：0x208 = Slot 13 =
    //     GetIncompleteItemInventorySlot，0x268/0x278 = UncheckedAccept/Expel）。
    //   TryAcceptOnce (0x7B0020)：n = min(数量, numTransfer, MaxNumRemove(dragged))，
    //     n >= item.unitCount 整体移动，否则 GetSplitItem 拆分，
    //     最后 SlotMarker.StackItemUnchecked(item, targetItem, n) 真正合并。
    // ───────────────────────────────────────────────────────────────────────
    // ── 闸门 1：MayStack 放行同名物品 ──
    [HarmonyPatch(typeof(GameItem), nameof(GameItem.MayStack))]
    private static class Patch_MayStack
    {
        private static bool Prefix(GameItem __instance, GameItem otherItem, ref bool __result)
        {
            try
            {
                if (!StackEnabled) { Diag("MayStack 跳过（mod 关闭）"); return true; }
                if (otherItem == null || __instance == otherItem)
                { Diag($"MayStack 跳过（null 或同一物品）"); return true; }

                // ══════════════════════════════════════════════════════════════
                //  ★ 0.10.0（阶段1）合并决策单一源 + （阶段3）决策/导流解耦。
                //    顺序：先问 Steerer（是否导流）→ 再问 MergePolicy（是否拦截）。
                //    导流 = 返回 false 让原生走"找空格"分支（不是"禁止合并"）；
                //    拦截 = MergePolicy Deny。
                //    回滚：z9_UseMergePolicy=false 时跳过本块，走下方 0.9.9 旧内联判据。
                // ══════════════════════════════════════════════════════════════
                if (UseMergePolicy)
                {
                    var instInvNew = TryGet(() => __instance.parentInventory);
                    if (DiagOn) ProbeScopeInventory(instInvNew, "落点堆所在(MayStack)");   // §七 1.4：DiagLog 关时不进展针
                    var ctxNew = new MergeContext(instInvNew, __instance, otherItem);

                    // ① 导流优先：需要让原生找空格（Alt 快移 / 柜台）→ 返回 false。
                    //    这是"先别合并"，不是"禁止合并"——语义与 MergePolicy.Deny 区分开。
                    if (ShouldSteerToEmptySlot(ctxNew))
                    {
                        DiagOnce("steer." + SafeId(__instance) + "/" + SafeId(otherItem),
                            $"[导流] MayStack 返回 false（导流：让原生找空格）：" +
                            $"{__instance.identifier} / {otherItem.identifier}");
                        __result = false;
                        return false;
                    }

                    // ② 决策：MergePolicy 定夺是否拦截。
                    var verdict = Evaluate(ctxNew, out var reasonNew);
                    if (verdict == MergeVerdict.Deny)
                    {
                        // ★ 拦截必须始终输出（限流只针对"放行/成功"类）——实测要抓的正是拦截原因。
                        Diag($"MayStack 拦截（MergePolicy·{reasonNew}）：{__instance.identifier} / {otherItem.identifier}");
                        __result = false;
                        return false;
                    }
                    // Allow → 继续下方同名判定。
                    var idAn = __instance.identifier;
                    var idBn = otherItem.identifier;
                    if (!string.IsNullOrEmpty(idAn) &&
                        string.Equals(idAn, idBn, StringComparison.Ordinal))
                    {
                        DiagRate("maystack.ok." + idAn, 3, $"MayStack 放行（同名）：{idAn}");
                        __result = true;
                        return false;             // 放行同名，跳过原生委托检查
                    }
                    DiagRate("maystack.diff." + SafeId(__instance) + "/" + SafeId(otherItem), 3,
                        $"MayStack 走原生（不同名）：{idAn} / {idBn}");

                    return true;                  // 不同名 / 空 id 走原生（仍 false）
                }

                // ══════════ 0.9.9 旧内联判据（回滚用，保留未删）══════════
                //   ★ T2.1 variant B（2026-10-12）：整段原样搬入 MergePolicy.Legacy.cs 的
                //     LegacyMayStack（参数名沿用 __instance/otherItem/__result，可与 git 历史逐行对照）。
                //     判定顺序 / 日志文案 / WarnOnce key 均未改；回滚 = cfg z9_UseMergePolicy=false。
                //     （上方 if (UseMergePolicy) 的所有路径都已 return，故无需 else。）
                return LegacyMayStack(__instance, otherItem, ref __result);
            }
            catch (Exception e)
            {
                Log.Error($"MayStack prefix 异常: {e}");
                return true;
            }
        }
    }

    // ── 闸门 2：GetStackSlot 托管复刻（跳过 mayStackItemFunc 检查）──
    // 原生逻辑（0x797720 反汇编）：
    //   校验 → m = parentInventory.GetIncompleteItemInventorySlot(dragged)
    //        → m.item = dragged; m.targetItem = this
    //        → m2 = 同上再取一个; m2.targetItem = this
    //        → m.numTransfer = this.MaxNumAdd(m2)
    [HarmonyPatch(typeof(GameItem), nameof(GameItem.GetStackSlot))]
    private static class Patch_GetStackSlot
    {
        private static bool Prefix(GameItem __instance, GameItem itemToBeStacked, ref SlotMarker __result)
        {
            try
            {
                if (!StackEnabled) return true;
                if (itemToBeStacked == null || __instance == itemToBeStacked) return true;

                // ★ 0.10.0（阶段1）：与 MayStack / TryAcceptOnce 共用同一个决策源。
                //   纵深防御：MayStack 若已放行，本层用同一 Evaluate 再核一遍，结论天然一致。
                if (UseMergePolicy)
                {
                    var slotInvNew = TryGet(() => __instance.parentInventory);
                    if (DiagOn) ProbeScopeInventory(slotInvNew, "落点堆所在(GetStackSlot)");   // §七 1.4
                    var ctxNew = new MergeContext(slotInvNew, __instance, itemToBeStacked);
                    var verdict = Evaluate(ctxNew, out var reasonNew);
                    if (verdict == MergeVerdict.Deny)
                    {
                        // ★ 拦截始终输出。
                        Diag($"GetStackSlot 拦截（MergePolicy·{reasonNew}）：{__instance.identifier} / {itemToBeStacked.identifier}");
                        __result = null;
                        return false;
                    }
                    // 同名检查（Allow/Unknown 后仍需确认同名才构造堆叠落点）。
                    var idAn = __instance.identifier;
                    var idBn = itemToBeStacked.identifier;
                    if (!string.IsNullOrEmpty(idAn) &&
                        string.Equals(idAn, idBn, StringComparison.Ordinal))
                    {
                        // 落到下方共用段：构造 marker。
                    }
                    else
                    {
                        return true;              // 不同名 / 空 id 交给原生（返回 null，不堆叠）
                    }
                }
                else
                {
                    // ══════════ 0.9.9 旧内联判据（回滚用，保留未删）══════════
                    //   ★ T2.1 variant B（2026-10-12）：整段原样搬入 MergePolicy.Legacy.cs 的
                    //     LegacyGetStackSlot。返回 null = 同名非空，继续落到下方共用段构造 marker。
                    var legacyRet = LegacyGetStackSlot(__instance, itemToBeStacked, ref __result);
                    if (legacyRet.HasValue) return legacyRet.Value;
                }
                // ══════════ 共用段：构造堆叠落点 marker ══════════

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
                    m.numTransfer = int.MaxValue;               // 原生同款兜底 (0x7fffffff)
                }

                DiagRate("gss.ok." + SafeId(__instance), 3,
                    $"GetStackSlot {__instance.identifier}：numTransfer={m.numTransfer}（m2={(m2 != null ? "有" : "无")}）");
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

    // ═══════════════════════════════════════════════════════════════════════
    // 0.10.1 修复（问题2）：`TryAcceptOnce` 不再「硬拒（返 0）」—— 改为【纯观测直通】
    //
    // ── 历史（0.6.5 的原始对策，现已推翻）──
    //   0.6.5 把闸门下沉到 `SlotMarker.TryAcceptOnce`，命中即 `__result = 0; return false;`，
    //   意图是「所有合并路径的公共瓶颈」。当时假设「返 0 = 无处可放，上层会安全留在原处」。
    //
    // ── 为什么这个假设是错的（反汇编实锤，2026-10-11）──
    //   `GraphUtils.TryAcceptAll`(0x766650) 是**逐格填充循环**（最多 0x28 轮）：
    //       循环:
    //         slot = FindValidSlot();                 // 0x3fa0 → TryFindOneValidInventorySlot → MayStack
    //         if (slot == null) break;                // 0x1807666d1
    //         if (slot.numTransfer <= 0) break;       // 0x1807666d7
    //         n = SlotMarker.TryAcceptOnce(...);      // 0x1807b0020 ← 本挂点
    //         if (n == 0) break;                      // 0x1807666ea ★ 返 0 立即跳出
    //         accepted += n;
    //   ⇒ 我们「返 0」等于**当场打断整个填充循环**：既不合并，也**不会再去找空格**。
    //     玩家现象 = 「Ctrl 跨容器既不合并、也不再自动寻路快移到空位」（用户 2026-10-11 反馈）。
    //
    //   ★ `MayStack` = 0x799120 已被 `TryFindOneValidInventorySlot`(0x77BB5C) 调用，
    //     其 false 分支是 `je loop_top`（**跳过该候选 → 继续扫下一格 → 最终落到空槽分支**）
    //     —— 这才是原生正确的「不合并但寻路」信号。
    //   ⇒ 决策必须在 `MayStack` 层做（导流），`TryAcceptOnce` 绝不能返 0 打断循环。
    //
    // ── 修复 ──
    //   本挂点**不再改写 __result、不再 return false**（= 完全直通原生）。
    //   仅保留一个可选观测：命中「本会被拒」的条件时打一条 Diag（不改行为）。
    //   真正的数据保护仍在 `MayStack` / `GetStackSlot`（同源 MergePolicy.Evaluate）
    //   —— 那两层返 false 会让原生**导流到空槽**，比「硬拒打断循环」既安全又可用。
    //
    //   ★ 与 0.9.9 柜台修复同源：柜台禁堆也是从「TryAcceptOnce 返 0」改为
    //     「MayStack 返 false 导流」。本次把该教训推广到全部判据。
    // ═══════════════════════════════════════════════════════════════════════

    [HarmonyPatch(typeof(SlotMarker), nameof(SlotMarker.TryAcceptOnce))]
    private static class Patch_SlotTryAcceptOnce
    {
        private static bool Prefix(SlotMarker __instance, ref int __result)
        {
            try
            {
                // ★ 0.10.1：无论判定结果如何，一律直通原生（不改 __result / 不 return false），
                //   以免触发上游 TryAcceptAll 的 break（问题2 根因）。
                if (!DiagOn) return true;

                if (!StackEnabled) return true;
                var target = __instance.targetItem;
                if (target == null) return true;      // 空槽 = 移动，非合并
                var source = __instance.item;
                if (source == null) return true;

                var reason = GetMergeGateReason(__instance, target, source);
                if (reason == null) return true;

                // 仅供排查：本次「若在 MayStack 层就会导流/拦截」的合并，在这里只记录不干预。
                DiagRate("observe." + SafeId(source) + "/" + SafeId(target), 3,
                    $"[直通观测] 合并候选被判定为「{reason}」——本层不拦截（由 MayStack 导流）：" +
                    $"源 {SafeId(source)} ×{source.unitCount} → 目标堆 {SafeId(target)} ×{target.unitCount}" +
                    $"（容器 {SafeId(__instance.inventory)}）");
                return true;
            }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                WarnOnce("slotaccept", $"直通观测异常，本次放行: {e.Message}");
                return true;
            }
        }
    }
}


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
                        LogOnce("steer." + SafeId(__instance) + "/" + SafeId(otherItem), Level.Diag,
                            $"[导流] MayStack 返回 false（导流：让原生找空格）：" +
                            $"{__instance.identifier} / {otherItem.identifier}");
                        __result = false;
                        return false;
                    }

                    // ② 决策：MergePolicy 定夺是否拦截。
                    var verdict = Evaluate(ctxNew, out var reasonNew);
                    if (verdict == MergeVerdict.Deny)
                    {
                        // ★ 0.14.0（用户定稿）：拦截日志按【物品对 + 原因】限流（max 3）——
                        //   原生每次悬停会调 2 次 MayStack，裸 Diag 导致同一行成对刷屏；
                        //   max 3 覆盖双调用且保留可观察性，换原因/换物品对重新计数。
                        LogOnce("deny.ms." + SafeId(__instance) + "/" + SafeId(otherItem) + "|" + reasonNew,
                            Level.Diag,
                            $"MayStack 拦截（MergePolicy·{reasonNew}）：{__instance.identifier} / {otherItem.identifier}",
                            max: 3);
                        __result = false;
                        return false;
                    }
                    // Allow → 继续下方同名判定。
                    var idAn = __instance.identifier;
                    var idBn = otherItem.identifier;
                    if (!string.IsNullOrEmpty(idAn) &&
                        string.Equals(idAn, idBn, StringComparison.Ordinal))
                    {
                        LogOnce("maystack.ok." + idAn, Level.Diag, $"MayStack 放行（同名）：{idAn}", max: 3);
                        __result = true;
                        return false;             // 放行同名，跳过原生委托检查
                    }
                    LogOnce("maystack.diff." + SafeId(__instance) + "/" + SafeId(otherItem), Level.Diag,
                        $"MayStack 走原生（不同名）：{idAn} / {idBn}", max: 3);

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
                        // ★ 0.14.0：与 MayStack 同策略 —— 按【物品对 + 原因】限流（max 3）。
                        LogOnce("deny.gss." + SafeId(__instance) + "/" + SafeId(itemToBeStacked) + "|" + reasonNew,
                            Level.Diag,
                            $"GetStackSlot 拦截（MergePolicy·{reasonNew}）：{__instance.identifier} / {itemToBeStacked.identifier}",
                            max: 3);
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
    //
    // ── H3 合并（2026-10-12）──
    //   原先本观测补丁与 StackPatches.AltTransfer.cs 的 `Patch_CtrlMergeGateOnSlotMarker`
    //   各自挂在同一方法上（同方法双 Prefix）。Harmony 按注册顺序执行 Prefix，
    //   任一 return false 则跳过原方法 ⇒ 谁先跑是隐式依赖，改一个可能影响另一个。
    //   现合并为下方单一类，执行顺序显式化：
    //     阶段1 = Ctrl 快移合并闸门（仅 CtrlTransferScope 内；违禁合并返 0 硬拒）
    //     阶段2 = 观测直通（仅 DiagLog=true；永不改 __result）
    //     最后   = return true 直通原生
    //   ★ 与 Ctrl 闸门相关的完整设计（为何返 0 不置 null 等）见
    //     StackPatches.AltTransfer.cs 的 Patch_CtrlHandlerRelease 段注释。
    // ═══════════════════════════════════════════════════════════════════════

    // ★ 2026-10-13：自搬运重入深度。>0 时本补丁的 Prefix 立即直通原生 ——
    //   否则 TryFillEmptySlots 里自调用 SlotMarker.TryAcceptOnce 会无限递归。
    //   （放在 Main 层而非嵌套类内：嵌套类私有成员对 Main 不可见。）
    [ThreadStatic] private static int _selfAcceptDepth;

    // TryAcceptOnce(int) 的唯一补丁（H3：Ctrl 闸门 + 观测直通，顺序显式）。
    [HarmonyPatch(typeof(SlotMarker), nameof(SlotMarker.TryAcceptOnce), new[] { typeof(int) })]
    private static class Patch_SlotMarkerTryAcceptOnce
    {
        private static bool Prefix(SlotMarker __instance, object[] __args, ref int __result)
        {
            try
            {
                if (_selfAcceptDepth > 0) return true;   // 自搬运 → 直通原生，防递归

                // ══════════════════════════════════════════════════════════════
                // ★ 阶段0（2026-10-14 新增）：机器容器「全程不允许堆叠 + 每格 1 件」
                //   —— 下沉到公共瓶颈，【不限于快移作用域】。
                //
                //  ── 为什么必须下沉（用户 2026-10-14 实机反馈）──
                //   原实现的机器保护分成两层、各有缺口：
                //     · MergePolicy 的机器 Deny（MayStack 层）只对「找槽器」选出的**合并候选**生效；
                //     · MaxNumAdd 的钳 1 只改 numTransfer（容量），上游仍可能按整堆调用。
                //   于是出现两条漏网路径：
                //     ① Ctrl+鼠标左键快移：落点=机器里已有的同名堆 → 硬拒 → 整条快移失败
                //        （连 ×1 都进不去机器）——这是「只拦不导」；
                //     ② Ctrl+鼠标拖动整堆到机器图标/机器格：绕过 MayStack 候选筛选，
                //        整堆一次性进机器一格（机器「一槽一实例」前提被破坏）。
                //   `SlotMarker.TryAcceptOnce` 是【所有路径的唯一公共瓶颈】（44 处调用，
                //   含 EndDrag 兜底循环 / GraphUtils.TryAcceptOnce·TryAcceptAll·TryAcceptAllMid·
                //   InsertToActiveContainer / 快移三分支）⇒ 在这一层收口即可全覆盖。
                //
                //  ── 行为（只针对「落点容器是机器容器」）──
                //    ① 落点槽【已有物品】（合并候选）→ 禁止合并，改走 TryFillEmptySlots
                //       （逐格找空槽、每格 1 件）；填进去了就返回实移件数，
                //       机器已无空槽则返 0 硬拒（物品留源、不丢、不覆盖）。
                //    ② 落点槽【是空槽】且本次请求 >1 件 → 同样改走 TryFillEmptySlots
                //       （一次把整堆铺进机器所有空槽，符合用户「自动放在空格处，直到无空格」）；
                //       若一格都没铺进去（异常/取不到槽）→ 不硬拒，退回原生（见下"降到 1 件"）。
                //       ★ 无论走哪条，机器内永远不会出现 unitCount > 1。
                //
                //  ── 安全边界 ──
                //    · cfg `MachineOnePerSlot`（默认 true）可整体回滚本层；
                //    · 不改变「非机器容器」的任何行为（逐字节不变）；
                //    · ②的"退还原生"路径由 cfg `MachineOnePerSlot` + 本层的 amount 钳制
                //      共同保证「最多 1 件/次」——即使 TryFillEmptySlots 全失败也不会塞整堆；
                //    · 返 0 只在机器【确实无空槽】时发生（与用户要求一致：无空格才拒绝）。
                // ══════════════════════════════════════════════════════════════
                if (StackEnabled && MachineOnePerSlot)
                {
                    var minv = __instance.inventory;
                    var msrc = __instance.item;
                    int amount = ReadAmountArg(__args);

                    // ★ 成本守门（热路径）：只有「本次是合并候选」或「本次想放 >1 件」时，
                    //   才值得去查机器判据（IsMachineContainer 要上溯 GetParentItem 链）。
                    //   单件进空槽是机器唯一合法的用法，直接放过 —— 与原行为逐字节一致。
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
                                    int moved = TryFillEmptySlots(__instance, msrc);
                                    if (moved > 0)
                                    {
                                        __result = moved;
                                        LogOnce("machine.spread." + SafeId(msrc) + "@" + Ptr(minv).ToString("X"),
                                            Level.Diag,
                                            $"[机器容器·逐格铺开] {SafeId(msrc)} 放入空槽 ×{moved}" +
                                            $"（每格 1 件，容器 {SafeId(minv)}）", max: 3);
                                        return false;
                                    }
                                    // 一格都没铺进去 → 退回原生，但先把本次数量降到 1（下面这段）
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

                // ── 阶段1：Ctrl/Alt 快移作用域内的【其它】违禁合并闸门 ──
                //   （机器容器的合并已在阶段0 被接管，这里只剩容器/名单/状态等判据）
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
                                // ★ 违禁合并 → 返 0 硬拒（fail-safe：物品留在源容器，不丢、不覆盖）。
                                //   为什么不置 targetItem=null 导流：见 AltTransfer.cs 作用域段注释。
                                __result = 0;
                                // ★ 0.14.0：按【物品对 + 原因】限流（max 3），防连续快移同因刷屏。
                                LogOnce("deny.ctrl." + SafeId(source) + "/" + SafeId(target) + "|" + reason,
                                    Level.Diag,
                                    $"[Ctrl 快移·拦截] 合并候选被判定为「{reason}」→ 拒绝本次合并（物品留在源处）：" +
                                    $"源 {SafeId(source)} ×{source.unitCount} → 目标堆 {SafeId(target)} ×{target.unitCount}" +
                                    $"（容器 {SafeId(__instance.inventory)}）", max: 3);
                                return false;   // 硬拒：GraphUtils.TryAcceptOnce 单次调用返 0 ⇒ 物品留源
                            }
                        }
                    }
                    // 走到这里 = Ctrl 作用域内但允许合并 → 继续阶段2（不 return）
                }

                // ── 阶段2：观测直通（仅 DiagLog=true；任何情况下不改 __result / 不 return false）──
                //   ★ 0.10.1：一律直通原生，以免触发上游 TryAcceptAll 的 break（问题2 根因）。
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
                                // 仅供排查：本次「若在 MayStack 层就会导流/拦截」的合并，在这里只记录不干预。
                                LogOnce("observe." + SafeId(source) + "/" + SafeId(target), Level.Diag,
                                    $"[直通观测] 合并候选被判定为「{reason}」——本层不拦截（由 MayStack 导流）：" +
                                    $"源 {SafeId(source)} ×{source.unitCount} → 目标堆 {SafeId(target)} ×{target.unitCount}" +
                                    $"（容器 {SafeId(__instance.inventory)}）", max: 3);
                            }
                        }
                        else if (source.unitCount > 1)
                        {
                            // ★★ 2026-10-13 新增：空槽【放置】观测（DiagLog 门控，只读、不改行为）。
                            //   动因（用户实机反馈）：「Ctrl+点击快移」与「Ctrl+拖动整堆」都能把整堆
                            //   塞进熔炉的一个格子里，绕过机器容器判据。本行用于一次性定死链路：
                            //     · 复现时【能看到】本行 ⇒ 该路径确实经 `SlotMarker.TryAcceptOnce`
                            //       ⇒ 在此层钳量即可覆盖（含 Ctrl 路径）；
                            //     · 复现时【看不到】本行 ⇒ 该路径绕过本方法（很可能走
                            //       GraphUtils.InsertToActiveContainer / TryAcceptAll 直落 L1）
                            //       ⇒ 需改钳 GraphUtils 链。
                            //   同时打印「本容器是否被机器判据识别」+ 容器归属描述 —— 一并定位
                            //   owner: token 到底能不能匹配机器容器（v3 §六 R6 的关键未知量）。
                            var inv = __instance.inventory;
                            LogOnce("observe.place." + SafeId(source) + "@" + Ptr(inv).ToString("X"), Level.Diag,
                                $"[落点观测·放置] 整堆入【空槽】候选：{SafeId(source)} ×{source.unitCount}" +
                                $"，容器={DescribeContainer(inv)}" +
                                $"，机器判据={IsMachineContainer(inv)}" +
                                $"，marker.numTransfer={__instance.numTransfer}", max: 5);
                        }
                    }
                }

                return true;   // 直通原生（不改 __result）
            }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                LogOnce("slotaccept", Level.Warn, $"TryAcceptOnce 补丁异常，本次放行: {e.Message}");
                return true;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ★ 2026-10-13 引入 / ★★ 2026-10-14 修正：机器容器【改投空槽】
    //
    //  ── 反汇编依据（GameAssembly，已离线取证）──
    //   `GraphUtils::InsertToActiveContainer(GraphNodeStorage, GameItem)` @ RVA 0x766020：
    //        mov  esi, [rbp + 0x1F8]             ; esi = item.unitCount
    //        test esi, esi / jle …               ; <=0 直接退
    //        xor  ebx, ebx / xor edi, edi        ; accepted = 0, i = 0
    //    循环 cmp  ebx, esi / jge …              ; while (accepted < unitCount)
    //        mov  ecx, 4 ; call 0x180003FA0      ; ★ 接口 slot 4 = 每次重新找一个槽
    //        test rax, rax / je …                ; 找不到槽 → 退出
    //        cmp  [rax + 0x38], 0 / jle …        ; numTransfer <= 0 → 退出
    //        call SlotMarker::TryAcceptOnce(marker, unitCount - accepted, 0)
    //        test eax, eax / je … ; add ebx, eax ; inc edi ; cmp edi,0x28
    //  ⇒ 原生「塞进活动容器」本身就是【按件循环 + 每件重新找槽】。而"找槽"
    //    `TryFindOneValidInventorySlot` 会经 `MayStack` ⇒ 我们的机器 Deny 让它跳过合并候选
    //    ⇒ 只拿到空槽 ⇒ 天然「每格 1 件 + 逐格铺开」。本方法即照抄这个语义。
    //
    //  ── ★★ 2026-10-14 修正（用户实机日志证实，P0 级）──
    //   旧写法第 2 行是 `origin.inventory as GameGridInventory` —— **对机器内库恒为 null**：
    //     日志《Latest.log》16:23:48 作用域探针：
    //       `1 号容器：id=<空id> 类=GameInventory @… 所属=furnace 内含=common_ore`
    //     ⇒ 熔炉的内库是**裸 `GameInventory`**（不是 GameGridInventory）⇒ 旧代码直接 return 0
    //     ⇒ 机器分支永远失败 ⇒ 表现为「机器里有同名物品时，Ctrl/Alt 快移彻底进不去机器」
    //       （正是用户 2026-10-14 反馈的现象 1）。
    //     ⇒ 改用**基类** `GameInventory.TryFindOneValidInventorySlot`（虚分发到具体实现，
    //       GameGridInventory / GameSlotInventory / 裸 GameInventory 通吃）。
    //
    //  ── ★ 另一处修正：不再用「指针去重」──
    //    `TryFindOneValidInventorySlot`（→ `TryInventorySlot` @0x77BE40）**每次都 new 一个
    //    SlotMarker**（`call 0x1807B01F0` 构造），所以 `Ptr(m)` 每次都是新地址 ⇒ 旧 HashSet
    //    去重几乎不命中（反而可能因地址复用而提前 break）。改为**成功放置计数 + 连续占用计数**。
    //
    //  ── 安全边界 ──
    //    · 只投 `targetItem == null` 的空槽（即使 finder 给了合并候选也不接）。
    //    · 每次只接受 1 件（`amount=1`；TryAcceptOnce 内部还会再取
    //      min(1, numTransfer, MaxNumRemove)）⇒ 机器内永远不会出现 unitCount > 1。
    //    · 连续 8 次「拿到已占用槽」即判定没有新空槽、退出（防 finder 反复返回同一槽）。
    //    · 上限 128 轮兜底；任何异常 → 返回已成功件数（0 则由调用方按原逻辑处理）。
    //    · 重入：本方法内自调用 TryAcceptOnce，靠 `_selfAcceptDepth` 让本补丁 Prefix 直通原生。
    // ═══════════════════════════════════════════════════════════════════════
    private static int TryFillEmptySlots(SlotMarker origin, GameItem source)
    {
        try
        {
            if (origin == null || source == null) return 0;
            // ★ 用基类 GameInventory（机器内库可能是裸 GameInventory，见上方修正注释）
            var inv = origin.inventory;
            if (inv == null) return 0;

            int moved = 0;
            int want = Math.Min(source.unitCount, 64);
            int guard = 0;
            int occupiedSeen = 0;
            int consecutiveOccupied = 0;
            int nullAt = -1;              // 第几次扫描 finder 直接返回 null（= 该容器确无空槽）
            SlotMarker lastOccupied = null;
            _selfAcceptDepth++;
            try
            {
                while (moved < want && guard++ < 128)
                {
                    SlotMarker m;
                    try { m = inv.TryFindOneValidInventorySlot(source); } catch { break; }
                    if (m == null) { nullAt = guard; break; }

                    if (m.targetItem != null || m.numTransfer < 1)
                    {
                        // 已占用 / 放不进：试下一个（不计 placed）。连续 8 次都这样 ⇒ 没有新空槽了。
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
                    else break;                        // 真放不进（非占用）→ 停
                }
            }
            finally
            {
                _selfAcceptDepth--;
            }

            // ★ 2026-10-14 探针（DiagLog 门控，只读）：把「机器内找空槽」的全过程说清楚 ——
            //   这一次调用到底是「本来就没空槽」还是「找槽失败」，一行区分：
            //     · moved > 0                     ⇒ 正常（找到了空槽并放入）
            //     · moved = 0 且 nullAt ≥ 1       ⇒ finder 直接返回 null ⇒ 该容器确实没有空槽
            //     · moved = 0 且 occupiedSeen > 0 ⇒ finder 反复给「已占用/放不进」的槽
            //       （看末次占用槽的 targetItem/numTransfer：若 numTransfer>0 说明容量层没把它压住）
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

    // 读 `SlotMarker.TryAcceptOnce(int amount = -1)` 的**本次实参**。
    //   ★ 用 `object[] __args` 而非按名注入 —— 与 ModifierPairTransfer 的同款理由：
    //     形参名是 interop 生成物、随版本可能变；`__args[0]` 与名字无关，天然鲁棒。
    //   ★ 取不到返回 -1（= 原生语义「按 numTransfer 决定」），不会误判成"小数量"。
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


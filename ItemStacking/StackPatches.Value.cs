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
    // 计价与上限：GetNegociatedValue / MaxNumRemove / MaxNumAdd
    // ══════════════════════════════════════════════════════════════════
    // ───────────────────────────────────────────────────────────────────────
    // ★ 0.9.0：交易系统按【整堆总价值】计价
    //
    // 问题（反汇编实锤，base+RVA，ImageBase=0x180000000）：
    //   游戏里有两条价值路径，对 unitCount(0x1F8) 的处理**不一样**：
    //
    //   ① GameItem.GetValue(List<GameCharacterItem>, GameCharacterItem, bool) @ 0x7979E0
    //        0x180797A4B  test dil, dil            ; 第 4 参（bool）
    //        0x180797A50  mov  ecx, [rbx + 0x1F8]  ; ★ 读 unitCount
    //        0x180797A68  imul rax, rdx            ; ★ 单价 × 数量
    //      ⇒ 这条**已经**按整堆计价（消费者：GameInventory.GetTotalValue → 财富/分数显示）。
    //
    //   ② GameItem.GetNegociatedValue() @ 0x796340
    //        内部：PowerHelper.GetPowerSourceItemValue → AccumulateFeatureStages
    //              → GetCurrentChildValue → WaterHelper.GetContainerPrice
    //              → ComposeStagedValue(0x792E30) → 与调试串拼接后返回
    //        **全程没有任何一处读 [rbx + 0x1F8]**（已逐条反汇编确认 0x796340~0x796A73）
    //      ⇒ 这条只返回**单件**价值。
    //
    // 而**交易系统 100% 走 ②**（全库 call 站点扫描结果）：
    //   PlayerStore.SellItem / OnItemsSold / BuyItem / BuyItemFromTable /
    //   CanBuyItemFromTable / HasBudgetLeftToBuy / GetWholesalePrice /
    //   GetTotalEstimatedValue / GetTypesByValue / StealItem / HandleInsurance /
    //   StoreClient(显示与预算) / NegociationUIManager(SumNegociatedValue /
    //   GetFinalOfferValue / GetMultiSellValue / InitUIWithItemSellMode) /
    //   SoldItem.TrackItem / SecData.CheckSoldChem·Contraband·DirtyWater·EndangermentItem /
    //   ContrabandHelper.GetReportReward / ExchangeList …
    //
    // ⇒ 后果：本 mod 放开的 ×N 堆，上架卖出只按 1 件结账，其余 N-1 件的价值直接蒸发；
    //   「批量卖出」（SellMultiple）逐件调 SellItem，同样每堆只按 1 件算。
    //
    // 做法：**只在这一处** Postfix 乘数量 —— 一处补丁即让全部交易口径统一按整堆计价，
    //   不去逐个改调用方（30+ 个调用点，改一处必然漏，且漏了就是静默半价）。
    //
    // ★ 连带影响（同一方法也用于，必须知情）：
    //   ① 盗窃 / 检查被搜出的损失价值（PlayerStore.StealItem、SecData 系列）→ ×N
    //   ② 卖掉毒品 / 赃物 / 脏水 / 濒危物品的犯罪价值（CheckSold*）→ ×N
    //   ③ 保险理赔（HandleInsurance）→ ×N
    //   ④ 客户预算封顶（SellItem 内 `cmp [rbx+0x34], esi / cmovl esi, [rbx+0x34]`）：
    //      堆总价 > 客户剩余预算时，**只付得起预算上限**。
    //      ⇒ 必须实机确认柜台那边是按整堆销毁还是按付钱数量扣 ——
    //         若出现「付一半钱扣走整堆」，那是新的数据损失漏洞（见 StackValueTotalTotal cfg 警告）。
    //
    // 为安全起见：unitCount ≤ 0（异常存档）或开关关闭时一律不动原生结果；
    // 结果上限钳在 int.MaxValue —— 因为**调用方大量按 32 位 eax 使用返回值**
    // （例：SellItem `mov rsi, rax / mov r14d, eax`），超 32 位会被静默截断成负数。
    //
    // ★ 0.9.7：Priority.First —— ×unitCount 必须在所有 Postfix 中**最先执行**。
    //   否则其他加价 mod（如 +20% 议价加成）若先跑，加价加在【单件价】上，
    //   再被我们 ×N ⇒ 加价也被放大 N 倍（堆 10 件 = 加价 ×10）。
    //   设为 First 后：我们 ×N → 其他 mod 加价加在【整堆总价】上 ⇒ 只加一次。
    [HarmonyPatch(typeof(GameItem), nameof(GameItem.GetNegociatedValue))]
    [HarmonyPriority(HarmonyLib.Priority.First)]   // 0.9.7：确保 ×count 在所有加价 mod 之前
    private static class Patch_GetNegociatedValue
    {
        private static void Postfix(GameItem __instance, ref long __result)
        {
            try
            {
                if (!StackEnabled) return;
                if (!StackValueTotalOn) return;
                if (__instance == null) return;

                int count = __instance.unitCount;
                if (count <= 1) return;          // 单件 = 原生行为，不动

                long unit = __result;
                if (unit <= 0) return;           // 原生已给 0/负（不可售 / 未定价）→ 不放大

                long total = unit * count;
                if (total > int.MaxValue)
                {
                    // 调用方按 32 位取值，放行必然溢出成负数 ⇒ 钳到上限并告警。
                    total = int.MaxValue;
                    WarnOnce("valuetotal-overflow",
                        $"整堆计价溢出：{SafeId(__instance)} 单价 {unit} × {count} 已超过 int.MaxValue，" +
                        "已钳到 int.MaxValue（部分高价值堆可能出现计价封顶，请调低堆叠上限）。");
                }

                __result = total;
                ProbeNegociatedValue(__instance, unit, count, total);
            }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                Log.Error($"整堆计价 postfix 异常（已退回单件计价）: {e}");
            }
        }
    }

    // ── 兜底：无 maxSlotRemoveItemFunc 委托时，移除上限 = 全部数量 ──
    // TryAcceptOnce 用 min(..., MaxNumRemove(dragged)) 钳制合并量；
    // 原版委托恒空，若原生默认小值会导致每次只合并 1 个。
    // 无委托时把结果抬到 unitCount（语义精确：最多移走你拥有的全部）。
    [HarmonyPatch(typeof(GameItem), nameof(GameItem.MaxNumRemove))]
    private static class Patch_MaxNumRemove
    {
        private static void Postfix(GameItem __instance, ref int __result)
        {
            try
            {
                if (!StackEnabled) return;
                if (__instance.maxSlotRemoveItemFunc != null) return;
                var inv = __instance.parentInventory;
                if (inv != null && inv.maxSlotRemoveItemFunc != null) return;
                int before = __result;
                if (__instance.unitCount > __result) __result = __instance.unitCount;
                DiagRate("maxnumremove." + SafeId(__instance), 3,
                    $"MaxNumRemove {__instance.identifier}：{before} -> {__result}（unitCount={__instance.unitCount}）");
            }
            catch (Exception e)
            {
                Log.Error($"MaxNumRemove postfix 异常: {e}");
            }
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    // A'. 放开堆叠上限：Postfix GameItem.MaxNumAdd(SlotMarker)
    //
    // 原生取证结论（GameAssembly 0x798160，从汇编反推的伪 C）：
    //   MaxNumAdd(slot) = Min(两层 maxSlotAmountItemFunc 的 InvokeAllReduce)
    //                     - slot.targetItem.unitCount   → 返回「还能再放进几个」
    //   游戏用 InvokeFuncExtensions.InvokeAllReduce 以 il2cpp 反射
    //   Delegate.DynamicInvoke 遍历委托链 —— 注入 mono 委托
    //   （DelegateSupport.ConvertDelegate）会 TargetException 崩溃（0.1.0 实测）。
    //   所以绝不碰 maxSlotAmountItemFunc，直接在消费方 MaxNumAdd 的返回值上
    //   抬高剩余空间：max(原值, NewStackCap - 目标堆已有数量)。
    //   只提升、不收紧，保留游戏原有全部约束（min func 等）。
    //
    // 无参版 MaxNumAdd() 是尾调用 jmp 到本方法同一 native 入口（0x798430 →
    // jmp 0x798160），本补丁一并覆盖。
    // ───────────────────────────────────────────────────────────────────────

    [HarmonyPatch(typeof(GameItem), "MaxNumAdd", new[] { typeof(SlotMarker) })]
    private static class Patch_MaxNumAdd
    {
        private static void Postfix(GameItem __instance, SlotMarker slot, ref int __result)
        {
            try
            {
                if (slot == null) return;   // 无参转发路径（slot 未构造）不动

                int before = __result;
                var target = slot.targetItem;
                if (target != null)
                {
                    // 堆到已有同名堆上：剩余空间抬到新上限
                    int want = StackCap - target.unitCount;
                    if (want > __result) __result = want;
                    DiagRate("maxnumadd.stack." + SafeId(__instance) + "/" + SafeId(target), 3,
                        $"MaxNumAdd {__instance.identifier} -> 目标堆 {target.identifier}：{before} -> {__result}（目标 unitCount={target.unitCount}，StackCap={StackCap}）");
                }
                else if (__result < StackCap)
                {
                    // ★ 0.3.1 修复：空槽 marker（TryInventorySlot 构造，targetItem==null）。
                    //   原生取证：MaxNumAdd 两层 maxSlotAmountItemFunc 均为 null 时默认返回 1
                    //   （原版堆叠上限），→ TryInventorySlot.numTransfer = min(1, requestedNum)
                    //   → EndDrag 空格路径 TryAcceptOnce(-1) 只收 1 个并 GetSplitItem 拆分
                    //   = 「Ctrl 整堆放到空格只移 1 个」+「短拖拆 1 个造成 UI 重叠」的根因。
                    //
                    // ★★ 0.6.2 修正：空槽容量一律以 StackCap 为【硬上限】，不再随 unitCount 抬高。
                    //   旧写法 `Math.Max(StackCap, unitCount)` 会让「旧档遗留的大堆」（例如早先
                    //   用 999 上限叠出的堆，读档后仍存在）在放进空格时被抬到 999，
                    //   从而一次性绕过新上限 —— 与「自定义上限」的核心预期矛盾，且会反向污染新档。
                    //   「Ctrl 整堆一次进格」的需求由 currentRemove（StartDrag 已设为 maxRemove）
                    //   经 TryAcceptOnce 的 requestedNum 天然满足，不需要在 MaxNumAdd 上开口子：
                    //   TryAcceptOnce 内部 n = min(requestedNum, numTransfer, MaxNumRemove)，
                    //   numTransfer = StackCap 即可保证一次进格且不超上限。
                    int want = StackCap;
                    if (want > __result) __result = want;
                    DiagRate("maxnumadd.empty." + SafeId(__instance), 3,
                        $"MaxNumAdd {__instance.identifier} -> 空槽：{before} -> {__result}（unitCount={__instance.unitCount}，StackCap={StackCap}）");
                }
            }
            catch (Exception e)
            {
                Log.Error($"MaxNumAdd postfix 异常: {e}");
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Preferences;
using UnityEngine;

namespace ItemStacking;

// 计价与上限：GetNegociatedValue / MaxNumRemove / MaxNumAdd
//
// ★ 整堆计价（cfg StackValueTotal）
//   GetNegociatedValue 原生只返回单件价，交易系统 30+ 调用点全走它
//   ⇒ 在此 Postfix ×unitCount 即可全局覆盖（一处补丁 = 全部口径统一）。
//   详见 README.md 「整堆计价」章节。
//
// ★ 三个补丁的位置关系
//   Patch_GetNegociatedValue —— 交易结算价（唯一入口）
//   Patch_MaxNumRemove      —— 「一堆最多能取走多少」（TryAcceptOnce 用）
//   Patch_MaxNumAdd         —— 「一堆最多还能再放进几个」（TryAcceptOnce 用）
public partial class Main
{
    // ── 整堆计价 ──
    //   Priority.First：确保 ×count 在其它加价 mod 之前执行
    //   （否则对方加价加在单件价上，我们再 ×N 会放大 N 倍）。
    [HarmonyPatch(typeof(GameItem), nameof(GameItem.GetNegociatedValue))]
    [HarmonyPriority(HarmonyLib.Priority.First)]
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
                if (count <= 1) return;          // 单件 = 原生行为

                long unit = __result;
                if (unit <= 0) return;           // 原生已判不可售/未定价 → 不放大

                long total = unit * count;
                // 调用方大量按 32 位 eax 使用返回值，超 32 位会静默截断成负数 ⇒ 钳 int.MaxValue。
                if (total > int.MaxValue)
                {
                    total = int.MaxValue;
                    LogOnce("valuetotal-overflow", Level.Warn,
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

    // ── 移除上限兜底 ──
    //   TryAcceptOnce 用 min(..., MaxNumRemove(dragged)) 钳制合并量。
    //   原版 maxSlotRemoveItemFunc 委托恒空、原生默认小值 → 每次只合 1 件。
    //   无委托时抬到 unitCount（语义：最多移走你拥有的全部）。
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
                LogOnce("maxnumremove." + SafeId(__instance), Level.Diag,
                    $"MaxNumRemove {__instance.identifier}：{before} -> {__result}（unitCount={__instance.unitCount}）", max: 3);
            }
            catch (Exception e)
            {
                Log.Error($"MaxNumRemove postfix 异常: {e}");
            }
        }
    }

    // ── 堆叠上限放开 ──
    //   MaxNumAdd(slot) 原生 = Min(两层 maxSlotAmountItemFunc) - targetItem.unitCount
    //   （返回「该槽还能放几个」）。
    //   委托不能注入（il2cpp DynamicInvoke 会 TargetException），故在消费方直接抬返回值。
    //   无参版 MaxNumAdd() 是 jmp 到同一 native 入口，本补丁一并覆盖。
    [HarmonyPatch(typeof(GameItem), "MaxNumAdd", new[] { typeof(SlotMarker) })]
    private static class Patch_MaxNumAdd
    {
        private static void Postfix(GameItem __instance, SlotMarker slot, ref int __result)
        {
            try
            {
                if (slot == null) return;   // 无参转发路径不动

                // ★ 机器容器：每格 1 件。见 README.md「机器容器保护」。
                if (StackEnabled)
                {
                    bool machine = IsMachineContainer(slot.inventory);
                    // 判据异常（故障位）时也按机器容器处理（保守钳 1）。
                    if (!machine && ConsumeJudgeFault(FaultProtection)) machine = true;
                    if (machine)
                    {
                        // 只降不升：原生给 0（= 已占用槽 / 该物品 cap 已满）必须保持 0，
                        // 那是游戏自己「寻路跳过该候选、改扫空槽」的信号；改成 1 会让 finder
                        // 反复返回同一已占用槽 ⇒ 「机器里有同名物品时什么都放不进去」。
                        if (__result > 1)
                        {
                            LogOnce("machine.add." + SafeId(__instance), Level.Diag,
                                $"机器容器内 MaxNumAdd 压到 1：{SafeId(__instance)}（原 {__result}）" +
                                (slot.targetItem != null ? $"，目标堆 {SafeId(slot.targetItem)}" : "，空槽"), max: 3);
                            __result = 1;
                        }
                        return;
                    }
                }

                int before = __result;
                var target = slot.targetItem;
                if (target != null)
                {
                    // 堆到已有同名堆：剩余空间抬到新上限。
                    int want = StackCap - target.unitCount;
                    if (want > __result) __result = want;
                    LogOnce("maxnumadd.stack." + SafeId(__instance) + "/" + SafeId(target), Level.Diag,
                        $"MaxNumAdd {__instance.identifier} -> 目标堆 {target.identifier}：{before} -> {__result}（目标 unitCount={target.unitCount}，StackCap={StackCap}）", max: 3);
                }
                else if (__result < StackCap)
                {
                    // 空槽 marker：原生默认返回 1（原版堆叠上限），→ TryAcceptOnce 只收 1 件
                    // ⇒ 「Ctrl 整堆放到空格只移 1 个」的根因。
                    // 一律以 StackCap 为硬上限（不随 unitCount 抬高，避免旧档大堆借道绕过新上限）。
                    int want = StackCap;
                    if (want > __result) __result = want;
                    LogOnce("maxnumadd.empty." + SafeId(__instance), Level.Diag,
                        $"MaxNumAdd {__instance.identifier} -> 空槽：{before} -> {__result}（unitCount={__instance.unitCount}，StackCap={StackCap}）", max: 3);
                }

                // ★ 硬上限探针 + 可选收紧（cfg StrictStackCap）
                //   原生有两条会给出超大值的路径：slot[0x18]==null 早退返回 0x7FFFFFFF；
                //   外部 mod 注入 maxSlotAmountItemFunc。
                //   探针常驻（只读）；收紧本体由 cfg 控制（默认关）。
                //   下限恒保 1 —— 返 0 会打断 GraphUtils.TryAcceptAll 的逐格填充循环。
                int hardCap = (target != null) ? (StackCap - target.unitCount) : StackCap;
                if (hardCap < 1) hardCap = 1;
                if (before > hardCap)
                {
                    LogOnce("maxnumadd.clamp." + SafeId(__instance), Level.Diag,
                        $"MaxNumAdd 超出硬上限：{SafeId(__instance)} 原生={before} → 硬上限={hardCap}" +
                        $"（StackCap={StackCap}，目标堆={(target != null ? target.unitCount.ToString() : "空槽")}）" +
                        (StrictStackCap ? "，已收紧" : "，未收紧（StrictStackCap=false）"), max: 3);
                    if (StrictStackCap) __result = hardCap;
                }
            }
            catch (Exception e)
            {
                Log.Error($"MaxNumAdd postfix 异常: {e}");
            }
        }
    }
}
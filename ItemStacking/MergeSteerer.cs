using System;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace ItemStacking;

public partial class Main
{
    // ══════════════════════════════════════════════════════════════════════
    //  0.10.0 · 阶段3：把【决策】与【导流】两个职责拆开
    //
    //  ── 问题（0.9.9 的职责纠缠）──
    //  0.9.9 用 `MayStack = false` 同时表达了两件不同的事：
    //    ① 决策：这一堆【不能】合并（拦截）；
    //    ② 导流：这一堆【先别】合并 —— 目的是让原生
    //       `TryFindOneValidInventorySlot` 的"合并同名堆"循环全部落空，
    //       从而落到它自己的"找空格"分支。
    //  两者都用 false，语义混在一起 ⇒ 「为什么返回 false」无法区分，
    //  且柜台场景在 MayStack 是"导流"、在 TryAcceptOnce 又是"真拦截"，自相矛盾。
    //
    //  ── 本类做什么 ──
    //  只回答一个问题：**本场景是否应"强制禁并"以触发原生找空格（导流）？**
    //    · true  = 导流（让原生找空格）—— MayStack 返回 false，但语义是"先别合并"，
    //              不是"禁止合并"；
    //    · false = 不导流 —— 是否合并交给 MergePolicy 决策。
    //
    //  ── 需要导流的场景（0.9.9 实证）──
    //   ① Alt 快移（AltTransferScope）：必须逐件/定量快移，不能并进已有堆。
    //      依据：GameGridScrollableInventory(0x77B750) 合并循环在 0x77BB4E 调 MayStack，
    //      false 即 je 循环；循环耗尽唯一出口是其空槽分支 0x77BCAD。
    //   ② 柜台（IsCounterInventory，仅 BlockCounterStack=true 时）：
    //      ExpeditionHelper.SetHirelingEquipmentOnCounter → GraphUtils.TryAcceptAll 是
    //      「逐格填充整堆」循环，某轮被拒返回 0 就 break ⇒ 必须导流到空槽，
    //      否则跨场景转移只搬极少件（0.7.1 实机 bug）。
    //   ③ §四 配对快移（0.11.0 Alt / 0.12.0 Shift）：由 AltPairMerge 开关决定（见下方 ⓪）。
    //
    //  ★ 与 MergePolicy 的关系：导流【优先于】决策。MayStack 先问 Steerer；
    //    true ⇒ 直接 false（导流），不进 MergePolicy。false ⇒ 进 MergePolicy 决策。
    // ══════════════════════════════════════════════════════════════════════

    // 是否应导流（强制禁并 → 让原生找空格）。
    //   ctx 提供落点容器（slotInventory），用于判定柜台场景。
    //   ★ 只读判断，不改变任何状态。
    internal static bool ShouldSteerToEmptySlot(in MergeContext ctx)
    {
        try
        {
            // ⓪ §四 配对快移（Alt / Shift 两套，0.11.0 / 0.12.0）：**由 AltPairMerge 开关决定**。
            //     · PairNoMerge=true （cfg AltPairMerge=false）→ 导流（强制逐件找空位）。
            //     · PairNoMerge=false（cfg AltPairMerge=true ）→ 不导流（允许合并）。
            //    ★ 必须放在 AltTransferScope 之前：配对搬运【不】进 AltTransferScope
            //      （它走自己的 Prefix 跳过原生），但若玩家同时开着旧 Alt 接管开关，
            //      二者语义以配对开关为准。
            //    ★ 返回 false 时**直接返回**，不再看 AltTransferScope —— 否则配对模式下
            //      "允许合并"会被 AltTransferScope 的恒定导流覆盖掉，合并永远不生效。
            //    ★ Shift 配对不经 AltTransferScope（Patch_AltHandlerRelease 不挂 Shift handler），
            //      故这里必须一并覆盖 ShiftPairActive，否则 Shift 的「禁止合并」开关失效。
            if (AltPairActive || ShiftPairActive)
                return PairNoMerge;

            // ① Alt 快移作用域：恒定导流（本场景就是要逐件/定量搬，不能合并）。
            if (AltTransferScope) return true;

            // ② 柜台容器：仅 BlockCounterStack=true 时 IsCounterInventory 才为真
            //    （默认关闭 ⇒ 默认不导流，与 0.9.9 一致）。
            if (IsCounterInventory(ctx.slotInventory)) return true;

            return false;
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            // 导流判据异常 → 不导流（保守：让 MergePolicy 正常决策拦截）。
            LogOnce("steerer", Level.Warn, $"[导流] 判据异常，本次不导流（交 MergePolicy 决策）: {e.Message}");
            return false;
        }
    }
}

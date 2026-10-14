using System;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace ItemStacking;

// 决策【导流】与【拦截】的职责分离
//
// ★ 问题（0.9.9 的职责纠缠）
//   0.9.9 用 `MayStack = false` 同时表达了两件不同的事：
//     ① 决策：这一堆不能合并（拦截）；
//     ② 导流：这一堆先别合并 —— 目的是让原生搜索跳过「叠加优先」分支、
//             落到它自己的「找空格」分支。
//   ⇒ 「为什么返回 false」无法区分。
//
// ★ 本类做什么
//   只回答一个问题：本场景是否应"强制禁并"以触发原生找空格（导流）？
//     · true  = 导流（MayStack 返 false，但语义是"先别合并"）；
//     · false = 不导流（是否合并交给 MergePolicy 决策）。
//
// ★ 与 MergePolicy 的关系
//   导流【优先于】决策。MayStack 先问 Steerer；true ⇒ 直接 false（导流），
//   不进 MergePolicy；false ⇒ 进 MergePolicy 决策。
public partial class Main
{
    // 是否应导流（强制禁并 → 让原生找空格）。
    internal static bool ShouldSteerToEmptySlot(in MergeContext ctx)
    {
        try
        {
            // ⓪ 配对快移（Alt / Shift 两套）：由 AltPairMerge 开关决定。
            //    · PairNoMerge=true （cfg AltPairMerge=false）→ 导流（强制逐件找空位）。
            //    · PairNoMerge=false（cfg AltPairMerge=true ）→ 不导流（允许合并）。
            //   ★ 必须放在 AltTransferScope 之前：配对搬运不经过 AltTransferScope，
            //     但若玩家同时开着旧 Alt 接管开关，二者语义以配对开关为准。
            //   ★ Shift 配对不经 AltTransferScope，故这里一并覆盖 ShiftPairActive。
            if (AltPairActive || ShiftPairActive)
                return PairNoMerge;

            // ① Alt 快移作用域：恒定导流（本场景就是要逐件/定量搬，不能合并）。
            if (AltTransferScope) return true;

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
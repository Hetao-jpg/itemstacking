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
    // 拖动：ItemMouseDragHandler.StartDrag（拖起数量控制）
    //
    // ★ 原生逻辑（StartDrag 反汇编实证）
    //   maxRemove     = MaxNumRemove()   ← 已由 Patch_MaxNumRemove 抬到 unitCount
    //   currentRemove = maxRemove（默认 = 整堆）
    //   LeftShift  → currentRemove = round(maxRemove * 0.1)
    //   LeftAlt    → currentRemove = 1
    //   之后 OnEventMouseScroll 可用滚轮微调 currentRemove。
    //   落点用 TryAcceptOnce(currentRemove) 消费 —— 改这一处即可全链路生效。
    //
    // ★ 本补丁的改动（只加 Ctrl 分支）
    //   Ctrl 按住 → currentRemove = maxRemove（整堆）
    //   其余情况【完全交还原版】：无修饰键=整堆，Shift=10%，Alt=1 个。
    [HarmonyPatch(typeof(ItemMouseDragHandler), "StartDrag")]
    private static class Patch_StartDrag
    {
        private static void Postfix(ItemMouseDragHandler __instance)
        {
            try
            {
                if (!_ctrlWholeStack.Value) return;
                if (__instance.maxRemove <= 1) return;   // 单件物品，无需处理

                bool ctrl = Input.GetKey(KeyCode.LeftControl)
                         || Input.GetKey(KeyCode.RightControl);
                if (ctrl)
                {
                    __instance.currentRemove = __instance.maxRemove;
                    LogOnce("startdrag.ctrl", Level.Diag,
                        $"StartDrag Ctrl 整堆：currentRemove={__instance.currentRemove}/{__instance.maxRemove}", max: 3);
                }
                // 其余情况一律交还原生（不动 currentRemove）。
            }
            catch (Exception e)
            {
                Log.Error($"StartDrag postfix 异常: {e}");
            }
        }
    }
}
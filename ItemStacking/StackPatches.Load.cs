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
    // 拖动：ItemMouseDragHandler.StartDrag
    // （原「读档补满」SaveItemNode.OnAfterDeserialize 补丁已于 2026-10-12 整体删除）
    // ══════════════════════════════════════════════════════════════════

    // ───────────────────────────────────────────────────────────────────────
    // C. 拖起数量控制：普通拖动 = 1 个，Ctrl+拖动 = 整堆
    //
    // 原生取证（ItemMouseDragHandler.StartDrag 0x7BB750 反汇编实锤）：
    //   maxRemove     = MaxNumRemove()          ← 0.2.0 postfix 已抬到 unitCount
    //   currentRemove = maxRemove
    //   Input.GetKey(LeftShift)  → currentRemove = round(maxRemove * 0.1)  取10%
    //   Input.GetKey(LeftAlt)    → currentRemove = 1                       取1个
    //   （0x130=304=LeftShift, 0x134=308=LeftAlt，interop KeyCode 枚举核实）
    //   之后 OnEventMouseScroll 在 maxRemove>1 时可用滚轮微调 currentRemove，
    //   EndDrag 落点用 TryAcceptOnce(currentRemove) 消费 —— 拿多少、放多少、
    //   合并多少共用同一个 currentRemove，改这一处即可全链路生效。
    //
    // 本补丁在原生 StartDrag 之后收尾：
    //   Ctrl 按住 → currentRemove = maxRemove（整堆）
    //   无修饰键 → currentRemove = 1（保留单件拖动）
    //   Shift / Alt 不碰，保持原生行为；滚轮微调在拖起后照常可用。
    // ───────────────────────────────────────────────────────────────────────

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
                    __instance.currentRemove = __instance.maxRemove;   // 整堆
                    DiagRate("startdrag.ctrl", 3,
                        $"StartDrag Ctrl 整堆：currentRemove={__instance.currentRemove}/{__instance.maxRemove}");
                    return;
                }

                // 无任何修饰键 = 单个；Shift（原生10%）/ Alt（原生1个）让给原生
                bool shift = Input.GetKey(KeyCode.LeftShift)
                          || Input.GetKey(KeyCode.RightShift);
                bool alt = Input.GetKey(KeyCode.LeftAlt)
                        || Input.GetKey(KeyCode.RightAlt);
                if (!shift && !alt)
                {
                    __instance.currentRemove = 1;
                    DiagRate("startdrag.single", 3,
                        $"StartDrag 单件：currentRemove=1（maxRemove={__instance.maxRemove}）");
                }
            }
            catch (Exception e)
            {
                Log.Error($"StartDrag postfix 异常: {e}");
            }
        }
    }
}


using System;
using Il2Cpp;

namespace ItemStacking;

// 0.9.9 旧内联判据（回滚专用）
//
// ★ 为什么留着
//   cfg z9_UseMergePolicy = false 时，三个挂点仍要走 0.9.9 那套逐条内联判据。
//   新单源 MergePolicy.Evaluate 若实机发现漏判据，回滚只需改 cfg，不必重编。
//
// ★ 三条抽取的出处
//   LegacyMayStack        ← StackPatches.Merge.cs · Patch_MayStack.Prefix 的旧分支
//   LegacyGetStackSlot    ← StackPatches.Merge.cs · Patch_GetStackSlot.Prefix 的 else 分支
//   LegacyMergeGateReason ← StackGates.cs · GetMergeGateReason 的旧顺序段
//
// ★ 等价性
//   参数名沿用原 __instance / otherItem / __result ⇒ 方法体可与 git 历史逐行对照；
//   判定顺序、日志文案、WarnOnce key 一律未改。
//   LegacyGetStackSlot 返回 null = 「同名非空，落到挂点里的共用段构造 marker」。
public partial class Main
{
    // 闸门1（MayStack）：0.9.9 旧内联判据。
    private static bool LegacyMayStack(GameItem __instance, GameItem otherItem, ref bool __result)
    {
        // 旧路径的 Alt 导流检查（新路径已由 ShouldSteerToEmptySlot 承担）。
        if (AltTransferScope)
        {
            Diag($"[Alt 落点] MayStack 返回 false（Alt 作用域导流，让原生走找空格分支）：" +
                 $"{__instance.identifier} / {otherItem?.identifier}");
            __result = false;
            return false;
        }
        var instInv = TryGet(() => __instance.parentInventory);
        if (DiagOn) ProbeScopeInventory(instInv, "落点堆所在(MayStack)");
        // P0：装了东西的容器禁止堆叠，避免丢失容器内物品。
        if (IsContainer(__instance) || IsContainer(otherItem))
        {
            Diag($"MayStack 拦截（容器）：{__instance.identifier} / {otherItem.identifier}");
            __result = false;
            return false;
        }
        // 黑名单：鉴定物品 / 赃物 / 未拥有 / 电量不同，任一命中即拒绝。
        var blockReason = GetBlacklistReason(__instance, otherItem);
        if (blockReason != null)
        {
            Diag($"MayStack 拦截（黑名单·{blockReason}）：{__instance.identifier} / {otherItem.identifier}");
            __result = false;
            return false;
        }
        // 容器黑名单（双端判定）。★ 黑名单优先于白名单。
        var excludeReason = GetExcludeReason(__instance, otherItem);
        if (excludeReason != null)
        {
            Diag($"MayStack 拦截（排除·{excludeReason}）：{__instance.identifier}");
            __result = false;
            return false;
        }
        // 作用域（白名单）。黑名单未命中才走到这里。
        var scopeReason = GetScopeReason(__instance);
        if (scopeReason != null)
        {
            Diag($"MayStack 拦截（作用域·{scopeReason}）：{__instance.identifier}");
            __result = false;
            return false;
        }
        // identifier 非空才允许「同名放行」（空 id 无法区分物品类型）。
        var idA = __instance.identifier;
        var idB = otherItem.identifier;
        if (!string.IsNullOrEmpty(idA) &&
            string.Equals(idA, idB, StringComparison.Ordinal))
        {
            Diag($"MayStack 放行（同名）：{idA}");
            __result = true;
            return false;
        }
        if (string.IsNullOrEmpty(idA) || string.IsNullOrEmpty(idB))
        {
            Diag($"MayStack 拦截（空 identifier）：'{idA}' / '{idB}'");
            __result = false;
            return false;
        }
        Diag($"MayStack 走原生（不同名）：{__instance.identifier} / {otherItem.identifier}");
        return true;
    }

    // 闸门2（GetStackSlot）：0.9.9 旧内联判据。
    //   返回 null = 交给挂点的「共用段」构造 marker。
    private static bool? LegacyGetStackSlot(GameItem __instance, GameItem itemToBeStacked, ref SlotMarker __result)
    {
        // P0：装了东西的容器不产生堆叠槽位（纵深防御）。
        if (IsContainer(__instance) || IsContainer(itemToBeStacked))
        {
            __result = null;
            return false;
        }
        // 黑名单纵深防御（万一 MayStack 已放行）。
        var blockReason = GetBlacklistReason(__instance, itemToBeStacked);
        if (blockReason != null)
        {
            Diag($"GetStackSlot 拦截（黑名单·{blockReason}）：{__instance.identifier} / {itemToBeStacked.identifier}");
            __result = null;
            return false;
        }
        // 容器黑名单纵深防御。
        var excludeReason = GetExcludeReason(__instance, itemToBeStacked);
        if (excludeReason != null)
        {
            Diag($"GetStackSlot 拦截（排除·{excludeReason}）：{__instance.identifier}");
            __result = null;
            return false;
        }
        // 作用域纵深防御。
        var scopeReason = GetScopeReason(__instance);
        if (scopeReason != null)
        {
            Diag($"GetStackSlot 拦截（作用域·{scopeReason}）：{__instance.identifier}");
            __result = null;
            return false;
        }
        var idA = __instance.identifier;
        var idB = itemToBeStacked.identifier;
        if (string.IsNullOrEmpty(idA) || string.IsNullOrEmpty(idB))
        {
            Diag($"GetStackSlot 拦截（空 identifier）：'{idA}' / '{idB}'");
            __result = null;
            return false;
        }
        if (!string.Equals(idA, idB, StringComparison.Ordinal))
            return true;                  // 不同名交给原生（返回 null，不堆叠）
        // 同名且非空 ⇒ 交回调用方落到「共用段：构造 marker」。
        return null;
    }

    // 闸门3（GetMergeGateReason · TryAcceptOnce 路径）：0.9.9 旧判定顺序。
    private static string LegacyMergeGateReason(SlotMarker slot, GameItem target, GameItem source)
    {
        // 1) 容器黑名单（黑名单优先）。
        var exclude = GetExcludeReason(target, source);
        if (exclude != null) return exclude;

        // 2) 堆叠作用域（白名单）。
        var scope = GetScopeReason(target);
        if (scope != null) return scope;

        // 3) 物品黑名单。
        var block = GetBlacklistReason(target, source);
        if (block != null) return block;

        // 4) 装了东西的容器禁止合并（防数据丢失）。
        if (IsContainer(target) || IsContainer(source)) return "容器（含内部物品）";

        return null;
    }
}
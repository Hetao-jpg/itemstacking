using System;
using Il2Cpp;

namespace ItemStacking;

public partial class Main
{
    // ══════════════════════════════════════════════════════════════════════
    //  T2.1 · 0.9.9 旧内联判据（回滚专用）—— variant B：【抽文件】而不是删除
    //
    //  ── 为什么留着 ──
    //    cfg z9_UseMergePolicy = false 时，三个挂点仍要走 0.9.9 那套「逐条内联判据」。
    //    新单源 MergePolicy.Evaluate 若实机发现漏判据，回滚只需改 cfg，不必重编、不必回 git。
    //    （若哪天确认彻底不需要回滚，可走 optional 任务 T2.1.variant_A 整段删除。）
    //
    //  ── 三条抽取的出处（合并前的行号，见 git tag phase1）──
    //    LegacyMayStack        ← StackPatches.Merge.cs · Patch_MayStack.Prefix 的旧分支
    //    LegacyGetStackSlot    ← StackPatches.Merge.cs · Patch_GetStackSlot.Prefix 的 else 分支
    //    LegacyMergeGateReason ← StackGates.cs · GetMergeGateReason 的旧顺序段
    //
    //  ── 等价性 ──
    //    · 参数名沿用原 __instance / otherItem / __result 等 ⇒ 方法体可与 git 历史逐行对照；
    //      方法体未包 try/catch，异常仍抛给挂点原有的 catch（行为与内联时一致）。
    //    · 判定顺序、日志文案、WarnOnce key 一律未改。
    //    · LegacyGetStackSlot 返回 null = 「同名非空，落到挂点里的共用段去构造 marker」，
    //      （原先那条分支是不 return、直接 fall-through 的）。
    // ══════════════════════════════════════════════════════════════════════

    // 闸门1（MayStack）：0.9.9 旧内联判据。
    private static bool LegacyMayStack(GameItem __instance, GameItem otherItem, ref bool __result)
    {
        // ══════════ 以下为 0.9.9 旧内联判据（回滚用，保留未删）══════════
        // ★ 旧路径的 Alt 导流检查（新路径已由 ShouldSteerToEmptySlot 承担；
        //   回滚到 false 时仍需它，否则丧失 Alt 快移导流）。
        if (AltTransferScope)
        {
            Diag($"[Alt 落点] MayStack 返回 false（Alt 作用域导流，让原生走找空格分支）：" +
                 $"{__instance.identifier} / {otherItem?.identifier}");
            __result = false;
            return false;
        }
        // ★★★ 0.7.1：柜台内禁止堆叠 —— **在这里让 MayStack 恒 false**（而不是只在
        //   `SlotMarker.TryAcceptOnce` 上拒绝）。为什么必须提前到这一层：
        //
        //   实机现象（用户 2026-10-08）：「堆叠物品从垃圾站/配给站/杰克逊博士转场景回商店，
        //   堆叠数变成 1，其余留在原地」。反汇编定位到根因链：
        //     ExpeditionHelper.SetHirelingEquipmentOnCounter(0x8B6820)
        //       → GraphUtils.TryAcceptAll(柜台窗口, item, -1)（0x766650）
        //       → { 循环：TryFindOneValidInventorySlot(item) → 若 numTransfer<=0 **break**；
        //                 SlotMarker.TryAcceptOnce(剩余) → **返回 0 也 break** }
        //   ⇒ 这是「逐格填充整堆」的循环。只要**某一轮被拒绝返回 0**，整个循环立刻 break，
        //     后面的格子再空也不再用 —— 表现就是「只搬进去 N 个，其余留在原地」。
        //
        //   0.6.5~0.7.0 的「柜台禁堆」只在 `SlotMarker.TryAcceptOnce` 里 `__result = 0`，
        //   ⇒ **正好触发上面那个 break**：柜台里已有同名堆时，第一轮拿到「合并落点」→
        //     被我们拒 → break → 整堆只搬进去极少（甚至 0/1）个。
        //
        //   正解（与 0.7.0 Alt 的手法同源、且更贴合原生语义）：
        //     **在 `MayStack` 这一层让柜台内的合并判定恒 false** —— 原生
        //     `TryFindOneValidInventorySlot` 的"合并同名堆"循环自然全部落空，直接给出
        //     **空槽落点**（`numTransfer > 0`）→ `TryAcceptOnce` 走"移动"路径把整堆放进去
        //     → 逐格填充循环得以继续到放完为止。
        //     ⇒ 柜台里每个商品依旧**各自占一格**（不合并），但跨场景转移不会丢件。
        //
        //   ★ 判据用 `__instance.parentInventory`（柜台里已有那件所在的容器）。
        //   ★ 只在"目标物品已在柜台容器内"时生效；玩家在背包里整理不受影响。
        var instInv = TryGet(() => __instance.parentInventory);
        if (DiagOn) ProbeScopeInventory(instInv, "落点堆所在(MayStack)");   // §七 1.4
        if (instInv != null && IsCounterInventory(instInv))
        {
            LogOnce("maystack.counter", Level.Warn,
                $"[柜台禁堆] MayStack 已拦截（柜台内不合并，让原生走找空格分支）：" +
                $"{__instance.identifier} / {otherItem.identifier}");
            __result = false;
            return false;
        }
        // P0：装了东西的容器禁止堆叠，避免丢失容器内物品。
        if (IsContainer(__instance) || IsContainer(otherItem))
        {
            Diag($"MayStack 拦截（容器）：{__instance.identifier} / {otherItem.identifier}");
            __result = false;
            return false;                 // 拦截堆叠，交给原生（恒 false）
        }
        // P3（0.5.0）：堆叠黑名单 —— 鉴定物品 / 赃物 / 未拥有 / 电量不同，任一命中即拒绝。
        var blockReason = GetBlacklistReason(__instance, otherItem);
        if (blockReason != null)
        {
            Diag($"MayStack 拦截（黑名单·{blockReason}）：{__instance.identifier} / {otherItem.identifier}");
            __result = false;
            return false;
        }
        // 需求3（0.5.1）：堆叠作用域（白名单容器）。__instance 是被堆叠到的物品。
        // 0.6.0 方向4：容器黑名单，双端判定。★ 0.9.3：黑名单优先于白名单（命中即 return，
        // 拒绝原因显示黑名单而非白名单 —— 用户拍板：两个名单同时命中时黑名单赢）。
        var excludeReason = GetExcludeReason(__instance, otherItem);
        if (excludeReason != null)
        {
            Diag($"MayStack 拦截（排除·{excludeReason}）：{__instance.identifier}");
            __result = false;
            return false;
        }
        // 需求3（0.5.1）：作用域（白名单）。黑名单未命中才走到这里。
        var scopeReason = GetScopeReason(__instance);
        if (scopeReason != null)
        {
            Diag($"MayStack 拦截（作用域·{scopeReason}）：{__instance.identifier}");
            __result = false;
            return false;
        }
        // ★ 0.9.4：identifier 非空才允许「同名放行」。
        //   实机（2026-10-10 出发选背包 UI）：多件 identifier 为空字符串的不同物品被
        //   string.Equals("", "", Ordinal) 判成同名并合并（日志「放行（同名）：」后无名字）。
        //   空 id 无法区分物品类型，一律不放行，交给原生（恒 false）。
        var idA = __instance.identifier;
        var idB = otherItem.identifier;
        if (!string.IsNullOrEmpty(idA) &&
            string.Equals(idA, idB, StringComparison.Ordinal))
        {
            Diag($"MayStack 放行（同名）：{idA}");
            __result = true;
            return false;                 // 放行同名，跳过原生委托检查
        }
        if (string.IsNullOrEmpty(idA) || string.IsNullOrEmpty(idB))
        {
            Diag($"MayStack 拦截（空 identifier）：'{idA}' / '{idB}'");
            __result = false;
            return false;
        }
        Diag($"MayStack 走原生（不同名）：{__instance.identifier} / {otherItem.identifier}");
        return true;                      // 不同名走原生（仍 false）
    }

    // 闸门2（GetStackSlot）：0.9.9 旧内联判据。返回 null = 交给挂点的「共用段」构造 marker。
    private static bool? LegacyGetStackSlot(GameItem __instance, GameItem itemToBeStacked, ref SlotMarker __result)
    {
        // ══════════ 以下为 0.9.9 旧内联判据（回滚用，保留未删）══════════
        // P0：装了东西的容器不产生堆叠槽位（纵深防御，万一 MayStack 已放行）。
        if (IsContainer(__instance) || IsContainer(itemToBeStacked))
        {
            __result = null;
            return false;
        }
        // P3（0.5.0）：黑名单纵深防御（万一 MayStack 已放行）。
        var blockReason = GetBlacklistReason(__instance, itemToBeStacked);
        if (blockReason != null)
        {
            Diag($"GetStackSlot 拦截（黑名单·{blockReason}）：{__instance.identifier} / {itemToBeStacked.identifier}");
            __result = null;
            return false;
        }
        // 0.6.0 方向4：容器黑名单，双端判定，纵深防御。★ 0.9.3：优先于白名单。
        var excludeReason = GetExcludeReason(__instance, itemToBeStacked);
        if (excludeReason != null)
        {
            Diag($"GetStackSlot 拦截（排除·{excludeReason}）：{__instance.identifier}");
            __result = null;
            return false;
        }
        // 需求3（0.5.1）：作用域纵深防御（万一 MayStack 已放行）。黑名单未命中才走到这里。
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
        // ★ 与合并前一致：同名且非空 ⇒ 不设 __result，交回调用方落到「共用段：构造 marker」。
        return null;
    }

    // 闸门3（GetMergeGateReason · TryAcceptOnce 路径）：0.9.9 旧判定顺序。
    private static string LegacyMergeGateReason(SlotMarker slot, GameItem target, GameItem source)
    {
        // ══════════ 以下为 0.9.9 旧顺序（回滚用，保留未删）══════════
        // 1) 柜台（0.6.5 新增，运行时判据）
        if (IsCounterSlot(slot)) return "交易柜台内禁止堆叠";

        // 2) 容器黑名单（0.6.5 起对快移路径也生效；0.9.3 起 token 匹配）
        //    ★ 黑名单优先：命中即 return，后续白名单判定不会覆盖（GetMergeGateReason 中
        //      黑名单在白名单之前 —— 用户 0.9.3 拍板：两个名单同时命中时黑名单赢）。
        //    双端判定：落点 或 拖起方 任一容器命中即拦。
        //    只拦「合并」；空槽放置（移动）仍放行（快移进/出黑名单容器的空槽不受限）。
        var exclude = GetExcludeReason(target, source);
        if (exclude != null) return exclude;

        // 3) 堆叠作用域（白名单）
        var scope = GetScopeReason(target);
        if (scope != null) return scope;

        // 4) 物品黑名单（鉴定/赃物/未拥有/电量/动物/鼠/酒瓶/耐久/液体容器/按id）
        var block = GetBlacklistReason(target, source);
        if (block != null) return block;

        // 5) 装了东西的容器禁止合并（与 MayStack 同判据，防数据丢失）
        if (IsContainer(target) || IsContainer(source)) return "容器（含内部物品）";

        return null;
    }
}
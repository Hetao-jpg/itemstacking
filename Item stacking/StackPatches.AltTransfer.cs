using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Preferences;
using UnityEngine;

namespace ItemStacking;

// Alt 快移作用域 + TryFindOneValidInventorySlot 落点改写
//
// ★ 两个作用域（[ThreadStatic] + 计数，异常安全）
//   AltTransferScope     —— Alt 快移
//   CtrlTransferScope    —— Ctrl 快移（合并闸门见 Merge.cs 的阶段 1）
//
// ★ Alt 快移的语义（0.13.x 定稿）
//   默认 AltPairMode="ctrl" ⇒ Alt 转发给原生 Ctrl handler 的 OnEventRelease
//   （raycast → 父窗口 → 活动容器 → 落点搜索全部交原生）。
//   本 mod 只保留「数量可配」+「机器容器改投空槽」两件事。
//   ⇒ AltRelayToCtrl 为真时【不进】本文件的作用域、也不改写落点。
//
// ★ Patch_TryFindOneValidSlot 的挂点选择
//   `GameInventory.TryFindOneValidInventorySlot` 是抽象方法（无 IL 体），
//   Harmony 不能直接 patch 抽象声明 ⇒ 用 [HarmonyTargetMethods] 运行期枚举具体子类。
//   详见 README.md「Alt 快移」章节。
public partial class Main
{
    // Alt 快移作用域：仅包住「会实际发生快移」的那一次原生调用。
    [ThreadStatic] private static int _altTransferScope;
    internal static bool AltTransferScope => _altTransferScope > 0;

    [HarmonyPatch(typeof(ItemQuickTransferAltHandler), "OnEventRelease")]
    private static class Patch_AltHandlerRelease
    {
        private static void Prefix(ItemQuickTransferAltHandler __instance)
            => AltTransferEnter(__instance.currentItem, __instance.isQuickTransfer);

        private static Exception Finalizer(Exception __exception)
            => AltTransferLeave(__exception);
    }

    [HarmonyPatch(typeof(ItemQuickTransferAltHandler1), "OnEventRelease")]
    private static class Patch_AltHandler1Release
    {
        private static void Prefix(ItemQuickTransferAltHandler1 __instance)
            => AltTransferEnter(__instance.currentItem, __instance.isQuickTransfer);

        private static Exception Finalizer(Exception __exception)
            => AltTransferLeave(__exception);
    }

    // 进入 Alt 快移作用域。只在「确实会发生快移」时置位。
    //   ★ AltRelayToCtrl 时直接不进 —— 避免 ShouldSteerToEmptySlot 恒导流
    //     和 Patch_TryFindOneValidSlot 改写被转发那次调用的落点/数量。
    private static void AltTransferEnter(GameItem currentItem, bool isQuickTransfer)
    {
        try
        {
            if (!AltTransferTakeover) return;
            // ★ 修复（0.14.4）：AltPairEnable=false 时完全交还原版 ——
            //   之前这里不检查 AltPairEnable，导致「关闭 Alt 走本 mod」后
            //   Patch_TryFindOneValidSlot 的 Alt 分支仍会改写落点/数量、拒绝合并落点
            //   ⇒ 用户以为回到原版，实际落点被静默篡改。
            if (_altPairEnable != null && !_altPairEnable.Value) return;
            if (AltRelayToCtrl) return;
            if (!isQuickTransfer) return;
            if (currentItem == null) return;

            _altTransferScope++;
            LogOnce("altenter." + SafeId(currentItem), Level.Diag,
                $"Alt 快移作用域进入（depth={_altTransferScope}）：{SafeId(currentItem)} ×{currentItem.unitCount}，" +
                $"本次件数={AltQuickTransferAmount}", max: 3);
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("altenter", Level.Warn, $"Alt 快移作用域进入异常，本次退回原生整堆行为: {e.Message}");
        }
    }

    // 离开 Alt 快移作用域（Finalizer：正常/异常都配对递减，绝不残留）。
    private static Exception AltTransferLeave(Exception __exception)
    {
        try
        {
            if (_altTransferScope > 0) _altTransferScope--;
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            Log.Error($"Alt 快移作用域退出异常: {e}");
        }
        return __exception;
    }

    // ── Alt 作用域内改写槽位查找结果（落点 + 数量） ──
    //
    // ★ 挂点选择（为什么不能直接 patch 抽象声明）
    //   `GameInventory.TryFindOneValidInventorySlot` 是 abstract（Slot 26，无 IL 体）
    //   ⇒ HarmonyX 对无 IL 体的抽象方法会抛 NRE；且不会自动展开到子类 override。
    //   ⇒ 用 [HarmonyTargetMethods] 运行期反射枚举「声明了自己的实现」的具体子类。
    //
    // ★ 三重严格判据（避免挂到不该挂的类型上崩进程）
    //   ① 严格继承链回溯到 GameInventory（不用 IsAssignableFrom，对 interop 类型不可靠）
    //   ② 只收 DeclaredOnly 方法（子类自己声明的）
    //   ③ 必须是 override（GetBaseDefinition != 自身），排除 `new virtual`
    //      （GameItem 有一个同名 `new virtual`，native body 为 0，挂上即 AccessViolation）
    [HarmonyPatch]
    private static class Patch_TryFindOneValidSlot
    {
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            var found = new List<MethodBase>();
            var skipped = new List<string>();

            // ★ TargetMethods 由 HarmonyX 在 PatchAll 期间反射调用，此时 Main.Log 可能仍为 null
            //   （HarmonyInit 阶段早于 OnInitializeMelon）⇒ 必须全程 try + null 安全日志。
            void LogSafe(string msg) { try { Log?.Msg(msg); } catch { } }
            void LogErrSafe(string msg) { try { Log?.Error(msg); } catch { } }

            try
            {
                var baseType = typeof(GameInventory);

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types;
                    try { types = asm.GetTypes(); }
                    catch (ReflectionTypeLoadException rtle)
                    {
                        // interop 程序集常有少量类型加载失败；其余类型仍可用。
                        types = rtle.Types;
                    }
                    catch { continue; }

                    foreach (var t in types)
                    {
                        try
                        {
                            if (t == null || t.IsAbstract) continue;
                            if (!IsStrictSubclassOf(t, baseType)) continue;

                            var m = AccessTools.DeclaredMethod(
                                t, nameof(GameInventory.TryFindOneValidInventorySlot),
                                new[] { typeof(GameItem) });
                            if (m == null) continue;
                            if (m.IsAbstract) continue;

                            MethodInfo baseDef = null;
                            try { baseDef = m.GetBaseDefinition(); } catch { }
                            if (baseDef == null || baseDef == m)
                            {
                                skipped.Add($"{t.Name}(非override)");
                                continue;
                            }

                            found.Add(m);
                        }
                        catch { /* 单个类型异常不影响其它 */ }
                    }
                }
            }
            catch (Exception e)
            {
                LogErrSafe($"Alt 落点挂点枚举异常（将退回不 patch）: {e}");
            }

            try
            {
                if (found.Count == 0)
                    LogSafe("Item Stacking：[Alt 落点] 未找到任何 TryFindOneValidInventorySlot 实现，" +
                            "Alt 落点改写将不可用（退回原生整堆/合并行为）。");
                else if (DiagOn)
                {
                    LogSafe($"[Alt 落点] 已挂载 {found.Count} 个 TryFindOneValidInventorySlot 实现：" +
                            string.Join(", ", found.ConvertAll(m => m.DeclaringType?.Name)));
                    if (skipped.Count > 0)
                        LogSafe("[Alt 落点] 已排除（非 override，挂上会崩）：" + string.Join(", ", skipped));
                }
            }
            catch { }

            return found;
        }

        // 严格继承链判定：逐级 BaseType 回溯到 baseType。
        //   不用 IsAssignableFrom —— 对 IL2CPP interop 类型实测不可靠，会把 GameItem 误判为子类。
        private static bool IsStrictSubclassOf(Type t, Type baseType)
        {
            for (var cur = t; cur != null; cur = cur.BaseType)
            {
                if (cur == baseType) return true;
                if (cur.FullName == "Il2CppSystem.Object" || cur == typeof(object)) break;
            }
            return false;
        }

        private static void Postfix(GameItem item, ref SlotMarker __result)
        {
            try
            {
                // ── 非 Alt 路径：抬升 GameGridInventory 硬编码的 1 件 ──
                //   反汇编实锤 GameGridInventory.TryFindOneValidInventorySlot 结尾
                //   无条件写 numTransfer = 1（因为它只用于「可行性探测」）。
                //   跨场景回店走 GraphUtils.TryAcceptAll（逐格填充循环）：
                //     · 第 1 轮拿到 numTransfer=1 → 只接受 1 件
                //     · 第 2 轮该槽已占 → 可行性检查失败返回 null → 循环 break
                //   ⇒ 整堆只搬进去 1 件。修复：落点是空槽却只给 1 件时抬到 min(StackCap, unitCount)。
                if (!AltTransferScope)
                {
                    if (__result != null && __result.targetItem == null && __result.numTransfer == 1
                        && item != null && item.unitCount > 1
                        && !IsMachineContainer(__result.inventory))   // 机器容器要保「每格 1 件」
                    {
                        int raised = Math.Min(StackCap, item.unitCount);
                        if (raised > 1)
                        {
                            __result.numTransfer = raised;
                            LogOnce("lift." + SafeId(item), Level.Diag,
                                $"[落点抬升] GameGridInventory 硬编码 1 件 → {raised}：" +
                                $"{SafeId(item)} ×{item.unitCount}（容器 {SafeId(__result.inventory)}，StackCap={StackCap}）", max: 3);
                        }
                    }
                    return;
                }

                if (item == null) return;

                // 探针：Alt 落点快照（限流，避免快速 Alt 快移刷屏）。
                LogOnce("altlot." + SafeId(item), Level.Diag,
                    $"[Alt 落点] 命中：item={SafeId(item)} ×{item.unitCount}，" +
                    $"原生结果={(__result == null ? "null" : "有")}" +
                    (__result == null
                        ? ""
                        : $"，targetItem={SafeId(__result.targetItem)}，容器={SafeId(__result.inventory)}" +
                          $"，numTransfer={__result.numTransfer}"), max: 3);

                // ── Alt 作用域内：只对空槽落点做数量钳制 ──
                //   合并落点理论上已被 MayStack 的 Alt 分支挡掉（见 MergeSteerer.ShouldSteerToEmptySlot）。
                //   下面两段是纵深防御。

                // ① 防御性：原生仍给合并落点 → 改投 null（拒绝合并）。
                if (__result != null && __result.targetItem != null)
                {
                    LogOnce("altlot.merge." + SafeId(__result.targetItem), Level.Diag,
                        $"[Alt 落点] 原生仍给合并落点 targetItem={SafeId(__result.targetItem)}" +
                        $"（预期已被 MayStack 挡住）→ 拒绝合并、改投 null", max: 3);
                    __result = null;
                    return;
                }

                if (__result == null) return;

                // ② 有效性兜底：空槽落点必须 numTransfer > 0（= 游戏自己的"能放下"判据）。
                if (__result.numTransfer <= 0)
                {
                    LogOnce("altlot.full", Level.Diag,
                        $"[Alt 落点] 空槽落点 numTransfer={__result.numTransfer} <= 0（放不下）→ 改投 null", max: 3);
                    __result = null;
                    return;
                }

                // ③ 数量钳制：只搬 N 个（源堆原地减 N，位置不变）。
                int amount = AltQuickTransferAmount;
                if (__result.numTransfer > amount)
                {
                    int before = __result.numTransfer;
                    __result.numTransfer = amount;
                    LogOnce("altlot.clamp." + SafeId(__result.inventory), Level.Diag,
                        $"[Alt 落点] 数量钳制：numTransfer {before} → {amount}（容器 {SafeId(__result.inventory)}，targetItem={SafeId(__result.targetItem)}）", max: 3);
                }
            }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                LogOnce("altslot", Level.Warn, $"Alt 快移落点改写异常，本次退回原生落点: {e.Message}");
            }
        }
    }

    // ── Ctrl 快移合并闸门（作用域标志） ──
    //   闸门本体（拦截 / 导流）在 Merge.cs 的 Patch_SlotMarkerTryAcceptOnce 阶段 1。
    //   本类只负责包一层作用域，让阶段 1 能区分「Ctrl 快移」与其它路径。
    //
    //   ★ 为什么需要作用域：SlotMarker.TryAcceptOnce 是 Alt 快移 / 口袋转移 /
    //     余量归还 / 普通拖放 / Ctrl 快移 的共用瓶颈，不加作用域会误伤其它路径。
    //   ★ 回滚：cfg CtrlTransferMergeGate（默认 true）。
    [ThreadStatic] private static int _ctrlTransferScope;
    internal static bool CtrlTransferScope => _ctrlTransferScope > 0;

    [HarmonyPatch(typeof(ItemQuickTransferHandler), "OnEventRelease")]
    private static class Patch_CtrlHandlerRelease
    {
        private static void Prefix(ItemQuickTransferHandler __instance)
        {
            try
            {
                if (_ctrlTransferMergeGate == null || !_ctrlTransferMergeGate.Value) return;
                if (__instance == null) return;
                if (__instance.currentItem == null) return;
                if (!__instance.isQuickTransfer) return;
                _ctrlTransferScope++;
            }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                LogOnce("ctrlenter", Level.Warn, $"Ctrl 快移作用域进入异常，本次退回原生: {e.Message}");
            }
        }

        private static Exception Finalizer(Exception __exception)
        {
            try { if (_ctrlTransferScope > 0) _ctrlTransferScope--; }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                Log.Error($"Ctrl 快移作用域退出异常: {e}");
            }
            return __exception;
        }
    }
}
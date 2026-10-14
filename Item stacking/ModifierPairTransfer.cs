using System;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace ItemStacking;

// 修饰键【快移】配对 / Alt→Ctrl 转发
//
// ★ 为什么需要这个文件（原生做不了）
//   原生 `ItemQuickTransferAltHandler.OnEventRelease` 是【硬编码窗口派发器】：
//   raycast 拿到父窗口 → 比对 EmporiumEntry 的几个窗口编号 → 一律搬进展示柜。
//   派发链里【没有】「柜台 ↔ 主仓库」的互相配对比较 ⇒ 原生不存在双向配对。
//   本功能是【真·新功能】，必须自己决定目标容器。
//
// ★ 两条路径
//   1) AltRelayToCtrl（cfg AltPairMode="ctrl"，默认）：Alt 转发给原生 Ctrl handler，
//      目标判定全部交原生。本 mod 只保留「数量可配」+「机器容器改投空槽」两件事。
//   2) counter_inv：本 mod 自算配对容器（玩家柜台 ↔ 主仓库），走 PairTransferCore。
//   Shift 配对（ShiftPairEnable）走 PairTransferCore（展示柜 ↔ 主仓库）。
//
// ★ 与 StackPatches.AltTransfer.cs 的共存
//   两者挂在同一方法 OnEventRelease 上（两个 Prefix）。Harmony 按注册顺序执行，
//   任一 return false 就跳过原方法；Finalizer 仍会执行 ⇒ 本类不依赖注册顺序。
//
// ★ 回滚：cfg AltPairMode 改成 "native" / 关 ShiftPairEnable / 关 ModifierKeysEnabled。
public partial class Main
{
    // ── Alt 配对生效（cfg：总闸 + AltClickTransferToCounter + AltPairEnable + 模式="counter_inv"）──
    internal static bool AltPairActive =>
        AltTransferTakeover
        && _altPairEnable != null && _altPairEnable.Value
        && _altPairMode != null && _altPairMode.Value == Constants.AltPairModeCounterInv;

    // ── Alt→Ctrl 转发生效（同上，模式="ctrl"）──
    //   Alt 与 Ctrl 用同一条目标判定链（raycast → 父窗口 → 活动容器 → 落点搜索）。
    internal static bool AltRelayToCtrl =>
        AltTransferTakeover
        && _altPairEnable != null && _altPairEnable.Value
        && _altPairMode != null && _altPairMode.Value == Constants.AltPairModeCtrl;

    // ── Shift 配对生效（cfg：总闸 + ShiftPairEnable）──
    internal static bool ShiftPairActive =>
        ModifierKeysEnabled && _shiftPairEnable != null && _shiftPairEnable.Value;

    // 转发重入深度：>0 时不再接管（防止 Alt handler 转发给 Ctrl 时被再次转发）。
    [ThreadStatic] private static int _relayDepth;

    // 配对搬运「禁止合并」标志（cfg AltPairMerge=false 时置位）。
    //   MayStack 的配对分支读它 → 恒返 false → 原生落点搜索自然走到"找空格"分支。
    //   ★ Alt / Shift 共用（合并开关也是同一个 AltPairMerge）。
    [ThreadStatic] private static int _pairNoMerge;
    internal static bool PairNoMerge => _pairNoMerge > 0;

    // Alt→Ctrl 转发的「单件 / 禁并」作用域。
    //   值 > 0 表示本次 SlotMarker.TryAcceptOnce 处于 Alt 转发场景。
    //   语义：只搬 AltQuickTransferAmount 件、不合并、优先空槽。
    //   下沉到 SlotMarker.TryAcceptOnce 执行（见 StackPatches.Merge.cs 阶段 -1）。
    //   ★ 为什么不预裁源堆：预裁是"拆 + 还 + 搬"三步，非原子，余量可能进背包，UI 闪。
    [ThreadStatic] private static int _altRelaySingleScope;

    // ── 三个 handler 的 OnEventRelease Prefix（按键分派） ──
    //
    // ★ Prefix 返回 false = 跳过原生（我们已自己搬运 + 复刻尾部清理）。
    // ★ 为什么不能合并成一个 [HarmonyTargetMethods] 补丁类：
    //   三个 handler 共享基类 InputActionHandler，但 currentItem / isQuickTransfer /
    //   key / quickTransferKey 四个成员是【各自在叶子类上声明的 property】，
    //   基类上一个都没有 ⇒ 单一 Prefix 只能按基类型取参、读不到这些字段。
    //   ⇒ 保留 3 个类，只把 try/catch 样板抽到 RunPairRelease。
    //
    // ★ 尾部清理为什么必须手动复刻：
    //   原生尾部 = ResetAfter + currentItem=null + （松开 quickTransferKey 时）isQuickTransfer=false。
    //   我们 return false 跳过原生 ⇒ 尾部清理也被一起跳过 ⇒ 复现「再按 Alt 还搬 / 单点左键也搬」bug。
    private static bool RunPairRelease(InputActionHandler handler, Func<bool> dispatch,
                                       Action clearCurrentItem, Action resetFlag,
                                       string logKey, string typeName)
    {
        try
        {
            if (dispatch())
            {
                NativeTailCleanup(handler, clearCurrentItem, resetFlag);
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce(logKey, Level.Warn, $"修饰键配对快移异常（{typeName}），本次交还原生: {e.Message}");
            return true;
        }
    }

    [HarmonyPatch(typeof(ItemQuickTransferHandler), "OnEventRelease")]
    private static class Patch_PairReleaseQT
    {
        private static bool Prefix(ItemQuickTransferHandler __instance)
            => RunPairRelease(__instance,
                () => DispatchPairRelease(__instance, __instance.currentItem, __instance.isQuickTransfer,
                                          __instance.key, __instance.quickTransferKey),
                () => __instance.currentItem = null,
                () =>
                {
                    // ★ 0.14.3 修复：按修饰键【当前是否按住】强制状态，不依赖复位时序。
                    //   原逻辑只在 GetKeyUp 时复位；ResetAfter 可能已先行复位 →
                    //   按住 Alt 连点第二次时 isQuickTransfer 已 false ⇒ 必须松开再按才行。
                    //   新逻辑：按住 → true（保持 armed）；松开 → false（需重新按）。
                    __instance.isQuickTransfer = Input.GetKey(__instance.quickTransferKey);
                }, "pair-qt", "ItemQuickTransferHandler");
    }

    // Alt→Ctrl 转发要用的【本次 keysReleased 集合】。
    //   原生 Ctrl handler 的 OnEventRelease 依赖它（`test rbx,rbx → je 退出`；
    //   `keysReleased.Contains(this.key)` 为假也退出）；不传 = 静默空转。
    //   ★ 用 object[] __args 取参而非按名注入 —— 两个 Alt 变体的形参名不一致
    //     （dump.cs 里一个叫 keysReleased、一个叫 keysPressed），__args[0] 与名字无关。
    [ThreadStatic] private static object _relayKeys;

    [HarmonyPatch(typeof(ItemQuickTransferAltHandler), "OnEventRelease")]
    private static class Patch_PairReleaseAlt
    {
        private static bool Prefix(ItemQuickTransferAltHandler __instance, object[] __args)
        {
            _relayKeys = (__args != null && __args.Length > 0) ? __args[0] : null;
            return RunPairRelease(__instance,
                () => DispatchPairRelease(__instance, __instance.currentItem, __instance.isQuickTransfer,
                                          __instance.key, __instance.quickTransferKey),
                () => __instance.currentItem = null,
                () => { __instance.isQuickTransfer = Input.GetKey(__instance.quickTransferKey); },
                "pair-alt", "ItemQuickTransferAltHandler");
        }
    }

    [HarmonyPatch(typeof(ItemQuickTransferAltHandler1), "OnEventRelease")]
    private static class Patch_PairReleaseAlt1
    {
        private static bool Prefix(ItemQuickTransferAltHandler1 __instance, object[] __args)
        {
            _relayKeys = (__args != null && __args.Length > 0) ? __args[0] : null;
            return RunPairRelease(__instance,
                () => DispatchPairRelease(__instance, __instance.currentItem, __instance.isQuickTransfer,
                                          __instance.key, __instance.quickTransferKey),
                () => __instance.currentItem = null,
                () => { __instance.isQuickTransfer = Input.GetKey(__instance.quickTransferKey); },
                "pair-alt1", "ItemQuickTransferAltHandler1");
        }
    }

    // ── 按键分派：按实例的 key/quickTransferKey 判定修饰键 → 路由 ──
    //   ★ 同时检查 key 与 quickTransferKey：prefab 里两字段语义无静态保证，任一是 Shift/Alt 即命中。
    private static bool DispatchPairRelease(InputActionHandler inst, GameItem item, bool isQuickTransfer,
                                            KeyCode key, KeyCode quickTransferKey)
    {
        if (_relayDepth > 0) return false;      // 转发中：不再接管（防互相转发）
        if (!isQuickTransfer) return false;
        if (item == null) return false;
        if (!ModifierKeysEnabled) return false;

        bool shift = IsShiftKey(key) || IsShiftKey(quickTransferKey);
        bool alt = IsAltKey(key) || IsAltKey(quickTransferKey);

        if (shift) return TryShiftPairTransfer(item);
        if (alt) return AltRelayToCtrl ? TryRelayItemToCtrl(item) : TryAltPairTransfer(item);
        return false;
    }

    private static bool IsShiftKey(KeyCode k) => k == KeyCode.LeftShift || k == KeyCode.RightShift;
    private static bool IsAltKey(KeyCode k) => k == KeyCode.LeftAlt || k == KeyCode.RightAlt;

    // ── 复刻原生尾部状态清理 ──
    //   顺序与原生一致：ResetAfter(this) → currentItem=null → resetFlag()。
    //   全程 try/catch：清理失败也绝不能让异常冒泡（否则打断输入管线）。
    private static void NativeTailCleanup(InputActionHandler handler, Action clearCurrentItem, Action resetFlag)
    {
        try
        {
            var mgr = InputActionManager.current;
            if (mgr != null) mgr.ResetAfter(handler);
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("pair-resetafter", Level.Warn, $"配对快移尾部 ResetAfter 失败（忽略）: {e.Message}");
        }

        try { clearCurrentItem(); }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("pair-clearitem", Level.Warn, $"配对快移尾部 currentItem 置空失败（忽略）: {e.Message}");
        }

        try { resetFlag(); }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("pair-resetflag", Level.Warn, $"配对快移尾部 isQuickTransfer 复位失败（忽略）: {e.Message}");
        }
    }

    // ── Alt 配对（玩家柜台 0x98 ↔ 主仓库 0x30） ──
    private static bool TryAltPairTransfer(GameItem item)
    {
        if (!AltPairActive) return false;
        var ee = EmporiumEntry.Instance;
        if (ee == null) return false;

        var counter = TryGet(() => ee.backInvinvElement);   // 玩家柜台（back counter，自己这侧）
        var inv = TryGet(() => ee.invElement);          // 主仓库

        bool noMerge = _altPairMerge != null && !_altPairMerge.Value;
        return PairTransferCore(item, counter, inv, "玩家柜台", "主仓库", noMerge, "[Alt配对]");
    }

    // ── Shift 配对（展示柜 0x40 ↔ 主仓库 0x30） ──
    private static bool TryShiftPairTransfer(GameItem item)
    {
        if (!ShiftPairActive) return false;
        var ee = EmporiumEntry.Instance;
        if (ee == null) return false;

        var showcase = TryGet(() => ee.showcaseElement);
        var inv = TryGet(() => ee.invElement);

        bool noMerge = _altPairMerge != null && !_altPairMerge.Value;   // 与 Alt 共用
        return PairTransferCore(item, showcase, inv, "展示柜", "主仓库", noMerge, "[Shift配对]");
    }

    // ══════════════════════════════════════════════════════════════════════
    // Alt → 转发给「Ctrl 快移」的原生逻辑（cfg AltPairMode="ctrl"，默认）
    //
    // ★ 做法：把本次按下的状态借给 ItemQuickTransferHandler（= Ctrl 的 handler），
    //   调用它自己的 OnEventRelease ⇒ raycast → FindParentWindow → CanInsertToActiveContainer
    //   → TryAcceptOnce 全链由原生完成，与「Ctrl+点击」逐字节同源。
    //   本 mod 只保留两件事：
    //     ① 数量：请求量下沉到 SlotMarker.TryAcceptOnce 钳制（阶段 -1）；
    //     ② 安全：转发后的这次调用同样受 Ctrl 合并闸门约束 ⇒ 机器容器自动改投空槽。
    //
    // ★ 为什么不自造落点判定（0.7.0 的教训）：
    //   GameInventory.TryInventorySlot 用的是【鼠标当前位置】；Alt 点击时鼠标在源物品上，
    //   放置校验全挂 ⇒ numTransfer=0 ⇒ 什么都不搬。复用原生那条链才是唯一稳的。
    //
    // ★ 失败面全部 fail-safe 交还原生 Alt。
    // ══════════════════════════════════════════════════════════════════════
    private static bool TryRelayItemToCtrl(GameItem item)
    {
        try
        {
            if (item == null) return false;

            var ctrl = TryGet(() => ItemQuickTransferHandler.current);
            if (ctrl == null) return false;

            // 防互相转发：Ctrl handler 的键位若其实是 Shift/Alt（prefab 数据异常），不转发。
            bool notCtrlKey = IsShiftKey(ctrl.key) || IsShiftKey(ctrl.quickTransferKey)
                           || IsAltKey(ctrl.key) || IsAltKey(ctrl.quickTransferKey);
            if (notCtrlKey)
            {
                LogOnce("relay-ctrl-key", Level.Warn,
                    $"[Alt→Ctrl 转发] Ctrl handler 的键位异常（key={ctrl.key} / quickTransferKey={ctrl.quickTransferKey}），" +
                    "为避免互相转发，本次交还原生 Alt。");
                return false;
            }

            // keysReleased null 时原生 Ctrl 什么都不做（静默空转）⇒ 明确告警并交还原生 Alt。
            var keys = _relayKeys as Il2CppSystem.Collections.Generic.ISet<KeyCode>;
            if (keys == null)
            {
                LogOnce("relay-keys", Level.Warn,
                    "[Alt→Ctrl 转发] 取不到本次 keysReleased（类型不符或形参缺失），为避免静默空转，本次交还原生 Alt。");
                return false;
            }

            int amount = AltQuickTransferAmount;

            _relayDepth++;
            _altRelaySingleScope++;                 // 进入「单件·禁并」作用域
            try
            {
                ctrl.currentItem = item;
                ctrl.isQuickTransfer = true;
                ctrl.OnEventRelease(keys);          // ← 原生 Ctrl：目标判定 + 落点搜索 + 搬运
                if (DiagOn)
                    Diag($"[Alt→Ctrl 转发] {SafeId(item)} ×{item.unitCount} 已按 Ctrl 逻辑搬移" +
                         $"（请求 {amount} 件、禁并、优先空槽）");
            }
            finally
            {
                if (_altRelaySingleScope > 0) _altRelaySingleScope--;
                // 显式清理 Ctrl handler 状态，防止原生 OnEventRelease 提前 return 时残留
                try { ctrl.currentItem = null; } catch { }
                try { ctrl.isQuickTransfer = false; } catch { }
                _relayDepth--;
            }
            return true;
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("relay-ctrl", Level.Warn, $"[Alt→Ctrl 转发] 异常，本次交还原生 Alt: {e.Message}");
            return false;
        }
    }

    // ── 公共配对核心（Alt counter_inv / Shift 共用） ──
    //   物品当前在 cA → 搬到 cB；在 cB → 搬到 cA；两者皆非 → return false 交还原生。
    //   返回 true = 已处理（含「非本人所有 → 拒绝搬运但已接管」的情形）。
    private static bool PairTransferCore(GameItem item, GameGridInventory cA, GameGridInventory cB,
                                         string nameA, string nameB, bool noMerge, string tag)
    {
        if (item == null) return false;
        if (cA == null || cB == null) return false;

        // 用物品自身的 parentInventory 反查它在哪个容器里（比鼠标落点可靠）。
        var srcInv = TryGet(() => item.parentInventory);
        if (srcInv == null) return false;

        GameGridInventory target;
        string dir;
        if (SameNativePointer(srcInv, cA)) { target = cB; dir = nameA + " → " + nameB; }
        else if (SameNativePointer(srcInv, cB)) { target = cA; dir = nameB + " → " + nameA; }
        else return false;   // 既不在 A 也不在 B → 交还原生

        if (target == null)
        {
            if (DiagOn) Diag($"{tag} 目标容器不存在（{dir}），本次交还原生：{SafeId(item)} ×{item.unitCount}");
            return false;
        }

        // 归属门：非本人所有不搬（对齐原生）。返回 false 交还原生 ——
        //   原生 unowned 分支是「静默不搬 + 响通用音 + 自己清理」，
        //   若我们 return true 会连音效 + 清理一起吞掉。
        bool owned;
        try { owned = GeneralHelper.IsItemOwned(item); }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("pair-owned", Level.Warn, $"配对快移归属判定异常，本次交还原生: {e.Message}");
            return false;
        }
        if (!owned)
        {
            if (DiagOn) Diag($"{tag} 物品非本人所有，不搬（{dir}）：{SafeId(item)}——交还原生按其归属门处理");
            return false;
        }

        int amount = AltQuickTransferAmount;
        if (amount <= 0) amount = -1;

        int moved;
        if (noMerge)
        {
            _pairNoMerge++;
            try { moved = TryAcceptOnceInto(target, item, amount); }
            finally { if (_pairNoMerge > 0) _pairNoMerge--; }
        }
        else
        {
            moved = TryAcceptOnceInto(target, item, amount);
        }

        if (DiagOn) Diag($"{tag} {dir}：{SafeId(item)} ×{item.unitCount} → 目标 {SafeId(target)}，" +
             $"请求={amount}，实移={moved}，合并={(noMerge ? "关（强制空位）" : "开")}");
        return true;
    }

    // ── 搬运原语：调原生 GraphUtils.TryAcceptOnce(GraphNodeStorage, GameItem, int) ──
    //   反汇编证实它内部就是逐格填充循环（FindValidSlot → MayStack → 单格 TryAcceptOnce），
    //   一个调用就能把 N 件搬完，无需自写循环。
    //
    //   ★ 类型转换：GraphNodeStorage 在游戏里是【接口】，但 interop 生成成了【类】，
    //     而 GameGridInventory 的 interop 基链不含它 ⇒ 不能隐式转换。
    //     用 Il2CppObjectBase.Cast<T>() 让 native 层按 il2cpp 类型校验后返回同指针包装。
    private static int TryAcceptOnceInto(GameGridInventory target, GameItem item, int amount)
    {
        try
        {
            var node = target.Cast<GraphNodeStorage>();
            if (node == null) return 0;
            return GraphUtils.TryAcceptOnce(node, item, amount);
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("pair-accept", Level.Warn, $"配对快移调用原生 TryAcceptOnce 异常，本次不搬: {e.Message}");
            return 0;
        }
    }
}
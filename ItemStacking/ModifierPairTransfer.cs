using System;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace ItemStacking;

// ══════════════════════════════════════════════════════════════════════════
// §四 功能1：修饰键【双向快移】配对
//   · Alt  ：玩家柜台(backInvinvElement 0x98) ↔ 主仓库(invElement 0x30)
//   · Shift：展示柜(showcaseElement 0x40)     ↔ 主仓库(invElement 0x30)
// ══════════════════════════════════════════════════════════════════════════
//
// ── 为什么需要这个文件（原生做不了）─────────────────────────────────────
//
//  ★★★ 2026-10-11 反汇编定论（capstone，逐分支取证）：
//    原生 `ItemQuickTransferAltHandler.OnEventRelease` (0x7E1620) 是一个
//    **硬编码「窗口 → 目标容器」派发器**：
//
//        elem = RaycastElement<GameItemElement>(mousePosition)   // 0x7E17FA
//        item = elem.item                     // [elem+0x18]
//        w    = elem.FindParentWindow()       // [elem+0x10]  (0x7C6A30)
//        EE   = EmporiumEntry.Instance
//
//        if (w == EE.invWindow      (0x28)) → TryAcceptOnce(EE.showcaseWindow(0x38), item, -1)
//        if (w == EE.backInvWindow  (0x90)) → TryAcceptOnce(EE.showcaseWindow, item, -1)
//        if (w == EE.frontInvWindow (0x80)) → TryAcceptOnce(EE.showcaseWindow, item, -1)
//        else /* 展示柜等其它 */ { ... → TryAcceptOnce(EE.showcaseWindow, item, -1) }
//
//    派发链里【没有】0x30(invElement) / 0x98(backInvinvElement) / 0x40(showcaseElement)
//    的「互相配对」比较 ⇒ 方案文档 §四 假设的「柜台 ↔ 主仓库」「展示柜 ↔ 主仓库」双向配对
//    **原生不存在**。⇒ 本功能是【真·新功能】，必须自己决定目标容器。
//
// ── 实现方案 ────────────────────────────────────────────────────────────
//   ① 配对方向由 cfg 决定：
//        · Alt  ：`AltPairMode` = "counter_inv"（默认，玩家柜台↔主仓库）
//                             或 "native"（交还原生：主仓库/后仓/柜台前格 → 展示柜）。
//        · Shift：`ShiftPairEnable` 开 = 展示柜↔主仓库；关 = 交还原生（单向导向展示柜）。
//   ★ 0.11.1 修正：「玩家柜台」= `EmporiumEntry.backInvinvElement`(0x98)。
//      0.11.0 曾误用 `frontInvinvElement`(0x88)（游戏内部名 front counter，属顾客侧），
//      实机表现为「物品跑到客户的柜台」。反汇编证据：
//        · `AddDirectSellingItemToTable`(0x586710) 写 0x88，日志串 "front counter full"；
//        · 玩家侧（自己这排）柜台 = 0x98，对应串 "back counter full"。
//      用户实机指认 0x98 为「玩家柜台」⇒ 采用 0x98。
//   ② 搬运原语**复用原生** `GraphUtils.TryAcceptOnce(GraphNodeStorage, GameItem, int)`(0x766730)
//      —— 反汇编证实它内部**本身就是逐格填充循环**（FindValidSlot → MayStack → TryAcceptOnce
//         单格 → n==0 break），所以**不需要自写循环**，正反双向同一个函数调用，天然对称。
//      这是「优先用官方现成 API」总原则的直接应用。
//   ③ 挂点 = 三个 handler 的 `OnEventRelease` **Prefix**：配对生效时，自己算目标容器 + 调
//      TryAcceptOnce，然后 **return false 跳过原生**；否则 return true 完全交还原生。
//      ★ 为什么挂 OnEventRelease 而不是复用现有 TryFindOneValidInventorySlot 改写：
//        现有改写只在「原生选定的落点容器」内改槽位；而本质需求是**换一个容器**，
//        落点改写做不到（它是鼠标位置驱动的，且改不了 container）。挂入口最干净。
//   ④ 数量 = `AltQuickTransferAmount`（Alt/Shift 共用同一配置，对称）。
//   ⑤ 合并开关 = `AltPairMerge`（Alt/Shift 共用）：
//        · 开 → 允许 TryAcceptOnce 合并同类堆（受 MergePolicy 全链路约束，含状态比较）。
//        · 关 → **强制逐件 + 找空位**：用 MayStack 导流（`_pairNoMerge` 标志，
//              见 Patch_MayStack 的配对分支），目标装不下则本次不搬（不丢件）。
//   ⑥ ★ 0.12.0：**按键分派**（不是按类名分派）——
//        三个 handler 结构逐字段一致（dump.cs：key@0x20 / quickTransferKey@0x24 /
//        currentItem@0x28 / isQuickTransfer@0x30），但「哪个类挂哪个修饰键」是
//        prefab 的 [SerializeField] 数据，静态不可见（Awake 只自注册 current，
//        反汇编实锤）⇒ 运行时读实例的 key/quickTransferKey 判键路由：
//          Shift → TryShiftPairTransfer；Alt → TryAltPairTransfer；其它(Ctrl 等) → 交还原生。
//
// ── 安全性 ─────────────────────────────────────────────────────────────
//   · Prefix 内全程 try/catch；异常 → return true 交还原生（fail-safe）。
//   · 只在「确实该接管」时 return false（配对存在 + 物品非空 + 已拥有 + 键位命中）。
//   · 复用 `_altTransferScope`（AltTransferScope）让现有 MayStack 导流逻辑继续生效。
//   · 所有游戏对象访问都做 null 检查；EmporiumEntry.Instance 为 null（非交易场景）→ 交还原生。
//   · ★ O3(a) 定案：Shift 点「既不在展示柜也不在主仓库」的物品 → return false 交还原生，
//     不擅自另找落点（尊重原生「导向展示柜」的兜底语义）。
//
// ── ★★ 与既有 `Patch_AltHandlerRelease`（StackPatches.AltTransfer.cs）的共存 ──
//   两者挂在**同一个方法** `OnEventRelease` 上（两个 Prefix）。Harmony 行为：
//     · 所有 Prefix 都会执行（按注册顺序）；任一返回 false ⇒ 跳过原方法体。
//     · **Finalizer 仍会执行**（即使原方法被跳过）⇒ 既有 scope 的 enter/leave 仍配对。
//   ⇒ 本类**不依赖**注册顺序，也不依赖 `AltTransferScope`。
//   ★ 回滚方式：把 cfg `AltPairMode` 改成 "native" / 关 `ShiftPairEnable` / 关总闸
//     `ModifierKeysEnabled` ⇒ 对应配对立刻退化为 `return false`（Prefix 返回 true），
//     行为等同未装本功能。
// ══════════════════════════════════════════════════════════════════════════

public partial class Main
{
    // ── Alt 配对生效（cfg: 总闸 + AltClickTransferToCounter + AltPairEnable + AltPairMode）──
    internal static bool AltPairActive =>
        AltTransferTakeover
        && _altPairEnable != null && _altPairEnable.Value
        && _altPairMode != null && _altPairMode.Value == Constants.AltPairModeCounterInv;

    // ★★ 2026-10-13（用户定稿）：Alt 改为【转发给 Ctrl 快移的原生逻辑】。
    //   语义 = 「Alt 与 Ctrl 的目标判定完全一致」：不再由本 mod 自造落点（旧：主仓库↔柜台固定配对），
    //   而是把状态借给 `ItemQuickTransferHandler`（Ctrl 的 handler）并调用它自己的 OnEventRelease，
    //   于是 raycast → 父窗口 → 活动容器 → 落点搜索 全部由游戏原生完成。
    //   本 mod 只保留两件事：① 数量可配（AltQuickTransferAmount，默认 1）；② 机器容器改投空槽。
    //   生效条件与 AltPairActive 同源，只是模式值换成 "ctrl"。
    internal static bool AltRelayToCtrl =>
        AltTransferTakeover
        && _altPairEnable != null && _altPairEnable.Value
        && _altPairMode != null && _altPairMode.Value == Constants.AltPairModeCtrl;

    // ── Shift 配对生效（cfg: 总闸 + ShiftPairEnable）──
    internal static bool ShiftPairActive =>
        ModifierKeysEnabled && _shiftPairEnable != null && _shiftPairEnable.Value;

    // ★ 2026-10-13：Ctrl 转发重入深度。>0 时：
    //   · DispatchPairRelease 不再接管（防止"Ctrl handler 的键位若恰是 Alt/Shift"时互相转发）；
    //   · 其余补丁照常工作（Ctrl 合并闸门 / 机器改投空槽 都要在这次转发里生效）。
    [ThreadStatic] private static int _relayDepth;

    // ── 配对搬运时的「禁止合并」标志（cfg: AltPairMerge 关闭时置位）──
    //   [ThreadStatic] 与 _altTransferScope 同款：只包住本次调用，异常安全。
    //   MayStack 的配对分支读它 → 恒返 false → 原生落点搜索自然走到"找空格"分支。
    //   ★ 0.12.0：Alt / Shift 共用同一标志（合并开关也是同一个 AltPairMerge）。
    [ThreadStatic] private static int _pairNoMerge;

    // 配对搬运是否正在「禁止合并」模式（供 Patch_MayStack 判定）。
    internal static bool PairNoMerge => _pairNoMerge > 0;

    // ── 挂点：三个 handler 的 OnEventRelease Prefix（按键分派）──
    //
    //   ★ Prefix 返回 false = 跳过原生方法体（我们已自己搬运）。
    //   ★ 返回 true = 交还原生（默认，或非交易场景，或任何异常）。
    //
    // ★★★ 0.11.2 关键修复（用户 2026-10-11 实机反馈）：「Alt+点击搬完之后，
    //   单独按 Alt 也会搬、单独点左键也会搬」—— 根因是 **我们 return false 跳过原生后，
    //   原生的尾部状态清理也被一起跳过了**（反汇编尾部：ResetAfter(this) / currentItem=null /
    //   松开 quickTransferKey 时 isQuickTransfer=false）。⇒ 一旦接管就**在 Prefix 里复刻尾部清理**。
    //
    //   ★ `keysReleased.Contains(quickTransferKey)` 的等价判据用 `Input.GetKeyUp(quickTransferKey)`：
    //     `InputActionManager.UpdateKeys` 正是用 `Input.GetKeyUp(k)` 在同帧构建 keysReleased 的
    //     （反汇编 0x7A9878），故同帧等价，且无需依赖 IL2CPP 的 `ISet<KeyCode>` 形参注入。
    //
    // ── 为什么不合并成一个 [HarmonyTargetMethods] 补丁类（2026-10-12 结论）──
    //   三个 handler **确实共享**非泛型基类 `InputActionHandler`（EmptyActionHandler →
    //   InputActionHandler），但 `currentItem` / `isQuickTransfer` / `key` / `quickTransferKey`
    //   四个成员是【各自在叶子类上声明的 property】，基类上一个都没有（已用 Mono.Cecil
    //   实测 interop DLL 确认）。合并后的单一 Prefix 只能按基类型取参 ⇒ 读不到这四个成员。
    //   ⇒ 按计划退回 fallback：**保留 3 个类**，只把 try/catch 样板抽到 RunPairRelease。
    //     （日志 key pair-qt / pair-alt / pair-alt1 保持独立，见 F3。）
    private static bool RunPairRelease(InputActionHandler handler, Func<bool> dispatch,
                                       Action clearCurrentItem, Action resetFlag,
                                       string logKey, string typeName)
    {
        // ★ 三个委托全部【在 try 之内】求值 —— 与合并前的三个手写 Prefix 逐语句等价：
        //   读 currentItem/key 抛异常也会被这里接住并 return true 交还原生。
        try
        {
            if (dispatch())
            {
                NativeTailCleanup(handler, clearCurrentItem, resetFlag);
                return false;      // 已处理：跳过原生（含其尾部清理，故上面手动复刻）
            }
            return true;           // 不接管：交还原生（原生自己会清理）
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
                () => { if (Input.GetKeyUp(__instance.quickTransferKey)) __instance.isQuickTransfer = false; },
                "pair-qt", "ItemQuickTransferHandler");
    }

    // ★ 2026-10-13：Alt→Ctrl 转发要用的【本次 keysReleased 集合】。
    //   为什么必须转发它（反汇编 `ItemQuickTransferHandler::OnEventRelease` 0x7E2090 实证）：
    //     test rbx, rbx → je 退出        （rbx = keysReleased 形参，null 直接什么都不做）
    //     keysReleased.Contains(this.key@0x20) 为假 → 退出
    //   即：原生 Ctrl 搬运**依赖这个集合**，不传 = 静默空转。
    //   ★ 用 Harmony 的 `object[] __args` 取参而非按名注入：两个 Alt 变体的形参名不一致
    //     （dump.cs 里一个叫 keysReleased、一个叫 keysPressed），按名注入会在其中一个上
    //     抛「找不到参数」。`__args[0]` 与名字无关，天然鲁棒。
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
                () => { if (Input.GetKeyUp(__instance.quickTransferKey)) __instance.isQuickTransfer = false; },
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
                () => { if (Input.GetKeyUp(__instance.quickTransferKey)) __instance.isQuickTransfer = false; },
                "pair-alt1", "ItemQuickTransferAltHandler1");
        }
    }

    // ── 按键分派：按实例的 key/quickTransferKey 判定修饰键 → 路由到对应配对 ──
    //   返回 true = 已接管（Prefix 应跳过原生并复刻尾部清理）；
    //   返回 false = 不接管（Prefix 交还原生）。
    //   ★ 同时检查 key 与 quickTransferKey：prefab 里两字段的语义无静态保证，任一是
    //     Shift/Alt 即命中（对字段语义不确定天然鲁棒）。
    private static bool DispatchPairRelease(InputActionHandler inst, GameItem item, bool isQuickTransfer,
                                            KeyCode key, KeyCode quickTransferKey)
    {
        if (_relayDepth > 0) return false;      // ★ 转发中：不再接管（防互相转发）
        if (!isQuickTransfer) return false;    // 非快移 → 交还原生
        if (item == null) return false;
        if (!ModifierKeysEnabled) return false; // 总闸关 → 交还原生

        bool shift = IsShiftKey(key) || IsShiftKey(quickTransferKey);
        bool alt   = IsAltKey(key)   || IsAltKey(quickTransferKey);

        if (shift) return TryShiftPairTransfer(item);
        if (alt)   return AltRelayToCtrl ? TryRelayItemToCtrl(item) : TryAltPairTransfer(item);
        return false;                          // 其它修饰键（Ctrl 等）→ 交还原生
    }

    private static bool IsShiftKey(KeyCode k) => k == KeyCode.LeftShift || k == KeyCode.RightShift;
    private static bool IsAltKey(KeyCode k)   => k == KeyCode.LeftAlt   || k == KeyCode.RightAlt;

    // ── 复刻原生 OnEventRelease 的尾部状态清理（我们跳过原生时手动执行）──
    //   顺序与原生一致：ResetAfter(this) → currentItem=null → (松开快移键时) isQuickTransfer=false。
    //   全程 try/catch：清理失败也绝不能让异常冒泡（否则会打断游戏输入管线）。
    private static void NativeTailCleanup(InputActionHandler handler, Action clearCurrentItem, Action resetFlag)
    {
        try
        {
            var mgr = InputActionManager.current;
            if (mgr != null) mgr.ResetAfter(handler);   // 与原生 0x1807e1d67 一致
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("pair-resetafter", Level.Warn, $"配对快移尾部 ResetAfter 失败（忽略）: {e.Message}");
        }

        try { clearCurrentItem(); }             // ★ 核心：清空 currentItem，杜绝「重复再搬一次」
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("pair-clearitem", Level.Warn, $"配对快移尾部 currentItem 置空失败（忽略）: {e.Message}");
        }

        try { resetFlag(); }                    // 与原生一致：仅当本次松开的是快移键时才复位开关
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("pair-resetflag", Level.Warn, $"配对快移尾部 isQuickTransfer 复位失败（忽略）: {e.Message}");
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  §四·2 两套配对入口（各自取容器 + 合并开关，再共用 PairTransferCore）
    // ══════════════════════════════════════════════════════════════════════

    // Alt 配对：玩家柜台(0x98) ↔ 主仓库(0x30)
    private static bool TryAltPairTransfer(GameItem item)
    {
        if (!AltPairActive) return false;
        var ee = EmporiumEntry.Instance;
        if (ee == null) return false;              // 非交易场景 → 交还原生

        //   ★ 0.11.1：「玩家柜台」= `backInvinvElement`(0x98)（back counter，玩家自己这侧），
        //     不是 `frontInvinvElement`(0x88)（front counter，顾客侧）。
        var counter = TryGet(() => ee.backInvinvElement);   // 0x98  玩家柜台
        var inv     = TryGet(() => ee.invElement);          // 0x30  主仓库

        bool noMerge = _altPairMerge != null && !_altPairMerge.Value;
        return PairTransferCore(item, counter, inv, "玩家柜台", "主仓库", noMerge, "[Alt配对]");
    }

    // Shift 配对：展示柜(0x40) ↔ 主仓库(0x30)
    private static bool TryShiftPairTransfer(GameItem item)
    {
        if (!ShiftPairActive) return false;
        var ee = EmporiumEntry.Instance;
        if (ee == null) return false;              // 非交易场景 → 交还原生

        var showcase = TryGet(() => ee.showcaseElement);    // 0x40  展示柜
        var inv      = TryGet(() => ee.invElement);         // 0x30  主仓库

        bool noMerge = _altPairMerge != null && !_altPairMerge.Value;   // 与 Alt 共用合并开关
        return PairTransferCore(item, showcase, inv, "展示柜", "主仓库", noMerge, "[Shift配对]");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  ★★ 2026-10-13（用户定稿）：Alt → 转发给「Ctrl 快移」的原生逻辑
    //
    //  ── 需求原话 ──
    //    「把 Alt+鼠标左键点击快移的目标窗口判定，改为和 Ctrl 键一样的逻辑。」
    //      → 关键点不是"鼠标所在窗口"，而是【不要本 mod 自造落点】，直接复用 Ctrl 那套。
    //
    //  ── 做法（不做任何自造判定）──
    //    把本次按下的状态借给 `ItemQuickTransferHandler`（= 按 Ctrl 的那个 handler），
    //    然后调用它自己的 `OnEventRelease()` —— 于是
    //      RaycastElement → TreeNodeRender::FindParentWindow → CanInsertToActiveContainer/
    //      InsertToActiveContainer → GraphUtils.TryAcceptOnce
    //    整条链全部由游戏原生完成，与「Ctrl+点击」【逐字节同源】。
    //    本 mod 只保留两件事：
    //      ① 数量：转发前把源堆预裁到 `AltQuickTransferAmount`（默认 1），余量归还源容器；
    //      ② 安全：被转发的这次调用同样受 Ctrl 合并闸门约束 ⇒ 机器容器会自动【改投空槽】。
    //
    //  ── 为什么不自造落点判定（上一版的教训）──
    //    0.7.0 曾自己调 `GameInventory.TryInventorySlot(item,shape,state)`(0x784580) 找空格，
    //    但那用的是**鼠标当前位置**；Alt 点击时鼠标在源物品上 ⇒ 放置校验全挂 ⇒ numTransfer=0
    //    ⇒ 什么都不搬。⇒ 复用原生那条链才是唯一稳的。
    //
    //  ── 失败面（全部 fail-safe，交还原生 Alt）──
    //    · 没有 Ctrl handler 实例（未进场景）→ false；
    //    · Ctrl handler 的键位竟不是 Ctrl（prefab 异常）→ false（防互相转发）；
    //    · 任何异常 → false。
    // ══════════════════════════════════════════════════════════════════════
    private static bool TryRelayItemToCtrl(GameItem item)
    {
        try
        {
            if (item == null) return false;

            var ctrl = TryGet(() => ItemQuickTransferHandler.current);
            if (ctrl == null) return false;         // 未进场景 / 未注册 → 交还原生 Alt

            // 防互相转发：Ctrl handler 的键位若其实是 Shift/Alt（prefab 数据异常），不转发。
            bool notCtrlKey = IsShiftKey(ctrl.key) || IsShiftKey(ctrl.quickTransferKey)
                           || IsAltKey(ctrl.key)   || IsAltKey(ctrl.quickTransferKey);
            if (notCtrlKey)
            {
                LogOnce("relay-ctrl-key", Level.Warn,
                    $"[Alt→Ctrl 转发] Ctrl handler 的键位异常（key={ctrl.key} / quickTransferKey={ctrl.quickTransferKey}），" +
                    "为避免互相转发，本次交还原生 Alt。");
                return false;
            }

            // ★ 本次的 keysReleased（Alt handler 收到的那份，直接转交）。
            //   null 时原生 Ctrl 会「什么都不做」（test rbx,rbx → je 退出），属静默空转，
            //   故这里明确告警并交还原生 Alt，不留无声失败面。
            var keys = _relayKeys as Il2CppSystem.Collections.Generic.ISet<KeyCode>;
            if (keys == null)
            {
                LogOnce("relay-keys", Level.Warn,
                    "[Alt→Ctrl 转发] 取不到本次 keysReleased（类型不符或形参缺失），为避免静默空转，本次交还原生 Alt。");
                return false;
            }

            // ① 数量预裁（只裁不丢）
            int amount = AltQuickTransferAmount;    // 已钳在 [1,9999]；=1 即"逐件快移"
            if (amount > 0) PreTrimToAmount(item, amount);

            // ② 借状态 + 转发原生 Ctrl 逻辑
            _relayDepth++;
            try
            {
                ctrl.currentItem = item;
                ctrl.isQuickTransfer = true;
                ctrl.OnEventRelease(keys);          // ← 原生 Ctrl：目标判定 + 落点搜索 + 搬运
                if (DiagOn)
                    Diag($"[Alt→Ctrl 转发] {SafeId(item)} ×{item.unitCount} 已按 Ctrl 逻辑搬移（数量上限 {amount}）");
            }
            finally
            {
                _relayDepth--;
            }
            return true;                            // 已处理：RunPairRelease 会接着复刻 Alt handler 的尾部清理
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("relay-ctrl", Level.Warn, $"[Alt→Ctrl 转发] 异常，本次交还原生 Alt: {e.Message}");
            return false;
        }
    }

    // 把 item 预裁到 amount 件（★只裁不丢）：
    //   拆出的余量走《DestroyFix》同款「归还源容器（含背包兜底）」路径，
    //   与 `SplitBeforeDestroy + ReturnRemainder` 用的是同一个原语与同一份归还逻辑。
    //   语义依据（反汇编 §3.2 `GetSplitItem`）：原堆留原处并减少 n，返回的新实例是"拆出的那 n 件"。
    private static void PreTrimToAmount(GameItem item, int amount)
    {
        try
        {
            if (item == null || amount <= 0) return;
            int n = item.unitCount;
            if (n <= amount) return;                 // 无需裁剪

            var srcInv = TryGet(() => item.parentInventory);
            GameItem rem = null;
            try { rem = item.GetSplitItem(n - amount); } catch { }
            if (rem != null)
            {
                ReturnRemainder(rem, srcInv);        // DestroyFix 的余量归还（源容器 → 背包 → 告警）
                if (DiagOn)
                    Diag($"[Alt→Ctrl 转发] 预裁数量：{SafeId(item)} {n} → {item.unitCount}，余量 {rem.unitCount} 已归还");
            }
            else
            {
                LogOnce("relay-trim", Level.Warn,
                    $"[Alt→Ctrl 转发] 预裁失败（本次可能搬走整堆 {n} 件）：{SafeId(item)}");
            }
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("relay-trim-ex", Level.Warn, $"[Alt→Ctrl 转发] 预裁异常（忽略，按原数量搬）: {e.Message}");
        }
    }

    // ── 公共配对核心（Alt / Shift 共用，O2）──
    //   cA / cB = 两个互为源/目标的配对容器；nameA/nameB 用于日志；tag 为日志前缀。
    //   规则：物品当前在 cA → 搬到 cB；在 cB → 搬到 cA；**两者皆非 → return false 交还原生**（O3a）。
    //   返回 true = 已处理（含「非本人所有 → 拒绝搬运但已接管」的情形）。
    private static bool PairTransferCore(GameItem item, GameGridInventory cA, GameGridInventory cB,
                                         string nameA, string nameB, bool noMerge, string tag)
    {
        if (item == null) return false;
        if (cA == null || cB == null) return false;   // 某侧配对容器此刻不存在 → 交还原生（原生有兜底）

        // ★ 判定「这一件现在在哪个容器里」→ 决定目标容器（配对是双向的）。
        //   用物品自身的 parentInventory 反查（比鼠标落点更可靠，且不受 Raycast 影响）。
        var srcInv = TryGet(() => item.parentInventory);
        if (srcInv == null) return false;

        GameGridInventory target;
        string dir;
        if (SameNativePointer(srcInv, cA))
        {
            target = cB; dir = nameA + " → " + nameB;
        }
        else if (SameNativePointer(srcInv, cB))
        {
            target = cA; dir = nameB + " → " + nameA;
        }
        else
        {
            // 物品既不在 A 也不在 B（例如在柜台/后仓/背包）→ 不接管，交还原生。
            return false;
        }

        if (target == null)
        {
            if (DiagOn) Diag($"{tag} 目标容器不存在（{dir}），本次交还原生：{SafeId(item)} ×{item.unitCount}");
            return false;   // 配对容器不存在 → 交还原生
        }

        // ★ 归属门（对齐原生：IsItemOwned 为假则不搬，防盗窃/防误移他人财物）。
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
            // ★ H4 取证结论（2026-10-12，反汇编 ItemQuickTransferAltHandler.OnEventRelease 0x7E1620）──
            //   原生对未拥有物品的处理（两处 IsItemOwned 调用 0x7E1BE3 / 0x7E1CF0）：
            //     `test al,al; je 0x7E1D4B` → false 分支直跳**共享尾部清理**
            //     （InputActionManager.ResetAfter + currentItem=null），**不搬、无拒绝音、
            //     无光标/Toast 等专门反馈** —— 即原生 unowned 分支 = 「静默跳过搬运」。
            //   但注意：handler 开头有一次**通用释放音效**（AudioManager.TryPlayOne @ 0x7E190E）
            //     在归属门【之前】播放，原生路径下 unowned 也会响；且尾部清理由原生自己做。
            //   ⇒ 若此处 return true（跳过原生），会连通用音效+原生清理一起吞掉，
            //     与「关闭 mod 时」的表现出现可感知差异。故改为 return false 交还原生：
            //     原生自己走归属门 → 静默不搬 + 响通用音 + 自己清理 ⇒ 与原版逐项一致。
            return false;   // 交还原生（原生 unowned 分支本身就不会搬）
        }

        int amount = AltQuickTransferAmount;     // ≤0 视为整堆（-1）
        if (amount <= 0) amount = -1;

        int moved;
        if (noMerge)
        {
            _pairNoMerge++;
            try
            {
                moved = TryAcceptOnceInto(target, item, amount);
            }
            finally
            {
                if (_pairNoMerge > 0) _pairNoMerge--;
            }
        }
        else
        {
            moved = TryAcceptOnceInto(target, item, amount);
        }

        if (DiagOn) Diag($"{tag} {dir}：{SafeId(item)} ×{item.unitCount} → 目标 {SafeId(target)}，" +
             $"请求={amount}，实移={moved}，合并={(noMerge ? "关（强制空位）" : "开")}");
        return true;   // 已处理
    }

    // ── 搬运原语：调用原生 `GraphUtils.TryAcceptOnce(GraphNodeStorage, GameItem, int)` ──
    //   ★ 反汇编证实它内部就是逐格填充循环（0x766730 → … → MayStack → 单格 TryAcceptOnce），
    //     所以这一个调用就能把 N 件一次搬完，无需我们自写循环。
    //   ★ 返回实际搬运件数（0 = 一件都没搬，含目标满/状态不符/被 MayStack 挡）。
    //
    //   ★★ 类型转换说明（踩坑记录）：
    //     `GraphNodeStorage` 在游戏里是【接口】，但 Il2CppInterop 把它生成成了【类】
    //     （`class GraphNodeStorage : Il2CppObjectBase`）—— 而 `GameGridInventory` 的 interop
    //     基链是 `GameInventory → GameInventoryFunc → TagSystem`，**不含** `GraphNodeStorage`。
    //     ⇒ 不能隐式转换（CS1503）。正确做法 = Il2CppInterop 的标准机制 `Il2CppObjectBase.Cast<T>()`，
    //       它在 native 层按 il2cpp 类型校验后返回同指针的包装对象。
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

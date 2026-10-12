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
    // Alt 快移：OnEventRelease 作用域 + TryFindOneValidInventorySlot 落点改写
    // ══════════════════════════════════════════════════════════════════
    // ═══════════════════════════════════════════════════════════════════════
    // 0.7.0（Alt 快移重构）：复用原生"落点窗口"逻辑，只改「落点」与「数量」
    //
    // ── 0.6.4~0.6.6 的实现为什么不够好（三个实机现象的根因）──
    //   旧实现：Alt 快移时先 `src.GetSplitItem(unitCount - N)` 把源堆拆开，
    //   放行原生移走 N 个，再在 Finalizer 里用 `TryAcceptAllMid` 把"溢出部分"塞回源容器。
    //   现象①「移 1 个后其余自动寻路到别处」：
    //     `TryAcceptAllMid` 是**全容器搜索最佳落点**（先合并同名堆、再找空格），
    //     不是"放回原来那一格" ⇒ 溢出被寻路搬到别的空格去了。
    //   现象②「柜台已有同名物品时无法再移入」：
    //     原生快移只用 `TryFindOneValidInventorySlot` 取**落点窗口里第一个有效槽**，
    //     而"有效槽"优先是**已有同名堆** → 走合并 → 被本 mod 的柜台禁堆闸门
    //     （Patch_SlotTryAcceptOnce）返回 0 拒绝；且原生循环**第一个非空容器就 break、不 continue**
    //     ⇒ 拒绝后不再找空格 ⇒ 看起来"放不上去"。
    //   现象③「只能仓库→柜台」：假象。三个快移处理器（Ctrl / Alt / Alt1）逻辑逐条一致，
    //     目标容器都是"鼠标落点所在窗口下的容器" ⇒ **原生天然双向**。
    //
    // ── 0.7.0 的改法（更简单、更贴近原生）──
    //   ★ 关键认识：`SlotMarker.TryAcceptOnce` 内部
    //       `n = Min(amount, marker.numTransfer, item.MaxNumRemove())`
    //     ⇒ **只要把 marker.numTransfer 钳小，就无需拆堆即可控制移动数量**；
    //       而 `GetSplitItem` 只改 `[item+0x1F8] -= n`、**不动槽位**
    //       ⇒ **源堆原地减 N、位置完全不变**（正是需求②）。
    //   ★ 挂点选在 `GameInventory.TryFindOneValidInventorySlot`（vtable **slot 26**）。
    //     调用链已实锤：`ItemQuickTransfer*.OnEventRelease`
    //       → `GraphUtils.TryAcceptOnce(node, item, -1)`（0x766730）
    //       → 经 IL2CPP 接口解析 stub（0x180003FA0，`[vtable+0xB0]` 接口表）
    //         调 `GraphNodeStorage` 接口 **slot 4 = TryFindOneValidInventorySlot**
    //       （dump.cs：`GraphNodeStorage` slot 4 = TryFindOneValidInventorySlot(GameItem)；
    //         `GameInventory` vtable slot 26 = 它的实现。）
    //     三方一致 ⇒ **改这个方法的返回值，就是改原生的落点**。
    //
    //   在 Alt 快移作用域内（scope 标志，见 AltTransferScope）：
    //     ① 若 `__result.targetItem != null`（原生选了"合并同名堆"）→
    //        改用"找空格"的落点（由原生自己的 `TryInventorySlot` 网格碰撞给出，
    //        **天然避开已有物品、不重叠**）。
    //        → 需求①：再次移动单件、避开原有物品、不堆叠、不 UI 重叠。
    //     ② 把最终 marker 的 `numTransfer` 钳到 AltQuickTransferAmount
    //        → 需求②：只搬 N 个、源堆原地减 N。
    //
    //   ★ 为什么"找空格"不用自己写算法：反汇编 `GameGridInventory.TryFindOneValidInventorySlot`
    //     （0x77B750）确认它内部就是「先按 MayStack 找同名堆，找不到再走 TryInventorySlot
    //     按网格形状找空格」，空格由网格碰撞挑出 ⇒ 不重叠。
    //     我们只需把"合并结果"换成"再调一次、但要求空槽"的结果。
    //
    //   ★ 删除旧机制：整套 `GetSplitItem 拆堆 + FlushAltOverflow/ReturnAltOverflow 归还`
    //     已整体移除（它是现象①的元凶，也是"静默丢件"风险的最大来源）。
    // ═══════════════════════════════════════════════════════════════════════

    // Alt 快移作用域标志：仅包住 Alt 快移那一次原生调用。
    // 用 [ThreadStatic] + 计数，异常安全（Finalizer 配对递减）。
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

    // ── 0.7.0：Alt 作用域内「禁止合并」的闸门 ──
    //
    // ★ 不新增补丁类 —— 直接在既有的 `Patch_MayStack`（闸门 1）**Prefix 开头加一段 Alt 判定**
    //   （见文件上部 `Patch_MayStack.Prefix` 的 `AltTransferScope` 分支）。
    //
    // ★ 为什么用 `GameItem.MayStack`（0x799120）：它是"两物品能否叠在一起"的**唯一否决点**。
    //   原生 4 个 `TryFindOneValidInventorySlot` 实现的"合并同名堆"循环**都调它**
    //   （实锤：`GameGridScrollableInventory` 0x77BB4E 循环体内 `call 0x799120`；
    //     `MayStack==false` → `je 0x77BB30` 继续循环 → 循环耗尽的**唯一出口**
    //     就是它自己的"找空格"分支 0x77BCAD，其成功判据是 `numTransfer > 0`
    //     实锤 0x77BDBA `cmp [rax+0x38],0` / `jg` → return）。
    //   ⇒ Alt 作用域内让它恒 false，原生就**自己**给出真正的空格落点：
    //     避开已有物品、不堆叠、不重叠（需求①），且**不依赖鼠标坐标**。
    //
    // ★ 为什么不在 Postfix 里自己找空格（上一版的错）：
    //   `GameInventory.TryInventorySlot(item, shape, state)`(0x784580) 用的是**鼠标当前位置**
    //   （`[单例+0xB8]+0x38/0x3C`）。Alt 点击时鼠标在源物品上 → 放置校验全挂 →
    //   marker 的 `numTransfer = 0` → `TryAcceptOnce` 算 `n = Min(amount,0,…) = 0`
    //   → **什么都不搬**。实机日志实锤：`TryInventorySlot → targetItem=null, numTransfer=0`。
    //
    // ★ 只在 `AltTransferScope` 内生效 ⇒ 普通拖放堆叠完全不受影响。

    // 进入 Alt 快移作用域。只在「确实会发生快移」时置位，其余一律不碰。
    private static void AltTransferEnter(GameItem currentItem, bool isQuickTransfer)
    {
        try
        {
            // 未开总闸 / 未开接管 → 不进作用域，完全交还原生。
            if (!AltTransferTakeover) return;
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

    // ── 核心：Alt 作用域内改写槽位查找结果（落点 + 数量）──
    //
    // ★★★ 挂点选择（2026-10-08 反汇编实锤 + HarmonyX 源码核对）：
    //   `GameInventory.TryFindOneValidInventorySlot` 在 dump.cs 里是 **abstract**
    //   （`// RVA: -1 ... Slot: 26`），**没有实现体**。
    //   HarmonyX 2.10.2 的 `AccessTools.GetMethodPatcher` 对没有 IL 体的抽象方法
    //   会抛 `NullReferenceException("No suitable patcher found …")` ——
    //   **不能直接 patch 抽象声明**（且 HarmonyX 不会自动展开到子类 override，
    //    已核对 PatchClassProcessor/PatchFunctions 源码，无此逻辑）。
    //
    //   ⇒ 正确做法：**枚举所有「声明了自己的 TryFindOneValidInventorySlot」的具体子类**
    //     并逐个 patch。用 [HarmonyTargetMethods] 在运行期反射枚举，未来游戏加子类也自动覆盖。
    //   实测存在实现的具体子类（script.json）：
    //     GameGridInventory / GameGridScrollableInventory / GameSlotInventory /
    //     GameCharacterRaidInventory（均为 GameInventory 直接子类）。
    //   （`GameItem` / `GridPixelElement` / `PixelWindow` 等也恰好有同名方法，
    //     但它们不是 GameInventory 子类，**必须排除**，否则误伤。）
    [HarmonyPatch]
    private static class Patch_TryFindOneValidSlot
    {
        // 枚举所有「真正继承 GameInventory、且自己 override 了该方法」的具体类型。
        //
        // ★★★ 2026-10-08 实机翻车后重写（原版枚举导致启动即 AccessViolation 闪退）：
        //   原因：`GameItem` **不是** GameInventory 子类，却混进了候选（当时用
        //         `GameInventory.IsAssignableFrom(GameItem)` 判定 —— 在 IL2CPP interop 类型上
        //         该判据**不可靠**：GameItem 声明了一个同名 `public unsafe **virtual**`
        //         方法（非 override），其 native 方法指针为 0，
        //         被 Harmony 挂 detour 时 `Dobby.HookAttach(0)` → AccessViolationException → 进程猝死。
        //
        //   ⇒ 改为【三重严格判据】，任一不满足即**明确排除**（并打日志说明原因）：
        //     ① 必须能从 `t.BaseType` 逐级回溯到 `GameInventory`（严格继承链，不用 IsAssignableFrom）。
        //     ② 方法必须是 **override**（`GetBaseDefinition() != 自身`），排除 `new virtual` 声明
        //        （GameItem 那种）；此类方法 native body 可能为 0，挂了就崩。
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            var found = new List<MethodBase>();
            var skipped = new List<string>();

            // ★★ 0.7.0 修复（实机 NRE 根因）：`TargetMethods()` 由 HarmonyX 在 PatchAll 期间
            //   **反射调用**，此时 **`Main.Log` 可能仍为 null** —— 报错栈实锤：
            //     `Failed to HarmonyInit PatchAll: ItemStacking.Main+Patch_TryFindOneValidSlot`
            //     `NullReferenceException at Patch_TryFindOneValidSlot.TargetMethods()`
            //   `TargetMethods()` **没有 try 包住的那几行**（`var baseType = typeof(GameInventory);`
            //   之外，第一次实机失败发生在这里之前/之间），且**所有 `Log.*` 调用都在 try 外**
            //   → Log 为 null 时直接 NRE 冒泡，整类补丁注册失败（4 个实现的 Postfix 全丢）。
            //   `IsStrictSubclassOf` / `AccessTools.DeclaredMethod` 自身的异常也同理。
            //   ⇒ 对策：**全程 try 包住，且用一个 null 安全的日志 helper**（保留原有诊断输出）。
            void LogSafe(string msg)
            {
                try { Log?.Msg(msg); } catch { /* 日志失败不影响挂点枚举 */ }
            }
            void LogErrSafe(string msg)
            {
                try { Log?.Error(msg); } catch { }
            }

            try
            {
                var baseType = typeof(GameInventory);

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types;
                    try { types = asm.GetTypes(); }
                    catch (ReflectionTypeLoadException rtle)
                    {
                        // ★ 不能整段跳过 —— IL2CPP interop 程序集常有少量类型加载失败，
                        //   但绝大多数类型（含我们要找的）在 ex.Types 里可用。
                        types = rtle.Types;
                    }
                    catch { continue; }

                    foreach (var t in types)
                    {
                        try
                        {
                            if (t == null || t.IsAbstract) continue;

                            // ① 严格继承链判定（逐级 BaseType 回溯到 Il2Cpp.GameInventory）。
                            if (!IsStrictSubclassOf(t, baseType)) continue;

                            // ② 只收「自己声明」的方法（DeclaredOnly）。
                            var m = AccessTools.DeclaredMethod(
                                t, nameof(GameInventory.TryFindOneValidInventorySlot),
                                new[] { typeof(GameItem) });
                            if (m == null) continue;
                            if (m.IsAbstract) continue;      // 抽象（无体）→ 跳过

                            // ③ 必须是 override（GetBaseDefinition 不等于自身），
                            //    排除 `new virtual` 声明（native body 可能为 0 → 挂上即崩）。
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
                // ★ 故障（一个都没挂上）属告警，始终输出 —— 否则 Alt 快移静默退化成原生整堆。
                if (found.Count == 0)
                    LogSafe("Item Stacking：[Alt 落点] 未找到任何 TryFindOneValidInventorySlot 实现，" +
                            "Alt 落点改写将不可用（退回原生整堆/合并行为）。");
                // ★ 以下两条是「挂载明细」诊断信息，受 DiagLog 总闸控制（0.9.0）。
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

        // 严格继承链判定：逐级 `BaseType` 回溯，命中 baseType 才为 true。
        // （不用 Type.IsAssignableFrom —— 对 IL2CPP interop 类型实测不可靠，
        //   会把 GameItem 这种非子类误判为子类。）
        private static bool IsStrictSubclassOf(Type t, Type baseType)
        {
            for (var cur = t; cur != null; cur = cur.BaseType)
            {
                if (cur == baseType) return true;
                // 到 Il2CppSystem.Object / System.Object 即止，避免死循环
                if (cur.FullName == "Il2CppSystem.Object" || cur == typeof(object)) break;
            }
            return false;
        }

        private static void Postfix(GameItem item, ref SlotMarker __result)
        {
        try
        {
            // ★★★ 0.7.1 探针：非 Alt 作用域下的「单件钳制」排查（跨场景转移只带 1 个的嫌疑点）。
            //   背景（用户 2026-10-08 反馈）：「堆叠物品从垃圾站/配给站/杰克逊博士转场景回商店，
            //   堆叠数变成 1，再次回场景时多余的那些还在原地」—— 典型的"只搬了 1 个"。
            //   本探针在【非 Alt】路径且原生给出 numTransfer > 1 时记录一次（按容器+数量去重），
            //   用于确认「原生本身也只给 1」还是「某个下游把数量吃掉了」。
            if (!AltTransferScope && __result != null && __result.numTransfer > 0)
                ProbeNonAltMarker(item, __result);

            // ★★★ 0.7.1 修复：「GameGridInventory 硬编码 numTransfer=1」导致的整堆只搬 1 件。
            //
            //   反汇编实锤 `GameGridInventory.TryFindOneValidInventorySlot`(0x763AE0) 的结尾：
            //       0x180763B89  mov dword ptr [rbx + 0x38], 1     ; ★ 无条件 numTransfer = 1
            //   它**是** `GameGridInventory` 的一个合法实现（可行性检查通过就返回 marker），
            //   但把"这一格能放几个"永远写成 1 —— 因为它本来只用于"可行性探测"。
            //
            //   为什么这会变成实机故障（用户 2026-10-08）：
            //     `EmporiumEntry.frontInvinvElement` 的类型就是 **GameGridInventory**，
            //     跨场景回店时 `ExpeditionHelper.SetHirelingEquipmentOnCounter`(0x8B6820)
            //     走 `GraphUtils.TryAcceptAll(柜台, item, -1)`(0x766650) —— 那是**逐格填充循环**：
            //        { 找槽 → SlotMarker.TryAcceptOnce(剩余) → 累加；若某轮返回 0 或 numTransfer<=0 就 break }
            //     第 1 轮拿到 `numTransfer=1` → 只接受 1 件；第 2 轮该槽已被占 →
            //     `GameGridInventory` 的可行性检查失败 → 返回 null → **循环 break**。
            //     ⇒ 整堆只搬进去 1 件，其余"留在原地"（正是用户描述）。
            //
            //   ⇒ 修复：非 Alt 路径下，若落点是**空槽**（targetItem == null）却只给 1 件，
            //     就把它抬到 `min(StackCap, item.unitCount)` —— 语义与 `Patch_MaxNumAdd` 的空槽
            //     分支完全一致（那条路径被 GameGridInventory 绕过了，所以这里补上）。
            //   ★ 只抬升、不收紧：原生给 >1 的一律不动；非空槽（合并落点）不动。
            //   ★ 抬到 unitCount 而非直接 StackCap，避免"旧档大堆"借道绕过新上限。
            if (!AltTransferScope)
            {
                if (__result != null && __result.targetItem == null && __result.numTransfer == 1
                    && item != null && item.unitCount > 1)
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

            // ★ 0.7.0 曾改为逐次无条件输出，0.9.0 收回 DiagLog 总闸之下 ——
            //   排查 Alt 落点时开 DiagLog 即可看到每一次的完整快照。
            //   ★ 0.12.2 限流：同一物品最多打 3 次，避免快速 Alt 快移刷屏。
            if (DiagOn)
            {
                try
                {
                    LogOnce("altlot." + SafeId(item), Level.Diag,
                        $"[Alt 落点] 命中：item={SafeId(item)} ×{item.unitCount}，" +
                        $"原生结果={( __result == null ? "null" : "有")}" +
                        (__result == null
                            ? ""
                            : $"，targetItem={SafeId(__result.targetItem)}，容器={SafeId(__result.inventory)}" +
                              $"，numTransfer={__result.numTransfer}"), max: 3);
                }
                catch { }
            }

            // ★★ 0.7.0 修订（2026-10-08 17:47 实机 + 反汇编双证）：
            //   **不再自己用 TryInventorySlot 找空格**（上一版的做法），
            //   而是**在 Alt 作用域内让 `GameItem.MayStack` 一律返回 false** ——
            //   于是原生 `TryFindOneValidInventorySlot` 的"合并同名堆"循环
            //   （GameGridScrollableInventory 0x77BB4E 处 `call MayStack(0x799120)`）
            //   **自然一个都不命中**，直接落到它**自己的"找空格"分支**（0x77BCAD），
            //   返回一个真正可用的空槽 marker（其成功判据同样是 `numTransfer > 0`，
            //   实锤：0x77BDBA `cmp [rax+0x38],0` `jg` 才 return）。
            //
            //   ── 为什么废弃上一版（自己调 TryInventorySlot）──
            //   `GameInventory.TryInventorySlot(item, shape, state)`(0x784580) 内部把
            //   **鼠标当前位置**当作 itemGraphicsCenterPosition 转发给 5 参版本
            //   （0x7845E8 起 `[某单例+0xB8] + 0x38/0x3C`）。
            //   Alt 点击时鼠标**在源物品上、不在柜台格内** ⇒ 放置校验全失败 ⇒
            //   返回 marker 的 `numTransfer = 0`（`GameGridInventory.TryInventorySlot`
            //   0x763BC0 里三个校验任一 `je 0x763daf` 就直接跳尾声、不写 numTransfer）。
            //   日志实锤：`TryInventorySlot → targetItem=null, numTransfer=0`
            //   ⇒ 我把它当"空格"返回 → `SlotMarker.TryAcceptOnce` 算 `n=Min(amount,0,…)=0`
            //   ⇒ **什么都不搬 = "Alt 放不过去"**。
            //
            //   现在这条路彻底不需要位置：交给游戏自己的空格搜索。
            //   （`MayStack` 的 gate 见 `Patch_MayStack`，只在 AltTransferScope 内生效。）

            // ① 若原生仍返回"合并落点"（理论上 MayStack 已挡掉；防御性兜底）
            //    → 改投 null，让原生 continue 去试下一个容器（避免真的合并）。
            if (__result != null && __result.targetItem != null)
            {
                if (DiagOn)
                {
                    try
                    {
                        Log?.Msg($"[Alt 落点] 原生仍给合并落点 targetItem={SafeId(__result.targetItem)}" +
                                 $"（预期已被 MayStack 挡住）→ 拒绝合并、改投 null");
                    }
                    catch { }
                }
                __result = null;
                return;
            }

            if (__result == null) return;

            // ② 有效性兜底：空槽落点必须 `numTransfer > 0`（= 游戏自己的"能放下"判据）。
            if (__result.numTransfer <= 0)
            {
                if (DiagOn)
                {
                    try
                    {
                        Log?.Msg($"[Alt 落点] 空槽落点 numTransfer={__result.numTransfer} <= 0（放不下）→ 改投 null");
                    }
                    catch { }
                }
                __result = null;
                return;
            }

            // ③ 数量钳制：只搬 N 个（需求②：源堆原地减 N、位置不变）。
            int amount = AltQuickTransferAmount;
            if (__result.numTransfer > amount)
            {
                int before = __result.numTransfer;
                __result.numTransfer = amount;
                if (DiagOn)
                {
                    try
                    {
                        Log?.Msg($"[Alt 落点] 数量钳制：numTransfer {before} → {amount}（容器 {SafeId(__result.inventory)}，targetItem={SafeId(__result.targetItem)}）");
                    }
                    catch { }
                }
            }
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("altslot", Level.Warn, $"Alt 快移落点改写异常，本次退回原生落点: {e.Message}");
        }
    }
    }

    // （SafeId 已抽出至 Il2CppHelpers.cs，2026-10-12。）

    // ══════════════════════════════════════════════════════════════════════
    //  ★ 0.12.1 · Ctrl 快移合并闸门（问题1）
    // ══════════════════════════════════════════════════════════════════════
    // ── 现象（用户 2026-10-11）──
    //   「Ctrl+鼠标左键点击快移，可以无视容器堆叠白黑名单，强制移动到目标容器内。」
    //
    // ── 根因（反汇编 `ItemQuickTransferHandler.OnEventRelease` 0x7e2090 实证）──
    //   Ctrl 快移【不走】拖放路径，也【不走】Alt 落点改写路径，而是：
    //     RaycastElement<GameItemElement> → 拿鼠标下物品
    //       → FindParentWindow → 落点窗口
    //       → 一串 EmporiumEntry 窗口编号硬编码比对（EE+0x28 主仓库 / +0x90 柜台 / +0x80 前格 …）
    //       → 得目标 GameInventory
    //       → call 0x1807e273f `GraphUtils.TryAcceptOnce(容器, 物品, -1)`（一次调用，整堆）
    //   ★★ `GraphUtils.TryAcceptOnce`(0x766730) 内部【现造 SlotMarker】，其 targetItem 由
    //      「找第一个有效槽」决定 —— 若该槽有同类堆即【合并】。这条链**完全不查 `MayStack`**
    //      （已反汇编实锤：`SlotMarker.TryAcceptOnce` 内无 MayStack 调用），
    //      也**不经过** `GameInventory.TryFindOneValidInventorySlot`（那条只服务 Alt）。
    //   ⇒ 于是 Ctrl 快移的【合并】分支绕过了 MergePolicy 全部闸门（含白黑名单）。
    //
    // ── 对策（用户定调：「只在合并时拦，放置不管」）──
    //   照抄已验证的 Alt 作用域范式（Patch_AltHandlerRelease 的 Prefix + Finalizer）：
    //     ① 在 `ItemQuickTransferHandler.OnEventRelease` 上包一层「Ctrl 快移作用域」标志；
    //     ② 在通用瓶颈 `SlotMarker.TryAcceptOnce` 上加 Prefix：作用域内且 targetItem != null
    //        （= 合并）时问 MergePolicy；Deny 则【返 0 硬拒】——物品留在源容器（fail-safe，
    //        不丢、不覆盖、不溢出）。
    //     ③ 放置分支（targetItem == null）一律放行。
    //
    //   ★ 为什么【返 0 硬拒】而不是「置 targetItem=null 导流」：
    //     在 `SlotMarker.TryAcceptOnce`(0x7b0020) 里，`targetItem != null` 意味着原生已经
    //     **锁定了一个已有同名堆的槽**。把 targetItem 置 null 会让原生把这件物品当"放进
    //     那个已占用的格"（放置分支），可能**覆盖/溢出**那一堆 —— 不可接受。
    //     返 0 =「本次没收」。`GraphUtils.TryAcceptOnce`(0x766730) 是单次调用（非 TryAcceptAll
    //     的逐格循环），返 0 ⇒ 剩余件数留在源容器，安全。
    //   ★ 需要真正"落空位"的场景由 `Patch_MayStack` 的既有 Deny→false 承担（若 Ctrl 快移
    //     经 MayStack 会自然导流）；本闸门只兜住"完全绕过 MayStack"的合并。
    //
    //   ★ 作用域标志是【必须】的：`SlotMarker.TryAcceptOnce` 是 Alt 快移 / 口袋转移 /
    //     余量归还 / 普通拖放 / Ctrl 快移 的共用瓶颈。不加作用域会误伤其它路径。
    //
    //   ★ 回滚：cfg `CtrlTransferMergeGate`（默认 true）。关 = 完全不进作用域，行为与改前一致。
    // ══════════════════════════════════════════════════════════════════════

    // Ctrl 快移作用域标志：仅包住 Ctrl 快移那一次 OnEventRelease。
    //   [ThreadStatic] + 计数，异常安全（Finalizer 配对递减，绝不残留）。
    [ThreadStatic] private static int _ctrlTransferScope;
    internal static bool CtrlTransferScope => _ctrlTransferScope > 0;

    [HarmonyPatch(typeof(ItemQuickTransferHandler), "OnEventRelease")]
    private static class Patch_CtrlHandlerRelease
    {
        private static void Prefix(ItemQuickTransferHandler __instance)
        {
            try
            {
                if (!_ctrlTransferMergeGate.Value) return;
                if (__instance == null) return;
                // Ctrl 快移的触发条件与原生一致：currentItem 非空 且 isQuickTransfer 为真。
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

    // Ctrl 快移作用域内的【合并闸门·唯一落点】。
    //   挂 `SlotMarker.TryAcceptOnce(int)`(0x7b0020) —— 它是合并/放置的**共同瓶颈**，
    //   且**只有在这里**才能读到引擎现造的 `targetItem`（合并分支的判据）。
    //   ★ 与既有的 `Patch_SlotTryAcceptOnce`（纯观测直通，0.10.1）**不同**：
    //     那个补丁任何时候都不干预；本补丁**只在 Ctrl 快移作用域内**拦截违禁合并。
    //
    //   ★★ 为什么「返 0 硬拒」而不是「把 targetItem 置 null 导流」：
    //     · `SlotMarker.TryAcceptOnce` 拿到的 marker 由 `GraphUtils.TryAcceptOnce`
    //       内"找第一个有效槽"决定 —— 若它返回的是【已有同名堆】的槽位（targetItem!=null），
    //       说明原生已锁定了**那一格**。若我们把 targetItem 置 null，原生会把这件物品
    //       当作"放进那个已占用的格"（放置分支），可能**覆盖/溢出**该堆 —— 不可接受。
    //     · 返 0 =「本次什么都没收」。对 `GraphUtils.TryAcceptOnce`（单次调用，非 TryAcceptAll
    //       的逐格循环）而言，返回量 = 剩余未搬数 ⇒ 物品**留在源容器**，fail-safe。
    //     · 真正要"放进空位"（导流到空槽）的正解是让原生重新找空格 —— 那走 MayStack 层：
    //       若 Ctrl 快移**经过** MayStack（与本 mod Alt 同源，实测三条快移 handler 一致），
    //       现有 `Patch_MayStack.Prefix` 已对任何 Deny 返回 false → 原生落空槽；
    //       若**不经过** MayStack（完全绕过），本闸门返 0 兜底，杜绝违禁合并。
    [HarmonyPatch(typeof(SlotMarker), nameof(SlotMarker.TryAcceptOnce), new[] { typeof(int) })]
    private static class Patch_CtrlMergeGateOnSlotMarker
    {
        private static bool Prefix(SlotMarker __instance, ref int __result)
        {
            try
            {
                if (!CtrlTransferScope) return true;      // ★ 只服务 Ctrl 快移，其余直通
                if (!StackEnabled) return true;

                var target = __instance.targetItem;
                if (target == null) return true;          // 空槽 = 放置 → 明确放行（用户定调）

                var source = __instance.item;
                if (source == null) return true;

                // 复用单一源决策（含白名单/黑名单/状态闸门）。inventory 作权威容器。
                var reason = GetMergeGateReason(__instance, target, source);
                if (reason == null) return true;          // Allow → 直通原生合并

                // ★ 违禁合并 → 返 0 硬拒（fail-safe：物品留在源容器，不丢、不覆盖）。
                __result = 0;
                Diag($"[Ctrl 快移·拦截] 合并候选被判定为「{reason}」→ 拒绝本次合并（物品留在源处）：" +
                     $"源 {SafeId(source)} ×{source.unitCount} → 目标堆 {SafeId(target)} ×{target.unitCount}" +
                     $"（容器 {SafeId(__instance.inventory)}）");
                return false;
            }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                LogOnce("ctrlgate", Level.Warn, $"Ctrl 快移合并闸门异常，本次放行合并: {e.Message}");
                return true;
            }
        }
    }
}


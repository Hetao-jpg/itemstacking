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
    // ══════════════════════════════════════════════════════════════════════
    //  0.9.6：堆叠物品被"整堆销毁"修复补丁
    //
    //  问题（用户 2026-10-10 反馈）：
    //    把一叠电子元件拖到水瓶打印机上触发升级，只升级了 1 次，
    //    但整叠 N 件全部消失 —— 原版 UpgradePrinter 内部对整个 GameItem
    //    调 Destroy()，不检查 unitCount。
    //
    //  根因：原版假设 unitCount == 1（单件交互）。堆叠 mod 下 unitCount = N
    //    ⇒ Destroy 一次销毁整堆，但逻辑上只消耗 1 件。
    //
    //  同类路径（均待修）：
    //    MachineBottlePrinter.UpgradePrinter(printer, upgradeItem)
    //    ModuleHelper.RestoreModule(gameItem, electronicPart)
    //    ModuleHelper.RepairExtractor(extractor, electronicPart)
    //
    //  修复思路（Prefix + Postfix）：
    //    Prefix：若 unitCount > 1，先 GetSplitItem 把多余部分拆成独立
    //            GameItem（原品减到 1），存入 __state。
    //    原版正常运行：只 Destroy 那 1 件（正确行为）。
    //    Postfix：把拆出的余量塞回源容器（parentInventory）。
    //            源容器满时用 GraphUtils.TryAcceptAll 全容器搜索。
    // ══════════════════════════════════════════════════════════════════════

    // ── 共用：余量归还 ──
    //   remainder = 从原品拆出的多余部分（已不在任何容器中）。
    //   sourceInv = 拆堆前记录的源容器（可能是 null —— 拖拽中物品已脱离容器）。
    private static void ReturnRemainder(GameItem remainder, GameInventory sourceInv)
    {
        if (remainder == null) return;
        try
        {
            // ① 优先塞回源容器
            if (sourceInv != null)
            {
                try
                {
                    var grid = sourceInv as GameGridInventory;
                    if (grid != null)
                    {
                        int accepted = GraphUtils.TryAcceptAllMid(grid, remainder);
                        if (accepted >= remainder.unitCount)
                        {
                            Diag($"[销毁修复] 余量已归还源容器：{SafeId(remainder)} ×{remainder.unitCount}");
                            return;
                        }
                        int leftover = remainder.unitCount - accepted;
                        if (leftover > 0 && accepted > 0)
                            remainder = remainder.GetSplitItem(leftover) ?? remainder;
                    }
                }
                catch (Exception e)
                {
                    Diag($"[销毁修复] 塞回源容器失败: {e.Message}");
                }
            }

            // ② 兜底：尝试塞进玩家背包（EmporiumEntry.invElement）
            var entry = TryGet(() => EmporiumEntry.Instance);
            var backpack = entry != null ? TryGet(() => entry.invElement) : null;
            if (backpack != null && !SameInventory(backpack, sourceInv))
            {
                try
                {
                    int accepted = GraphUtils.TryAcceptAllMid(backpack, remainder);
                    if (accepted >= remainder.unitCount)
                    {
                        Diag($"[销毁修复] 余量已归还背包：{SafeId(remainder)} ×{remainder.unitCount}");
                        return;
                    }
                    remainder = remainder.GetSplitItem(remainder.unitCount - accepted) ?? remainder;
                }
                catch (Exception e)
                {
                    Diag($"[销毁修复] 塞回背包失败: {e.Message}");
                }
            }

            // ③ 实在放不下：打警告（不静默丢弃，让用户知道）
            WarnOnce("remainder",
                $"[销毁修复] 余量 {SafeId(remainder)} ×{remainder.unitCount} 无法归还任何容器！");
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            WarnOnce("remainder-ex", $"[销毁修复] 归还余量异常: {e.Message}");
        }
    }

    // ── 共用：销毁前拆堆（Prefix 主体）／销毁后归还余量（Postfix 主体）──
    //   ★ §七 2.3（0.11.0）：补丁 1/2/3（UpgradePrinter / RestoreModule / RepairExtractor）
    //     的 Prefix+Postfix 除「被拆物品参数名 + 日志 key」外完全相同。此前提炼公共实现，
    //     三处只保留三行委托 —— 行为逐字节一致（含异常语义 / 日志 key），可单独回滚
    //     （把某处改回手写即可）。
    //
    //   SplitBeforeDestroy：若 item.unitCount > 1，拆出 (n-1) 件为独立 GameItem（原品减到 1），
    //     原品保持原位不动（原版随后 Destroy 的仍是这 1 件）。
    //     返回 (源容器, 余量)；无需处理时返回 (null, null)。
    //     ★ 返回元组而非 out 参数，便于 Prefix 里的 __state 直接赋值得原子化。
    private static (GameInventory inv, GameItem rem) SplitBeforeDestroy(GameItem item, string logKey, string tag)
    {
        try
        {
            if (item == null) return (null, null);
            int n = item.unitCount;
            if (n <= 1) return (null, null);                  // 单件，无需处理

            GameInventory srcInv = TryGet(() => item.parentInventory);
            GameItem rem = item.GetSplitItem(n - 1);
            Diag($"[销毁修复] {tag} 前拆堆：{SafeId(item)} {n} → 1，余量={SafeId(rem)} ×{rem?.unitCount ?? 0}");
            return rem != null ? (srcInv, rem) : (null, null);
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            WarnOnce(logKey, $"{tag} 拆堆异常（不影响原操作）: {e.Message}");
            return (null, null);
        }
    }

    // 销毁后归还余量（Postfix 主体）。__state 由 SplitBeforeDestroy 填充。
    private static void ReturnRemainderAfter((GameInventory inv, GameItem rem) state, string logKey, string tag)
    {
        try
        {
            if (state.rem != null)
                ReturnRemainder(state.rem, state.inv);
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            WarnOnce(logKey, $"{tag} 归还余量异常: {e.Message}");
        }
    }

    // ── 共用：读/写槽位 currentItem（0.9.8）──
    //   GameSlotInventory.currentItem 是私有成员，且 interop 可能暴露成属性
    //   （get_currentItem/set_currentItem 均存在）或字段 —— 运行时按「字段优先、
    //   属性兜底」解析，避免编译期依赖成员形态。
    private static GameItem GetSlotCurrent(object slot)
    {
        if (slot == null) return null;
        try
        {
            const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic
                                 | BindingFlags.Instance;
            var t = slot.GetType();
            var f = t.GetField("currentItem", F);
            if (f != null) return f.GetValue(slot) as GameItem;
            var p = t.GetProperty("currentItem", F);
            if (p != null) return p.GetValue(slot) as GameItem;
            WarnOnce("no-currentitem", $"[槽位成员] {t.Name} 无 currentItem 成员（游戏改版？），清槽跳过");
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            WarnOnce("get-currentitem", $"[槽位成员] 读 currentItem 异常: {e.Message}");
        }
        return null;
    }

    private static void SetSlotCurrent(object slot, GameItem value)
    {
        if (slot == null) return;
        try
        {
            const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic
                                 | BindingFlags.Instance;
            var t = slot.GetType();
            var f = t.GetField("currentItem", F);
            if (f != null) { f.SetValue(slot, value); return; }
            var p = t.GetProperty("currentItem", F);
            if (p != null) { p.SetValue(slot, value); return; }
            WarnOnce("no-currentitem-set", $"[槽位成员] {t.Name} 无 currentItem 可写（游戏改版？），清槽跳过");
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            WarnOnce("set-currentitem", $"[槽位成员] 写 currentItem 异常: {e.Message}");
        }
    }

    // ── 补丁 1：MachineBottlePrinter.UpgradePrinter ──
    [HarmonyPatch(typeof(MachineBottlePrinter), nameof(MachineBottlePrinter.UpgradePrinter))]
    private static class Patch_UpgradePrinter
    {
        private static void Prefix(GameItem printer, GameItem upgradeItem, out (GameInventory inv, GameItem rem) __state)
            => __state = SplitBeforeDestroy(upgradeItem, "up-printer", "UpgradePrinter");

        private static void Postfix((GameInventory inv, GameItem rem) __state)
            => ReturnRemainderAfter(__state, "up-printer-pf", "UpgradePrinter");
    }

    // ── 补丁 2：ModuleHelper.RestoreModule ──
    [HarmonyPatch(typeof(ModuleHelper), nameof(ModuleHelper.RestoreModule))]
    private static class Patch_RestoreModule
    {
        private static void Prefix(GameItem gameItem, GameItem electronicPart, out (GameInventory inv, GameItem rem) __state)
            => __state = SplitBeforeDestroy(electronicPart, "restore-mod", "RestoreModule");

        private static void Postfix((GameInventory inv, GameItem rem) __state)
            => ReturnRemainderAfter(__state, "restore-mod-pf", "RestoreModule");
    }

    // ── 补丁 3：ModuleHelper.RepairExtractor ──
    [HarmonyPatch(typeof(ModuleHelper), nameof(ModuleHelper.RepairExtractor))]
    private static class Patch_RepairExtractor
    {
        private static void Prefix(GameItem extractor, GameItem electronicPart, out (GameInventory inv, GameItem rem) __state)
            => __state = SplitBeforeDestroy(electronicPart, "repair-ext", "RepairExtractor");

        private static void Postfix((GameInventory inv, GameItem rem) __state)
            => ReturnRemainderAfter(__state, "repair-ext-pf", "RepairExtractor");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  0.9.8：三个「堆叠只搬 1 件 / 整堆被销毁」修复
    // ══════════════════════════════════════════════════════════════════════

    // ── 补丁 4：EmporiumEntry.TransferAfterhourToInv（拾荒口袋整堆跨场景转移）──
    //   问题（用户 2026-10-11 反馈）：转换场景（回家）时，拾荒口袋里的堆叠物品
    //   只能判定带走 1 件。
    //   根因（反汇编 0x7D7D00）：原版对 3 个口袋槽（Backpack 0x158 / Right 0x160 /
    //   Left 0x150）逐个调 GraphUtils.TryAccept(dest, item) —— 而 TryAccept(0x7667E0)
    //   内部硬编码 `SlotMarker.TryAcceptOnce(marker, amount=1)`（0x766832 `lea edx,[r8+1]`）：
    //   找到落点就只搬 1 件、返回 ≥0 即进入下一个口袋，永远不搬第 2 件。
    //   单件时代正确；堆叠 mod 下口袋放 N 件 → 只带 1 件回家。
    //
    //   参考容器的跨场景修复（0.7.1，StackPatches.cs Patch_FindValidSlot）：
    //   容器/柜台走的是 GraphUtils.TryAcceptAll 逐格填充循环
    //   （ExpeditionHelper.SetHirelingEquipmentOnCounter → TryAcceptAll(柜台, item, -1)），
    //   配合 numTransfer 抬升可以搬完整堆 —— 口袋这条路没走 TryAcceptAll，
    //   所以单件钳制的抬升救不了它。
    //
    //   修复：Prefix 用与容器同款的逐格填充循环 GraphUtils.TryAcceptAllMid(dest, item, -1)
    //   按 dest 链整堆转移（interop 未给 GameInventory 实现 GraphNodeStorage 接口，编译期用不了
    //   TryAcceptAll，Mid 版参数就是 GameGridInventory，0.9.6 归还余量同款）；
    //   全部移走后清空 pocket.currentItem，让原版单件逻辑看到 null 跳过。
    //   部分失败（目的地全满）：留槽不动 + WarnOnce —— 原版逻辑对其同样无害
    //   （dest 链已耗尽，原版 TryAccept 全部返回 -1 落空）。
    //
    //   ★★ 1.0.0 修正（玩家反馈 2）：dest 链从 3 级【砍为 2 级】，移除 showcaseElement 兜底。
    //   0.9.8 照抄了原版 3 级链（0x98 → 0x30 → 0x40），当 backInv/inv 装不下时物品滑进
    //   showcaseElement（展示柜）—— 这正是「口袋堆叠物品回家被错误导流到展示柜」的直接来源。
    //   showcase 只是链尾兜底，不是设计落点；砍掉后拿不下的部分留在口袋（原版语义）。
    //   与"unitCount=1 不触发"的现象吻合（下方 `unitCount <= 1` 直接 return，走原生路径）。
    [HarmonyPatch(typeof(EmporiumEntry), nameof(EmporiumEntry.TransferAfterhourToInv))]
    private static class Patch_TransferAfterhourToInv
    {
        private static void Prefix(EmporiumEntry __instance)
        {
            try
            {
                if (__instance == null) return;
                TransferPocket(__instance, TryGet(() => __instance.afterhourPocketSlotInvBackpack), "背包袋");
                TransferPocket(__instance, TryGet(() => __instance.afterhourPocketSlotInvRight), "右袋");
                TransferPocket(__instance, TryGet(() => __instance.afterhourPocketSlotInvLeft), "左袋");
            }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                WarnOnce("ah-transfer", $"[口袋转移] 整堆转移异常（不影响原版单件逻辑）: {e.Message}");
            }
        }

        private static void TransferPocket(EmporiumEntry entry, GameInventory pocket, string label)
        {
            if (pocket == null) return;
            var item = GetSlotCurrent(pocket);
            if (item == null || item.unitCount <= 1) return;   // 单件：原版本来正确，不动

            int remaining = item.unitCount;
            bool anySuccess = false;
            // ★ 1.0.0 修复「误放展示柜」：dest 链【只保留前 2 级】——
            //   原版 TryAccept 的三连是 0x98(backInv) → 0x30(inv) → 0x40(showcase)，
            //   但 showcase 只是链尾兜底；照抄三级会导致前两级装不下时物品滑进展示柜。
            //   现砍为 backInv → inv 两级：拿不下的部分【留在口袋】（符合"口袋物品回到玩家仓库"
            //   的原设计意图），绝不再滑向展示柜。
            //   （见方案文档 §二；若后续玩家不接受"留口袋"，再升级为"优先柜台"的自定义落点。）
            var dests = new GameGridInventory[]
            {
                TryGet(() => entry.backInvinvElement),
                TryGet(() => entry.invElement),
            };
            foreach (var dest in dests)
            {
                if (dest == null) continue;
                try
                {
                    // ★ interop 实测 GameGridInventory 未实现 GraphNodeStorage 接口
                    //   （CS0029），GraphUtils.TryAcceptAll(dest, …) 编译不过 ——
                    //   改用同族的 TryAcceptAllMid(GameGridInventory, …)（0.9.6
                    //   归还余量同款，逐格填充整堆），返回值 = 本轮实际移走件数。
                    int before = item.unitCount;
                    int moved = GraphUtils.TryAcceptAllMid(dest, item, -1);
                    anySuccess = true;
                    remaining = before - moved;
                }
                catch (Exception e)
                {
                    Diag($"[口袋转移] {label} TryAcceptAllMid 异常: {e.Message}");
                    break;
                }
                if (remaining <= 0) break;
            }

            if (anySuccess && remaining <= 0)
            {
                // 整堆已移走 —— 槽位若仍挂着引用则清空，原版随后读到 null 直接跳过
                if (GetSlotCurrent(pocket) != null)
                    SetSlotCurrent(pocket, null);
                Diag($"[口袋转移] {label} 整堆转移完成：{SafeId(item)} ×{item.unitCount}");
            }
            else if (remaining > 0)
            {
                WarnOnce("pocket-dest-full",
                    $"[口袋转移] {label} {SafeId(item)} 还剩 {remaining} 件目的地已满，留在口袋");
            }
        }
    }

    // ── 补丁 5：MachineFurnace.<>c__DisplayClass0_0.<Furnace>b__5（熔炉消耗）──
    //   问题（用户 2026-10-11 反馈）：熔炉输入端放堆叠，过夜整堆被销毁。
    //   根因：lambda 内部按条目（FindAllChildrenType 清单）做销毁，条目 = 整个
    //   GameItem（unitCount 不减）—— 堆叠 mod 下一条目 = N 件，一次销毁吃掉整堆
    //   （反汇编 0x575E90..0x576A50：主销毁循环 0x576731-0x57677C 销毁清单末尾
    //   r14 条、垃圾分支 ≥6 条销毁前 6 条，均不看 unitCount）。
    //   修复：与补丁 1-3 同源 —— Prefix 把输入容器（itemInventoryLeft）里所有
    //   unitCount>1 的堆拆到 1 件（原品留 1），原版销毁只吃这 1 件；
    //   Postfix ReturnRemainder 把余量塞回源容器。
    //   （单堆 count<2 不熔炼属另一现象，不在本补丁范围。）
    //
    //   目标是 private 嵌套显示类的编译器生成方法 —— ★0.9.9 实测（解析 interop
    //   metadata）真名：类型 = `Il2Cpp.MachineFurnace+__c__DisplayClass0_0`（生成器
    //   把 `<>` 合法化成 `__`）、方法 = `_Furnace_b__5`。0.9.8 的候选写的是
    //   `<>c__` 原名 + `/` 分隔变体，双双落空；解析必须走 Prepare 门禁，
    //   绝不能让 TargetMethod 返回 null（否则 HarmonyX 抛异常中断整个 PatchAll）。
    [HarmonyPatch]
    private static class Patch_FurnaceConsume
    {
        // ★ 0.9.9：目标解析结果缓存。MelonLoader 会先在 HarmonyInit 阶段自动
        //   按类型 PatchAll 一次，OnInitializeMelon 里又手动 PatchAll 一次，
        //   两次共用本缓存（解析只做一次）。
        private static MethodBase _target;
        private static bool _resolved;

        private static void EnsureResolved()
        {
            if (_resolved) return;
            _resolved = true;
            try { _target = ResolveFurnaceLambda(); }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                _target = null;
            }
        }

        // ★ 0.9.9 门禁：解析失败 ⇒ 返回 false，Harmony 跳过本补丁类
        //   （TargetMethod 不会被调用，更不会返回 null）。
        //   0.9.8 实机事故：HarmonyInit 阶段 WarnOnce 触发 NRE、OnInitializeMelon
        //   阶段返回 null 抛 "returned an unexpected result: null" —— 手动 PatchAll
        //   整体中断，ValidatePatches 与启动汇总日志全没跑。
        private static bool Prepare()
        {
            EnsureResolved();
            if (_target == null)
            {
                WarnOnce("furnace-lambda",
                    "[熔炉修复] 找不到 _Furnace_b__5（游戏更新改名？），熔炉堆叠销毁修复失效");
                return false;
            }
            return true;
        }

        private static MethodBase TargetMethod()
        {
            EnsureResolved();
            return _target;
        }

        // 自检（ValidatePatches）与补丁共用同一解析器。
        internal static bool FurnaceTargetOk()
        {
            try { EnsureResolved(); return _target != null; }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                return false;
            }
        }

        internal static MethodBase ResolveFurnaceLambda()
        {
            // ① 字符串候选（真名在首位）
            foreach (var tn in FurnaceTypeCandidates)
            {
                Type t = null;
                try { t = AccessTools.TypeByName(tn); } catch { }
                if (t == null) continue;
                var m = FindFurnaceLambda(t);
                if (m != null) return m;
            }
            // ② 枚举式兜底：MachineFurnace 的全部嵌套类型逐个找方法，
            //    未来游戏/生成器再改名也只需认方法特征（含 Furnace + b__5）。
            Type ft = null;
            try { ft = AccessTools.TypeByName("Il2Cpp.MachineFurnace"); } catch { }
            if (ft != null)
            {
                Type[] nested;
                try { nested = ft.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic); }
                catch (ReflectionTypeLoadException rtle)
                {
                    nested = Array.FindAll(rtle.Types, x => x != null);
                }
                catch (Exception e)
                {
                    if (e is OutOfMemoryException || e is StackOverflowException) throw;
                    nested = Type.EmptyTypes;
                }
                foreach (var nt in nested)
                {
                    var m = FindFurnaceLambda(nt);
                    if (m != null) return m;
                }
            }
            return null;
        }

        private static MethodBase FindFurnaceLambda(Type t)
        {
            const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic
                                 | BindingFlags.Instance | BindingFlags.Static
                                 | BindingFlags.DeclaredOnly;
            MethodInfo[] ms;
            try { ms = t.GetMethods(F); } catch { return null; }
            // ① 名称精确 + 签名核对 (GameItem, GameInventory, SlotMarker)
            foreach (var m in ms)
            {
                if (Array.IndexOf(FurnaceMethodCandidates, m.Name) < 0) continue;
                var ps = m.GetParameters();
                if (ps.Length == 3
                    && ps[0].ParameterType == typeof(GameItem)
                    && ps[1].ParameterType == typeof(GameInventory)
                    && ps[2].ParameterType == typeof(SlotMarker))
                    return m;
            }
            // ② 名称精确（签名对不上也先用，避免过度校验把真目标排除）
            foreach (var m in ms)
                if (Array.IndexOf(FurnaceMethodCandidates, m.Name) >= 0) return m;
            // ③ 宽松：同时含 "Furnace" 与 "b__5"
            foreach (var m in ms)
                if (m.Name.Contains("Furnace") && m.Name.Contains("b__5")) return m;
            return null;
        }

        internal static readonly string[] FurnaceTypeCandidates =
        {
            "Il2Cpp.MachineFurnace+__c__DisplayClass0_0",   // ★0.9.9 实测真名（<> → __）
            "Il2Cpp.MachineFurnace+<>c__DisplayClass0_0",   // 未改名变体
            "Il2Cpp.MachineFurnace/__c__DisplayClass0_0",   // 斜杠嵌套变体
            "MachineFurnace+__c__DisplayClass0_0",          // 无命名空间变体
        };
        internal static readonly string[] FurnaceMethodCandidates =
        {
            "_Furnace_b__5",
            "<Furnace>b__5",
        };

        private static void Prefix(object __instance, out List<(GameInventory inv, GameItem rem)> __state)
        {
            __state = null;
            try
            {
                if (__instance == null) return;
                // 显示类字段 itemInventoryLeft（public，interop 属性或字段均可）
                const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic
                                     | BindingFlags.Instance;
                var t = __instance.GetType();
                GameInventory inv = null;
                var pf = t.GetProperty("itemInventoryLeft", F);
                if (pf != null) inv = pf.GetValue(__instance) as GameInventory;
                if (inv == null)
                {
                    var ff = t.GetField("itemInventoryLeft", F);
                    if (ff != null) inv = ff.GetValue(__instance) as GameInventory;
                }
                if (inv == null) return;

                var children = inv.childItems;
                if (children == null || children.Count == 0) return;
                // 快照后逐个拆堆（拆堆不改清单，快照仅为防御）
                var snap = new List<GameItem>();
                for (int i = 0; i < children.Count; i++)
                {
                    var c = children[i];
                    if (c != null) snap.Add(c);
                }
                foreach (var c in snap)
                {
                    if (c.unitCount <= 1) continue;
                    int n = c.unitCount;
                    GameInventory src = TryGet(() => c.parentInventory);
                    GameItem rem = c.GetSplitItem(n - 1);
                    if (rem == null) continue;
                    if (__state == null) __state = new List<(GameInventory, GameItem)>();
                    __state.Add((src ?? inv, rem));
                }
                if (__state != null)
                    Diag($"[熔炉修复] 输入 {__state.Count} 个堆已拆至 1 件，余量待原版销毁后归还");
            }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                WarnOnce("furnace-split", $"[熔炉修复] 拆堆异常（不影响熔炼本身）: {e.Message}");
            }
        }

        private static void Postfix(List<(GameInventory inv, GameItem rem)> __state)
        {
            try
            {
                if (__state == null) return;
                foreach (var (src, rem) in __state)
                    ReturnRemainder(rem, src);
            }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                WarnOnce("furnace-restore", $"[熔炉修复] 归还余量异常: {e.Message}");
            }
        }
    }

    // ── 补丁 6：MachineDesequencer.FinishDesequencer（密码解序器空白卡槽整堆销毁）──
    //   问题（用户 2026-10-11 反馈）：密码解序器把堆叠放进空白卡槽，整堆被销毁。
    //   根因（反汇编 0x550C80，尾部 0x550FB8-0x550FE5）：原版收尾无条件
    //   Destroy(输入槽.currentItem) —— 不看 unitCount，单件时代正确，
    //   堆叠下一次销毁整堆。
    //   修复：同补丁 1-3 —— Prefix 把输入槽堆拆到 1 件，原版销毁那 1 件；
    //   Postfix 走「找槽 → TryAcceptOnce(-1)」逐格循环（TryAcceptAll 内部同款）
    //   把余量塞回输入槽，失败再走 ReturnRemainder 兜底。
    //   参数序实证（FullMachineUpgrades.cs:4121）：FinishDesequencer(machine, input, output)
    //   ⇒ __1 = 输入槽。
    [HarmonyPatch(typeof(MachineDesequencer), nameof(MachineDesequencer.FinishDesequencer))]
    private static class Patch_FinishDesequencer
    {
        private static void Prefix(object __1, out (GameInventory inv, GameItem rem) __state)
        {
            __state = (null, null);
            try
            {
                var item = GetSlotCurrent(__1);
                if (item == null || item.unitCount <= 1) return;
                int n = item.unitCount;
                GameInventory src = TryGet(() => item.parentInventory);
                GameItem rem = item.GetSplitItem(n - 1);
                if (rem != null)
                {
                    __state = (src, rem);
                    Diag($"[解序器修复] 输入槽拆堆：{SafeId(item)} {n} → 1，余量={SafeId(rem)} ×{rem.unitCount}");
                }
            }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                WarnOnce("deseq-split", $"[解序器修复] 拆堆异常: {e.Message}");
            }
        }

        private static void Postfix(object __1, (GameInventory inv, GameItem rem) __state)
        {
            try
            {
                if (__state.rem == null) return;
                var remId = SafeId(__state.rem);
                int remN = __state.rem.unitCount;
                int left = remN;
                // 原版已销毁那 1 件，输入槽空着 —— 余量优先塞回输入槽（继续可用）。
                // ★ interop 实测 GameInventory 未实现 GraphNodeStorage 接口（CS0029），
                //   不能直接 GraphUtils.TryAcceptAll(输入槽, …)；改走 TryAcceptAll
                //   内部同款的「找槽 → TryAcceptOnce(-1)」逐格循环，挂在 GameInventory
                //   的抽象方法上虚分发到 GameSlotInventory 实现（TryAcceptOnce 返回
                //   实际接受数，见 0x7B01B9 `mov eax,esi`）。
                var gi = __1 as GameInventory;
                if (gi != null && left > 0)
                {
                    try
                    {
                        int guard = 0;
                        while (left > 0 && guard++ < 64)
                        {
                            var marker = gi.TryFindOneValidInventorySlot(__state.rem);
                            if (marker == null || marker.numTransfer <= 0) break;
                            int accepted = marker.TryAcceptOnce(-1);
                            if (accepted <= 0) break;
                            left -= accepted;
                        }
                        if (left <= 0)
                        {
                            Diag($"[解序器修复] 余量已塞回输入槽：{remId} ×{remN}");
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Diag($"[解序器修复] 塞回输入槽失败: {e.Message}");
                    }
                }
                ReturnRemainder(__state.rem, __state.inv);
            }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                WarnOnce("deseq-restore", $"[解序器修复] 归还余量异常: {e.Message}");
            }
        }
    }
}

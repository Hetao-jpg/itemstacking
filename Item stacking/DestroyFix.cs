using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Preferences;
using UnityEngine;

namespace ItemStacking;

// 堆叠物品「整堆被销毁」修复
//
// ★ 根因
//   游戏多处按 unitCount == 1 假设写逻辑（Destroy(整个 item)、UpgradePrinter 等）。
//   堆叠 mod 下 unitCount = N ⇒ Destroy 一次销毁整堆，逻辑上只消耗 1 件。
//
// ★ 统一修复模式（Prefix + Postfix）
//   Prefix：若 unitCount > 1，先 GetSplitItem 把多余部分拆成独立 item（原品减到 1），
//           原版正常运行 → 只 Destroy 那 1 件。
//   Postfix：把拆出的余量塞回源容器（parentInventory）；源容器满 → 背包兜底 → 告警。
//
// ★ 覆盖路径
//   1) MachineBottlePrinter.UpgradePrinter      —— 水瓶打印机升级
//   2) ModuleHelper.RestoreModule               —— 模块修复
//   3) ModuleHelper.RepairExtractor             —— 采矿机修理
//      （1/2/3 合并为 Patch_ConsumeSecondArg —— 三者除「第 2 个参数名 + 日志 key」外逐字节一致）
//   4) EmporiumEntry.TransferAfterhourToInv     —— 拾荒口袋跨场景
//   5) MachineFurnace.<Furnace>b__5             —— 熔炉消耗
//   6) MachineDesequencer.FinishDesequencer     —— 密码解序器
public partial class Main
{
    // ── 余量归还 ──
    //   remainder = 从原品拆出的多余部分（已不在任何容器中）。
    //   sourceInv = 拆堆前记录的源容器（可能 null —— 拖拽中物品已脱离容器）。
    //
    //   ★ GraphUtils.TryAcceptAllMid 返回【剩余未接受量】，不是「已接受量」。
    //     判据：返回 ≤ 0 = 全部搬完；> 0 则剩余件仍留在 remainder 自身（孤儿 item）。
    //     旧实现把它当「已接受量」用 ⇒ 判据反了（部分成功时误判「全归还」）。
    private static void ReturnRemainder(GameItem remainder, GameInventory sourceInv)
    {
        if (remainder == null) return;
        int total = remainder.unitCount;
        if (total <= 0) return;
        try
        {
            // ① 优先塞回源容器。
            //   ★ 用基类 GameInventory（机器内库是裸 GameInventory，不是 GameGridInventory）。
            if (sourceInv != null)
            {
                try
                {
                    int moved = TryReturnIntoInventory(sourceInv, remainder);
                    if (IsPlacedInContainer(remainder))
                    {
                        Diag($"[销毁修复] 余量已归还源容器：{SafeId(remainder)} ×{total}");
                        return;
                    }
                    if (moved > 0)
                        Diag($"[销毁修复] 源容器只收下 {moved}/{total} 件，余 {remainder.unitCount} 件改投背包");
                }
                catch (Exception e)
                {
                    Diag($"[销毁修复] 塞回源容器失败: {e.Message}");
                }
            }

            // ② 兜底：尝试塞进玩家背包（EmporiumEntry.invElement）
            var entry = TryGet(() => EmporiumEntry.Instance);
            var backpack = entry != null ? TryGet(() => entry.invElement) : null;
            if (backpack != null && !SameNativePointer(backpack, sourceInv))
            {
                try
                {
                    TryReturnIntoInventory(backpack, remainder);
                    if (IsPlacedInContainer(remainder))
                    {
                        Diag($"[销毁修复] 余量已归还背包：{SafeId(remainder)} ×{total}");
                        return;
                    }
                }
                catch (Exception e)
                {
                    Diag($"[销毁修复] 塞回背包失败: {e.Message}");
                }
            }

            // ③ 实在放不下：打警告（不静默丢弃）
            LogOnce("remainder", Level.Warn,
                $"[销毁修复] 余量 {SafeId(remainder)} ×{remainder.unitCount} 无法归还任何容器！");
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("remainder-ex", Level.Warn, $"[销毁修复] 归还余量异常: {e.Message}");
        }
    }

    // ── 把「孤儿物品」逐格塞进【任意】GameInventory ──
    //   ★ 不要求目标是 GameGridInventory —— 机器内库是裸 GameInventory。
    //   ★ 返回值只用于日志；判断「是否全部归位」一律看 item.parentInventory
    //     （IsPlacedInContainer），不依赖任何 GraphUtils 返回值语义。
    //   ★ 重入：内部自调用 TryAcceptOnce，靠 _selfAcceptDepth 让本 mod 的补丁直通原生。
    private static int TryReturnIntoInventory(GameInventory inv, GameItem item)
    {
        if (inv == null || item == null) return 0;
        int moved = 0, guard = 0, consecutiveOccupied = 0;
        _selfAcceptDepth++;
        try
        {
            while (item.unitCount > 0 && guard++ < 64)
            {
                SlotMarker m;
                try { m = inv.TryFindOneValidInventorySlot(item); } catch { break; }
                if (m == null) break;
                if (m.targetItem != null || m.numTransfer < 1)
                {
                    // 该槽已占用 / 放不进 → 试下一个；连续 8 次 ⇒ 判定没有空槽了
                    if (++consecutiveOccupied >= 8) break;
                    continue;
                }
                consecutiveOccupied = 0;

                m.item = item;
                int n;
                try { n = m.TryAcceptOnce(-1); } catch { break; }
                if (n <= 0) break;
                moved += n;

                // 整件被搬走（原生 Expel 分支）时 unitCount 不变、但已挂上归属容器 ⇒ 收工。
                if (IsPlacedInContainer(item)) break;
            }
        }
        finally
        {
            _selfAcceptDepth--;
        }
        return moved;
    }

    // 物品是否「已经在某个容器里」= 全部归位。
    //   依据（反汇编 SlotMarker.TryAcceptOnce）：
    //     整件接受走 Expel 分支 ⇒ unitCount 不变但 parentInventory 被挂上；
    //     部分接受走 GetSplitItem ⇒ 源对象 unitCount 减少、parentInventory 仍为 null（孤儿）。
    private static bool IsPlacedInContainer(GameItem item)
    {
        try { return item != null && item.parentInventory != null; }
        catch { return false; }
    }

    // ── 销毁前拆堆（Prefix 主体） ──
    //   item.unitCount > 1 时，拆出 (n-1) 件为独立 GameItem（原品减到 1），
    //   原品保持原位不动（原版随后 Destroy 的仍是这 1 件）。
    //   返回 (源容器, 余量)；无需处理时返回 (null, null)。
    private static (GameInventory inv, GameItem rem) SplitBeforeDestroy(GameItem item, string logKey, string tag)
    {
        try
        {
            if (item == null) return (null, null);
            int n = item.unitCount;
            if (n <= 1) return (null, null);

            GameInventory srcInv = TryGet(() => item.parentInventory);
            GameItem rem = item.GetSplitItem(n - 1);
            Diag($"[销毁修复] {tag} 前拆堆：{SafeId(item)} {n} → 1，余量={SafeId(rem)} ×{rem?.unitCount ?? 0}");
            return rem != null ? (srcInv, rem) : (null, null);
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce(logKey, Level.Warn, $"{tag} 拆堆异常（不影响原操作）: {e.Message}");
            return (null, null);
        }
    }

    // 销毁后归还余量（Postfix 主体）。
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
            LogOnce(logKey, Level.Warn, $"{tag} 归还余量异常: {e.Message}");
        }
    }

    // ── 读/写槽位 currentItem（字段优先、属性兜底） ──
    //   GameSlotInventory.currentItem 是私有成员，interop 可能暴露成字段或属性，
    //   运行时按「字段优先、属性兜底」解析，避免编译期依赖成员形态。
    private static GameItem GetSlotCurrent(object slot)
    {
        if (slot == null) return null;
        try
        {
            const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var t = slot.GetType();
            var f = t.GetField("currentItem", F);
            if (f != null) return f.GetValue(slot) as GameItem;
            var p = t.GetProperty("currentItem", F);
            if (p != null) return p.GetValue(slot) as GameItem;
            LogOnce("no-currentitem", Level.Warn, $"[槽位成员] {t.Name} 无 currentItem 成员（游戏改版？），清槽跳过");
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("get-currentitem", Level.Warn, $"[槽位成员] 读 currentItem 异常: {e.Message}");
        }
        return null;
    }

    private static void SetSlotCurrent(object slot, GameItem value)
    {
        if (slot == null) return;
        try
        {
            const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var t = slot.GetType();
            var f = t.GetField("currentItem", F);
            if (f != null) { f.SetValue(slot, value); return; }
            var p = t.GetProperty("currentItem", F);
            if (p != null) { p.SetValue(slot, value); return; }
            LogOnce("no-currentitem-set", Level.Warn, $"[槽位成员] {t.Name} 无 currentItem 可写（游戏改版？），清槽跳过");
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("set-currentitem", Level.Warn, $"[槽位成员] 写 currentItem 异常: {e.Message}");
        }
    }

    // ── 补丁 1/2/3 合并：销毁型消费 ──
    //   三个方法（UpgradePrinter / RestoreModule / RepairExtractor）的 Prefix/Postfix
    //   除「第 2 个参数 + 日志 key」外逐字节一致，合并为一个 [HarmonyTargetMethods] 补丁类。
    //   ★ 参数取法：Harmony 的 __1 = 原方法第 2 个参数（0 基下标），三处被消费的都是第 2 参。
    //   ★ ValidatePatches 里三条独立自检（StackPatches.cs）必须保留：
    //     TargetMethods 任一目标解析失败只会跳过该目标，自检负责把它变成启动告警。
    [HarmonyPatch]
    private static class Patch_ConsumeSecondArg
    {
        private static readonly (Type type, string method, string key, string tag)[] Targets =
        {
            (typeof(MachineBottlePrinter), nameof(MachineBottlePrinter.UpgradePrinter), "up-printer", "UpgradePrinter"),
            (typeof(ModuleHelper),         nameof(ModuleHelper.RestoreModule),          "restore-mod", "RestoreModule"),
            (typeof(ModuleHelper),         nameof(ModuleHelper.RepairExtractor),        "repair-ext", "RepairExtractor"),
        };

        private static readonly Type[] TwoGameItems = { typeof(GameItem), typeof(GameItem) };

        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            // 每个目标独立容错：找不到就跳过该目标并告警，绝不 yield null
            // （Harmony 遇 null / 抛异常会中断整个补丁类的注册）。
            for (int i = 0; i < Targets.Length; i++)
            {
                var t = Targets[i];
                MethodBase m = null;
                try { m = AccessTools.Method(t.type, t.method, TwoGameItems); }
                catch (Exception e) when (e is not OutOfMemoryException and not StackOverflowException) { }
                if (m != null) yield return m;
                else LogOnce("consume-target", Level.Warn,
                    $"[销毁修复] 找不到 {t.type.Name}.{t.method}（游戏改名？该处余量归还失效）");
            }
        }

        private static void Prefix(GameItem __1, MethodBase __originalMethod,
                                   out (GameInventory inv, GameItem rem) __state)
        {
            var (key, tag) = Find(__originalMethod);
            __state = SplitBeforeDestroy(__1, key, tag);
        }

        private static void Postfix((GameInventory inv, GameItem rem) __state,
                                    MethodBase __originalMethod)
        {
            var (key, tag) = Find(__originalMethod);
            ReturnRemainderAfter(__state, key + "-pf", tag);
        }

        // 按方法名回查日志 key / tag（三个方法名互不相同，无歧义）。
        // 查不到给兜底值、绝不抛异常 —— 日志 key 不能反过来让补丁失效。
        private static (string key, string tag) Find(MethodBase m)
        {
            var name = m?.Name;
            for (int i = 0; i < Targets.Length; i++)
                if (Targets[i].method == name) return (Targets[i].key, Targets[i].tag);
            return ("consume", "Consume");
        }
    }

    // ── 补丁 4：EmporiumEntry.TransferAfterhourToInv（拾荒口袋跨场景转移） ──
    //   根因：原版对 3 个口袋槽逐个调 GraphUtils.TryAccept(dest, item) ——
    //         TryAccept 内部硬编码 TryAcceptOnce(marker, amount=1)，只搬 1 件。
    //   修复：Prefix 用 GraphUtils.TryAcceptAllMid(dest, item, -1) 按 dest 链整堆转移；
    //         全部移走后清空 pocket.currentItem。
    //   部分失败（目的地全满）：留槽不动 + WarnOnce。
    //
    //   ★ dest 链只保留 backInv → inv 两级，移除 showcaseElement 兜底
    //     （0.9.8 照抄原版 3 级链 ⇒ 前两级装不下时物品滑进展示柜）。
    //   ★ TryAcceptAllMid 返回值是【剩余未接受量】（不是已接受量），见 ReturnRemainder 注释。
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
                LogOnce("ah-transfer", Level.Warn, $"[口袋转移] 整堆转移异常（不影响原版单件逻辑）: {e.Message}");
            }
        }

        private static void TransferPocket(EmporiumEntry entry, GameInventory pocket, string label)
        {
            if (pocket == null) return;
            var item = GetSlotCurrent(pocket);
            if (item == null || item.unitCount <= 1) return;   // 单件：原版本来正确，不动

            int remaining = item.unitCount;
            bool anySuccess = false;
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
                    // TryAcceptAllMid 返回值是【剩余未接受量】—— 返回 ≤0 表示全部搬完。
                    remaining = GraphUtils.TryAcceptAllMid(dest, item, -1);
                    anySuccess = true;
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
                LogOnce("pocket-dest-full", Level.Warn,
                    $"[口袋转移] {label} {SafeId(item)} 还剩 {remaining} 件目的地已满，留在口袋");
            }
        }
    }

    // ── 补丁 5：MachineFurnace.<>c__DisplayClass0_0.<Furnace>b__5（熔炉消耗） ──
    //   根因：lambda 内部按条目（FindAllChildrenType 清单）做销毁，条目 = 整个 GameItem。
    //         堆叠 mod 下一条目 = N 件，一次销毁吃掉整堆。
    //   修复：与补丁 1-3 同源 —— Prefix 把输入容器里所有 unitCount > 1 的堆拆到 1 件。
    //
    //   ★ 目标是 private 嵌套显示类的编译器生成方法：
    //     类型 = Il2Cpp.MachineFurnace+__c__DisplayClass0_0（生成器把 <> 合法化成 __）；
    //     方法 = _Furnace_b__5。
    //   ★ 解析必须走 Prepare 门禁，绝不能让 TargetMethod 返回 null
    //     （Harmony 会抛异常中断整个 PatchAll）。
    [HarmonyPatch]
    private static class Patch_FurnaceConsume
    {
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

        // 解析失败 ⇒ 返回 false，Harmony 跳过本补丁类（TargetMethod 不会被调用，不会返回 null）。
        private static bool Prepare()
        {
            EnsureResolved();
            if (_target == null)
            {
                LogOnce("furnace-lambda", Level.Warn,
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
                catch (ReflectionTypeLoadException rtle) { nested = Array.FindAll(rtle.Types, x => x != null); }
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
                                 | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
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
            "Il2Cpp.MachineFurnace+__c__DisplayClass0_0",   // 实测真名（<> → __）
            "Il2Cpp.MachineFurnace+<>c__DisplayClass0_0",
            "Il2Cpp.MachineFurnace/__c__DisplayClass0_0",
            "MachineFurnace+__c__DisplayClass0_0",
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
                // 显示类字段 itemInventoryLeft（interop 属性或字段均可）
                const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
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
                // 快照后逐个拆堆（防御：拆堆可能改清单）
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
                LogOnce("furnace-split", Level.Warn, $"[熔炉修复] 拆堆异常（不影响熔炼本身）: {e.Message}");
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
                LogOnce("furnace-restore", Level.Warn, $"[熔炉修复] 归还余量异常: {e.Message}");
            }
        }
    }

    // ── 补丁 6：MachineDesequencer.FinishDesequencer（解序器空白卡槽整堆销毁） ──
    //   根因：原版收尾无条件 Destroy(输入槽.currentItem)，不看 unitCount。
    //   修复：同补丁 1-3 —— Prefix 把输入槽堆拆到 1 件；Postfix 先尝试塞回输入槽，
    //         失败再走 ReturnRemainder 兜底。
    //   参数序实证：FinishDesequencer(machine, input, output) ⇒ __1 = 输入槽。
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
                LogOnce("deseq-split", Level.Warn, $"[解序器修复] 拆堆异常: {e.Message}");
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
                // 用「找槽 → TryAcceptOnce(-1)」逐格循环（GameInventory 未实现 GraphNodeStorage 接口）。
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
                LogOnce("deseq-restore", Level.Warn, $"[解序器修复] 归还余量异常: {e.Message}");
            }
        }
    }
}
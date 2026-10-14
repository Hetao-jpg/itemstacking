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
    //
    // ★★ 2026-10-14（P0-3）修正：`GraphUtils.TryAcceptAllMid` 返回的是
    //   【剩余未接受量】，不是「已接受量」——旧代码把它当「已接受量」用，判据整个反了。
    //
    //   ── 反汇编实证（GameAssembly，ImageBase=0x180000000）──
    //     `GraphUtils::TryAcceptAllMid` @ RVA 0x7664B0：
    //       0x1807664c0  mov  edi, r8d          ; edi = amount（<0 时取 item.unitCount）
    //       0x1807664eb  xor  ebx, ebx          ; ebx = accepted = 0
    //     循环（≤0x28 轮，与 TryAcceptAll 同构）：
    //       0x18076651f  call SlotMarker::TryAcceptOnce(marker, amount-accepted, 0)
    //       0x180766528  add  ebx, eax          ; accepted += n
    //       0x180766536  sub  edi, ebx          ; ★ edi = amount - accepted
    //       0x18076653d  mov  eax, edi          ; ★ 返回值 = 剩余未接受量
    //     ⇒ 与同族的 `TryAcceptAll`(0x766650)（《物品堆叠机制.md》§8 标注「int 剩余未接受量」）语义一致。
    //
    //   ── 为什么旧判据「accepted >= remainder.unitCount」恒为真（两个语义前提）──
    //     ① 「整批接受」走 SlotMarker 的 Expel 分支（0x7B0020 → 虚表[0x278]），把物品对象
    //        【整体】搬进容器 ⇒ unitCount 不变（= N），此时返回 0  ⇒ 0 >= N 为假；
    //     ② 「部分接受」走 GetSplitItem 分支 ⇒ 源对象 unitCount 减少为 N-A，
    //        此时返回 N-A ⇒ (N-A) >= (N-A) 为真。
    //     ⇒ 后果：① 的「已归还源容器」成功分支【不可达】、日志永不打印；
    //             部分成功时误判「全归还」提前 return ⇒ ② 背包兜底与 ③ 丢失告警全被跳过。
    //
    //   ⇒ 正确判据只有一条：**返回 ≤ 0 = 全部搬完**；> 0 则剩余件仍留在
    //     `remainder` 自身（无容器孤儿，unitCount 已同步为剩余量），直接进下一级兜底。
    private static void ReturnRemainder(GameItem remainder, GameInventory sourceInv)
    {
        if (remainder == null) return;
        int total = remainder.unitCount;   // ★ 原始件数：全归还时 unitCount 不变(Expel)或归 0，日志要用它
        if (total <= 0) return;
        try
        {
            // ① 优先塞回源容器
            //   ★★ 2026-10-14（P0 级，实机日志证实）：旧写法 `sourceInv as GameGridInventory`
            //   对【机器内库】恒为 null —— 熔炉/解序器等的内库是裸 `GameInventory`：
            //     日志《Latest.log》16:23:48 作用域探针：
            //       `1 号容器：id=<空id> 类=GameInventory @… 所属=furnace 内含=common_ore`
            //   ⇒ 旧代码 ① 整段被跳过，余量只能走 ② 背包；背包也满时就彻底没地方去。
            //   ⇒ 改用与类型无关的 TryReturnIntoInventory（基类 GameInventory，
            //     试 GameGridInventory / GameSlotInventory / 裸 GameInventory 通吃）。
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

            // ③ 实在放不下：打警告（不静默丢弃，让用户知道）
            LogOnce("remainder", Level.Warn,
                $"[销毁修复] 余量 {SafeId(remainder)} ×{remainder.unitCount} 无法归还任何容器！");
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            LogOnce("remainder-ex", Level.Warn, $"[销毁修复] 归还余量异常: {e.Message}");
        }
    }

    // ── 共用：把「孤儿物品」逐格塞进【任意】GameInventory ──
    //   ★ 2026-10-14 新增。语义与原生 `GraphUtils.TryAcceptAll` 同构（找槽 → TryAcceptOnce，≤64 轮），
    //     但两点不同：
    //       ① 不要求目标是 GameGridInventory —— 机器内库是裸 `GameInventory`
    //          （实机日志：`类=GameInventory … 所属=furnace`），旧用 `TryAcceptAllMid`
    //          必须强转 GameGridInventory，故对机器恒失败；
    //       ② 返回值**只用于日志**：判断「是否全部归位」一律看 `item.parentInventory`
    //          （见 IsPlacedInContainer），绝不依赖任何 GraphUtils 返回值语义
    //          —— 那正是 P0-3 踩过的坑（TryAcceptAllMid 返回的是「剩余量」）。
    //   ★ 重入：内部自调用 TryAcceptOnce，靠 `_selfAcceptDepth` 让本 mod 的
    //     SlotMarker.TryAcceptOnce 补丁直通原生（否则会被「机器容器每格 1 件」那层再拦一次）。
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

                m.item = item;                       // 原生 GetStackSlot 同款：finder 未必回填 item
                int n;
                try { n = m.TryAcceptOnce(-1); } catch { break; }
                if (n <= 0) break;
                moved += n;

                // 整个对象被搬走（原生 Expel 分支）时 unitCount【不变】、但已挂上归属容器 ⇒ 收工。
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
    //   依据（0x7B0020 反汇编）：整件接受走 Expel 分支 ⇒ unitCount 不变但 parentInventory 被挂上；
    //   部分接受走 GetSplitItem ⇒ 源对象 unitCount 减少、parentInventory 仍为 null（孤儿）。
    private static bool IsPlacedInContainer(GameItem item)
    {
        try { return item != null && item.parentInventory != null; }
        catch { return false; }
    }

    // ── 共用：销毁前拆堆（Prefix 主体）／销毁后归还余量（Postfix 主体）──
    //   ★ §七 2.3（0.11.0）：补丁 1/2/3（UpgradePrinter / RestoreModule / RepairExtractor）
    //     的 Prefix+Postfix 除「被拆物品参数名 + 日志 key」外完全相同，先提炼公共实现
    //     （三行委托），2026-10-12 进一步合并为单个 [HarmonyTargetMethods] 补丁类
    //     Patch_ConsumeSecondArg —— 行为逐字节一致（含异常语义 / 日志 key）。
    //     回滚：git 历史 dc3d11a 里仍保留三份手写补丁类。
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
            LogOnce(logKey, Level.Warn, $"{tag} 拆堆异常（不影响原操作）: {e.Message}");
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
            LogOnce(logKey, Level.Warn, $"{tag} 归还余量异常: {e.Message}");
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
            const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic
                                 | BindingFlags.Instance;
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

    // ── 补丁 1/2/3 合并：销毁型消费（三者的 Prefix/Postfix 除「第 2 个参数 + 日志 key」
    //    外逐字节一致，故合并为一个 [HarmonyTargetMethods] 补丁类）──
    //   ★ 合并前的三个手写补丁见 git 历史 dc3d11a（Patch_UpgradePrinter /
    //     Patch_RestoreModule / Patch_RepairExtractor），日志 key 与行为完全不变。
    //   ★ 参数取法：Harmony 的 `__1` = 原方法第 2 个参数（0 基下标），
    //     三处被消费的都是第 2 参（upgradeItem / electronicPart / electronicPart）。
    //   ★ ValidatePatches 里三条独立自检（StackPatches.cs）【必须保留】：
    //     TargetMethods 任一目标解析失败只会跳过该目标，自检负责把它变成启动告警。
    [HarmonyPatch]
    private static class Patch_ConsumeSecondArg
    {
        // (目标类型, 方法名, 日志 key, 方法名 tag) —— key 沿用合并前三份手写值（含 "-pf" 后缀）。
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
            // （Harmony 遇 null / 抛异常会中断整个补丁类的注册，三条自检负责兜底）。
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
                    //   归还余量同款，逐格填充整堆）。
                    // ★★ 2026-10-14（P0-3）修正：它的返回值是【剩余未接受量】
                    //   （反汇编 0x7664B0 尾部 `sub edi,ebx / mov eax,edi`，见 ReturnRemainder 的取证注释），
                    //   旧写法 `before - moved` 把它当「已接受量」⇒ 整堆搬走时算成「一件没搬」：
                    //     · 会继续拿同一物品往 dest 链下一级搬（口袋物品落到主仓库而不是玩家柜台）；
                    //     · 最后打一条假的「目的地已满，留在口袋」；
                    //     · 且不再执行 SetSlotCurrent(pocket, null) 清槽。
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
                LogOnce("deseq-restore", Level.Warn, $"[解序器修复] 归还余量异常: {e.Message}");
            }
        }
    }
}

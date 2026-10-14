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
    // 补丁目标自检
    //
    // ★ 背景：字符串式 Harmony 补丁在游戏改名对应方法时会「静默失效」
    //   （Harmony 仅告警，功能无提示消失）。
    //   本方法在 PatchAll 后用 AccessTools 逐一复核目标方法存在性，缺失则明确告警。
    //
    // ★ 使用两种复核方式：
    //   · Check(t, name, args)      —— 按签名精确查
    //   · CheckByName(t, name)      —— 按名枚举全部重载（类型有重载时用）
    //   · CheckMember(t, name)      —— 属性或字段任一存在即通过（interop 把 il2cpp
    //                                  字段暴露成属性，只查 Field 会恒判失败）
    //
    // ★ 缺失的后果（为什么每一项都必须自检）：
    //   · 容器/物品类判据缺失 → 数据丢失防护静默失效（不可逆）
    //   · 上限/移除类判据缺失 → 堆叠上限/合并量失效
    //   · 机器容器判据缺失   → 机器内堆叠 → 丢件/崩坏
    //   · 状态/值域类判据缺失 → 拆分后状态污染
    private static void ValidatePatches()
    {
        var failures = new List<string>();

        void Check(Type t, string name, Type[] args, string label)
        {
            if (AccessTools.Method(t, name, args) == null)
                failures.Add($"  - {label}  ({t.FullName}.{name})");
        }

        void CheckByName(Type t, string name, string label)
        {
            MethodInfo found = null;
            try
            {
                foreach (var m in AccessTools.GetDeclaredMethods(t))
                {
                    if (m != null && m.Name == name) { found = m; break; }
                }
            }
            catch { }
            if (found == null)
                failures.Add($"  - {label}  ({t.FullName}.{name})");
        }

        void CheckMember(Type t, string name, string label)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic
                                     | BindingFlags.Instance | BindingFlags.Static;
            if (t.GetProperty(name, flags) == null && t.GetField(name, flags) == null)
                failures.Add($"  - {label}  ({t.FullName}.{name})");
        }

        // ── 核心堆叠闸门 ──
        Check(typeof(GameItem), nameof(GameItem.MayStack), new[] { typeof(GameItem) }, "MayStack");
        Check(typeof(GameItem), nameof(GameItem.GetStackSlot), new[] { typeof(GameItem) }, "GetStackSlot");
        Check(typeof(GameItem), nameof(GameItem.MaxNumRemove), Type.EmptyTypes, "MaxNumRemove");
        Check(typeof(GameItem), "MaxNumAdd", new[] { typeof(SlotMarker) }, "MaxNumAdd(SlotMarker)");
        Check(typeof(ItemMouseDragHandler), "StartDrag", Type.EmptyTypes, "ItemMouseDragHandler.StartDrag");

        // ── 容器 / 归属判据 ──
        Check(typeof(GameItem), "FindAllChildItems", Type.EmptyTypes, "FindAllChildItems(容器检测)");
        Check(typeof(GameItem), "get_parentInventory", Type.EmptyTypes, "GameItem.parentInventory(容器归属判据)");
        Check(typeof(GameInventory), nameof(GameInventory.GetParentItem), Type.EmptyTypes, "GameInventory.GetParentItem(owner 匹配链)");

        // ── 状态 / 值域判据（私有依赖必须一并复核） ──
        Check(typeof(GameItem), nameof(GameItem.IsTag), new[] { typeof(string) }, "IsTag(黑名单判据)");
        Check(typeof(GameItem), nameof(GameItem.GetTagReadonly), new[] { typeof(string) }, "GetTagReadonly(物种/值域判据)");
        Check(typeof(DurabilityHelper), "GetMaxDurability", new[] { typeof(GameItem) }, "GetMaxDurability(耐久判据)");
        Check(typeof(EmporiumEntry), "get_Instance", Type.EmptyTypes, "EmporiumEntry.Instance(柜台)");
        CheckMember(typeof(GameItem), "_state_k__BackingField", "GameItem._state_k__BackingField(状态闸门)");
        CheckMember(typeof(TagSystem), "dict", "TagSystem.dict(状态闸门)");
        CheckMember(typeof(TagState), "valueEnabled", "TagState.valueEnabled(状态闸门)");
        CheckMember(typeof(TagState), "valueInt", "TagState.valueInt(状态闸门)");
        CheckMember(typeof(TagState), "valueLong", "TagState.valueLong(状态闸门)");
        CheckMember(typeof(TagState), "valueBool", "TagState.valueBool(状态闸门)");
        CheckMember(typeof(TagState), "valueFloat", "TagState.valueFloat(状态闸门)");
        CheckMember(typeof(TagState), "valueDouble", "TagState.valueDouble(状态闸门)");
        CheckMember(typeof(TagState), "internalValueString", "TagState.internalValueString(状态闸门)");

        // ── 销毁修复补丁的依赖 ──
        Check(typeof(MachineBottlePrinter), nameof(MachineBottlePrinter.UpgradePrinter),
              new[] { typeof(GameItem), typeof(GameItem) }, "UpgradePrinter(水瓶打印机升级)");
        Check(typeof(ModuleHelper), nameof(ModuleHelper.RestoreModule),
              new[] { typeof(GameItem), typeof(GameItem) }, "RestoreModule(模块修复)");
        Check(typeof(ModuleHelper), nameof(ModuleHelper.RepairExtractor),
              new[] { typeof(GameItem), typeof(GameItem) }, "RepairExtractor(采矿机修理)");
        Check(typeof(GameItem), nameof(GameItem.GetSplitItem), new[] { typeof(int) }, "GetSplitItem(拆堆)");
        Check(typeof(GraphUtils), "TryAcceptAll",
              new[] { typeof(GraphNodeStorage), typeof(GameItem), typeof(int) }, "GraphUtils.TryAcceptAll(余量归还)");
        CheckByName(typeof(GraphUtils), "TryAcceptAllMid", "GraphUtils.TryAcceptAllMid(口袋转移/余量归还)");
        Check(typeof(MachineDesequencer), nameof(MachineDesequencer.FinishDesequencer),
              new[] { typeof(GameItem), typeof(GameSlotInventory), typeof(GameSlotInventory) },
              "FinishDesequencer(解序器整堆销毁修复)");
        Check(typeof(EmporiumEntry), nameof(EmporiumEntry.TransferAfterhourToInv), Type.EmptyTypes,
              "TransferAfterhourToInv(拾荒口袋整堆转移)");
        CheckMember(typeof(EmporiumEntry), "afterhourPocketSlotInvBackpack", "EmporiumEntry.afterhourPocketSlotInvBackpack");
        CheckMember(typeof(EmporiumEntry), "afterhourPocketSlotInvRight", "EmporiumEntry.afterhourPocketSlotInvRight");
        CheckMember(typeof(EmporiumEntry), "afterhourPocketSlotInvLeft", "EmporiumEntry.afterhourPocketSlotInvLeft");
        CheckMember(typeof(GameSlotInventory), "currentItem", "GameSlotInventory.currentItem(槽位物品)");
        if (!Patch_FurnaceConsume.FurnaceTargetOk())
            failures.Add("  - _Furnace_b__5(熔炉消耗拆堆) (MachineFurnace+__c__DisplayClass0_0)");

        // ── Alt / Ctrl / Shift 快移的挂点与依赖 ──
        CheckByName(typeof(ItemQuickTransferAltHandler), "OnEventRelease", "ItemQuickTransferAltHandler.OnEventRelease");
        CheckByName(typeof(ItemQuickTransferAltHandler1), "OnEventRelease", "ItemQuickTransferAltHandler1.OnEventRelease");
        CheckByName(typeof(ItemQuickTransferHandler), "OnEventRelease", "ItemQuickTransferHandler.OnEventRelease");
        Check(typeof(InputActionManager), nameof(InputActionManager.ResetAfter),
              new[] { typeof(InputActionHandler) }, "InputActionManager.ResetAfter(尾部清理)");
        CheckMember(typeof(ItemQuickTransferAltHandler), "currentItem", "ItemQuickTransferAltHandler.currentItem");
        CheckMember(typeof(ItemQuickTransferAltHandler), "isQuickTransfer", "ItemQuickTransferAltHandler.isQuickTransfer");
        CheckMember(typeof(ItemQuickTransferAltHandler), "quickTransferKey", "ItemQuickTransferAltHandler.quickTransferKey");
        CheckMember(typeof(ItemQuickTransferAltHandler), "key", "ItemQuickTransferAltHandler.key");
        CheckMember(typeof(ItemQuickTransferAltHandler1), "key", "ItemQuickTransferAltHandler1.key");
        CheckMember(typeof(ItemQuickTransferAltHandler1), "quickTransferKey", "ItemQuickTransferAltHandler1.quickTransferKey");
        CheckMember(typeof(ItemQuickTransferHandler), "key", "ItemQuickTransferHandler.key");
        CheckMember(typeof(ItemQuickTransferHandler), "quickTransferKey", "ItemQuickTransferHandler.quickTransferKey");
        CheckMember(typeof(ItemQuickTransferHandler), "currentItem", "ItemQuickTransferHandler.currentItem");
        CheckMember(typeof(ItemQuickTransferHandler), "isQuickTransfer", "ItemQuickTransferHandler.isQuickTransfer");

        // ── SlotMarker 成员（落点改写 / Ctrl 合并闸门依赖） ──
        CheckMember(typeof(SlotMarker), "targetItem", "SlotMarker.targetItem");
        CheckMember(typeof(SlotMarker), "inventory", "SlotMarker.inventory");
        CheckMember(typeof(SlotMarker), "item", "SlotMarker.item");
        CheckMember(typeof(SlotMarker), nameof(SlotMarker.numTransfer), "SlotMarker.numTransfer");
        Check(typeof(SlotMarker), nameof(SlotMarker.TryAcceptOnce), new[] { typeof(int) },
              "SlotMarker.TryAcceptOnce(合并闸门·全路径)");

        // ── 快移落点改写（Alt 快移的核心挂点） ──
        //   GameInventory.TryFindOneValidInventorySlot 是 abstract，用 [HarmonyTargetMethods] 枚举子类
        //   ⇒ 此处只自检「至少存在一个具体实现」。
        CheckByName(typeof(GameGridInventory), nameof(GameGridInventory.TryFindOneValidInventorySlot),
                    "GameGridInventory.TryFindOneValidInventorySlot(Alt落点改写①)");
        CheckByName(typeof(GameGridInventory), "TryFindOneValidInventorySlot_TopMiddle",
                    "GameGridInventory.TryFindOneValidInventorySlot_TopMiddle(存档用)");
        Check(typeof(GameInventory), nameof(GameInventory.TryInventorySlot),
              new[] { typeof(GameItem), typeof(GridShape), typeof(TagSystem) },
              "GameInventory.TryInventorySlot(item,shape,state)(Alt空格落点)");
        Check(typeof(GameItem), "get_shape", Type.EmptyTypes, "GameItem.shape");
        Check(typeof(GameItem), "get_state", Type.EmptyTypes, "GameItem.state");
        Check(typeof(GraphUtils), "TryAcceptOnce",
              new[] { typeof(GraphNodeStorage), typeof(GameItem), typeof(int) }, "TryAcceptOnce(数量语义基准)");

        // ── EmporiumEntry 柜台格 getter（slot: token 匹配依赖） ──
        Check(typeof(EmporiumEntry), "get_frontInvinvElement", Type.EmptyTypes,
              "EmporiumEntry.frontInvinvElement(slot: token)");
        Check(typeof(EmporiumEntry), "get_backInvinvElement", Type.EmptyTypes,
              "EmporiumEntry.backInvinvElement(slot: token)");
        Check(typeof(EmporiumEntry), "get_backInvinvElementCounter", Type.EmptyTypes,
              "EmporiumEntry.backInvinvElementCounter(slot: token)");

        // ── 整堆计价的挂点 ──
        Check(typeof(GameItem), nameof(GameItem.GetNegociatedValue), Type.EmptyTypes,
              "GameItem.GetNegociatedValue(整堆计价入口)");

        // ── 内置名单 id 存在性核对（只告警，不阻断） ──
        //   数据源 = Mods/item_catalog.json（第三方工具生成的物品目录）。
        CheckBlacklistIdsExist();

        if (failures.Count == 0)
            Log.Msg("Item Stacking 补丁目标自检通过：全部目标方法均已找到。");
        else
            Log.Warning("Item Stacking 补丁目标自检失败，以下功能已静默失效：\n" +
                        string.Join("\n", failures) +
                        "\n（游戏可能已更新方法名；请升级本 mod 或核对游戏版本。）");
    }

    // ── 内置黑名单 id 存在性核对 ──
    //   把「按 id 精确匹配」的名单项与 item_catalog.json 比对，列出查不到的 id（只告警，不阻断）。
    //   ★ 为什么用这个文件而不是游戏目录 API：DirectoryMaster.Item(id) 有副作用（会真的创建
    //     物品实例），绝不能用于探测。
    //   ★ 只核对「精确 id 名单」—— 前缀/子串/后缀是特征匹配，不对存在性。
    private static void CheckBlacklistIdsExist()
    {
        try
        {
            var path = FindItemCatalogJson();
            if (path == null)
            {
                Diag($"[名单自检] 未找到 item_catalog.json，跳过内置 id 存在性核对（不影响功能）。");
                return;
            }

            string text;
            try { text = System.IO.File.ReadAllText(path); }
            catch (Exception e)
            {
                if (e is OutOfMemoryException || e is StackOverflowException) throw;
                Diag($"[名单自检] 读取 item_catalog.json 失败，跳过核对：{e.Message}");
                return;
            }

            var known = MiniJson.ExtractAllStringsByKey(text, "id");
            if (known == null || known.Count == 0)
            {
                Diag("[名单自检] item_catalog.json 未解析出任何 id，跳过核对。");
                return;
            }
            var aliasIds = MiniJson.ExtractAllStringsByKey(text, "aliases");
            if (aliasIds != null)
                foreach (var a in aliasIds) known.Add(a);

            // 精确 id 名单核对。
            var missing = new List<string>();
            foreach (var id in BuiltinBlockItemIds)
                if (!string.IsNullOrWhiteSpace(id) && !known.Contains(id))
                    missing.Add(id);
            foreach (var id in BuiltinBlockSuffixExempt)
                if (!string.IsNullOrWhiteSpace(id) && !known.Contains(id))
                    missing.Add(id);

            if (missing.Count == 0)
            {
                Log.Msg($"[名单自检] 内置黑名单 id 全部命中物品目录（{BuiltinBlockItemIds.Length} 项，目录 {known.Count} 项）。");
            }
            else
            {
                // 「查不到」多因目录文件不全/旧版，不代表物品不存在、不影响拦截。
                Log.Msg($"[名单自检] 内置黑名单有 {missing.Count} 项 id 在目录文件里查不到" +
                        "（多为目录文件不全/旧版所致，不代表物品不存在、不影响拦截；" +
                        "如需精确核对，请更新 Mods\\item_catalog.json）:\n  - " +
                        string.Join("\n  - ", missing));
            }

            // 机器容器名单（owner: 后半段 / 裸 id）同样核对。
            // 机器 token 查不到意味着【对应机器的堆叠拦截静默失效】—— 数据保护级别，必须 Warning。
            var missingMachine = new List<string>();
            if (MachineContainerTokens != null)
            {
                foreach (var tok in MachineContainerTokens)
                {
                    if (string.IsNullOrWhiteSpace(tok)) continue;
                    var t = tok.Trim();
                    if (t.StartsWith(Constants.TokenSlotPrefix, StringComparison.OrdinalIgnoreCase)) continue;
                    var id = t;
                    if (id.StartsWith(Constants.TokenOwnerPrefix, StringComparison.OrdinalIgnoreCase))
                        id = id.Substring(Constants.TokenOwnerPrefix.Length).Trim();
                    if (id.Length == 0) continue;
                    if (!known.Contains(id)) missingMachine.Add(t);
                }
            }
            if (missingMachine.Count > 0)
            {
                Log.Warning($"[名单自检] 机器容器名单有 {missingMachine.Count} 项 token 在目录文件里查不到" +
                        "（目录不全，或游戏已改名 —— 对应机器的堆叠拦截将静默失效，" +
                        "请用最新目录核对后再决定是否调整名单）:\n  - " +
                        string.Join("\n  - ", missingMachine));
            }
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            Diag($"[名单自检] 存在性核对异常，跳过：{e.Message}");
        }
    }

    // 定位 item_catalog.json：Mods 目录（DLL 同目录）优先，其次 UserData。
    private static string FindItemCatalogJson()
    {
        try
        {
            var dllDir = System.IO.Path.GetDirectoryName(typeof(Main).Assembly.Location);
            if (!string.IsNullOrEmpty(dllDir))
            {
                var p1 = System.IO.Path.Combine(dllDir, "item_catalog.json");
                if (System.IO.File.Exists(p1)) return p1;
            }
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
        }
        try
        {
            var p2 = System.IO.Path.Combine(
                MelonLoader.Utils.MelonEnvironment.UserDataDirectory, "item_catalog.json");
            if (System.IO.File.Exists(p2)) return p2;
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
        }
        return null;
    }
}
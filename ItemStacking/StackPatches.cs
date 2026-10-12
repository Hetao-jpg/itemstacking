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
    // ── P1：补丁目标自检 ──
    // 字符串式 Harmony 补丁（MaxNumAdd / StartDrag）在游戏
    // 改名对应方法时会「静默失效」（Harmony 仅告警，功能无提示消失）。
    // 这里在 PatchAll 后逐一用 AccessTools 复核目标方法是否存在，缺失则明确告警。
    private static void ValidatePatches()
    {
        var failures = new List<string>();
        void Check(Type t, string name, Type[] args, string label)
        {
            if (AccessTools.Method(t, name, args) == null)
                failures.Add($"  - {label}  ({t.FullName}.{name})");
        }
        // 按名检查（参数类型不便引用时用；找不到该名的任何方法即视为失效）。
        // ★ 0.7.0 修复：原名版本调 `AccessTools.Method(t, name)`（不传参数类型）——
        //   当类型有重载时会抛 `AmbiguousMatchException`，整个自检中断（日志实锤：
        //   `Il2Cpp.GameGridInventory:TryFindOneValidInventorySlot` 有 (GameItem) 与
        //   (GameItem, bool) 两个重载）。改为「按名枚举全部重载」，找得到任一即算通过。
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
        // 成员存在性检查（属性优先）：Il2CppInterop 把 il2cpp 字段暴露成【属性】(get_/set_)，
        //   类型里并没有同名 CLR 字段 —— 只查 AccessTools.Field 会恒判失败。
        void CheckMember(Type t, string name, string label)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic
                                     | BindingFlags.Instance | BindingFlags.Static;
            if (t.GetProperty(name, flags) == null && t.GetField(name, flags) == null)
                failures.Add($"  - {label}  ({t.FullName}.{name})");
        }
        Check(typeof(GameItem), nameof(GameItem.MayStack), new[] { typeof(GameItem) }, "MayStack");
        Check(typeof(GameItem), nameof(GameItem.GetStackSlot), new[] { typeof(GameItem) }, "GetStackSlot");
        Check(typeof(GameItem), nameof(GameItem.MaxNumRemove), Type.EmptyTypes, "MaxNumRemove");
        Check(typeof(GameItem), "MaxNumAdd", new[] { typeof(SlotMarker) }, "MaxNumAdd(SlotMarker)");
        // （原 SaveItemNode.OnAfterDeserialize 自检随「读档补满」功能一并删除，2026-10-12）
        Check(typeof(ItemMouseDragHandler), "StartDrag", Type.EmptyTypes, "ItemMouseDragHandler.StartDrag");
        // 3.4 #3：IsContainer 依赖 FindAllChildItems —— 若游戏改名此方法，容器防护会「静默失效」
        // （比补丁失效更危险：不报错却允许合并容器，导致内部物品丢失）。此处一并纳入自检。
        Check(typeof(GameItem), "FindAllChildItems", Type.EmptyTypes, "FindAllChildItems(容器检测)");
        // 0.5.0：黑名单依赖 IsTag —— 若游戏改名此方法，黑名单整体静默失效（鉴定/赃物/所有权
        // 防护全丢，退回放任堆叠）。纳入自检。
        Check(typeof(GameItem), nameof(GameItem.IsTag), new[] { typeof(string) }, "IsTag(黑名单判据)");
        // 0.5.2：鼠判据依赖 GetTagReadonly（读 ANIMAL_SPECIES_TAG 的 str 值）—— 若游戏改名
        // 此方法，「鼠拦截」会静默失效（动物拦截仍在，但关掉 BlockAnimal 后鼠就能叠了）。纳入自检。
        Check(typeof(GameItem), nameof(GameItem.GetTagReadonly), new[] { typeof(string) }, "GetTagReadonly(鼠物种判据)");
        // 0.6.0 方向3a 依赖 DurabilityHelper.GetMaxDurability（判耐久，若游戏改名则耐久拦截静默失效）。
        Check(typeof(DurabilityHelper), "GetMaxDurability", new[] { typeof(GameItem) }, "GetMaxDurability(耐久判据)");
        // 0.6.0 方向4 依赖 EmporiumEntry.Instance（历史：Alt 快移曾自己找柜台库存；0.6.4 起
        //   本 mod 已不再自己找容器，改走原生「落点窗口」逻辑。保留这条自检是因为
        //   EmporiumEntry 仍是本 mod 其它判据的登记项，删掉会漏掉一次「游戏改类名」的预警）。
        Check(typeof(EmporiumEntry), "get_Instance", Type.EmptyTypes, "EmporiumEntry.Instance(柜台)");

        // ── 0.9.6：堆叠销毁修复补丁的依赖 ──
        Check(typeof(MachineBottlePrinter), nameof(MachineBottlePrinter.UpgradePrinter),
              new[] { typeof(GameItem), typeof(GameItem) }, "UpgradePrinter(水瓶打印机升级)");
        Check(typeof(ModuleHelper), nameof(ModuleHelper.RestoreModule),
              new[] { typeof(GameItem), typeof(GameItem) }, "RestoreModule(模块修复)");
        Check(typeof(ModuleHelper), nameof(ModuleHelper.RepairExtractor),
              new[] { typeof(GameItem), typeof(GameItem) }, "RepairExtractor(采矿机修理)");
        Check(typeof(GameItem), nameof(GameItem.GetSplitItem), new[] { typeof(int) }, "GetSplitItem(拆堆)");
        Check(typeof(GraphUtils), "TryAcceptAll",
              new[] { typeof(GraphNodeStorage), typeof(GameItem), typeof(int) }, "GraphUtils.TryAcceptAll(余量归还)");
        // ★ 0.10.0（阶段5 / mod评价.txt #128）：DestroyFix 与 Patch_TransferAfterhourToInv
        //   实际调用的是 `GraphUtils.TryAcceptAllMid(GameGridInventory, GameItem, int)`，
        //   但历史自检只查了 TryAcceptAll。若游戏改名 TryAcceptAllMid，拾荒口袋/熔炉等
        //   「余量归还」路径会【静默失效】（物品丢失），必须一并纳入自检。
        CheckByName(typeof(GraphUtils), "TryAcceptAllMid",
                    "GraphUtils.TryAcceptAllMid(口袋转移/余量归还)");

        // ── 0.9.8：拾荒口袋 / 熔炉 / 密码解序器 三个堆叠修复的依赖 ──
        Check(typeof(MachineDesequencer), nameof(MachineDesequencer.FinishDesequencer),
              new[] { typeof(GameItem), typeof(GameSlotInventory), typeof(GameSlotInventory) },
              "FinishDesequencer(解序器整堆销毁修复)");
        Check(typeof(EmporiumEntry), nameof(EmporiumEntry.TransferAfterhourToInv), Type.EmptyTypes,
              "TransferAfterhourToInv(拾荒口袋整堆转移)");
        CheckMember(typeof(EmporiumEntry), "afterhourPocketSlotInvBackpack",
                    "EmporiumEntry.afterhourPocketSlotInvBackpack(拾荒口袋槽)");
        CheckMember(typeof(EmporiumEntry), "afterhourPocketSlotInvRight",
                    "EmporiumEntry.afterhourPocketSlotInvRight(拾荒口袋槽)");
        CheckMember(typeof(EmporiumEntry), "afterhourPocketSlotInvLeft",
                    "EmporiumEntry.afterhourPocketSlotInvLeft(拾荒口袋槽)");
        CheckMember(typeof(GameSlotInventory), "currentItem",
                    "GameSlotInventory.currentItem(槽位物品·拆堆读/清槽写)");
        // 熔炉 lambda 目标（★0.9.9 起与 Patch_FurnaceConsume 共用同一解析器：
        // 候选名 + 枚举式兜底）——找不到即熔炉拆堆静默失效，必须启动时明确告警。
        if (!Patch_FurnaceConsume.FurnaceTargetOk())
            failures.Add("  - _Furnace_b__5(熔炉消耗拆堆) (MachineFurnace+__c__DisplayClass0_0)");

        // ── 0.6.4 / 0.7.0：Alt 快移的真实挂点与依赖 ──
        // 0.6.3 挂在 ItemMouseDragHandler 上是错的（它不监听 Alt，见文件末尾长注释）。
        CheckByName(typeof(ItemQuickTransferAltHandler), "OnEventRelease",
                    "ItemQuickTransferAltHandler.OnEventRelease(Alt快移①)");
        CheckByName(typeof(ItemQuickTransferAltHandler1), "OnEventRelease",
                    "ItemQuickTransferAltHandler1.OnEventRelease(Alt快移②)");
        // ★ 0.12.0（§四·2）：Shift 配对挂在 ItemQuickTransferHandler 上（三个 handler 统一按键分派）。
        CheckByName(typeof(ItemQuickTransferHandler), "OnEventRelease",
                    "ItemQuickTransferHandler.OnEventRelease(Shift快移)");
        // ★ 0.11.2：Alt 配对快移「跳过原生 → 手动复刻尾部清理」依赖以下成员：
        //   InputActionManager.ResetAfter（复位输入管线）、handler.currentItem（清空防重复搬运）、
        //   isQuickTransfer（复位快移开关）、quickTransferKey（判定是否松开快移键）。
        //   游戏改名 ⇒ 清理静默失效 → 复现「再按 Alt 还搬 / 单点左键也搬」bug，而自检仍报通过。
        // ★ 0.12.0：按键分派读 handler.key；三个 handler 都要查（任一缺失 ⇒ 该 handler 分派失效）。
        Check(typeof(InputActionManager), nameof(InputActionManager.ResetAfter),
              new[] { typeof(InputActionHandler) }, "InputActionManager.ResetAfter(Alt快移尾部清理)");
        CheckMember(typeof(ItemQuickTransferAltHandler), "currentItem", "ItemQuickTransferAltHandler.currentItem(Alt尾部清空)");
        CheckMember(typeof(ItemQuickTransferAltHandler), "isQuickTransfer", "ItemQuickTransferAltHandler.isQuickTransfer(Alt尾部复位)");
        CheckMember(typeof(ItemQuickTransferAltHandler), "quickTransferKey", "ItemQuickTransferAltHandler.quickTransferKey(Alt尾部判据)");
        CheckMember(typeof(ItemQuickTransferAltHandler), "key", "ItemQuickTransferAltHandler.key(按键分派①)");
        CheckMember(typeof(ItemQuickTransferAltHandler1), "key", "ItemQuickTransferAltHandler1.key(按键分派②)");
        CheckMember(typeof(ItemQuickTransferAltHandler1), "quickTransferKey", "ItemQuickTransferAltHandler1.quickTransferKey(按键分派②)");
        CheckMember(typeof(ItemQuickTransferHandler), "key", "ItemQuickTransferHandler.key(按键分派·Shift)");
        CheckMember(typeof(ItemQuickTransferHandler), "quickTransferKey", "ItemQuickTransferHandler.quickTransferKey(按键分派·Shift)");
        CheckMember(typeof(ItemQuickTransferHandler), "currentItem", "ItemQuickTransferHandler.currentItem(Shift尾部清空)");
        CheckMember(typeof(ItemQuickTransferHandler), "isQuickTransfer", "ItemQuickTransferHandler.isQuickTransfer(Shift尾部复位)");
        // ★ 0.12.1（问题1）：Ctrl 快移合并闸门依赖 SlotMarker.targetItem（0x30）——
        //   它是「本次是合并还是放置」的唯一判据，且我们会在导流时把它写成 null（可写字段）。
        //   若游戏改字段名，闸门会静默失效（Ctrl 快移又可无视名单合并），故纳入自检。
        CheckMember(typeof(SlotMarker), "targetItem", "SlotMarker.targetItem(Ctrl快移合并闸门)");
        CheckMember(typeof(SlotMarker), "inventory", "SlotMarker.inventory(Ctrl快移合并闸门·容器)");
        CheckMember(typeof(SlotMarker), "item", "SlotMarker.item(Ctrl快移合并闸门·源)");
        // ★ 0.7.0：Alt 快移改为「改写槽位查找结果」实现 —— 核心挂点是
        //   `GameInventory.TryFindOneValidInventorySlot`（**抽象**，Slot 26）的**各具体子类实现**。
        //   调用链实锤：ItemQuickTransfer*.OnEventRelease → GraphUtils.TryAcceptOnce(0x766730)
        //   → 接口解析 stub(0x180003FA0, ecx=4) → GraphNodeStorage 接口 slot 4 =
        //     TryFindOneValidInventorySlot。
        //   ★ 不能直接 patch 抽象声明（无 IL 体，HarmonyX GetMethodPatcher 会抛）；
        //     已改用 [HarmonyTargetMethods] 在运行期枚举具体子类（见 Patch_TryFindOneValidSlot）。
        //   此处只自检「至少存在一个具体实现」——若游戏删光实现，Alt 落点改写会失效。
        CheckByName(typeof(GameGridInventory), nameof(GameGridInventory.TryFindOneValidInventorySlot),
                    "GameGridInventory.TryFindOneValidInventorySlot(Alt落点改写①)");
        CheckByName(typeof(GameGridInventory), "TryFindOneValidInventorySlot_TopMiddle",
                    "GameGridInventory.TryFindOneValidInventorySlot_TopMiddle(存档用)");
        // 空格落点依赖 GameInventory.TryInventorySlot(item, shape, state) 这个公开便捷重载
        //   （RVA 0x784580）——它在内部转发到抽象 5 参版本，由子类的网格碰撞挑出"空格"。
        //   若游戏删掉/改签名此重载，Alt 找空格会静默失效（退回原生"只合并同名堆"的落点）。
        Check(typeof(GameInventory), nameof(GameInventory.TryInventorySlot),
              new[] { typeof(GameItem), typeof(GridShape), typeof(TagSystem) },
              "GameInventory.TryInventorySlot(item,shape,state)(Alt空格落点)");
        // 形状/状态来源属性：item.shape / item.state（dump.cs get_shape/get_state）。
        Check(typeof(GameItem), "get_shape", Type.EmptyTypes, "GameItem.shape(Alt空格形状)");
        Check(typeof(GameItem), "get_state", Type.EmptyTypes, "GameItem.state(Alt空格状态)");
        // ★ 状态一致性闸门（不可豁免的最终硬闸门）的【私有依赖】自检：
        //   StatesEqual 直接读 GameItem._state_k__BackingField（state 属性的 backing 字段）、
        //   TagSystem.dict 与 TagState 的 7 个值字段。旧自检只查了 get_state ——
        //   若游戏改掉 backing 字段名而 get_state 仍在，StatesEqual 会抛异常 → Evaluate 判 Deny
        //   →【堆叠整体静默失效】，而自检仍报「通过」。故这些私有成员必须一并纳入自检。
        CheckMember(typeof(GameItem), "_state_k__BackingField", "GameItem._state_k__BackingField(状态闸门)");
        CheckMember(typeof(TagSystem), "dict", "TagSystem.dict(状态闸门)");
        CheckMember(typeof(TagState), "valueEnabled", "TagState.valueEnabled(状态闸门)");
        CheckMember(typeof(TagState), "valueInt", "TagState.valueInt(状态闸门)");
        CheckMember(typeof(TagState), "valueLong", "TagState.valueLong(状态闸门)");
        CheckMember(typeof(TagState), "valueBool", "TagState.valueBool(状态闸门)");
        CheckMember(typeof(TagState), "valueFloat", "TagState.valueFloat(状态闸门)");
        CheckMember(typeof(TagState), "valueDouble", "TagState.valueDouble(状态闸门)");
        CheckMember(typeof(TagState), "internalValueString", "TagState.internalValueString(状态闸门)");
        // 契约自检：TryAcceptOnce 必须还是「amount<0 展开成整堆」的那个版本 ——
        //   数量钳制建立在「marker.numTransfer 是 Min(amount,numTransfer,...) 的一环」这个前提上。
        Check(typeof(GraphUtils), "TryAcceptOnce",
              new[] { typeof(GraphNodeStorage), typeof(GameItem), typeof(int) }, "TryAcceptOnce(数量语义基准)");
        // SlotMarker 成员 —— 落点改写直接读写 targetItem / numTransfer，游戏改名会静默失效。
        // ★ 修正：互操作把 il2cpp 字段暴露成属性，旧代码只查 AccessTools.Field 会恒判失败、
        //   每次启动误报「静默失效」。改用 CheckMember（属性或字段任一存在即通过）。
        CheckMember(typeof(SlotMarker), nameof(SlotMarker.targetItem), "SlotMarker.targetItem（Alt 落点判据）");
        CheckMember(typeof(SlotMarker), nameof(SlotMarker.numTransfer), "SlotMarker.numTransfer（Alt 数量钳制）");

        // ── 0.6.5：合并闸门的真实挂点 ──
        // 这是【所有合并路径】的公共瓶颈（拖放 / Alt 快移 / Ctrl 快移全部经此）。
        // 若游戏改掉这个方法签名或删掉它，柜台禁堆/作用域/黑名单对快移路径会静默失效。
        Check(typeof(SlotMarker), nameof(SlotMarker.TryAcceptOnce), new[] { typeof(int) },
              "SlotMarker.TryAcceptOnce(合并闸门·全路径)");
        // 柜台判据依赖 EmporiumEntry 的三个柜台格实例（0.6.6 修正：front / back / counter）。
        //   反汇编证实「柜台」= frontInvinvElement(0x88) + backInvinvElement(0x98)（GetCountersItems），
        //   backInvinvElementCounter(0xA0) 为第三个柜台格。任一 getter 缺失即视为柜台判据可能失效。
        Check(typeof(EmporiumEntry), "get_frontInvinvElement", Type.EmptyTypes,
              "EmporiumEntry.frontInvinvElement(柜台正面格)");
        Check(typeof(EmporiumEntry), "get_backInvinvElement", Type.EmptyTypes,
              "EmporiumEntry.backInvinvElement(柜台背面格)");
        Check(typeof(EmporiumEntry), "get_backInvinvElementCounter", Type.EmptyTypes,
              "EmporiumEntry.backInvinvElementCounter(柜台第三格)");

        // ── 0.9.0：整堆计价的挂点 ──
        // GameItem.GetNegociatedValue() 是交易系统的【唯一计价入口】（卖出/买入/批发/
        // 估价/盗窃/犯罪价值/保险 全走它）。游戏改名此方法 ⇒ 整堆计价静默失效（退回单件计价），
        // 而不会有任何报错 —— 与堆叠上限失效同级，必须纳入自检。
        Check(typeof(GameItem), nameof(GameItem.GetNegociatedValue), Type.EmptyTypes,
              "GameItem.GetNegociatedValue(整堆计价入口)");

        // ── 0.10.0（阶段5）：内置黑名单 id 存在性核对 ──
        //   内置名单是硬编码/外部 JSON 提供的字符串 id。游戏若删改物品 id，相关拦截会
        //   【静默失效】（既无报错也不命中）—— 这是本项目最痛的一类故障。
        //   这里在启动时把名单里「查不到对应物品」的 id 列出来告警（只告警、不阻断）。
        //   ★ 数据源：游戏物品目录（GameItemCatalog / 类似静态表）。取不到就跳过本项核对
        //     （不能让"目录 API 变了"变成新的启动失败点 —— 那反而是引入了新的静默面）。
        CheckBlacklistIdsExist();

        if (failures.Count == 0)
            Log.Msg("Item Stacking 补丁目标自检通过：全部目标方法均已找到。");
        else
            Log.Warning("Item Stacking 补丁目标自检失败，以下功能已静默失效：\n" +
                        string.Join("\n", failures) +
                        "\n（游戏可能已更新方法名；请升级本 mod 或核对游戏版本。）");
    }

    // ── 0.10.0（阶段5）：内置黑名单 id 存在性核对 ──
    //  把「按 id 精确匹配」的名单项与游戏物品目录比对，列出查不到的 id（只告警，不阻断）。
    //  数据源 = `Mods/item_catalog.json`（由别的工具生成的 480 项目录，含 id + aliases）。
    //  ★ 为什么用这个文件而不是游戏目录 API：
    //    · `DirectoryMaster.Item(id)` 有副作用（会真的创建物品实例），绝不能用于探测；
    //    · `DirectoryMaster.GetIdentifierList<T>(string)` 需要泛型类型参数，IL2CPP 互操作下
    //      反射构造泛型调用既脆弱又可能触发 JIT 链（本项目 0x80131506 铁律），不值得为"启动告警"冒这个险；
    //    · 而目录 JSON 是纯文本，读取零副作用、失败就跳过，安全侧。
    //  ★ 只核对「精确 id 名单」（BlockItemIds / SuffixExempt）——
    //    前缀/子串/后缀是特征匹配，本就不该也不可能逐个对存在性。
    private static void CheckBlacklistIdsExist()
    {
        try
        {
            var path = FindItemCatalogJson();
            if (path == null)
            {
                // 目录文件不存在 = 正常（该文件由第三方工具生成，不是必需）。静默跳过。
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
            // aliases 也纳入已知集合（id 与 aliases 都可能与游戏 identifier 对应）。
            var aliasIds = MiniJson.ExtractAllStringsByKey(text, "aliases");
            if (aliasIds != null)
                foreach (var a in aliasIds) known.Add(a);

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
                // ★ 文案修正（2026-10-12）：这些「查不到」的 id 绝大多数是【目录文件 item_catalog.json
                //   不全/旧版】（如酿酒 DLC 的 be_* 物品），不代表游戏里真的没有、也不代表拦截失效。
                //   拦截靠 identifier 精确匹配，物品在游戏里存在就能拦，与目录文件无关。
                //   故降级为 Msg（非故障），并如实说明「需用最新目录文件核对」。
                Log.Msg($"[名单自检] 内置黑名单有 {missing.Count} 项 id 在目录文件里查不到" +
                        "（多为目录文件不全/旧版所致，不代表物品不存在、不影响拦截；" +
                        "如需精确核对，请更新 Mods\\item_catalog.json）:\n  - " +
                        string.Join("\n  - ", missing));
            }
        }
        catch (Exception e)
        {
            if (e is OutOfMemoryException || e is StackOverflowException) throw;
            // 核对是"锦上添花"，任何意外都不应影响启动。
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


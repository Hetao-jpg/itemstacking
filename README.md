# Item Stacking —— 物品堆叠增强 mod 开发项目

> 游戏：**Probably Stolen**（Demo 0.46D / Playtest 0.46D），Unity IL2CPP + MelonLoader 0.7.4  
> 目标：提升 / 自定义物品堆叠上限（"Stack Size Edit" 类功能）  
> 工作依据：`..\mod开发指南.md`（AI 操作版）  
> 堆叠机制数据 / 钩子点：见 `资料库\堆叠资料库.md`

## 方案（0.9.0：交易系统按【整堆总价值】计价 —— 已实机验收通过）

> **本轮问题**：本 mod 放开堆叠后，把一个 ×N 的堆卖给客户，**只按 1 件结账**，
> 其余 N-1 件的价值直接蒸发。

> **根因（反汇编实锤，`base+RVA`，ImageBase=0x180000000）**：
> 游戏里有**两条**价值路径，对 `unitCount`(`0x1F8`) 的处理**不一样**：

| # | 方法 | 对 unitCount 的处理 | 结论 |
|---|---|---|---|
| ① | `GameItem.GetValue(List<GameCharacterItem>, GameCharacterItem, bool)` @ `0x7979E0` | `0x180797A50  mov ecx,[rbx+0x1F8]` → `0x180797A68  imul rax,rdx` | **本就按整堆计价** |
| ② | `GameItem.GetNegociatedValue()` @ `0x796340` | `0x796340`~`0x796A73` 全程**无一处**读 `[rbx+0x1F8]` | **只返回单件价值** |

> 而**交易系统 100% 走 ②**（全库 call 站点扫描，`base+RVA`）：
> `PlayerStore.SellItem` / `OnItemsSold` / `BuyItem` / `BuyItemFromTable` / `CanBuyItemFromTable` /
> `HasBudgetLeftToBuy` / `GetWholesalePrice` / `GetTotalEstimatedValue` / `GetTypesByValue` /
> `StealItem` / `HandleInsurance` / `StoreClient`(显示与预算) /
> `NegociationUIManager`(`SumNegociatedValue` / `GetFinalOfferValue` / `GetMultiSellValue` /
> `InitUIWithItemSellMode`) / `SoldItem.TrackItem` / `SecData.CheckSoldChem·Contraband·DirtyWater·
> EndangermentItem` / `ContrabandHelper.GetReportReward` / `ExchangeList`
> ⇒ 30+ 个调用点，**改一处即可全覆盖**；逐个改调用方必然漏，漏一个就是静默半价。

> **改法（只加一个补丁）**：`Patch_GetNegociatedValue`
> —— `[HarmonyPatch(typeof(GameItem), nameof(GameItem.GetNegociatedValue))]` 的 Postfix
> 里 `__result *= unitCount`。
> ```csharp
> int count = __instance.unitCount;
> if (count <= 1) return;                 // 单件 = 原生行为，不动
> long unit = __result;
> if (unit <= 0) return;                  // 原生已判不可售/未定价 → 不放大
> long total = unit * count;
> if (total > int.MaxValue) { total = int.MaxValue; WarnOnce(...); }  // 见下方「为何钳 int.MaxValue」
> __result = total;
> ```
> **安全阀四条**：`StackEnabled` 关 → 不动；`StackValueTotal` 关 → 不动；
> `count<=1` → 不动；`unit<=0` → 不放大；溢出钳 `int.MaxValue` + 一次性告警；
> 异常时退回单件计价（`Log.Error`）。**只放大、不改判定逻辑**，不触碰任何委托字段。

> **★ 为何必须钳 `int.MaxValue`**：`GetNegociatedValue` 返回 `Int64`，
> 但**大量调用方按 32 位 `eax` 使用返回值**（例：`PlayerStore.SellItem`
> `mov rsi, rax` / `mov r14d, eax`）。超 32 位会被静默截断成**负数** ⇒ 白送/倒扣。
> 例：单价 3000 的堆 ×10 = 30000 没问题，但稀有高价值堆很容易破 2^31。

> **连带影响（用户已知悉并确认：属游戏本身机制，正常现象）**：
> 同一方法也用于 ① 盗窃/检查损失价值 ② 卖掉毒品/赃物/脏水/濒危物品的**犯罪价值**
> ③ 保险理赔 ④ 客户预算封顶。这些会**同步 ×N** —— 是「按整堆总价值计价」的必然结果。

> **实机验收（用户 2026-10-09 确认通过）**：
> - ✅ 启动日志「补丁目标自检」未报 `GetNegociatedValue(整堆计价入口)`（补丁挂上了）
> - ✅ `[整堆计价] xxx：单价 N × 2 件 = 总价 2N` 探针正常输出
> - ✅ **扣减语义正确**：卖出 ×N 的堆 → 柜台扣走 N 件、到账 N 倍价值（无「付一半钱扣整堆」）
> - ✅ 数值同步 ×N 属正常机制，非 bug
>
> ⚠️ 静态分析阶段最担心的一点已排除：`PlayerStore.SellItem` 里
> `cmp dword ptr [rbx+0x34], esi / cmovl esi,[rbx+0x34]`（客户预算硬封顶）
> 曾担心出现「付部分钱、扣走整堆」的数据损失 —— 实测**未发生**。

> **同版本附带变更**：`BlockCounterStack`（柜台禁堆）默认值 `true` → **`false`**。
> 理由：当初加这道闸门是为规避「柜台亏钱」；0.9.0 补上整堆计价后该前提已消失，
> 柜台合并不再亏钱，反而能腾出格位。担心同种商品难分拣者可手动改回 `true`。
> 判据代码（三格指针比对 `IsCounterSlot`）**保留未动**，随时可开。

> 版本 0.9.0，已实机验收。部署产物 md5 `77dedb6409895d3a7871cec1cbdb4416`（49152 字节）。
> 部署文件名 `Mods\物品堆叠.dll`（中文名），回滚点 `物品堆叠.dll.bak-20261009-180125`
> （0.8.1，md5 `33d585932981bc13952f91c3e53bebd5`）。

## 方案（0.7.0：Alt 快移重构 —— 落点改写 + 数量钳制；修三处实机故障）

> **本轮背景**：0.6.6 之后，用户实机测试 Alt 快移时反馈
> ①「移 1 个后其余自动寻路到别处」②「柜台已有同名物品时无法再移入」③「只能仓库→柜台」。
>
> **0.7.0 改法（更简单、更贴近原生）**：
> - 挂点从「自己拆堆 + 归还溢出」改为**改写原生落点**：
>   `GameInventory.TryFindOneValidInventorySlot`（vtable slot 26）的 4 个具体子类实现
>   （`GameGridInventory` / `GameGridScrollableInventory` / `GameSlotInventory` /
>    `GameCharacterRaidInventory`），用 `[HarmonyTargetMethods]` 运行期枚举。
> - 数量钳制：`SlotMarker.numTransfer` 钳到 `AltQuickTransferAmount`
>   （`SlotMarker.TryAcceptOnce` 内部 `n = Min(amount, numTransfer, MaxNumRemove())`）
>   ⇒ 源堆原地减 N、位置不变，**无需拆堆**。
>
> **★ 本轮实机日志（17:36）暴露的三处故障，已全部修复**：
>
> | # | 故障 | 根因 | 修复 |
> |---|---|---|---|
> | 1 | `Failed to HarmonyInit PatchAll` + `NullReferenceException at TargetMethods()` | `TargetMethods()` 期间 `Log` 仍为 null，而**所有 `Log.*` 都在 try 之外** → NRE 冒泡整类注册失败 | 全程 try + `LogSafe`/`LogErrSafe`（`Log?.Msg`）null 安全 helper |
> | 2 | `AmbiguousMatchException … GameGridInventory:TryFindOneValidInventorySlot` | `ValidatePatches.CheckByName` 用 `AccessTools.Method(t, name)` 不传参数类型 → 有重载（`(GameItem)`/`(GameItem,bool)`）即抛 | 改 `AccessTools.GetDeclaredMethods(t)` 按名枚举，找到任一即通过 |
> | 3 | Alt 排查无信息 | `WarnOnce("altslot-fire")` 只打一次 + `Diag` 默认关 | 改**逐次无条件**输出：命中快照 / 容器无空格 / 数量钳制 |
>
> **★★ 第二轮实机（17:47）：Alt 仍"放不过去" —— 根因确诊 + 修复**
>
> 日志实锤（规律：**柜台已有同名堆时必失败**）：
> ```
> [Alt 落点] 命中：item=toilet_paper ×1，… targetItem=toilet_paper，容器=backInv，numTransfer=1
> [Alt 找空格] backInv：TryInventorySlot → targetItem=null，numTransfer=0   ← 这里！
> ```
>
> **反汇编确诊（`base+RVA`，ImageBase=0x180000000）**：
> - `GameInventory.TryInventorySlot(item,shape,state)`(0x784580) 内部把 **鼠标当前位置**
>   （`[单例+0xB8]+0x38/0x3C`）当 `itemGraphicsCenterPosition`。
>   Alt 点击时鼠标在**源物品**上、不在柜台格内 → 放置校验全挂
>   （`GameGridInventory.TryInventorySlot` 0x763BC0 里三个校验任一失败就 `je 0x763DAF` 跳尾声、
>    **不写 numTransfer**）→ 返回的 marker 是 `targetItem=null, numTransfer=0` 的**空壳**。
> - `SlotMarker..ctor`(0x7B01F0) **不初始化 numTransfer**。
> - `TryAcceptOnce`(0x7B0020) 算 `n = Min(amount, 0, …) = 0` → **什么都不搬**（静默失效）。
> - 游戏自己的"能放下"判据也是 `numTransfer > 0`
>   （`GameGridScrollableInventory` 空槽分支 0x77BCAD 内 `0x77BDBA cmp [rax+0x38],0` / `jg`）。
>
> **修复**：
> 1. **废弃**"自己调 `TryInventorySlot` 找空格"（位置不对，必然失败）。
> 2. **Alt 作用域内让 `GameItem.MayStack`(0x799120) 恒 false** ——
>    原生 `TryFindOneValidInventorySlot` 的"合并同名堆"循环（`GameGridScrollableInventory`
>    0x77BB4E 内 `call MayStack`）全部落空 → **自动落到它自己的空槽分支**（0x77BCAD）
>    → 返回真正的空槽（`numTransfer>0`）。实现 = 在既有 `Patch_MayStack.Prefix` 开头加一段
>    `if (AltTransferScope) { __result=false; return false; }`（**不新建补丁类**）。
>    ⇒ 完全交给游戏自己的搜索，**不依赖鼠标坐标**，天然避开已有物品 / 不堆叠 / 不重叠。
> 3. Postfix 加兜底：合并落点 → null；`numTransfer <= 0` → null。
>
> **需求2：模组 / 节点加入堆叠黑名单**
> - `item_catalog.json` 实查：「模组」= **`module*`**（`system_module_*` / `farm_module_*` /
>   `furnace_module_*` / `printer_module_metal` / `blank_module` / `chem_module` /
>   `alarm_module_transmitter` / `crypto_module_*` / `module_extractor*` / `module_bay_expansion_kit` …）；
>   「节点」= **`node_small` / `node_medium`**。
> - 新增 `BuiltinBlockIdPrefixes = { "module", "node" }`，`GetBlacklistReason` 用
>   `identifier.StartsWith(prefix, OrdinalIgnoreCase)` 拦截 ⇒ 覆盖同族全部 id + 未来新模组。
>
> 版本 0.7.0。部署产物 md5 `1b02a41a3462fd72fb934ef666f84e0e`。
> 部署文件名 **`Mods\物品堆叠.dll`（中文名）**，备份 `物品堆叠.dll.bak-20261008-175157`。

## 方案（0.7.1：扩充黑名单 + 修「跨场景回店只搬 1 件」）

> **需求 0–4：扩充堆叠黑名单**（用户 2026-10-08 实机清单）
>
> | # | 用户需求 | 落地 id / 判据 |
> |---|---|---|
> | 0 | 净水片、识别卡（多种）入黑名单；**空白卡不入** | `water_tablet` + `water_tablet_container`；**后缀 `_keycard`** 覆盖 med/eng/cmd/sci/sec/ser/sup 7 种一次性钥匙卡，**豁免 `blank_keycard`** |
> | 1 | 空电池入黑名单 | 目录无「空电池」物品 ⇒ 指可充能电池系列，三个 id 全拦：`energy_credit` / `energy_credit_ext` / `energy_credit_breeder` |
> | 2 | 模组入黑名单 | 已有 前缀 `module*`（0.7.0） |
> | 3 | 莓果入黑名单 | `wine_berry`(荧光莓) / `stardust_berry_fruit`(星尘莓) / `bloomberry_seed`(花莓种子) / `wine_bloomberry`(荧光莓果酿) / `wine_gloomberry`(阴光莓果酿) |
> | 4 | 能量阵列入黑名单 | `powerblock` |
>
> 新增机制：**`BuiltinBlockIdSuffixes`（后缀名单）+ `BuiltinBlockSuffixExempt`（豁免表）**。
> `GetBlacklistReason` 用 `identifier.EndsWith(suffix, OrdinalIgnoreCase) && !IsSuffixExempt(id)` 判定，
> 保证「整个钥匙卡系列」一次覆盖，同时 `blank_keycard` 明确放行。
>
> **需求 5：修「堆叠物品转场景回店只搬 1 件」**（用户诊断："可能是柜台的拒绝堆叠判定导致的" —— **判断正确**）
>
> **根因链（反汇编实锤，`base+RVA`）**：
> 1. 雇员远征返回 → `StoreClientListSpec.CreateHirelingClientReturn`(0x62B170) 的对话结束回调
>    → **`ExpeditionHelper.SetHirelingEquipmentOnCounter`(0x8B6820)**：
>    ```
>    0x1808B68B3  mov rcx,[rcx+0x80]        ; frontInvWindow
>    0x1808B68C0  lea r8d,[r9-1]            ; amount = -1（整堆）
>    0x1808B68C9  jmp GraphUtils$$TryAcceptAll   ; 塞进柜台
>    ```
> 2. **`GraphUtils.TryAcceptAll(node, item, amount)`(0x766650) 是「逐格填充」循环**：
>    ```
>    0x1807666B3  cmp  ebx,edi / jge end     ; while (已接受 < 总量)
>    0x1807666C9  call [stub 0x180003FA0]    ; 接口 slot4 = TryFindOneValidInventorySlot(item)
>    0x1807666CE  test rax,rax / je end      ; ★ marker==null → **break**
>    0x1807666D3  cmp  [rax+0x38],0 / jle end; ★ numTransfer<=0 → **break**
>    0x1807666E3  call SlotMarker$$TryAcceptOnce(剩余)
>    0x1807666E8  test eax,eax / je end      ; ★ 返回 0 → **break**
>    0x1807666EC  add  ebx,eax               ; 已接受 += n
>    ```
> 3. **`GameGridInventory.TryFindOneValidInventorySlot`(0x763AE0) 结尾无条件硬编码 1 件**：
>    ```
>    0x180763B86  mov rax,rbx
>    0x180763B89  mov dword ptr [rbx+0x38], 1   ; ★ numTransfer = 1（永远）
>    ```
>    **而 `EmporiumEntry.frontInvinvElement` 的类型正是 `GameGridInventory`**（dump.cs 65282 行）。
>    ⇒ 第 1 轮只接受 1 件；第 2 轮该槽已占 → 它的可行性检查失败返回 null → **循环 break**
>    ⇒ **整堆只搬进去 1 件，其余"留在原地"**（与用户描述完全一致）。
>
> **0.6.5~0.7.0 的「柜台禁堆」为什么会加重它**：
> 那版只在 `SlotMarker.TryAcceptOnce` 里 `__result = 0` —— 正好命中上面第 3 个 break 分支，
> 让"已有同名堆"的柜台**第一轮就 break**（搬 0 件），比原生还差。
>
> **修复（两条，都贴合原生语义）**：
> 1. **柜台禁堆闸门从 `TryAcceptOnce` 上提到 `GameItem.MayStack`**：
>    在 `Patch_MayStack.Prefix` 加分支 —— 若 `__instance.parentInventory` 是柜台容器
>    （新增 `IsCounterInventory(GameInventory)`，复用 front/back/counter 三格指针比对），
>    则 `__result = false` ⇒ 原生 `TryFindOneValidInventorySlot` 的"合并同名堆"循环全部落空，
>    直接给出**空槽落点** ⇒ `TryAcceptAll` 的逐格填充得以继续到放完为止。
>    **柜台里每个商品依旧各自占一格（不合并），但不再丢件。**
> 2. **抬升 `GameGridInventory` 的硬编码 1 件**：在 `Patch_TryFindOneValidSlot.Postfix`
>    的非 Alt 分支里，若落点是空槽（`targetItem == null`）却只给 1 件、而源堆有多件，
>    则抬到 `min(StackCap, item.unitCount)` —— 与 `Patch_MaxNumAdd` 的空槽语义一致
>    （`GameGridInventory` 这条路径绕过了 `MaxNumAdd`，所以在此补上）。
>    ★ 只抬升不收紧；非空槽不动；用 `unitCount` 封顶，避免旧档大堆借道绕过上限。
>
> **诊断探针**：`ProbeNonAltMarker` —— 非 Alt 路径下「源堆多件却只给 1 件」时无条件打一行
> `[转场探针] 非Alt落点只给1件：…`（按容器+item 去重，上限 40 条），用于实测确认。
>
> 版本 0.7.1。部署产物 md5 `22cc99dc73f40a6b4bd30f77c051b03a`。
> 部署文件名 **`Mods\物品堆叠.dll`（中文名）**，备份 `物品堆叠.dll.bak-20261008-210418`。

## 方案（0.8.0：cfg 黑白名单重构为「四列表」）

> **用户决策**：把 cfg 的黑白名单整理成 **容器白名单 / 容器黑名单 / 物品白名单 / 物品黑名单** 四个列表；
> 「模组」这类（`module*`）纳入**物品黑名单**（内置前缀，继续硬编码在代码里）。
>
> **四列表语义（0.8.0 起）**：
>
> | 键 | 类型 | 语义 |
> |---|---|---|
> | `StackBlacklist` | bool | 保护总闸（保留旧键名，向后兼容）。关闭 = 全部黑白名单与类判据失效 |
> | `ContainerWhitelist` | string | **合并旧 StackScope + StrictWhitelist**。留空 = 不限容器；非空 = **只有名单内容器可堆叠** |
> | `ContainerBlacklist` | string | 原 StackExclude 改名。名单内容器禁止堆叠（双端判定，命中即拦截） |
> | `ItemWhitelist` | string | 新增。**黑名单优先**：命中者豁免「类判据」，但不豁免「按 id 黑名单」 |
> | `ItemBlacklist` | string | 原 BlockItemIds 改名。按 id 禁止堆叠（仅自定义项；内置名单在代码常量） |
> | `BlockCounterStack` | bool | 保留。柜台禁堆（容器黑名单的运行时实例版，三格指针比对） |
>
> **已移除的旧分项开关（改为恒定生效，P0 数据保护不给关）**：
> `BlockInspectable` / `BlockStolen` / `BlockUnowned` / `BlockChargeMismatch` / `BlockAnimal` /
> `BlockRat` / `BlockWineBottle` / `BlockDurable` / `BlockLiquidContainer` / `StrictWhitelist` /
> `StackScope` / `StackExclude` / `BlockItemIds`。
> ⇒ 这些类判据防的是「物品独立状态丢失」与「盗窃漏洞」，关掉必然出事故。要放行个别物品用【物品白名单】豁免。
>
> **物品白名单的精确语义（黑名单优先）**：
> - 命中 `ItemWhitelist` 的物品 → 跳过**类判据**整体拦截
>   （可鉴定 / 赃物 / 未拥有 / 电量不同 / 活体动物 / 鼠 / 酒瓶 / 耐久 / 液体容器）。
> - 但**仍会**继续走【按 id 黑名单】（内置 id + 前缀 `module*`/`node*` + 后缀 `*_keycard` + 自定义 `ItemBlacklist`）
>   —— id 级黑名单是死命令，白名单不能翻案。
>
> **模组 → 物品黑名单（用户明确要求）**：沿用 0.7.0 的 `BuiltinBlockIdPrefixes = { "module", "node" }`
> 前缀匹配（保留，覆盖同族全部 id + 未来新模组）。属「内置」项，不写进 cfg，见启动日志「物品黑名单·内置」行。
>
> 版本 0.8.0。部署产物 md5 `9737628622a3485b886651fdcfbb0c40`。
> 部署文件名 **`Mods\物品堆叠.dll`（中文名）**，备份 `物品堆叠.dll.bak-20261008-212156`。

## 方案（0.8.1：模组黑名单从「前缀」改为「子串」—— 修复模组仍能堆叠）

> **用户实测反馈**：模组在游戏里仍然可以堆叠。
>
> **根因（对照 `item_catalog.json` 480 条 + `items_catalog.md` 运行期表排查）**：
> 0.7.0~0.8.0 用 `BuiltinBlockIdPrefixes = { "module", "node" }` 的 **StartsWith 前缀匹配**，
> 但游戏里绝大多数模组的 id 是 **module 在中间/末尾** 的形式，前缀匹配抓不到：
>
> | 形式 | 举例（id） | 前缀 `module` 命中？ |
> |---|---|---|
> | `module_*`（module 开头） | `module_extractor` / `module_bay_expansion_kit` | ✅ 命中 |
> | `system_module_*` | `system_module_corrupt` / `eco` / `efficiancy` / `fineness` / `overclock` / `performance` / `quality` / `ruined` / `streamlining`（9 个） | ❌ 漏 |
> | `farm_module_*` | `nightmare_cap` / `fillerweed_production` / `dream_cap` / `kotton_loom` / `smuggler` / `hydroreed_filter` / `nutrifruit`（7 个） | ❌ 漏 |
> | `crypto_module_*` | `cmd` / `eng` / `med` / `sec` / `ser` / `sup`（6 个，密码解序器芯片） | ❌ 漏 |
> | 其它含 module | `alarm_module_transmitter` / `blank_module` / `chem_module` / `furnace_module_blast` / `furnace_module_junk` / `printer_module_metal` / `evaporator_module_solvent_recovery` / `wage_ai_module` | ❌ 漏 |
> | `neural_core`（不含 module） | `system_capped_neural_core` / `system_uncapped_neural_core`（神经核心模组） | ❌ 漏 |
>
> **修复**：
> - 新增 `BuiltinBlockIdContains = { "module", "neural_core" }`，用 **`identifier.Contains`（子串）**
>   替换原来的 module 前缀匹配 —— module 在 id 哪个位置都能抓。
> - `BuiltinBlockIdPrefixes` 收缩为 `{ "node" }`（节点 `node_small`/`node_medium` 本来就在开头，前缀够用）。
> - 覆盖校验：`item_catalog.json` 里中文名含「模组/模块」的 **28 个物品全部命中、漏拦 0**；
>   id 含 `module` 的 32 个全部被子串覆盖；另有运行期专属 `wage_ai_module`（不稳定AI模组）也被覆盖。
>
> 版本 0.8.1。部署产物 md5 `0ec48ed5a115b5749a1fb1bae19f469a`。
> 部署文件名 **`Mods\物品堆叠.dll`（中文名）**，备份 `物品堆叠.dll.bak-20261008-213047`。

## 方案（0.6.6 修订：cfg 载入失败的真凶 = 非法 TOML）

> **实机日志暴露的真凶**（16:06:48）：
> ```
> [ERROR] Error while Loading Preferences from …UserData\MelonPreferences.cfg:
>   Tomlet.Exceptions.TomlInvalidValueException:
>   Expected the start of a number, string literal, boolean, array, or table on line 687, found 'w'
> [WARNING] Defaulting …MelonPreferences.cfg to Fallback Functionality to further avoid File Corruption...
> ```
> 第 687 行 = `BlockItemIds = water_test_strip,water_tablet` —— **未加引号的逗号分隔值在 TOML 里非法**
> （本 mod 旧版默认值经 ModManager / 手改落盘时丢了引号）。
>
> **连锁后果（严重）**：MelonLoader 载入失败 → 整份 cfg 进入 **Fallback 模式** →
> **全游戏所有 mod 的配置都无法再写盘**（这正是用户「cfg 改完存不住」的根因，
> 也解释了为什么新增的 `BlockCounterStack` / `StrictWhitelist` 一直不出现在 cfg 里，
> 尽管启动日志明明打了 `MelonPreferences Saved to …`）。
>
> **修复**：
> - `BlockItemIds` 默认值改 **空字符串**；内置拦截项 `water_test_strip` / `water_tablet`
>   移到代码常量 `BuiltinBlockItemIds`，不再经 cfg（空值不会触发该 bug）。
> - cfg 描述注明「自定义时务必用引号包起来」。
> - **手工修复**本机 `UserData\MelonPreferences.cfg` 第 687 行（加引号）+
>   全量扫描确认无其它非法裸值；备份 `MelonPreferences.cfg.bak-tomlfix-*`。
> - 柜台拦截改走 **无条件日志**（`[柜台禁堆] 已拦截合并…`，不受 `DiagLog` 影响），下次实测一眼可判。
>
> **本轮（第五轮）探针实锤**：
> ```
> [柜台探针] 本次合并落点容器 = [backInv @2FFF64EB000]
>   || EmporiumEntry：frontInvinv=[<空id> @30013B1AB40] / backInvinv=[backInv @2FFF64EB000] / backCounter=[<空id> @2FF4DB5DD80] …
> [柜台id] front=[<空id>] / back=[backInv @2FFF64EB000] / counter=[<空id>] / 背包=[<空id>] / 展示柜=[<空id>]
> ```
> - 落点 = `backInvinvElement`(0x98) **正是 0.6.6 新增判据覆盖的容器** → 三格判据方向正确。
> - 柜台容器**有 identifier = `backInv`** —— 旧版 `StackExclude = "back_inv"` 匹配不上的真因
>   就是**少写了中间的下划线**。
>
> 版本 0.6.6（修订）。部署产物 md5 `8150a06ec99376d143841bf8465f40db`（41984 字节）。

## 方案（0.6.6：修正柜台容器判据 —— 0.6.5 认错了容器）

> **0.6.6 本次变更（用户反馈：柜台堆叠问题没解决，怀疑柜台 id 认错 / 白名单黑名单没按期望实现）**：
>
> **根因（反汇编实锤）**：0.6.5 只用 `EmporiumEntry.backInvinvElementCounter`(**0xA0**) 一个容器做柜台判据，
> **但这根本不是玩家交互的那个容器**：
> - `EmporiumEntry.GetCountersItems`(0x7CE010) 读的是 `[rbx+0x88]`(frontInvinvElement)
>   **Concat** `[rbx+0x98]`(backInvinvElement) ⇒ 游戏眼中「柜台」= **front + back 两个格**。
> - `EmporiumEntry.Start`(0x7D49A0) 里三个柜台格构造尺寸都是 **18×4**（= `COUNTER_WIDTH` × `COUNTER_HEIGHT`）：
>   | 偏移 | 容器 | 尺寸 |
>   |---|---|---|
>   | `0x88` | `frontInvinvElement` | 18×4 |
>   | `0x98` | `backInvinvElement` | 18×4 |
>   | `0xA0` | `backInvinvElementCounter` | 18×4 |
> - 形状改写只发生在 0x88：`ResetFrontCounterGrid`(0x7D38B0) / `ExpandFrontCounterGrid`(0x7CB3B0)
>   都 `mov rcx,[rcx+0x88]` → `SetShape`；`SwapInv`(0x7D7BB0) 交换 0x88 ↔ 0x98。
> - `backInvinvElementCounter`(0xA0) **全库只有 3 处引用**（`Start` 构造 + `GetAllOwnedItems(Shallow)` 读），
>   玩家拖放/快移**永远落不到它** ⇒ **0.6.5 的判据恒 false，功能静默失效**（这就是「没解决」的原因）。
>
> **修复**：
> - `IsCounterSlot` 改为比对 **front(0x88) + back(0x98) + counter(0xA0)** 三个柜台格实例，任一命中即视为柜台。
> - 新增 `[柜台探针]`（每次真实合并打一行落点容器 + EmporiumEntry 全部候选容器 id/指针）
>   与 `[柜台id]`（进交易场景时一次性打印柜台各容器真实 identifier），便于实机对齐与填白名单。
>
> **白名单语义补齐**（用户原话「除了白名单里的容器 id 以外一律不实现堆叠功能」）：
> - 新增 cfg **`StrictWhitelist`**（默认**关**，保持向后兼容）。开启后：
>   仅 `StackScope` 白名单内容器可堆叠，**留空 = 任何容器都不堆叠**。
> - 关闭（默认）：`StackScope` 留空 = 不限范围（旧行为）。
>
> **补丁自检 20 → 22 项**（新增 `frontInvinvElement` / `backInvinvElement` 两个柜台格 getter 检查）。

## 方案（0.6.5：柜台禁堆补齐 —— 闸门下沉到「所有合并路径的公共瓶颈」）

> **0.6.5 本次变更（用户反馈）**：  
> 堆叠黑名单 / 作用域 / 容器排除**只拦住了鼠标拖放的合并**；  
> 按住 LeftAlt 或 LeftCtrl 点击物品时，第 2 件照样并进柜台里已有的同名堆。  
> 期望：除了白名单里的容器 id 以外，一律不实现堆叠功能。
>
> **根因（反汇编实锤）**：原有闸门挂在 `GameItem.MayStack` / `GameItem.GetStackSlot`  
> —— 那是 **拖放** 路径。键位快移走的是**完全独立的另一条路**：
>
> ```
> ItemQuickTransferAltHandler(1).OnEventRelease   (Alt, 0x7E1620 / 0x7E0C30)
> ItemQuickTransferHandler.OnEventRelease          (Ctrl, 0x7E2090)
>   → GraphUtils.TryAcceptOnce(GraphNodeStorage, GameItem, int)   0x766730
>   → SlotMarker.TryAcceptOnce(int)                               0x7B0020
>        · targetItem == null（空槽）→ GameInventory.UncheckedAccept（Slot 19，纯「移动」）
>        · targetItem != null（有堆）→ SlotMarker.StackItemUnchecked 0x7AFDF0  ← 真合并
> ```
>
> `SlotMarker.StackItemUnchecked` 是真正的合并实现，汇编只有两行算术：
>
> ```
> 0x7AFE41  add dword ptr [rdi + 0x1F8], esi    ; 目标堆.unitCount += n
> 0x7AFE50  sub dword ptr [rbx + 0x1F8], esi    ; 源堆.unitCount   -= n
> ```
>
> —— **一个 gate 都不查**（无 `MayStack`、无 `GetStackSlot`、无委托回调）  
> ⇒ 快移能绕过 100% 的既有判定。
>
> **修复（闸门下沉到公共瓶颈）**：新增补丁 `SlotMarker.TryAcceptOnce`。  
> 这条路由**所有**合并路径共用（拖放 `EndDrag`、Alt 快移、Ctrl 快移、任何未来入口）。  
> · **只在 `targetItem != null`（确实要合并）时判定**；空槽放置（`targetItem == null`）  
> 是「移动」不是「堆叠」，一律放行 —— 否则「Ctrl 整堆放空格」「Alt 快移进空位」会被误伤。  
> · 落点容器从 `slot.inventory`（`SlotMarker` +0x18，`GameInventory`）取；  
> 对快移路径它就是「鼠标落点窗口里第一个有效槽所在的容器」，与拖放语义一致。  
> · 判定完全复用既有函数（柜台 / `StackExclude` / `StackScope` / 物品黑名单 / 容器防护），  
> 保证「拖放拦的，快移也拦」。  
> · 命中即 `__result = 0`（= 剩余未接受数量 = 全部没接受），与原生「无处可放」时  
> 返回值语义一致 ⇒ 上层安全地把物品留在原处，不做任何数量搬运。
>
> **新增 cfg `BlockCounterStack`（默认开）**：交易柜台内禁止堆叠。  
> 判据用游戏自己的 `EmporiumEntry.Instance.backInvinvElementCounter`（柜台容器实例），  
> ★ **不硬编码 identifier** —— 柜台容器是运行时创建的，场景 `.assets` 里查不到，  
> 且 identifier 可能为空；唯一可靠的判据是游戏自己的实例引用（`SameInventory` 走  
> `Pointer` 比对，避免托管往返）。关闭则恢复 0.6.4 行为。  
> ★ `StackExclude`（容器排除列表）现在对**所有**合并路径生效（原先只拦拖放）。
>
> **补丁自检 18 → 20 项**（新增 `SlotMarker.TryAcceptOnce` + `EmporiumEntry.backInvinvElementCounter`）。  
> 版本 0.6.4 → 0.6.5。部署产物 md5 `13934eefb3f4d7827eeed3c7f6019acf`（36352 字节）。

## 方案（0.6.4：Alt 快移真正生效（挂对类）+ cfg 局内改动可保存）

> **0.6.4 本次变更（用户反馈：问题1 Alt 点击仍整堆快移；问题2 局内改 cfg 存不住）**：
>
> **问题1 根因（反汇编实锤）**：**0.6.3 挂错了类**。真正执行 Alt 快移的是  
> `ItemQuickTransferAltHandler`（TypeDefIndex 1453）/ `ItemQuickTransferAltHandler1`（1451）  
> —— 末尾带 `1` 的是双份场景变体，两者字段与 IL 完全对称  
> （`key`@0x20、`quickTransferKey`@0x24、`currentItem`@0x28、`isQuickTransfer`@0x30，均 `[SerializeField]`）。  
> 而 `ItemMouseDragHandler` **根本不监听 Alt**：  
> `get_keyListeners`(0x7BC100) 只造 `{key, rotateCW, rotateCCW, flip, reset, scrollMult10, scrollMult100, scrollIgnore}`，  
> `OnEventPress`(0x7BAEC0) 只读 `[rbx+0x24]/0x40/0x44/0x48` —— 没有 Alt 字段。  
> ⇒ 0.6.3 的补丁从未被触发，原生整堆行为原封不动。
>
> **原生逻辑（`ItemQuickTransferAltHandler.OnEventRelease` 0x7E1620 完整反汇编）**：
>
> ```
> if (!KeyOverlaps(keys, [r14+0x20] key)) → 收尾
> if ([r14+0x30] isQuickTransfer == 0) → 收尾
> newItem = RenderHandler.RaycastElement<GameItemElement>(Input.mousePosition)
> parentWindow = newItem.treeNodeRender[+0x2d0].FindParentWindow()
> if (newItem == [r14+0x28] currentItem && newItem.MayRemove())
>     foreach (GameInventory inv in GraphUtils.FindChildrenType<GameInventory>(parentWindow))
>         if (inv == newItem.parentInventory) continue;        ← 跳过源容器
>         slot = inv.vtable[0x2d8](); if (slot == null) continue;
>         GraphUtils.TryAcceptOnce(inv, newItem, -1)           ← 0x180766730
> 收尾: InputActionManager.ResetAfter(this); currentItem = null; isQuickTransfer = false
> ```
>
> **两个关键结论**：  
> ① **原生就是双向的** —— 目标容器由「鼠标落点所在窗口」决定（柜台→仓库 / 仓库→柜台都行）。  
> 旧版误会「Alt 只能指向柜台」，是因为补丁压根没生效。  
> ② 唯一不符需求的是那个 `-1`。`GraphUtils.TryAcceptOnce(GraphNodeStorage, GameItem, int amount=-1)`  
> （RVA 0x766730）里：`if (amount < 0) amount = item.unitCount;` ⇒ **-1 在这展开成整堆**。  
> 内部 `SlotMarker.TryAcceptOnce(amount)`(0x7B0020) 会 `Math.Min(amount, capacity)` + `GetSplitItem(n)` 真拆分；  
> `TryAcceptOnce` 返回的是**剩余未接受数量**。  
> **修复方式（最少侵入、完全复用原生流程）**：补丁改挂  
> `ItemQuickTransferAltHandler.OnEventRelease` / `ItemQuickTransferAltHandler1.OnEventRelease`。  
> `Prefix` 先把源堆裁成 `AltQuickTransferAmount` 个（溢出暂存 `[ThreadStatic]`），  
> 放行原生（此时原生「整堆」== 裁剪后的 N 个），`Finalizer` 归还溢出。  
> 音效 / 窗口分支 / `ResetAfter` 等原生副作用全部保留。  
> ⚠️ **没有去改 `TryAcceptOnce` 的 amount 参数**：IL2CPP 下 `int` 走寄存器，interop 层用 `&amount`  
> 转 `il2cpp_runtime_invoke`，`ref` 修改不可靠（结构上做不到）。  
> **新增 cfg** `AltQuickTransferAmount`（默认 1；9999 = 等价原版整堆）。  
> 关闭 `AltClickTransferToCounter` 或 `AltClickOnClickMode` 任一项 → 完全交还原生整堆。  
> **遗落防护**：溢出暂存于 `[ThreadStatic]` 字段，`Finalizer` 归还；另加两处兜底  
> （下次 `Prefix` 开头自愈、`OnUpdate` 检测残留时归还），`FlushAltOverflow` 幂等，  
> 源容器未知时显式 `Error` 报错。
>
> **问题2 根因（两份反编译 + 日志三重印证）**：  
> `MelonPreferences.Save()`（MelonLoader.decompiled.cs:8101）→  
> `file.InsertIntoDocument(cat.Identifier, entry.Identifier, entry.Save())` → `IO.File.Save()` →  
> `File.WriteAllText(FilePath, document.SerializedValue)` —— **整份内存 document 写盘**；  
> 而 `InsertIntoDocument` → `subTable.PutValue(QuoteKey(key), value)` 是用**运行时 entry 当前值覆盖该键**。  
> 内存 `document` **只**在 `Load()`（`TomlParser.ParseFile`）时重建。  
> 另一侧，**ModManager 1.2.3** 走 `PreferencesStore.ApplyBoolPreferences`(行 5043) →  
> `UpdatePreferenceLine`(行 5099) 用 `File.WriteAllLines(ConfigPath, …)` **纯原始文本写盘，  
> 全程不碰 MelonLoader 的内存 document**。  
> ⇒ 后续任何全局 `MelonPreferences.Save()` 都会用「过期 document」把用户的 raw-text 修改整份盖回去。  
> **日志实证**：第 794 行 `[12:57:42.323] Preferences Saved!`（无 melon 前缀，在别的 mod 卸载之后）  
> —— 确实存在这样一次全局保存。另第 657 行 `[ModManager] the site has no maintained detail for
> "Item Stacking"` 证实 ModManager 无本 mod 站点详情（走的是 `CollectFromRuntime` 反射路径）。  
> **修复方式（低频轮询 + 采纳磁盘值）**：`OnUpdate` 里每 1.5s 比对  
> `UserData/MelonPreferences.cfg` 的 mtime；变新（= 外部刚写过）则解析 `[ItemStacking]` 段，  
> 把与运行时 entry 不一致的项**采纳进 entry**，然后**立即 `MelonPreferences.Save()`** 落盘消除竞态，  
> 并重读 mtime 作基线。**方向性判据：mtime 变新 = 外部写的 = 磁盘权威。**  
> （`MelonPreferences.DefaultFile` 是 `internal`，路径自拼  
> `MelonEnvironment.UserDataDirectory + "MelonPreferences.cfg"`。）  
> 日志：采纳到改动时输出 `[cfg 保活] 已采纳 cfg 文件中的改动（…），并按运行时值回写。`
>
> **补丁自检 15 → 18 项**（新增 2 项 `OnEventRelease` + 2 项 `GraphUtils`，移除 2 项挂错类的旧检查）。  
> 版本 0.6.3 → 0.6.4。部署产物 md5 `93d69984a7210bdd30ddbc8e0949e88c`（33792 字节）。

## 方案（0.6.3：柜台排除改为纯手动 + Alt+左键「点击」快移）

> **0.6.3 本次变更（两项需求）**：  
> ① **需求1：StackExclude 不再自动排除交易柜台**。旧版留空时运行时探测 `EmporiumEntry`  
> 柜台容器并一刀切禁堆；实测会在柜台整理背包时误伤，且柜台亏钱问题的正解是「Alt 快移逐件上架」。  
> 现 `StackExclude` 只按 cfg 手动生效，留空 = 不排除任何容器；黑名单机制完整保留。  
> ② **需求2：修正 Alt+左键「点击」不生效**。★ 反汇编实锤（全库只 26 处 `Input.GetKey`）：  
> **原版根本没有「Alt+点击→柜台」这个动作**；`LeftAlt(0x134)` 只出现在 `StartDrag`(→currentRemove=1)、  
> `ItemMultiSelectHandler`、开发者控制台三处。且 `StartDrag` **唯一调用方是 `OnEventMouseMove`**，  
> 仅在鼠标位移超过 `dragThreshold(0x20)` 时触发 ⇒ 纯「点击」（按下抬起、不移动）**永不调 StartDrag**，  
> 旧版挂在 StartDrag 上的快移对点击完全无效 —— 这正是「没和左键点击结合起来」的真因。  
> 现新增 `AltClickOnClickMode`（默认开）：挂 `ItemMouseDragHandler.OnEventRelease`，  
> 判据 `!IsDraggingItem(0x68) && Alt && currentItem(0x50)!=null` → 快移 1 个到柜台；  
> Alt+拖拽走原 StartDrag 路径。两处共用核心 `TryFastTransferOneToCounter`。  
> 自检 14 → 15 项（新增 `OnEventRelease` 依赖）。版本 0.6.2 → 0.6.3。  
> ⚠️ **该版实际仍未生效** —— 挂错了类（`ItemMouseDragHandler` 不监听 Alt），真修复见 v0.6.4。

## 方案（0.6.2：Alt 快移安全修复 + 空槽上限收回 + 版本/文档统一）

> **0.6.2 本次变更（依据外部评审「优化建议」逐条落地，反汇编取证后精确修正）**：  
> ① **Alt 快移柜台数据风险修复**（P0，最高风险项）。反汇编 `GameItem.GetSplitItem`(0x797500)  
> 完整体实锤：`sub [rbx+0x1f8], ebp` 先扣源堆，`requestedAmount >= unitCount` 走「整体移动」  
> 路径且造实例失败时**源堆已扣却不恢复**。原代码对「仅 1 件」也调 `GetSplitItem(1)` → 可数据损坏；  
> 现改为**仅 unitCount ≥ 2 才自行拆分，1 件放行原生**（原生 LeftAlt 本就是取 1 个）。  
> 又实锤 `GraphUtils.TryAcceptAllMid`(0x7664b0) 返回值 = `edi - ebx` = **剩余未接受数量**  
> （不是接受数）→ 原判据 `accepted < 1` 用反，柜台满时（剩余=1）误判成功 → 静默丢件。  
> 现改为 `left > 0` = 未全收 = 回退，且异常路径把已拆出的 one 归还来源，绝不遗落。  
> ② **空槽容量收回 StackCap**（P0）：`MaxNumAdd` 空槽原写 `Math.Max(StackCap, unitCount)`，  
> 旧档遗留大堆放进空格可一次超上限；现以 StackCap 为硬上限（Ctrl 整堆一次进格由 currentRemove 路径满足）。  
> ③ **版本/文档统一**（P0）：AssemblyInfo 0.5.2 → 0.6.2；README 补齐 0.6.x 配置项、  
> 自检 9→14 项、新风险与故障排查。  
> ④ **可观测性/鲁棒性**（P1）：fail-safe 降级加「一次性 Warning」（不再完全静默）、  
> 自检补全 Alt 快移三项依赖、读档补满补丁加 try/catch。


## 方案（0.5.2：0.5.1 全部 + 活体动物拦截 + 鼠拦截）

> **0.5.2 本次变更（用户反馈：鼠 / 动物不该堆叠）**：  
> ① **活体动物不堆叠** —— 新增 `BlockAnimal`：拦截带 `ANIMAL_ITEM_TAG` 的物品（鼠/鸡/牛等）。  
> 实证（RatPrinterProbe 2026-09-28 探针输出）：一只鼠带 **34 个 tag**，年龄 / 体重 / 健康 /  
> 饥饿 / 性别 / 怀孕进度 / 7 组基因等位基因（ALLELE_A、ALLELE_B）全是个体独立状态，  
> 合并后除第一个外全部丢失，并污染饲养、繁殖、过度拥挤判定。  
> ② **鼠单列一项** —— 新增 `BlockRat`：物种为 `ANIMAL_RAT` 的物品拒堆叠。  
> ⚠️ **关键取证：`ANIMAL_RAT` 不是 tag 键，而是 `ANIMAL_SPECIES_TAG` 的字符串值**  
> （探针实锤 `ANIMAL_SPECIES_TAG | enabled=True | str=ANIMAL_RAT`，与  
> `AnimalSpecies.Rat.speciesID == "ANIMAL_RAT"` 一致；判据与 `科学养鼠` mod 的  
> `RatData.IsRat()` 同源）。**写 `IsTag("ANIMAL_RAT")` 会恒 false = 静默失效**，  
> 因此走 `GetTagReadonly("ANIMAL_SPECIES_TAG").GetString()` 值比较。  
> 单列目的：允许关掉 `BlockAnimal`（放行其它动物）但仍单独禁鼠。  
> ③ 自检项 8 → **9**（新增 `GetTagReadonly` 复核，防鼠判据静默失效）。

> **0.5.1 本次变更（用户三需求）**：  
> ① **商人商品不堆叠** —— 把「所有权不一致」升级为「未拥有物品拦截」（`BlockUnowned`）：商人商品池  
> / 柜台上的未付款商品、场景里未捡起的物品都不参与堆叠，只放行「两件都已拥有」；  
> ② **电量不同的电池不堆叠** —— 新增 `BlockChargeMismatch`：比较 `power_source_item_energy`（int tag）  
> 电量值，不同则拒堆叠（否则除第一个外丢失剩余电量）；  
> ③ **堆叠作用域（需求3，可行性落地）** —— 新增 `StackScope`（白名单容器 id，逗号分隔，默认空=不限）。  
> 判据链（反汇编确认）：`GameItem.parentInventory`(0x190) → `GameInventory.identifier`(0x120 真字段)  
> → 与白名单比对，不在白名单即拒堆叠。可填 `save_bag` 把堆叠限定在存档袋内。

> **0.5.0 本次变更（P3 堆叠黑名单）**：修复三类用户实机反馈 bug ——  
> ① 鉴定物品（等离子/仿生机械手/肋排三明治等）自动堆叠，拖出来变默认样子和价值；  
> ② 商人出售的未付款物品被堆进已拥有堆后，除第一个外变「已拥有」可免费拿走（盗窃漏洞）；  
> ③ 赃物（有热度值）堆叠污染所有权/警察检查判定。  
> 根因：本 mod 放开堆叠后，游戏原版的 tag 黑名单（`mayStackItemFunc`）从未被设置过，  
> 导致有独立状态的物品也被合并。**判据全用官方只读口 `GameItem.IsTag()`**  
> （反汇编确认 `GeneralHelper.IsItemOwned` 内部就是 `IsTag("IS_OWNED_TAG")`），  
> 拦截三类：`INSPECTABLE_TAG`/`APPRAISAL_ITEM`（可鉴定）、`STOLEN_TAG`（赃物）、  
> `IS_OWNED_TAG` 不一致（所有权）。四开关可独立配置（默认全开）。

> **0.4.1 本次变更**：① `IsContainer` 的 catch 收窄为 `catch(Exception)` 并显式重抛致命异常  
> （OOM/栈溢出），避免静默吞掉严重异常；② 自检新增 `FindAllChildItems` 目标复核（游戏改名该  
> 方法时容器防护不再静默失效）；③ 新增「诊断日志」配置项 `DiagLog`（默认关闭），开启后在  
> 各补丁关键决策点输出 `[Diag]` 日志，便于排查堆叠/合并异常。
>
> **3.4 分析结论（未采纳项）**：建议 #2「`GetStackSlot` 的 `int.MaxValue` 改为  
> `Math.Min(int.MaxValue, StackCap*2)`」**不采纳**——`int.MaxValue` 即原生同款兜底  
> （0x7fffffff），真正的钳制在 `TryAcceptOnce` 的 `min(数量, numTransfer, MaxNumRemove)`；  
> 改 `StackCap*2` 会在「被拖物品数量 > 上限」的老存档场景下**少合并**（静默功能回退），  
> 且属于「自创、脱离原生」的写法，违背本 mod「贴原生、不引入不可知风险」的铁律。

> **0.4.0 本次变更**：P0 装了东西的容器禁止堆叠（防数据丢失）；P1 字符串式补丁目标自检  
> （游戏改名方法时不再静默失效，明确告警）；P2 堆叠上限下限钳到 ≥1，并整理资料库 §6 标记。

> **0.1.0 教训（实机报错，已撤销）**：v0.1.0 用 `DelegateSupport.ConvertDelegate` 给  
> `maxSlotAmountItemFunc` 注入 mono 委托 → 游戏自己的聚合函数  
> `InvokeFuncExtensions.InvokeAllReduce` 用 **il2cpp 反射 `Delegate.DynamicInvoke`  
> 遍历委托链**，mono 回调委托无法通过该路径调用 → 拖动物品即  
> `TargetException`（`GameItem.MaxNumAdd` ← `GameGridInventory.TryInventorySlot` ← 拖拽），  
> 堆叠整个瘫痪、ModManager 显示「报错」。  
> ★ **规律：给被 `InvokeAllReduce` 聚合的 `Func` 字段赋任何 mono 侧委托都会崩** ——  
> 不能碰 `maxSlotAmountItemFunc` / `minSlotAmountItemFunc` / `mayStackItemFunc`。

> **0.1.1 教训（不报错但不工作）**：只抬 `MaxNumAdd` 上限不够——游戏拖放落点  
> **根本没有走到堆叠分支**：`MayStack` 恒 false，占用格直接拒绝放置。

- **★ 0.2.0 核心：原生 EndDrag 本来就有完整堆叠机器，只是被闸住（反汇编实锤）**

  `ItemMouseDragHandler.EndDrag` (0x7BA010) 落点判定顺序：
  1. 普通放置：`TryFindOneValidInventorySlot` → `TryAcceptOnce`
  2. **堆叠分支：`lastItem.MayStack(dragged)` → `GetStackSlot(dragged)` → `TryAcceptOnce(n)`**  
     （n≥unitCount 整体移动，否则 `GetSplitItem` 拆分 → `StackItemUnchecked` 合并）
  3. 物品交互：`MayTarget/CanTarget/Target`
  `MayStack`(0x799120) 与 `GetStackSlot`(0x797720) 都先对**两件物品**的  
  `mayStackItemFunc`(0x20) 做 `InvokeAllReduce(OR)` —— 原版从未给物品设过该委托  
  → 恒 false → 堆叠永不触发。vtable 校准确证 GetStackSlot 内部虚调用 =  
  `parentInventory.GetIncompleteItemInventorySlot`（Slot 13；0x268/0x278 =  
  UncheckedAccept/Expel，base=0x138 由 Slot 21=get_childItems 反推）。

  **0.2.0 补丁面（全程不碰 InvokeAllReduce 委托）**：
  | # | 补丁                                | 类型      | 作用                                                |
  | - | --------------------------------- | ------- | ------------------------------------------------- |
  | 1 | `GameItem.MayStack`               | Prefix  | 同名 `identifier` 直接放行 true；**0.4.0 起：装了东西的容器直接拒绝** |
  | 2 | `GameItem.GetStackSlot`           | Prefix  | 托管复刻原生逻辑（跳过委托检查）；**0.4.0 起：容器纵深拒绝**               |
  | 3 | `GameItem.MaxNumRemove`           | Postfix | 无委托时抬到 `unitCount`（防合并量被钳成 1）                     |
  | 4 | `GameItem.MaxNumAdd(SlotMarker)`  | Postfix | 抬剩余空间到 `StackCap`（0.1.1 保留，0.4.0 改用下限钳制值）         |
  | 5 | `SaveItemNode.OnAfterDeserialize` | Postfix | 读档把满堆补到 `StackCap`（0.3.0 保留）                      |
  | 6 | `ItemMouseDragHandler.StartDrag`  | Postfix | Ctrl+整堆 / 无修饰键单件（0.3.0 保留）                        |
  | — | `Main.ValidatePatches()`          | 初始化自检   | P1：PatchAll 后逐一复核 6 个目标方法存在，缺失则明确告警（不再静默失效）       |
  **P0 容器防护（0.4.0）**：`IsContainer(it)` 用 `GameItem.FindAllChildItems().Count > 0` 判定  
  「装了东西的容器」。在 `MayStack` 与 `GetStackSlot` 两道闸都拦：任一物品是容器即拒绝堆叠。  
  空容器（`childItems` 为空）仍可叠（无内容可丢）；读取异常时 fail-safe 为「不阻断正常堆叠」。  
  调用发生在拖放落点的活 GameItem 上，与既有 `.identifier` 调用同风险级，已实机路径验证。
- **A'. Postfix 消费方 `GameItem.MaxNumAdd(SlotMarker)`**
  - 反汇编（0x798160）确认语义：`MaxNumAdd(slot)` = 两层 `maxSlotAmountItemFunc`  
    聚合取 `Min`，**再减去 `slot.targetItem.unitCount`** → 返回「该槽还能再放进几个」。  
    无参版 `MaxNumAdd()` 是尾调用 `jmp` 到同一 native 入口，一并覆盖。
  - Postfix：`__result = max(原值, NewStackCap - slot.targetItem.unitCount)`。  
    只提升、不收紧，保留游戏原有全部约束；空槽（targetItem==null）不动。
- **B. 读档改值（把存量物品补满）**  
  `SaveItemNode.OnAfterDeserialize` Postfix：若 `FillToCapOnLoad` 开启且物品 `unitCount`  
  **恰好等于旧上限 `OldStackCap`**（即当初已叠满），则改写为新上限 `NewStackCap`。  
  只动满堆、保留部分堆叠，不引入 clamp 风险。
- **C. 拖起数量控制（0.3.0）：普通拖动 = 1 个，Ctrl+拖动 = 整堆**
  - 原生取证（`ItemMouseDragHandler.StartDrag` 0x7BB750 反汇编）：拖起数量由  
    `currentRemove`(0x7C) 决定，原生初始化链 = `maxRemove`(0x78, 来自  
    `MaxNumRemove()`，0.2.0 已抬到 unitCount) → 默认拿 `maxRemove`；  
    **LeftShift → 拿 10%**（`round(max*0.1)`）；**LeftAlt → 拿 1 个**  
    （键值 0x130=304 / 0x134=308，经 interop `KeyCode` 枚举核实）。  
    拖起后 `OnEventMouseScroll` 在 `maxRemove>1` 时可用**滚轮微调数量**。  
    拿多少、放多少、合并多少共用同一个 `currentRemove`（`TryAcceptOnce` 消费）。
  - 即：0.2.0 抬高 `MaxNumRemove` 后，**游戏原生「修饰键选数量」机制被激活了**  
    —— 之前 `maxRemove` 恒为 1，Shift/Alt/滚轮全是死代码。
  - 本补丁 = `StartDrag` Postfix 收尾：Ctrl 按住 → `currentRemove = maxRemove`（整堆）；  
    无修饰键 → `currentRemove = 1`（保留单件拖动）；**Shift / Alt / 滚轮不碰，  
    保持原生**。不新增任何输入读取路径之外的逻辑，落点复用原生合并机器。
- **C2. 空槽容量修复（0.3.1）：修「Ctrl 整堆放空格只移 1 个」+「短拖拆 1 个 UI 重叠」**
  - 根因（反汇编实锤，两 bug 同根）：EndDrag **空格**路径 =  
    `lastInventory.TryInventorySlot(item, requestedNum, pos, shape, tagState)`  
    构造 marker，`numTransfer = Min(Min(MaxNumRemove, MaxNumAdd(marker)), requestedNum)`；  
    空槽 marker（`targetItem == null`）走 `MaxNumAdd` 原生默认 = **1**（原版堆叠上限），  
    → EndDrag 调 `TryAcceptOnce(-1)`（-1 = 取 marker.numTransfer）→ n = min(1, …) = 1  
    → `GetSplitItem(1)` 强拆 1 个、剩余放回原位。放同名堆走的是堆叠分支  
    （`GetStackSlot` 的 numTransfer 已被抬）所以正常——与实机现象完全吻合。
  - 修复：`Patch_MaxNumAdd` 补空槽分支：`targetItem == null` 且原值 < NewStackCap 时，  
    抬到 `max(NewStackCap, unitCount)`。空格 numTransfer 变为  
    `min(unitCount, NewStackCap, currentRemove)`：Ctrl = 整堆、滚轮 = 部分量、  
    普通 = 1，全部一次进格，不再产生拆分残留/重叠。
- **堆叠状态持久化（无需额外代码）**：`unitCount` 本就序列化在  
  `SaveItemNode.unitCount`(0x18)，存档自动保留堆叠数量；读档后游戏直接读字段，  
  不经 MaxNumAdd 钳制，不会掉回 1。Ctrl 整堆拿起后放到哪、存档读回都是整堆。

## 配置项（MelonPreferences 段 `ItemStacking`）

| Key                   | 类型     | 默认    | 说明                                                      |
| --------------------- | ------ | ----- | ------------------------------------------------------- |
| `EnableStacking`      | bool   | true  | 启用拖放堆叠（0.2.0 核心）                                        |
| `NewStackCap`         | int    | 10    | 单个槽位允许的最大堆叠数量                                           |
| `FillToCapOnLoad`     | bool   | true  | 读档时把「原满堆」补满到新上限                                         |
| `OldStackCap`         | int    | 10    | 打本 mod 前的默认堆叠上限，用于识别「已叠满」物品                             |
| `CtrlDragWholeStack`  | bool   | true  | Ctrl+拖动 = 拿起整堆；不按 = 只拿 1 个（0.3.0）                       |
| `DiagLog`             | bool   | false | 诊断日志开关：开启后在堆叠/合并决策点输出 `[Diag]` 日志（0.4.1）                |
| `StackBlacklist`      | bool   | true  | 堆叠黑名单总闸（0.5.0）：关闭后全部黑名单失效                               |
| `BlockInspectable`    | bool   | true  | 拦截可鉴定物品（`INSPECTABLE_TAG`/`APPRAISAL_ITEM`：等离子/手/三明治等）  |
| `BlockStolen`         | bool   | true  | 拦截赃物（`STOLEN_TAG`，有热度值）                                 |
| `BlockUnowned`        | bool   | true  | 拦截未拥有物品（商人商品/柜台未付款/场景物品，0.5.1）                          |
| `BlockChargeMismatch` | bool   | true  | 拦截电量不同的电池（`power_source_item_energy`，0.5.1）             |
| `BlockAnimal`         | bool   | true  | 拦截活体动物（`ANIMAL_ITEM_TAG`，0.5.2）                         |
| `BlockRat`            | bool   | true  | 拦截鼠（`ANIMAL_SPECIES_TAG` 的 str 值 == `ANIMAL_RAT`，0.5.2） |
| `StackScope`          | string | 空     | 堆叠作用域白名单容器 id（逗号分隔，如 `save_bag`；空=不限，0.5.1）             |

> ⚠️ 上表是 **0.5.2 时期的历史快照**，与当前代码不一致。**实际生效的配置项以代码
> `OnInitializeMelon` 里的 `cat.CreateEntry` 为准**（0.8.0 起重构为四列表架构）：
>
> | Key | 类型 | 默认 | 说明 |
> |---|---|---|---|
> | `EnableStacking` | bool | true | 启用堆叠总闸 |
> | `NewStackCap` | int | 10 | 堆叠上限（下限钳到 ≥1） |
> | `OldStackCap` | int | 10 | 识别「旧满堆」用 |
> | `FillToCapOnLoad` | bool | true | 读档补满 |
> | `CtrlDragWholeStack` | bool | true | Ctrl+拖动 = 整堆 |
> | `DiagLog` | bool | false | `[Diag]` 诊断日志 |
> | `StackBlacklist` | bool | true | 类判据保护总闸 |
> | `ContainerWhitelist` | string | 空 | 非空 = **仅**名单内容器可堆叠 |
> | `ContainerBlacklist` | string | 空 | 名单内容器禁止堆叠 |
> | `ItemWhitelist` | string | 空 | 豁免「类判据」（黑名单优先，不能翻案 id 黑名单） |
> | `ItemBlacklist` | string | 空 | 自定义 id 黑名单（内置名单在代码常量） |
> | **`BlockCounterStack`** | bool | **false** | 柜台禁堆（**0.9.0 起默认关**） |
> | **`StackValueTotal`** | bool | **true** | **交易系统按整堆总价值计价（0.9.0 新增，默认开）** |
> | `AltClickTransferToCounter` | bool | true | LeftAlt 快移到柜台（总闸） |
> | `AltClickOnClickMode` | bool | true | Alt 快移接管总开关 |
> | `AltQuickTransferAmount` | int | 1 | Alt 快移件数（9999 = 原生整堆） |
>
> ★ 含逗号的字符串值**必须用引号包起来**（`ItemBlacklist = "a,b"`）——
> 未加引号的裸值是非法 TOML，会让整份 `MelonPreferences.cfg` 载入失败并进入 Fallback，
> 连锁后果是**全游戏所有 mod 的配置都无法再写盘**（0.6.6 实机踩过，见该版本记录）。
>
> 局内手改 cfg 会被 1.5s 轮询的保活机制采纳（mtime 变新 = 磁盘权威），无需重启。

> 默认上限已于 0.3.2 从 999/50 降到 10：减小缺省堆叠量，降低误合并的破坏面  
> （详见下方「⚠️ 使用风险：容器堆叠会丢失容器内物品」）。  
> 若 `OldStackCap` 与 `NewStackCap` 相等，读档迁移 `B` 退化为无操作（只影响旧档补满，不破坏核心功能）。

## ⚠️ 使用风险：容器堆叠会丢失容器内物品

> 这是**数据不可逆丢失**类风险，使用前务必知悉。

- 本 mod 的堆叠判定只比对 `GameItem.identifier`（同名即允许合并，见 `Patch_MayStack` /  
  `Patch_GetStackSlot`），**不区分物品是否为「容器」**（箱子、背包等内部还能装其它物品的物体）。
- 合并走的是原生 `SlotMarker.StackItemUnchecked(item, targetItem, n)` —— 它只搬动  
  `unitCount` 数量，**不会合并容器内部的子物品数据**。
- **后果**：把两个装有物品的同名容器拖到一起合并时，被并入的那个容器的内部物品会  
  **永久丢失**（只剩目标容器的内容）。Ctrl+拖动整堆时尤其容易误合并。
- **✅ 0.4.0 已加代码防护（自动拦截）**：`IsContainer` 用 `FindAllChildItems().Count > 0`  
  判定「装了东西的容器」，在 `MayStack` 与 `GetStackSlot` 两道闸都拒绝其堆叠。**空容器仍可叠  
  （无内容可丢）**。即：**装了东西的容器无法被拖到一起合并**，从根本上杜绝该数据丢失路径。
- **剩余用户侧注意事项**：
  1. 仍建议合并前确认容器为空，双重保险；
  2. 旧版存档（0.4.0 之前）若已发生容器合并丢失，本 mod 无法恢复（数据已不存在）。

## 目录结构

```
Item stacking\
├─ _shared\CommonMod.props        # 公共构建属性（游戏路径/TFM/输出），从 ContainerSuite 精简
├─ ItemStacking\                  # mod 主工程（= AssemblyName = DLL 名 = 配置段名）
│  ├─ ItemStacking.csproj
│  ├─ AssemblyInfo.cs             # MelonInfo / MelonGame（公司名已核实：Questing Goose Studio）
│  └─ Main.cs                     # ★ A+B 实现（Harmony 补丁）
├─ 资料库\
│  ├─ 堆叠资料库.md               #   堆叠机制完整数据 / 方法 RVA / 钩子点 / 风险 / 实现进度（§8）
│  └─ 物品堆叠API速查.md          #   一页速查表
└─ StackSizeEdit-master\          # ⚠️ 无关：Minecraft Fabric 版，仅概念参考
```

## 构建

```powershell
cd 'D:\games\open code work\Item stacking'
dotnet build 'ItemStacking\ItemStacking.csproj' -c Release -nologo
# 产物：ItemStacking\bin\Release\ItemStacking.dll
```

## 部署与验证（**先备份、游戏必须已退出**）

```powershell
# 1) 备份现有 DLL（MelonLoader 0.7.4 只加载 Mods\ 根目录）
Copy-Item 'D:\SteamLibrary\steamapps\common\Probably Stolen Demo\Mods\ItemStacking.dll' `
         'D:\SteamLibrary\steamapps\common\Probably Stolen Demo\Mods\ItemStacking.dll.bak-$(Get-Date -f yyyyMMdd-HHmmss)' `
         -ErrorAction SilentlyContinue
# 2) 复制新产物
Copy-Item 'D:\games\open code work\Item stacking\ItemStacking\bin\Release\ItemStacking.dll' `
         'D:\SteamLibrary\steamapps\common\Probably Stolen Demo\Mods\ItemStacking.dll' -Force
# 3) 启动游戏，看日志应有：
#    [Item Stacking] 已加载：堆叠上限 = 10，读档补满 = True（旧上限 10）。
```

**验证清单（实机）**：

1. 日志出现加载行 → mod 被加载（静默无日志 = 没加载，先查 `MelonGame` 公司名）。
2. 拖一个同物品到另一个上，确认能叠到 > 旧上限（如 999）而不被截断。
3. 叠满后保存、重开，确认数量保持在 999（不被压回旧上限）。
4. 确认非堆叠类物品（唯一物/装备）行为无异常（MayStack 仍按 identifier 等判定，cap 只是数字上限）。

## 已知风险 / 未验证（资料库 §6）

- **⚠️ native 是否真用 `maxSlotAmountItemFunc` 算上限未实机验证**：它是 dump.cs 里  
  `GameItemFunc`/`GameInventoryFunc` 暴露的官方扩展点，理论上生效；但默认值、两层取交集  
  规则在 native 侧。若 A 不生效，需用 dnSpy/Ghidra 跟 `GameGridInventory` 构造 +  
  cap 消费方法，确认字段偏移 / 调用方。
- B 的 `OnAfterDeserialize` 改写能否传导到 `GameItem.unitCount` 取决于游戏用 node 字段建物品  
  的时机；若未传导，B 不补满但 A 仍有效，可手动重叠。
- 改 `unitCount` 会按「每单位价值」整体缩放总价（预期行为，非 bug）。
  ★ 0.9.0 起这条不再只是"预期"而是**已实现**：整堆计价补丁就是让交易口径
  按「每单位价值 × 数量」结账（此前只按每单位价值）。
- **红线**：不得 patch `UncheckedAccept` / `Expel` / `SetShape` 等网格操作（加载期崩溃  
  `0x80131506`）；本实现只写委托字段 / 改存档数值，未触碰这些路径。
- **⚠️ 容器堆叠会丢失内部物品**：0.4.0 已用 `IsContainer` 代码拦截（装了东西的容器禁止堆叠）。  
  旧版存档的既有丢失无法恢复；空容器仍可叠。详见上方「⚠️ 使用风险」小节。

## 当前状态

- ✅ 骨架 + 堆叠资料库（已建）
- ✅ **0.9.0 已实机验收通过**（`Main.cs`）：整堆计价（`StackValueTotal`）+ 柜台禁堆默认关
  - 交易系统 30+ 个计价口径统一按整堆价值结账，卖出 ×N 不再只收 1 件的钱
  - 扣减语义实测正确（无「付部分钱扣整堆」的数据损失）
  - 补丁自检新增 `GetNegociatedValue(整堆计价入口)` 一项（改名即静默失效，必须告警）
  - 部署 md5 `77dedb6409895d3a7871cec1cbdb4416`；回滚点 0.8.1 `.bak-20261009-180125`
- ✅ 0.8.x 已实现：四列表架构（容器白/黑名单、物品白/黑名单）+ 模组/节点按子串拦截
- ✅ 0.6.x 已实现：Alt 快移落点改写、柜台三格判据、cfg 保活（防 MelonPreferences 覆盖）
- ✅ 0.5.2 已实现：拖放堆叠(A') + 读档补满(B) + Ctrl 整堆(C) + 空槽容量修复(C2)
  - **P0 容器防护** + **P1 补丁自检** + **P2 上限下限钳制** + **P3 堆叠黑名单**
  - **未拥有拦截** + **电量拦截** + **动物拦截** + **鼠拦截** + **诊断日志 `DiagLog`**
    （`dotnet build` 0 警告 0 错误）
- ⏳ 长期未验证项：native 是否真用 `maxSlotAmountItemFunc` 算上限（见「已知风险」）

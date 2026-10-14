# Item Stacking —— 物品堆叠增强 mod

> 游戏：**Probably Stolen**（Demo 0.46D）· Unity IL2CPP + MelonLoader 0.7.4+
> 当前版本：**0.14.4** · 作者：核桃

---

## 一句话

放开游戏原生堆叠限制（默认上限 10），并配套解决因堆叠引起的交易、机器、容器、状态一致性问题。

---

## 目录结构

```
Item stacking/
├─ ItemStacking/                     mod 主工程（= DLL 名 = 配置段名）
│  ├─ ItemStacking.csproj
│  ├─ AssemblyInfo.cs
│  ├─ Main.cs                        生命周期 / 日志 / 补丁目标自检
│  ├─ Config.cs                      配置项声明
│  ├─ Constants.cs                   魔法常量集中
│  ├─ Il2CppHelpers.cs               IL2CPP 互操作安全层
│  ├─ StackGates.cs                  堆叠闸门判据（容器/黑名单/作用域）
│  ├─ MergePolicy.cs                 合并决策单一源
│  ├─ MergePolicy.Legacy.cs          0.9.9 旧路径（回滚用）
│  ├─ MergeSteerer.cs                导流 vs 拦截
│  ├─ StateConsistency.cs            状态一致性闸门
│  ├─ ValueGate.cs                   关键 tag 值域校验
│  ├─ BuiltinBlacklist.cs            内置黑名单 + 外置 JSON
│  ├─ DestroyFix.cs                  堆叠销毁修复（打印机/熔炉/解序器）
│  ├─ ModifierPairTransfer.cs        Alt/Shift 配对快移
│  ├─ StackPatches.cs                补丁自检
│  ├─ StackPatches.Load.cs           StartDrag
│  ├─ StackPatches.Merge.cs          MayStack / GetStackSlot / TryAcceptOnce
│  ├─ StackPatches.Value.cs          GetNegociatedValue / MaxNumRemove / MaxNumAdd
│  ├─ StackPatches.AltTransfer.cs    Alt 快移作用域
│  ├─ Probes.cs                      诊断探针
│  └─ CounterIdProbe.cs              柜台 id 打印
├─ README.md                         本文件（开发用）
├─ README.md.txt                     玩家使用说明（桌面版）
└─ README_mobile.txt                 玩家使用说明（移动版）
```

---

## 构建部署

```powershell
# 构建
cd 'D:\games\open code work\Item stacking'
dotnet build 'ItemStacking\ItemStacking.csproj' -c Release

# 部署
Copy-Item 'ItemStacking\bin\Release\ItemStacking.dll' `
  'D:\SteamLibrary\steamapps\common\Probably Stolen Demo\Mods\ItemStacking.dll' -Force

# 日志
Get-Content 'D:\SteamLibrary\steamapps\common\Probably Stolen Demo\MelonLoader\Latest.log' -Tail 50
```

---

## 运行时特性

- **改 cfg 后需重启游戏生效** —— MelonLoader 不监听文件变化。
  唯一例外：ModManager 面板改（它直接写 `Entry.Value`，立即生效）。
- **改 `item_blacklist.json` / `item_catalog.json` 后需重启游戏** —— 这两份文件只在启动时被读一次。

---

## 关键设计（按模块速查）

### 1. 堆叠放开（0.2.0 核心）

游戏原生 `EndDrag` 已具备完整合并机器，但被 `GameItem.MayStack` 恒 false 闸住。
`Patch_MayStack` Prefix 放行同名物品即可。

### 2. 合并决策单一源（0.10.0）

```
拖放 / Alt 快移 / Ctrl 快移
         ↓
   MergePolicy.Evaluate(ctx)
         ↓
  状态一致性 → 物品黑名单 → 容器黑名单 → 容器白名单
```

固定顺序，三路共用。回滚开关 `UseMergePolicy`。

### 3. 状态一致性闸门（1.0.0）

两件物品 `GameItem.state`（TagSystem）必须**完全相等**才可合。
不可豁免的最终硬闸门 —— 任何名单都不能放行状态不一致。

忽略名单（`ignoreTags`）避免把「UI 选中 / 机器进度 / 教程」等运行时 tag 误判为物品属性。

### 4. tag 值域校验（1.2.0）

游戏 `GetSplitItem` 拆分时用 `DirectoryMaster.Item(id)` **重新生成默认物品** —— 拆出的物品带默认 tag（满电/满耐久/默认纯度），而非原堆实际 tag。

本闸门要求：关键 tag（电量/耐久/纯度/品质/液体/次数）必须等于该 identifier 的默认模板值才可堆叠。
⇒ 从根上杜绝拆分后状态污染。

零值变体（0.14.0）：赃物热度 `STOLEN_VALUE_INT` 必须缺省或 0（0 热度可堆 / 非 0 不可堆）。

### 5. 机器容器保护（0.13.0 / 0.14.2）

熔炉/解序器/水瓶打印机等 13 个机器：**禁并 + 每格 1 件**。下沉到 `SlotMarker.TryAcceptOnce` 公共瓶颈，覆盖拖放/Ctrl拖动/快移全部入口。

### 6. 销毁修复（0.9.6~0.10.0）

游戏多处按 unitCount==1 假设写逻辑，堆叠后整堆被销毁。统一模式：
`Prefix` 拆堆到 1 件，原版销毁 1 件，`Postfix` 归还余量。

覆盖：打印机升级 / 模块修复 / 采矿机修理 / 拾荒口袋 / 熔炉 / 解序器。

### 7. 整堆计价（0.9.0 / 0.14.3）

`GetNegociatedValue` 原生只返回单件价，交易系统 30+ 调用点全走它 ⇒ 在它 Postfix ×`unitCount` 即可全局覆盖。

**0.14.3 关键修复**：`Main.cs` 的手动 `PatchAll` 与 MelonLoader 自动 PatchAll 叠加，导致 Postfix 挂了两遍 ⇒ `×count` 执行两次 ⇒ n² 放大（2 件 25 单价显示 100，应 50）。
**修复 = 删手动 PatchAll**。

### 8. Alt / Shift / Ctrl 快移

| 键 | 行为 |
|---|---|
| Alt + 点击 | 转发给原生 Ctrl 逻辑（0.13.x 起），数量由 `AltQuickTransferAmount` 决定 |
| Alt + 拖动 | 原版 1 个 |
| Shift + 点击 | 由 `ShiftPairEnable` 决定（默认关，交还原版单向导向展示柜） |
| Ctrl + 点击 | 原生快移 + 合并闸门（数据保护类硬拒 / 便利类导流空槽） |
| Ctrl + 拖动 | 整堆 |

---

## 版本历史（简）

| 版本 | 日期 | 主要变更 |
|---|---|---|
| 0.14.4 | 2026-10-15 | Config 键重命名 + 迁移（`z9_UseMergePolicy` → `UseMergePolicy`、`AltClickTransferToCounter` → `AltQuickTransferEnable`）；默认白名单移除 `bottle_printer`；修复 `AltPairEnable=false` 时落点被静默改写 |
| 0.14.3 | 2026-10-15 | **修复 PatchAll 双重挂载** 导致整堆计价 n² 放大 |
| 0.14.2 | 2026-10-14 | 机器容器三处修正：只降不升 / 基类 finder / 公共瓶颈 |
| 0.14.1 | 2026-10-14 | TryAcceptAllMid 返回值语义修正 / BuiltinIdBlacklist cfg 化 |
| 0.14.0 | 2026-10-13 | 赃物热度零值闸门 |
| 0.13.0 | 2026-10-13 | 机器容器拦截（自动拆堆） |
| 0.12.x | 2026-10-12 | Ctrl 快移合并闸门 / Alt/Shift 配对 / 白名单空值迁移 |
| 0.11.x | 2026-10-11 | Alt 双向快移 / slot: token 改游戏原生 id |
| 1.0.0 / 1.2.0 | 2026-10-11 | 状态一致性闸门 / tag 值域校验 |
| 0.9.0 | 2026-10-09 | 整堆计价 + 柜台禁堆默认关 |
| 0.7.x | 2026-10-08 | Alt 快移重构 / 扩充黑名单 / 转场只搬 1 件修复 |
| 0.6.x | 2026-10-08 | Alt 快移真正生效（挂对类）/ cfg 保活 / 柜台三格判据 |
| 0.5.x | 2026-09 | 堆叠黑名单（可鉴定/赃物/未拥有/电量/动物/鼠） |
| 0.2.0 | 2026-08 | 放开原生堆叠（核心） |

---

## 已知问题 / 未验证

1. **`GetStoreBuyPrice` 常量公式无调用者**（游戏本体内，非本 mod 问题）
2. **`NegociationUIManager.GetMultiSellValue` 内部是否逐件累加** —— 若会，整堆计价需改 patch 点
3. **native 是否真用 `maxSlotAmountItemFunc` 算上限** —— 未实机验证
4. **IL2CPP 反汇编定位到的 RVA 有时是 trampoline** —— 直接反汇编会误读

---

## TODO

- [ ] 反汇编 `NegociationUIManager.InitUIWithItemSellMode`，确认整堆计价的正确 patch 点
- [ ] 单测补充（当前 xUnit 27 例，目标 50+）
- [ ] 剥离 `MergePolicy.Legacy.cs`（若确认不再需要回滚）

---

## LICENSE

MIT（详见 `LICENSE`）。
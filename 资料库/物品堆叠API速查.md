# 物品堆叠 API 速查（Probably Stolen 0.46D）

> 完整说明见 `堆叠资料库.md`。RVA 来自 Il2CppDumper dump.cs（0.46D）。

## 字段

| 字段 | 类型 | 所属类 | 偏移 | 含义 |
|---|---|---|---|---|
| `unitCount` | int | `GameItem` | 0x1F8 | **堆叠数量** |
| `unitCount` | int | `SaveItemNode` | 0x18 | 存档里的堆叠数量 |
| `stackCountText` | TreeNodeRender | `GameItemElement` | 0x300 | 屏上堆叠数字（跟随 unitCount） |
| `unitValue` / `lateUnitValue` / `unitBaseValue` / `backupUnitValue` | long | `GameItem` | 0x200–0x218 | 每单位价值（随 unitCount 缩放） |

## 方法（GameItem）

| 方法 | RVA | 备注 |
|---|---|---|
| `SetUnitCount(int)` | 0x79B600 | **= SetAmount 别名** |
| `SetAmount(int)` | 0x79B600 | 同上 |
| `MayStack(GameItem)` | 0x799120 | 能否堆叠 |
| `GetStackSlot(GameItem)` | 0x797720 | 可叠放槽位 |
| `GetIncompleteSlotInInventory()` | 0x795E20 | 未叠满槽 |
| `GetSplitItem(int requestedAmount)` | 0x797500 | 拆分 |
| `MaxNumAdd()` / `MaxNumRemove()` | 0x798430 / 0x7984A0 | 单次交互上限 |
| `TryAcceptAll(GameItem)` / `TryUnloadAll()` | 0x79C6C0 / 0x79C8D0 | 整体收发 |

## 堆叠上限官方扩展点（委托，两层都有）

签名：`Func<GameItem, GameInventory, SlotMarker, int>`，`{ get; set; }`

| 委托 | 层 | getter RVA | setter RVA |
|---|---|---|---|
| `maxSlotAmountItemFunc` | 物品层 `GameItemFunc` / 库存层 `GameInventoryFunc` | 0x367F00 | 0x2F02F0 |
| `minSlotAmountItemFunc` | 两层 | 0x33FBD0 | 0x33FCE0 |
| `mayStackItemFunc` | 物品层 | 0x2BD8B0 | 0x2C2690 |
| `maxSlotAddItemFunc` | 两层 | 0x2D9290 | 0x35D660 |
| `maxSlotRemoveItemFunc` | 两层 | 0x33F710 | 0x35D980 |

## 库存类型（GameInventory 子类）

`GameCharacterRaidInventory`(1313) · `GameGridInventory`(1318) · `GameGridScrollableInventory`(1324) · `GameSlotInventory`(1331)

## 推荐钩子

- **放开上限（首选）**：set `mayStackItemFunc` / `maxSlotAmountItemFunc` → 不改 IL、可卸载
- **读档改值（最干净持久化）**：`SaveItemNode.OnAfterDeserialize` Postfix 改 `unitCount`
- **存档链路**：`SaveManager.DecodeNodes`(0x8146D0) / `EncodeNode`(0x8145E0) / `EncodeNodes`(0x815AA0)

## 红线

- 改 `unitCount` 会牵动价值缩放；`UncheckedAccept`/`Expel` **不在加载期跑**（0x80131506 崩溃）
- 默认上限值 / 合并算法在 native，需 Ghidra 跟进（见资料库 §6）

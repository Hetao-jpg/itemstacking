# 堆叠 mod · 容器套件白名单 token 备选（0.9.3）

对照来源：ContainerSuite 配置（MelonPreferences.cfg [CustomInfrastructure] 段）
BoxIds / BackpackIds / CageIds / MultiSlotIds / MultiSlotModuleIds / Wage* / ModContainerIds
+ 0.9.1 实机探针实测（owner: 已验证可用）。

## 推荐整串（箱 + 背包 + 主仓库/展示柜/海报暗格）

```
ContainerWhitelist = "owner:backpack_small,owner:backpack_medium,owner:backpack_large,owner:satchel,owner:fanny_pack,owner:backpack_medium_military,owner:backpack_large_military,owner:sec_box,owner:evidence_box,owner:storage_bay,owner:storage_bay_large,owner:smuggler_bay,owner:smuggler_bay_mod,owner:machine_bay,owner:machine_bay_ext,owner:service_box,owner:med_box,owner:eng_box,owner:storage_bay_chem,owner:storage_bay_gun,owner:smuggler_bay_mini,owner:makeshift_storage_bay,owner:expedition_box,owner:custom_storage_box,owner:void_bead_storage,owner:wage_gu_machine,owner:wage_ai_generator,owner:animal_cage,owner:rat_cage,owner:fridge,owner:recharger,owner:vacuum_robot,owner:furnace,owner:desequencer,owner:security_alarm,owner:mirage_projector,owner:water_purifier,owner:moisture_farm,owner:wine_rack,owner:bottle_printer,slot:save_bag,slot:showcase_save_bag,slot:hidden_save_bag"
```

## 精简版（最常用：随身背包 + 船舱储藏 + 主仓库/展示柜）

```
ContainerWhitelist = "owner:backpack_small,owner:backpack_medium,owner:backpack_large,owner:satchel,owner:fanny_pack,owner:storage_bay,owner:storage_bay_large,owner:smuggler_bay,owner:smuggler_bay_mod,owner:machine_bay,owner:machine_bay_ext,slot:save_bag,slot:showcase_save_bag"
```

## 分类明细

### 背包类（BackpackIds）
| 容器物品 id | token | 备注 |
|---|---|---|
| backpack_small | owner:backpack_small | 背包（小），探针已验证 |
| backpack_medium | owner:backpack_medium | 背包（基础），探针已验证 |
| backpack_large | owner:backpack_large | 背包（大），探针已验证 |
| satchel | owner:satchel | 挎包（小） |
| fanny_pack | owner:fanny_pack | 腰包 |
| backpack_medium_military | owner:backpack_medium_military | 背包（军用） |
| backpack_large_military | owner:backpack_large_military | 大型军用背包 |

### 箱类（BoxIds）
| 容器物品 id | token | 备注 |
|---|---|---|
| sec_box | owner:sec_box | 治安部物资箱 |
| evidence_box | owner:evidence_box | 治安部物证柜 |
| storage_bay | owner:storage_bay | 储藏区 |
| storage_bay_large | owner:storage_bay_large | 储藏区（扩建），探针已验证 |
| smuggler_bay | owner:smuggler_bay | 走私者暗格，探针已验证 |
| smuggler_bay_mod | owner:smuggler_bay_mod | 走私者暗格（改进），探针已验证 |
| machine_bay | owner:machine_bay | 机器区，探针已验证 |
| machine_bay_ext | owner:machine_bay_ext | 机器区（扩建），探针已验证 |
| service_box | owner:service_box | 后勤服务部物资箱 |
| med_box | owner:med_box | 医疗舱物资箱 |
| eng_box | owner:eng_box | 工程铸造厂物资箱 |
| storage_bay_chem | owner:storage_bay_chem | 化学家储物舱 |
| storage_bay_gun | owner:storage_bay_gun | 枪匠储物舱 |
| smuggler_bay_mini | owner:smuggler_bay_mini | 迷你走私舱 |
| makeshift_storage_bay | owner:makeshift_storage_bay | 简易储物舱 |
| expedition_box | owner:expedition_box | 远征补给箱 |

### 笼/多槽/模组槽/Mod 容器
| 容器物品 id | token |
|---|---|
| animal_cage / rat_cage | owner:animal_cage / owner:rat_cage |
| fridge / recharger / vacuum_robot | owner:fridge / owner:recharger / owner:vacuum_robot |
| furnace / desequencer / security_alarm / mirage_projector | owner:furnace 等 |
| water_purifier / moisture_farm / wine_rack / bottle_printer | owner:water_purifier 等 |
| custom_storage_box（蛙哥妙妙箱） | owner:custom_storage_box |
| void_bead_storage（虚空珠） | owner:void_bead_storage |
| wage_gu_machine / wage_ai_generator | owner:wage_gu_machine 等 |

### 交易场景槽位（slot:，仅交易窗口内有效）★ 0.11.2 起为游戏原生 id（ASCII）

| 槽位（中文，仅供人读） | EmporiumEntry 偏移 | token（游戏原生 id） |
|---|---|---|
| 主仓库（背包窗口） | 0x30 invElement | `slot:save_bag` |
| 展示柜 | 0x40 showcaseElement | `slot:showcase_save_bag` |
| 柜台前格 | 0x88 frontInvinvElement | `slot:frontInv` |
| 玩家柜台 | 0x98 backInvinvElement | `slot:back_inv_save_bag` |
| 柜台三格 | 0xA0 backInvinvElementCounter | `slot:backInvCounter` |
| 已售格 | 0x108 soldElement | `slot:sold_save_bag` |
| 海报暗格 | 0xC0 hiddenElement | `slot:hidden_save_bag` |
| 黑市左侧 | 0xB0 bazarLeftinvElement | `slot:bazarInv` |

> **为什么不用中文了**：旧版返回中文槽位名，cfg 里必须手打中文 —— 易错，且违背
> 「任何标识都以游戏文件为准」。0.11.2 起改用**游戏自带的 id**：
> 前 5 个（save_bag / showcase_save_bag / back_inv_save_bag / sold_save_bag / hidden_save_bag）
> 是游戏真实存在的**存档袋物品 id**（`items_catalog.md` 收录，`PlayerStore.saveBags` 字典键，
> 见 `ContainerItemDirectory.*SaveBag` 工厂 + `PlayerStore.GetSaveBag` 0x58D240）；
> 后 3 个槽游戏既无 identifier 也无存档袋 → 用 EmporiumEntry 的 **C# 成员名**作 token。
> 注：玩家柜台(0x98) 运行时 identifier = `backInv`，故裸 id token `backInv` 也能匹配到它。

### 无法用 token 匹配的（基建家具）
垃圾桶(trashcan) / 档案箱(dossier) / 磁带播放器(cassette_player) / 排水口(drain)：
- GetParentItem() 为 null → owner: 无效
- identifier 为空 → 裸 id 无效
- 不在 EmporiumEntry → slot: 无效

→ 白名单非空时这些容器**禁止堆叠**（符合「只允许在名单内堆叠」语义）。
  若想在里面也能堆，需等 mod 加「内容指纹」或「类名」判据，或临时把白名单留空。

## 推荐黑名单（柜台/已售/黑市 —— 禁堆）

```
ContainerBlacklist = "slot:back_inv_save_bag,slot:frontInv,slot:backInvCounter,slot:sold_save_bag,slot:bazarInv"
```

## 使用步骤

1. cfg 里把 `DiagLog = true`
2. 进游戏，把物品拖进/拖出每个你想堆叠的容器（触发闸门探针）
3. 退游戏，在游戏目录运行：
   ```
   powershell -ExecutionPolicy Bypass -File "D:\games\open code work\Item stacking\find_whitelist.ps1" -CopyToClipboard
   ```
4. 脚本会打印探针解析结果 + 推荐白名单串，并复制到剪贴板
5. 粘进 `UserData/MelonPreferences.cfg` 的 `[ItemStacking]` 段（保留外层引号）
6. 改完约 1.5s 生效（cfg 保活），无需重启；再拖一次验证 `[作用域探针] 白名单内=是`

## 已知注意

- `smuggler_bay` 系列探针出现过 `id=smuggler_bay 所属=smuggler_bay_mod`（identifier 继承父物品），
  用 `owner:` 比裸 id 更稳。
- 同 id 的所有实例会一起命中（白名单是「一类容器」不是「某一个箱子」）。
- 黑名单优先于白名单；只拦合并，空槽放置不受限。

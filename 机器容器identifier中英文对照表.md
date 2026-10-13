# 机器容器 identifier 中英文对照表

> 用途：`堆叠mod机器拦截方案v2.yaml`（机器容器自动拆堆）的 `machineContainers` 名单参考。
> 数据源：`Mods/item_catalog.json`（480 项目录，id + english + chinese + aliases）；
> 机器 id 注册点来自 `GameAssembly.dll` 反汇编（RE1，`FurnitureItemDirectory/HusbandryDirectory/ToolDirectory` 等）。
> 生成日期：2026-10-12 ｜ 消费点：`BuiltinBlacklist.IsMachineContainer`（MergePolicy 机器 Deny + MaxNumAdd 钳 1）。

## 一、默认拦截名单（写入 `DefaultMachineContainerTokens`，共 13 项）

| token（JSON 写法） | identifier | English | 中文名 | 注册/取证 | 备注 |
|---|---|---|---|---|---|
| `owner:furnace` | `furnace` | Furnace | 熔炉 | FurnitureItemDirectory.InitDirectory 注册 + MachineFurnace.Furnace 工厂 | DestroyFix.Patch_FurnaceConsume 已修（崩坏实证） |
| `owner:desequencer` | `desequencer` | Cryptographic Desequencer | 密码解序器 | FurnitureItemDirectory.InitDirectory 注册 + MachineDesequencer.Desequencer 工厂 | DestroyFix.Patch_FinishDesequencer 已修（崩坏实证） |
| `owner:bottle_printer` | `bottle_printer` | Bottle Printer | 水瓶打印机 | FurnitureItemDirectory.InitDirectory 注册 + MachineBottlePrinter.BottlePrinter/UpgradePrinter | DestroyFix.Patch_BottlePrinterUpgradePrinter 已修（崩坏实证） |
| `owner:box_dispenser` | `box_dispenser` | Box Dispenser | Box Dispenser | FurnitureItemDirectory.InitDirectory 注册 + MachineBoxDispenser.BoxDispenser 工厂 | 分配/发放逻辑按单件假设 |
| `owner:moisture_farm` | `moisture_farm` | Moisture Farm | 水分收集器 | FurnitureItemDirectory.InitDirectory 注册 + MachineMoistureFarm.MoistureFarm 工厂 | 种植格输入 |
| `owner:water_purifier` | `water_purifier` | Water Purifier | 净水器 | FurnitureItemDirectory.InitDirectory 注册 + MachinePurifier.WaterPurifier 工厂 | 净水处理输入 |
| `owner:wine_rack` | `wine_rack` | 催陈器 | 催陈器 | FurnitureItemDirectory.InitDirectory 注册 + MachineWineRack.AgeWell / PreBuiltItemHelper.CreateAgeWell | 陈化槽输入 |
| `owner:livestock_feed_dispenser` | `livestock_feed_dispenser` | Livestock Feed Dispenser | 牲畜饲料分配器 | HusbandryDirectory.InitDirectory 注册 | 饲料仓输入 |
| `owner:blender` | `blender` | Blender | 搅拌机 | MachineBlender.CreateTooltip 引用 id + 目录确认（数据表定义） | 料理/混合输入 |
| `owner:chem_finisher` | `chem_finisher` | Chem Finisher | 化学成品机 | MachineChemFinisher.CreateTooltip 引用 id + 目录确认（数据表定义） | 化学成品加工输入 |
| `owner:heating_plate` | `heating_plate` | Heating Plate | 加热板 | MachineHeatingPlate.CreateTooltip 引用 id + 目录确认（数据表定义） | 加热处理输入 |
| `owner:hydroponic` | `hydroponic` | Hydroponic System | 水培系统 | PreBuiltItemHelper.CreateHydroponic + MachineHydroponic.LoadHydroponicModuleInvData | 水培种植输入 |
| `owner:3d_printer` | `3d_printer` | 3D Printer | 3D打印机 | MachinePrinter（item_3d_printer_* 字符串）+ 目录确认；打印耗材/芯片输入 | 目录中另有别名条目 printer（同一物品） |

## 二、有意排除的机器 / 容器（不在默认名单，可经 JSON 按需添加）

| identifier | English | 中文名 | 排除理由 |
|---|---|---|---|
| `fridge` | （目录无此 id） | （目录无此 id） | 储物型：默认白名单已含 owner:fridge，堆叠玩法现行可用；无 unitCount 崩坏证据（R8 回归风险） |
| `recharger` | Recharger | 充能器 | 储物型：默认白名单已含 owner:recharger；无崩坏证据（R8 回归风险） |
| `cassette_player` | Cassette Player | 磁带播放器 | 单卡带槽、不消耗物品；无崩坏证据 |
| `powerblock` | PowerBloc | 能量阵列 | 作为物品已在 blockItemIds 拦截；无物品输入格证据 |
| `security_alarm` | Alarm System | 警报系统 | 无物品输入格（仅警报逻辑） |
| `mirage_projector` | Mirage Projector | 幻影投影仪 | 无物品输入格（仅投影/模块） |
| `deal_maker` | （目录无此 id） | （目录无此 id） | 增益型机器（议价概率），无物品输入格 |
| `test_gizmo` | Test Gizmo | Test Gizmo | 调试用物品，无输入格 |
| `turbo_booster` | Disposable Turbo Booster | 一次性涡轮增压器 | 工具类（ToolDirectory 注册），非容器 |
| `turbo_booster_adv` | Advanced Turbo Booster | 高级涡轮增压器 | 工具类（ToolDirectory 注册），非容器 |
| `vending_machine` | Vending Machine | 自动售货机 | 售货逻辑对堆叠的行为未取证（StoreVendingMachine 遍历风险不明），无崩坏实证 → 需要可经 JSON 加入 |
| `machine_bay` | Machine Bay | 机器区 | 纯储物：默认白名单已含 owner:machine_bay* |
| `machine_bay_ext` | Machine Bay (Expanded) | 机器区（扩建） | 纯储物：默认白名单已含 owner:machine_bay* |

## 三、方案 v2 占位符证伪（目录中不存在，勿照抄）

v2 方案 §四 default_tokens 的占位符经目录核对为**编造 id**，本作物品目录中不存在：

`brewing_barrel`, `incubator`, `concentrator`, `crafting_table`, `animal_feeder`

- 酿酒/孵化/浓缩相关内容仅有说明文书：`be_barrel_usage_guide` / `be_incubator_usage_guide` / `be_concentrator_usage_guide`（对应扩展模组的**指南**，其机器实体 id 未知）。
- 若安装了对应扩展模组，请先用游戏内目录/日志确认其机器真实 id，再经 `machineContainers` 添加。

## 四、JSON 用法（Mods/item_blacklist.json）

```json
{
  "machineContainers": [
    "owner:furnace",
    "owner:desequencer"
  ]
}
```

- token 三路形态与白名单一致：裸 id（容器 identifier 精确匹配）/ `slot:槽位` / `owner:所属物品id`（物品链任一层）。
- 机器容器判据**只读 `owner:` 与裸 id 两条匹配路**（机器格的容器是机器内部 inventory，沿所属物品链命中）。
- 字段缺省 → 用上方代码默认名单；`[]` → 显式清空（关闭机器拦截）；非空数组 → 整组覆盖。
- 名单规模与来源在启动日志 `[名单] … 机器=…` 一行可见；查不到的 id 由 `[名单自检]` 提示。

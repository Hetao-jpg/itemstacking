======================================================================
Item Stacking · 自定义物品堆叠
作者：核桃 | 版本：0.14.4
游戏：Probably Stolen 0.46D
环境：MelonLoader 0.7.4+
======================================================================

【安装】
把 ItemStacking.dll 放进游戏目录 Mods/ 文件夹，启动游戏即可。
卸载 = 删掉 DLL。

----------------------------------------------------------------------
【功能】
----------------------------------------------------------------------
· 拖放堆叠：同名物品合并，上限默认 10（可调）
· Ctrl+拖动整堆；无修饰键/Shift/Alt 交还原版
· Alt+点击：快移（与 Ctrl 同一条逻辑；默认 1 件，可调）
· Shift+点击：交还原版（导向展示柜）
· 状态一致性：电量/纯度/品质/磨损等完全相等的物品才可合
· 容器防护：装了东西的容器禁止合并
· 物品黑名单：node/module/keycard/文书等默认拦截；
  可在 cfg 补充自定义；可整体关闭
· 容器白名单 / 黑名单
· 机器防护：熔炉/解序器/打印机等机器内每格 1 件
· 整堆计价：交易系统按整堆总价值结账（默认关，见下方注意）
· 销毁修复：打印机升级 / 模块修复 / 采矿机 / 拾荒口袋 / 熔炉 / 解序器
· 补丁自检 + 诊断日志

----------------------------------------------------------------------
【重要默认：容器白名单】
----------------------------------------------------------------------
ContainerWhitelist 默认已含常用容器，装好即用：
  储藏区 / 走私暗格 / 机器区、小中大背包 / 挎包 / 腰包、动物笼、
  冰箱 / 充能器 / 清洁机器人 / 水瓶打印机、
  主仓库 / 展示柜 / 海报暗格 / 玩家柜台。

留空 = 任何容器都不堆叠（mod 等于未启用）。
含逗号时用引号包起来；已有 cfg 里的旧值优先于代码默认值。

示例（即默认值）：
ContainerWhitelist = "owner:storage_bay,owner:storage_bay_large,owner:smuggler_bay,owner:smuggler_bay_mod,owner:smuggler_bay_mini,owner:machine_bay,owner:machine_bay_ext,owner:backpack_small,owner:backpack_medium,owner:backpack_large,owner:satchel,owner:fanny_pack,owner:animal_cage,owner:fridge,owner:recharger,owner:vacuum_robot,owner:bottle_printer,slot:save_bag,slot:showcase_save_bag,slot:hidden_save_bag,slot:back_inv_save_bag"

token 前缀：
  owner:物品id（背包/船舱/笼子等）
  slot:槽位id（固定窗口格）

slot: 使用游戏原生 id：
  save_bag              = 主仓库
  showcase_save_bag     = 展示柜
  back_inv_save_bag     = 玩家柜台
  sold_save_bag         = 已售格
  hidden_save_bag       = 海报暗格
  frontInv              = 柜台前格
  backInvCounter        = 柜台三格
  bazarInv              = 黑市左侧

机器容器（熔炉 / 解序器 等）不要加入白名单。

----------------------------------------------------------------------
【配置项】
----------------------------------------------------------------------
位置：UserData/MelonPreferences.cfg 的 [ItemStacking] 段。
格式：配置项 | 默认 | 说明

EnableStacking | 开 | 启用拖放堆叠
NewStackCap | 10 | 堆叠上限
CtrlDragWholeStack | 开 | Ctrl+拖动 = 整堆
DiagLog | 关 | 诊断日志总开关（排障时开）
StackBlacklist | 开 | 保护总闸（关掉会停用 id 黑名单）
BuiltinIdBlacklist | 开 | 内置 id 黑名单（node/module/keycard/文书等）
ItemBlacklist | 空 | 自定义物品黑名单（在内置名单之外再补）
ContainerWhitelist | 常用容器集合 | ★ 空 = 全不生效
ContainerBlacklist | 空 | 容器黑名单
StrictStackCap | 关 | 堆叠上限硬收紧（默认关，只输出探针）
MachineOnePerSlot | 开 | 机器容器每格 1 件（禁并）
ModifierKeysEnabled | 开 | 修饰键快移总开关
AltClickTransferToCounter | 开 | Alt 快移总闸
AltPairEnable | 开 | Alt 快移是否走本 mod（关 = 交还原版）
ShiftPairEnable | 关 | Shift 双向配对（关 = 交还原版单向导向展示柜）
AltQuickTransferAmount | 1 | Alt 快移件数
AltPairMode | ctrl | Alt 快移目标模式
AltPairMerge | 开 | 配对快移时允许合并
StackValueTotal | 关 | 整堆计价（见下方注意）
ValueGate | 开 | tag 值域校验（防拆分后状态污染）
UseMergePolicy | 开 | 合并判定代码路径（普通玩家保持开）
AltQuickTransferEnable | 开 | Alt 快移总闸（原 AltClickTransferToCounter 改名）

值含逗号时用引号，例如：
ContainerWhitelist = "a,b"

----------------------------------------------------------------------
【外置黑名单】
----------------------------------------------------------------------
可在 Mods/item_blacklist.json 覆盖【内置名单】：

{
  "blockItemIds": ["metal_ingot"],
  "blockIdPrefixes": ["node"],
  "blockIdContains": ["module", "neural_core"],
  "blockIdSuffixes": ["_keycard"],
  "blockSuffixExempt": ["blank_keycard"],
  "machineContainers": ["owner:furnace", "owner:desequencer"],
  "ignoreTags": ["UI_SELECTED_TAG"]
}

· 某组不写 = 用代码默认；写 [] = 该组清空
· 文件不存在 / 格式错误 = 全部退回代码默认

----------------------------------------------------------------------
【热键】
----------------------------------------------------------------------
Ctrl + 拖动          拿起整堆
无修饰键 + 拖动      整堆（原版）
Shift + 拖动         拿 10%（原版）
Alt + 拖动           1 个（原版）
Alt + 点击           快移（与 Ctrl 同逻辑，默认 1 件）
拖起后滚轮           微调数量

----------------------------------------------------------------------
【注意】
----------------------------------------------------------------------
· 删 DLL = 全部功能失效；不要改 DLL 文件名。
· 装了东西的容器禁止堆叠（防内部物品丢失）。
· 整堆计价默认关；若装了加价 mod，开启后价格可能异常放大。
  ★ 若发现卖出价格是单价的 n² 倍，说明有 mod 与本 mod 同时在
     ×n，请在 ModManager 里停用其它加价 mod，或保持本项关闭。
· 第一次使用前建议备份存档。

----------------------------------------------------------------------
【日志】
----------------------------------------------------------------------
MelonLoader/Latest.log，搜 [Item Stacking]。
=====================================================================
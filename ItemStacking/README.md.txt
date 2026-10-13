======================================================================
Item Stacking · 自定义物品堆叠
作者：核桃
版本：0.14.0
游戏：Probably Stolen 0.46D
环境：MelonLoader 0.7.4+
======================================================================

【安装】
把 ItemStacking.dll 放进游戏目录 Mods/ 文件夹，启动游戏即可。

----------------------------------------------------------------------
【功能】
----------------------------------------------------------------------
· 拖放堆叠：同名物品合并
· 堆叠上限自定义（默认 10）
· Ctrl+拖动 = 整堆；无修饰键 = 只拿 1 个
· Alt 快移：玩家柜台 ↔ 主仓库
· Shift 快移：展示柜 ↔ 主仓库
· 状态一致性：电量/纯度/品质/磨损/拥有权等完全相等才可合并
· 容器防护：装了东西的容器禁止合并，防内部物品丢失
· 物品黑名单：仅 cfg 自定义 ItemBlacklist 生效
  （内置名单 node/module/keycard/文书等已于 2026-10-12 停用）
· 容器白名单 / 黑名单
· 整堆计价：卖出/买入/批发/估价按整堆总价值结账（默认关）
· 销毁修复：水瓶打印机/模块修复/采矿机/拾荒口袋/熔炉/解序器
· 补丁自检 + 诊断日志
（读档补满功能已于 2026-10-12 移除）

----------------------------------------------------------------------
【重要默认】
----------------------------------------------------------------------
ContainerWhitelist（容器白名单）默认已含常用容器：
  储藏区 / 走私暗格 / 机器区（含扩建）、小中大背包 / 挎包 / 腰包、
  动物笼、冰箱、充能器、清洁机器人、水瓶打印机、
  主仓库 / 展示柜 / 海报暗格 / 玩家柜台。
装好即用，不用改配置。

留空 = 任何容器都不堆叠（mod 等于未启用）。
含逗号时用引号包起来；已有 cfg 里的旧值优先于代码默认值，
要采用新默认：删除 cfg 中的 ContainerWhitelist 行后重启。

示例（即默认值）：
ContainerWhitelist = "owner:storage_bay,owner:storage_bay_large,owner:smuggler_bay,owner:smuggler_bay_mod,owner:smuggler_bay_mini,owner:machine_bay,owner:machine_bay_ext,owner:backpack_small,owner:backpack_medium,owner:backpack_large,owner:satchel,owner:fanny_pack,owner:animal_cage,owner:fridge,owner:recharger,owner:vacuum_robot,owner:bottle_printer,slot:save_bag,slot:showcase_save_bag,slot:hidden_save_bag,slot:back_inv_save_bag"

token 前缀：owner:物品id（背包/船舱/笼子等），
slot:槽位id（固定窗口格）。裸 id 大多数容器匹配不上（id 为空）。

slot: 使用游戏原生 id：
  save_bag              = 主仓库
  showcase_save_bag     = 展示柜
  back_inv_save_bag     = 玩家柜台
  sold_save_bag         = 已售格
  hidden_save_bag       = 海报暗格
  frontInv              = 柜台前格
  backInvCounter        = 柜台三格
  bazarInv              = 黑市左侧

机器容器（熔炉 / 解序器 / 其他 mod 机器）不要加入白名单，
否则可能丢物品。

----------------------------------------------------------------------
【配置项】
----------------------------------------------------------------------
位置：UserData/MelonPreferences.cfg 的 [ItemStacking] 段。
以下每行格式：配置项 | 默认 | 说明

EnableStacking | 开 | 启用拖放堆叠
NewStackCap | 10 | 堆叠上限
CtrlDragWholeStack | 开 | Ctrl 整堆
DiagLog | 关 | 诊断日志总开关
StackBlacklist | 开 | 堆叠保护总闸
ContainerWhitelist | 常用容器集合 | ★ 空 = 全不生效；已含船舱/背包/笼/设备/窗口格
ContainerBlacklist | 空 | 容器黑名单
ItemBlacklist | 空 | 自定义物品黑名单（内置名单已停用）
BlockCounterStack | 关 | 柜台内禁堆
StackValueTotal | 关 | 整堆计价
ModifierKeysEnabled | 开 | 修饰键快移总开关
AltClickTransferToCounter | 开 | Alt 快移总闸
AltPairEnable | 开 | Alt 配对快移
ShiftPairEnable | 开 | Shift 配对快移
AltQuickTransferAmount | 1 | Alt / Shift 快移件数
AltPairMode | counter_inv | Alt 配对模式
AltPairMerge | 开 | 配对快移时允许合并
z9_UseMergePolicy | 开 | 合并判定代码路径（普通玩家保持开）

值含逗号时用引号，例如：
ContainerWhitelist = "a,b"

----------------------------------------------------------------------
【外置黑名单】
----------------------------------------------------------------------
可在 Mods/item_blacklist.json 覆盖名单（注意：内置黑名单已停用，
block* 各组当前不产生实际拦截；ignoreTags 仍生效于状态一致性闸门）：

{
  "blockItemIds": ["metal_ingot", "powerblock"],
  "blockIdPrefixes": ["node"],
  "blockIdContains": ["module", "neural_core"],
  "blockIdSuffixes": ["_keycard"],
  "blockSuffixExempt": ["blank_keycard"],
  "ignoreTags": ["UI_SELECTED_TAG", "CUSTOM_SYSTEM_TAG"]
}

· 只写你关心的组，其余用代码默认值。
· 某组写 [] = 该组清空。
· 文件不存在 / 格式错误 = 全部退回代码默认。

----------------------------------------------------------------------
【热键】
----------------------------------------------------------------------
Ctrl + 拖动          拿起整堆
无修饰键 + 拖动      只拿 1 个
Shift + 拖动         拿 10%（原生）
Alt + 点击 / 拖动    快移（玩家柜台 ↔ 主仓库）
拖起后滚轮           微调数量

----------------------------------------------------------------------
【注意】
----------------------------------------------------------------------
· 删 DLL = 全部功能失效；不要改 DLL 文件名。
· 装了东西的容器禁止堆叠。
· 整堆计价默认关；若装了加价 mod，开启后价格可能异常放大。
· 第一次使用前建议备份存档。

----------------------------------------------------------------------
【日志】
----------------------------------------------------------------------
MelonLoader/Latest.log，搜 [Item Stacking]。
======================================================================
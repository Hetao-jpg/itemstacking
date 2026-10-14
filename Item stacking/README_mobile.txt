━━━━━━━━━━━━━━━━━━━━━━━━
Item Stacking · 自定义物品堆叠
作者：核桃 | 版本：0.14.4
游戏：Probably Stolen 0.46D
环境：MelonLoader 0.7.4+
━━━━━━━━━━━━━━━━━━━━━━━━

▎安装
把 ItemStacking.dll 放进
游戏目录 Mods/ 文件夹，
启动即可。卸载 = 删 DLL。

▎功能
· 拖放堆叠（上限默认 10）
· Ctrl+拖动整堆
· Alt+点击快移（与 Ctrl 同逻辑，
  默认 1 件）
· Shift 交还原版单向导向展示柜
· 状态完全相等才可合
· 装了东西的容器禁止合并
· 物品黑名单（node/module/
  keycard/文书等）默认开
· 容器白名单/黑名单
· 机器容器每格 1 件
· 整堆计价（默认关，见下方注意）
· 销毁修复：打印机/模块/采矿机/
  口袋/熔炉/解序器
· 补丁自检 / 诊断日志

▎重要默认
容器白名单（ContainerWhitelist）
默认已含常用容器，装好即用：
  储藏区 / 走私暗格 / 机器区、
  小中大背包 / 挎包 / 腰包、动物笼、
  冰箱 / 充能器 / 清洁机器人 / 水瓶打印机、
  主仓库 / 展示柜 / 海报暗格 / 玩家柜台。

留空 = 都不堆叠（等于没启用）。
含逗号用引号包起来；cfg 里已有旧值时优先，
要新默认就删掉该行重启。

示例（即默认值）：
ContainerWhitelist = "owner:storage_bay,...,owner:bottle_printer,slot:save_bag,slot:showcase_save_bag,slot:hidden_save_bag,slot:back_inv_save_bag"

owner:物品id（背包/船舱/笼子），
slot:槽位id（固定窗口格）。

slot: 用游戏原生 id：
  save_bag           = 主仓库
  showcase_save_bag  = 展示柜
  back_inv_save_bag  = 玩家柜台
  sold_save_bag      = 已售格
  hidden_save_bag    = 海报暗格
  frontInv           = 柜台前格
  backInvCounter     = 柜台三格
  bazarInv           = 黑市左侧

不要加机器容器（熔炉/解序器等）。

▎配置（常用几项）
在 UserData/MelonPreferences.cfg
的 [ItemStacking] 段。

启用堆叠（EnableStacking）：开
堆叠上限（NewStackCap）：10
Ctrl整堆（CtrlDragWholeStack）：开
诊断日志（DiagLog）：关
保护总闸（StackBlacklist）：开
容器白名单（ContainerWhitelist）：常用容器集合
  ★ 空 = 全不生效
内置名单（BuiltinIdBlacklist）：开
物品黑名单（ItemBlacklist）：空
机器每格1件（MachineOnePerSlot）：开
修饰键总开关（ModifierKeysEnabled）：开
Alt快移总闸（AltQuickTransferEnable）：开
Alt走本mod（AltPairEnable）：开
Shift配对（ShiftPairEnable）：关
快移件数（AltQuickTransferAmount）：1
Alt目标模式（AltPairMode）：ctrl
整堆计价（StackValueTotal）：关
  ★ 与加价 mod 可能冲突，见注意

值含逗号请加引号。

▎外置黑名单
Mods/item_blacklist.json 可覆盖内置名单
（不写=用默认，写[]=清空该组）：
{
  "blockItemIds": ["metal_ingot"],
  "machineContainers": ["owner:furnace"],
  "ignoreTags": ["UI_SELECTED_TAG"]
}
文件坏也退回默认。

▎热键
Ctrl+拖动       整堆
无修饰键+拖动   整堆（原版）
Shift+拖动      10%（原版）
Alt+拖动        1 个（原版）
Alt+点击        快移（与 Ctrl 同逻辑）
滚轮            微调数量

▎注意
· 删 DLL = 全部功能失效
· 不要改 DLL 文件名
· 装了东西的容器禁止堆叠
· 整堆计价默认关
  ★ 若卖出价格是单价的 n² 倍，说明有 mod 与本 mod 同时在 ×n，
    请停用其它加价 mod，或保持本项关闭
· 建议先备份存档

▎日志
MelonLoader/Latest.log
搜 [Item Stacking]

━━━━━━━━━━━━━━━━━━━━━━━━
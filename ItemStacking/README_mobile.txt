━━━━━━━━━━━━━━━━━━━━━━━━
Item Stacking · 自定义物品堆叠
作者：核桃 | 版本：0.12.0
游戏：Probably Stolen 0.46D
环境：MelonLoader 0.7.4+
━━━━━━━━━━━━━━━━━━━━━━━━

▎安装
把 ItemStacking.dll 放进
游戏目录 Mods/ 文件夹，
启动即可。

▎功能
· 拖放堆叠（同名合并）
· 堆叠上限自定义（默认 10）
· Ctrl+拖动整堆 / 读档补满
· Alt 快移：玩家柜台 ↔ 主仓库
· Shift 快移：展示柜 ↔ 主仓库
· 状态一致性：状态完全相等才可合
· 容器防护：装了东西禁止合并
· 物品黑名单：node/模组/神经核心/
  钥匙卡/文书等
· 容器白名单/黑名单
· 整堆计价（默认关）
· 销毁修复：打印机/模块/采矿机/
  口袋/熔炉/解序器
· 补丁自检 / 诊断日志

▎重要默认
容器白名单（ContainerWhitelist）
留空 = 任何容器都不堆叠
（等于没启用）。
要启用，必须填允许的容器，
含逗号时用引号包起来。

示例：
ContainerWhitelist = "owner:backpack_medium,owner:storage_bay,slot:save_bag"

slot: 用游戏原生 id：
  save_bag           = 主仓库
  showcase_save_bag  = 展示柜
  back_inv_save_bag  = 玩家柜台
  sold_save_bag      = 已售格
  hidden_save_bag    = 海报暗格
  frontInv           = 柜台前格
  backInvCounter     = 柜台三格
  bazarInv           = 黑市左侧

不要加机器容器（熔炉/解序器等），
会丢物品。

▎配置（中文说明）
在 UserData/MelonPreferences.cfg
的 [ItemStacking] 段。

启用堆叠（EnableStacking）：开
堆叠上限（NewStackCap）：10
读档补满（FillToCapOnLoad）：开
旧堆叠上限（OldStackCap）：10
Ctrl整堆（CtrlDragWholeStack）：开
诊断日志（DiagLog）：关
保护总闸（StackBlacklist）：开
容器白名单（ContainerWhitelist）：空
  ★ 空 = 全不生效
容器黑名单（ContainerBlacklist）：空
物品白名单（ItemWhitelist）：空
  （已无效，保留兼容）
物品黑名单（ItemBlacklist）：空
柜台禁堆（BlockCounterStack）：关
整堆计价（StackValueTotal）：关
修饰键总开关（ModifierKeysEnabled）：开
Alt快移总闸（AltClickTransferToCounter）：开
Alt配对（AltPairEnable）：开
Shift配对（ShiftPairEnable）：开
快移件数（AltQuickTransferAmount）：1
Alt配对模式（AltPairMode）：counter_inv
配对时合并（AltPairMerge）：开

值含逗号请加引号。

▎外置黑名单
Mods/item_blacklist.json 可覆盖：
{
  "blockItemIds": ["metal_ingot"],
  "blockIdPrefixes": ["node"],
  "blockIdContains": ["module"],
  "blockIdSuffixes": ["_keycard"],
  "blockSuffixExempt": ["blank_keycard"],
  "ignoreTags": ["UI_SELECTED_TAG"]
}
不写则用代码默认，
文件坏也退回默认。

▎热键
Ctrl+拖动       整堆
无修饰键        只拿1个
Shift+拖动      10%（原生）
Alt+点击/拖动   快移
滚轮            微调数量

▎注意
· 删 DLL = 全部功能失效
· 不要改 DLL 文件名
· 装了东西的容器禁止堆叠
· 整堆计价默认关
· 建议先备份存档

▎日志
MelonLoader/Latest.log
搜 [Item Stacking]

━━━━━━━━━━━━━━━━━━━━━━━━
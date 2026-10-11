<#
  堆叠 mod · 白名单 token 助手
  ────────────────────────────────────────────────────────────
  从 MelonLoader 日志解析 [作用域探针]，输出可直接填进
  ContainerWhitelist / ContainerBlacklist 的 token：
    · 所属=<物品id>  → owner:<物品id>
    · 槽位=<名字>    → slot:<名字>（仅交易场景 8 槽）
    · id=<非空>      → 裸 id

  用法（在游戏目录或任意位置 PowerShell 运行）：
    powershell -ExecutionPolicy Bypass -File find_whitelist.ps1
    powershell -ExecutionPolicy Bypass -File find_whitelist.ps1 -LogFile "D:\...\Latest.log"
    powershell -ExecutionPolicy Bypass -File find_whitelist.ps1 -Category Box
    powershell -ExecutionPolicy Bypass -File find_whitelist.ps1 -CopyToClipboard

  前置：游戏内把 MelonPreferences.cfg 的 DiagLog = true，
        在目标容器里拖一件物品触发合并/移动后退出（或留着），再跑本脚本。
#>
param(
    [string]$LogFile = "",
    [string]$Category = "All",   # All | Box | Backpack | Slot | Owner | Bare
    [switch]$CopyToClipboard,
    [switch]$IncludeBlacklistSuggest
)

$ErrorActionPreference = "Stop"

# 中文控制台：PS 5.1 默认 OEM 会把 UTF-8 中文打成乱码
try {
    [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
    $OutputEncoding = [System.Text.Encoding]::UTF8
} catch { }

# ── 定位日志 ──
function Find-LatestLog {
    if ($LogFile -and (Test-Path -LiteralPath $LogFile)) { return (Resolve-Path -LiteralPath $LogFile).Path }
    $candidates = @(
        "D:\SteamLibrary\steamapps\common\Probably Stolen Demo\MelonLoader\Latest.log",
        "$env:USERPROFILE\AppData\LocalLow\Questing Goose Studio\Probably Stolen Demo\MelonLoader\Latest.log"
    )
    # 从脚本位置向上找 MelonLoader
    $dir = Split-Path -Parent $MyInvocation.ScriptName
    for ($i = 0; $i -lt 6 -and $dir; $i++) {
        $candidates += (Join-Path $dir "MelonLoader\Latest.log")
        $candidates += (Join-Path $dir "Probably Stolen Demo\MelonLoader\Latest.log")
        $dir = Split-Path -Parent $dir
    }
    foreach ($c in $candidates) {
        if ($c -and (Test-Path -LiteralPath $c)) { return (Resolve-Path -LiteralPath $c).Path }
    }
    return $null
}

$log = Find-LatestLog
if (-not $log) {
    Write-Host "未找到 Latest.log。请用 -LogFile 指定，或先开 DiagLog=true 进游戏走一圈。" -ForegroundColor Red
    exit 1
}
Write-Host "日志: $log" -ForegroundColor Cyan

# ── 解析 [作用域探针] ──
# 格式示例:
# [作用域探针]   1 号容器：id=<空id>  类=GameInventory  @229DDF2F800  槽位=<不在EmporiumEntry内>  所属=backpack_medium  内含=…  角色=落点堆所在  白名单内=否  黑名单内=否
$pattern = '\[作用域探针\]\s*\d+\s*号容器：id=(?<id>\S+)\s+类=\S+\s+@(?<ptr>\S+)\s+槽位=(?<slot>[^ ]+(?:\s[^ ]+)*?)\s+所属=(?<owner>[^ ]+(?:\s[^ ]+)*?)\s+内含='

$seen = [ordered]@{}
$lines = Get-Content -LiteralPath $log -Encoding UTF8 | Where-Object { $_ -match '\[作用域探针\]' }
if (-not $lines) {
    Write-Host "日志里没有 [作用域探针] 行。请确认：DiagLog=true，且在游戏里拖过物品触发闸门。" -ForegroundColor Yellow
    Write-Host "（下方仍输出静态备查表，可直接抄）" -ForegroundColor DarkGray
}

foreach ($line in $lines) {
    if ($line -notmatch $pattern) { continue }
    $id    = $Matches['id'].Trim()
    $slot  = $Matches['slot'].Trim()
    $owner = $Matches['owner'].Trim()
    $ptr   = $Matches['ptr']

    # 内含留空时 owner/slot 可能粘连，二次清理：取到第一个已知字段边界
    # 已知 slot 值以「<」或中文名开头；owner 若含空格取第一段（多层链只用最近一层做 token）
    $owner1 = ($owner -split '\s+<\s+|\s+')[0]
    if ($owner1 -eq '<顶层容器>') { $owner1 = "" }

    $key = "$id|$slot|$owner1|$ptr"
    if ($seen.Contains($key)) { continue }
    $seen[$key] = [pscustomobject]@{
        Id     = $id
        Slot   = $slot
        Owner  = $owner1
        Ptr    = $ptr
        Tokens = @()
    }
}

# ── 生成 token ──
function Get-Tokens($entry) {
    $tokens = @()
    if ($entry.Slot -and $entry.Slot -notmatch '^<') {
        # 跳过「非交易场景」「<不在EmporiumEntry内>」「<未知槽位>」
        if ($entry.Slot -notin @('非交易场景','<不在EmporiumEntry内>','<未知槽位>')) {
            $tokens += "slot:$($entry.Slot)"
        }
    }
    if ($entry.Owner -and $entry.Owner -ne '<无id物品>') {
        $tokens += "owner:$($entry.Owner)"
    }
    if ($entry.Id -and $entry.Id -ne '<空id>' -and $entry.Id -ne '<空>') {
        $tokens += $entry.Id
    }
    return $tokens
}

foreach ($k in $seen.Keys) {
    $seen[$k] | Add-Member -NotePropertyName Tokens -NotePropertyValue (Get-Tokens $seen[$k]) -Force
}

# ── 分类 ──
$boxIds = @(
    'sec_box','evidence_box','storage_bay','storage_bay_large','smuggler_bay','smuggler_bay_mod',
    'machine_bay','machine_bay_ext','service_box','med_box','eng_box','storage_bay_chem',
    'storage_bay_gun','smuggler_bay_mini','makeshift_storage_bay','expedition_box',
    'custom_storage_box','void_bead_storage','wage_gu_machine','wage_ai_generator',
    'animal_cage','rat_cage','fridge','recharger','vacuum_robot',
    'furnace','desequencer','security_alarm','mirage_projector','water_purifier',
    'moisture_farm','wine_rack','bottle_printer'
)
$backpackIds = @(
    'backpack_small','backpack_medium','backpack_large','satchel','fanny_pack',
    'backpack_medium_military','backpack_large_military'
)

function Test-Category($entry) {
    switch -Regex ($Category) {
        '^Box$'      { return ($entry.Owner -in $boxIds) -or ($entry.Id -in $boxIds) }
        '^Backpack$' { return ($entry.Owner -in $backpackIds) -or ($entry.Id -in $backpackIds) }
        '^Slot$'     { return $entry.Tokens -match '^slot:' }
        '^Owner$'    { return $entry.Tokens -match '^owner:' }
        '^Bare$'     { return ($entry.Id -and $entry.Id -ne '<空id>' -and $entry.Id -ne '<空>') }
        default      { return $true }
    }
}

# ── 输出 ──
Write-Host "`n═══ 探针容器 → 可用 token ═══" -ForegroundColor Green
$allTokens = [System.Collections.Generic.List[string]]::new()
foreach ($k in $seen.Keys) {
    $e = $seen[$k]
    if (-not (Test-Category $e)) { continue }
    $tk = if ($e.Tokens.Count) { $e.Tokens -join '  |  ' } else { '（无可用 token —— 可能是垃圾桶/档案箱/磁带机等顶层家具）' }
    Write-Host ("  @{0}  id={1,-18} 槽位={2,-16} 所属={3,-24} → {4}" -f $e.Ptr, $e.Id, $e.Slot, $e.Owner, $tk)
    foreach ($t in $e.Tokens) { if ($t -notin $allTokens) { $allTokens.Add($t) } }
}

if ($allTokens.Count -eq 0) {
    Write-Host "`n（日志里暂无探针 token，跳过推荐段，直接看下方静态备查）" -ForegroundColor Yellow
}

# ── 推荐白名单（容器套件箱/背包 + 主仓库/展示柜）──
Write-Host "`n═══ 推荐 ContainerWhitelist（容器套件箱/背包 + 主仓库/展示柜）═══" -ForegroundColor Green
$recommended = [System.Collections.Generic.List[string]]::new()
foreach ($k in $seen.Keys) {
    $e = $seen[$k]
    $isSuite = ($e.Owner -in ($boxIds + $backpackIds)) -or ($e.Id -in ($boxIds + $backpackIds))
    $isShopSlot = $e.Slot -in @('背包(主仓库)','展示柜','海报暗格')
    if ($isSuite -or $isShopSlot) {
        foreach ($t in $e.Tokens) {
            # 优先 owner:，其次 slot:，最后裸 id
            if ($t -match '^owner:' -or $t -match '^slot:' -or ($t -and $t -notmatch '[:：]')) {
                if ($t -notin $recommended) { $recommended.Add($t) }
            }
        }
    }
}
$wl = ($recommended | Sort-Object -Unique) -join ','
Write-Host $wl -ForegroundColor White
Write-Host "`n粘进 cfg [ItemStacking] 段（含逗号务必带引号）：" -ForegroundColor DarkGray
Write-Host "  ContainerWhitelist = `"$wl`"" -ForegroundColor Yellow

if ($IncludeBlacklistSuggest) {
    Write-Host "`n═══ 推荐 ContainerBlacklist（柜台/已售/黑市 —— 禁堆）═══" -ForegroundColor Green
    $bl = [System.Collections.Generic.List[string]]::new()
    foreach ($k in $seen.Keys) {
        $e = $seen[$k]
        if ($e.Slot -in @('柜台前格','柜台后格','柜台三格','已售格','黑市左侧')) {
            foreach ($t in $e.Tokens) { if ($t -match '^slot:' -and $t -notin $bl) { $bl.Add($t) } }
        }
    }
    if ($bl.Count) {
        $bls = ($bl | Sort-Object -Unique) -join ','
        Write-Host "  ContainerBlacklist = `"$bls`"" -ForegroundColor Yellow
    } else {
        Write-Host "  （没探到柜台槽位 —— 先进交易场景拖一下再跑）" -ForegroundColor DarkGray
    }
}

if ($CopyToClipboard) {
    Set-Clipboard -Value $wl
    Write-Host "`n已复制白名单到剪贴板。" -ForegroundColor Cyan
}

# ── 对照：容器套件已知 owner token 全表（不依赖日志，静态备查）──
Write-Host "`n═══ 静态备查：容器套件 → owner token 全表 ═══" -ForegroundColor Green
Write-Host "（即使日志没探到，这些 owner: 也可以先填上试）" -ForegroundColor DarkGray
$static = [ordered]@{
    '箱类 BoxIds'        = ($boxIds | ForEach-Object { "owner:$_" }) -join ','
    '背包 BackpackIds'   = ($backpackIds | ForEach-Object { "owner:$_" }) -join ','
    '交易槽 Emporium'    = 'slot:save_bag,slot:showcase_save_bag,slot:hidden_save_bag,slot:frontInv,slot:back_inv_save_bag,slot:backInvCounter,slot:sold_save_bag,slot:bazarInv'
    '基建(无owner,靠slot)' = '垃圾桶/档案箱/磁带机 GetParentItem 为 null，owner: 匹配不上；只能留空放行或等游戏给 id'
}
foreach ($cat in $static.Keys) {
    Write-Host "  [$cat]"
    Write-Host "    $($static[$cat])"
}

Write-Host "`n完成。" -ForegroundColor Green

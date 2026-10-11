# ItemStacking 部署脚本
# 用法（PowerShell 5.1）：
#   .\deploy.ps1            查看当前状态（只读，不动文件）
#   .\deploy.ps1 -Deploy    清理遗留 + 备份 + 部署 + 校验
#
# ★ 部署名统一用【英文】ItemStacking.dll（= 程序集名 = MelonPreferences 配置段名）：
#   历史曾用中文名 物品堆叠.dll，0.9.x 起改回英文，原因有二 ——
#     ① PowerShell 5.1 的命令行参数按 ANSI(GBK) 解码，而脚本文件按 BOM 声明的编码读取；
#        中文文件名直接写在命令行里会被解成乱码 → 报「找不到路径」，
#        且 Copy-Item 会因「源不存在」把备份和部署一起静默跳过。
#     ② 中文名与英文名容易同时留在 Mods/ 根目录（两轮部署各留一份），
#        MelonLoader 会把两份都加载 ⇒ 同一个 mod 跑两遍（补丁重复 / 日志重复 / 行为不确定）。
#   ⇒ 本脚本每次部署都会自动把遗留的中文名备份后清掉。
#
# ★ 本文件必须存成 UTF8 **带 BOM**：脚本里有中文（历史文件名 / 提示文案），
#   无 BOM 时 PowerShell 5.1 会按 GBK 读，中文变乱码且路径匹配失败。

[CmdletBinding()]
param(
    [switch]$Deploy
)

$ErrorActionPreference = 'Stop'

$GameDir = 'D:\SteamLibrary\steamapps\common\Probably Stolen Demo'
$ModsDir = Join-Path $GameDir 'Mods'
$BuiltDll = 'D:\games\open code work\Item stacking\ItemStacking\bin\Release\ItemStacking.dll'
# 部署名：MelonLoader 按程序集名加载，ItemStacking.dll = 程序集名 = MelonPreferences 配置段名
#（0.9.x 起由中文名 物品堆叠.dll 改回英文名，理由见文件头注释）
$DeployName = 'ItemStacking.dll'
$DeployPath = Join-Path $ModsDir $DeployName
# 历史遗留的中文部署名：仍留在 Mods/ 根目录会造成同一个 mod 加载两份，部署时自动备份并清掉
$LegacyDeployPath = Join-Path $ModsDir '物品堆叠.dll'

function Get-Md5([string]$path) {
    (Get-FileHash -LiteralPath $path -Algorithm MD5).Hash
}

Write-Host '=== ItemStacking 部署 ===' -ForegroundColor Cyan
Write-Host ("Mods 目录 : " + $ModsDir)
Write-Host ("部署目标   : " + $DeployPath)

# ── 现状 ──────────────────────────────────────────────────────────────
if (-not (Test-Path -LiteralPath $ModsDir)) { throw "Mods 目录不存在：$ModsDir" }

Write-Host ''
Write-Host '--- Mods 根目录里的本 mod 相关文件 ---' -ForegroundColor Yellow
$related = Get-ChildItem -LiteralPath $ModsDir -File |
    Where-Object { $_.Length -eq 0 -or $_.Name -like '*堆叠*' -or $_.Name -like 'ItemStacking*' }
if (-not $related) { Write-Host '  （无）' }
foreach ($f in $related) {
    $md5 = try { Get-Md5 $f.FullName } catch { '读取失败' }
    Write-Host ("  {0,-10} {1:MM-dd HH:mm}  {2}  {3}" -f $f.Length, $f.LastWriteTime, $md5, $f.Name)
}

if (Test-Path -LiteralPath $BuiltDll) {
    Write-Host ''
    Write-Host '--- 待部署产物 ---' -ForegroundColor Yellow
    $bi = Get-Item -LiteralPath $BuiltDll
    Write-Host ("  {0}  {1:MM-dd HH:mm}  {2}  {3}" -f $bi.Length, $bi.LastWriteTime, (Get-Md5 $BuiltDll), $bi.Name)
} else {
    Write-Warning "未找到编译产物：$BuiltDll（请先 dotnet build）"
}

# ── 仅查看则到此为止 ──────────────────────────────────────────────────
if (-not $Deploy) {
    Write-Host ''
    Write-Host '（只读模式，未改动任何文件。加 -Deploy 执行备份+部署）' -ForegroundColor DarkGray
    return
}

# ── 部署 ──────────────────────────────────────────────────────────────
if (-not (Test-Path -LiteralPath $BuiltDll)) { throw "未找到编译产物：$BuiltDll" }

# 0) 前置检查：游戏在运行时 DLL 被占用，Copy-Item 会抛 IOException。
#    ★ 必须在【任何写操作之前】检查 —— 否则会先把遗留文件清掉、备份也做了，
#      最后复制失败，留下「清理了一半」的半成品状态。
$gameProcs = @(Get-Process -ErrorAction SilentlyContinue |
               Where-Object { $_.ProcessName -like 'Probably Stolen*' })
if ($gameProcs.Count -gt 0) {
    throw ("游戏正在运行（PID " + ($gameProcs.Id -join ',') + "），Mods 里的 DLL 被占用无法覆盖。" +
           "`n请先退出游戏再重新执行本脚本；本次未改动任何文件。")
}

Write-Host ''
Write-Host '--- 部署 ---' -ForegroundColor Yellow

# 1) 清理历史遗留的中文部署名（备份后删除）
#    为什么要清：Mods/ 根目录里同时存在 ItemStacking.dll 与 物品堆叠.dll 时，
#    MelonLoader 会把两份都当本 mod 加载 ⇒ 补丁打两遍 / 日志重复 / 行为不确定。
#    （0.9.x 之前一直部署中文名，不清理必然两份并存。）
if (Test-Path -LiteralPath $LegacyDeployPath) {
    $stamp0 = Get-Date -Format 'yyyyMMdd-HHmmss'
    $legacyBak = "$LegacyDeployPath.bak-$stamp0"
    Copy-Item -LiteralPath $LegacyDeployPath -Destination $legacyBak -Force
    Remove-Item -LiteralPath $LegacyDeployPath -Force
    Write-Host ("  已清理遗留中文名 -> 备份 " + (Split-Path $legacyBak -Leaf))
}

# 2) 备份现有文件（不存在则跳过，不视为错误）
if (Test-Path -LiteralPath $DeployPath) {
    $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $bak = "$DeployPath.bak-$stamp"
    Copy-Item -LiteralPath $DeployPath -Destination $bak -Force
    Write-Host ("  已备份 -> " + (Split-Path $bak -Leaf))
} else {
    Write-Host '  无现有文件，跳过备份（首次部署）'
}

# 3) 复制
try {
    Copy-Item -LiteralPath $BuiltDll -Destination $DeployPath -Force
} catch {
    throw ("部署失败（目标被占用？游戏可能刚被启动）：" + $_.Exception.Message)
}
Write-Host ('  已部署 ' + $DeployName)

# 4) 校验：比对 md5，不一致立即报错（静默失败是之前踩过的坑）
$srcMd5 = Get-Md5 $BuiltDll
$dstMd5 = Get-Md5 $DeployPath
if ($srcMd5 -ne $dstMd5) {
    throw "部署校验失败：源 $srcMd5 != 目标 $dstMd5"
}
Write-Host ("  校验通过 md5 = " + $dstMd5)

# 5) 并存风险自检：Mods 根目录里【任何】看起来是本 mod 的其它 dll 都会造成重复加载。
#    判据不只看 md5（内容不同同样是重复加载，旧版残留就是这种），
#    也看文件名特征（ItemStacking* / *堆叠*）。
$dupes = Get-ChildItem -LiteralPath $ModsDir -File |
    Where-Object { $_.Name -ne $DeployName -and $_.Extension -eq '.dll' -and
                   ($_.Name -like 'ItemStacking*' -or $_.Name -like '*堆叠*') }
if ($dupes) {
    Write-Host ''
    Write-Host '⚠ 以下文件看起来是本 mod 的其它副本（会造成重复加载/不确定）：' -ForegroundColor Red
    foreach ($d in $dupes) {
        # ⚠ 不能用三元表达式 ? : —— PowerShell 5.1 不支持（7.0+ 才有）
        $same = '（内容不同 = 旧版残留）'
        if ((Get-Md5 $d.FullName) -eq $dstMd5) { $same = '（与部署产物内容相同）' }
        Write-Host ("  · " + $d.Name + ' ' + $same)
    }
    Write-Host '  建议删除，只保留一份。' -ForegroundColor Red
}

Write-Host ''
Write-Host '完成。启动游戏后确认日志出现「Item Stacking 已加载：…」' -ForegroundColor Green
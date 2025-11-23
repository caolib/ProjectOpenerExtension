# PowerToys 扩展热重载脚本
# 使用 x-cmdpal://reload 快速重载,无需重启 PowerToys

param(
    [switch]$SkipBuild = $false
)

$ErrorActionPreference = "Stop"
$PackageName = "ProjectOpenerExtension_0.0.1.0_x64__8wekyb3d8bbwe"
$ProjectPath = "$PSScriptRoot\ProjectOpenerExtension\ProjectOpenerExtension.csproj"
$MsixPath = "$PSScriptRoot\ProjectOpenerExtension\AppPackages\ProjectOpenerExtension_0.0.1.0_x64_Debug_Test\ProjectOpenerExtension_0.0.1.0_x64_Debug.msix"

Write-Host "=== 🔥 热重载模式 ===" -ForegroundColor Cyan
Write-Host ""

# 1. 卸载旧版本
Write-Host "[1/4] 卸载旧版本..." -ForegroundColor Yellow
$package = Get-AppxPackage -Name "ProjectOpenerExtension" -ErrorAction SilentlyContinue
if ($package) {
    Remove-AppxPackage -Package $PackageName -ErrorAction SilentlyContinue
    Write-Host "✓ 旧版本已卸载" -ForegroundColor Green
} else {
    Write-Host "✓ 未找到旧版本" -ForegroundColor Green
}

# 2. 构建新版本
if (-not $SkipBuild) {
    Write-Host "[2/4] 构建项目..." -ForegroundColor Yellow
    dotnet build $ProjectPath -c Debug -r win-x64 /p:Platform=x64 /p:GenerateAppxPackageOnBuild=true /v:minimal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ 构建失败" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ 构建成功" -ForegroundColor Green
} else {
    Write-Host "[2/4] 跳过构建" -ForegroundColor Gray
}

# 3. 安装新版本
Write-Host "[3/4] 安装新版本..." -ForegroundColor Yellow
if (Test-Path $MsixPath) {
    Add-AppxPackage -Path $MsixPath
    Write-Host "✓ 新版本已安装" -ForegroundColor Green
} else {
    Write-Host "✗ 找不到 MSIX 包: $MsixPath" -ForegroundColor Red
    exit 1
}

# 4. 触发热重载
Write-Host "[4/4] 触发热重载..." -ForegroundColor Yellow
Start-Sleep -Milliseconds 500
Start-Process "x-cmdpal://reload"
Write-Host "✓ 已发送重载命令" -ForegroundColor Green

Write-Host ""
Write-Host "=== 🎉 热重载完成! ===" -ForegroundColor Cyan
Write-Host "扩展已更新,无需重启 PowerToys!" -ForegroundColor Green
Write-Host "按 Alt+Space 打开命令面板测试您的更改" -ForegroundColor Yellow

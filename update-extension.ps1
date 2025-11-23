# PowerToys 扩展快速更新脚本
# 自动卸载旧版本、构建、安装新版本并重启 PowerToys

param(
    [switch]$SkipBuild = $false,
    [switch]$SkipRestart = $false
)

$ErrorActionPreference = "Stop"
$PackageName = "ProjectOpenerExtension_0.0.1.0_x64__8wekyb3d8bbwe"
$ProjectPath = "$PSScriptRoot\ProjectOpenerExtension\ProjectOpenerExtension.csproj"
$MsixPath = "$PSScriptRoot\ProjectOpenerExtension\AppPackages\ProjectOpenerExtension_0.0.1.0_x64_Debug_Test\ProjectOpenerExtension_0.0.1.0_x64_Debug.msix"

Write-Host "=== PowerToys 扩展更新脚本 ===" -ForegroundColor Cyan
Write-Host ""

# 1. 关闭 PowerToys
Write-Host "[1/5] 关闭 PowerToys..." -ForegroundColor Yellow
$powertoysProcess = Get-Process -Name "PowerToys" -ErrorAction SilentlyContinue
if ($powertoysProcess) {
    Stop-Process -Name "PowerToys" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Write-Host "✓ PowerToys 已关闭" -ForegroundColor Green
} else {
    Write-Host "✓ PowerToys 未运行" -ForegroundColor Green
}

# 2. 卸载旧版本
Write-Host "[2/5] 卸载旧版本..." -ForegroundColor Yellow
$package = Get-AppxPackage -Name "ProjectOpenerExtension" -ErrorAction SilentlyContinue
if ($package) {
    Remove-AppxPackage -Package $PackageName -ErrorAction SilentlyContinue
    Write-Host "✓ 旧版本已卸载" -ForegroundColor Green
} else {
    Write-Host "✓ 未找到旧版本" -ForegroundColor Green
}

# 3. 构建新版本
if (-not $SkipBuild) {
    Write-Host "[3/5] 构建项目..." -ForegroundColor Yellow
    dotnet build $ProjectPath -c Debug -r win-x64 /p:Platform=x64 /p:GenerateAppxPackageOnBuild=true /v:minimal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ 构建失败" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ 构建成功" -ForegroundColor Green
} else {
    Write-Host "[3/5] 跳过构建" -ForegroundColor Gray
}

# 4. 安装新版本
Write-Host "[4/5] 安装新版本..." -ForegroundColor Yellow
if (Test-Path $MsixPath) {
    Add-AppxPackage -Path $MsixPath
    Write-Host "✓ 新版本已安装" -ForegroundColor Green
} else {
    Write-Host "✗ 找不到 MSIX 包: $MsixPath" -ForegroundColor Red
    exit 1
}

# 5. 重启 PowerToys
if (-not $SkipRestart) {
    Write-Host "[5/5] 重启 PowerToys..." -ForegroundColor Yellow
    $powertoysPath = "C:\Program Files\PowerToys\PowerToys.exe"
    if (Test-Path $powertoysPath) {
        Start-Process $powertoysPath
        Write-Host "✓ PowerToys 已重启" -ForegroundColor Green
    } else {
        Write-Host "! PowerToys 路径未找到,请手动启动" -ForegroundColor Yellow
    }
} else {
    Write-Host "[5/5] 跳过重启" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=== 更新完成! ===" -ForegroundColor Cyan
Write-Host "按 Alt+Space 打开命令面板测试您的更改" -ForegroundColor Green

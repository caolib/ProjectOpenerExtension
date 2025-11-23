# 快速重建脚本 - 只构建不安装
# 用于检查编译错误

param(
    [string]$Configuration = "Debug"
)

$ProjectPath = "$PSScriptRoot\ProjectOpenerExtension\ProjectOpenerExtension.csproj"

Write-Host "=== 快速构建 ===" -ForegroundColor Cyan
dotnet build $ProjectPath -c $Configuration -r win-x64 /p:Platform=x64 /p:GenerateAppxPackageOnBuild=false

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✓ 构建成功! 运行 .\update-extension.ps1 -SkipBuild 来快速安装" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "✗ 构建失败,请修复错误" -ForegroundColor Red
}

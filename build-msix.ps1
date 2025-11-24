# ============================================
# MSIX 包构建脚本
# 用于构建 Microsoft Store 发布包
# ============================================

param(
    [string]$Configuration = "Release",
    [string]$Platform = "x64",
    [switch]$Clean = $true
)

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  MSIX 包构建脚本" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$ProjectDir = "$PSScriptRoot\ProjectOpenerExtension"
$ProjectFile = "$ProjectDir\ProjectOpenerExtension.csproj"

# 显示构建配置
Write-Host "构建配置：" -ForegroundColor Yellow
Write-Host "  Configuration: $Configuration" -ForegroundColor White
Write-Host "  Platform: $Platform" -ForegroundColor White
Write-Host "  项目路径: $ProjectDir" -ForegroundColor White
Write-Host ""

# 验证项目文件存在
if (-not (Test-Path $ProjectFile)) {
    Write-Host "❌ 错误：找不到项目文件" -ForegroundColor Red
    Write-Host "   路径: $ProjectFile" -ForegroundColor Gray
    exit 1
}

# 清理旧的构建
if ($Clean) {
    Write-Host "清理旧的构建输出..." -ForegroundColor Yellow
    
    $pathsToClean = @(
        "$ProjectDir\bin\$Configuration",
        "$ProjectDir\obj\$Configuration",
        "$ProjectDir\AppPackages"
    )
    
    foreach ($path in $pathsToClean) {
        if (Test-Path $path) {
            Remove-Item -Path $path -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "  ✓ 已清理: $path" -ForegroundColor Gray
        }
    }
    
    Write-Host "✅ 清理完成" -ForegroundColor Green
    Write-Host ""
}

# 显示包标识信息
Write-Host "包标识信息：" -ForegroundColor Cyan
Write-Host "  Name: caolib.ProjectOpenerExtension" -ForegroundColor White
Write-Host "  Publisher: CN=1CAC90B8-3709-4D70-847A-683B7D151D03" -ForegroundColor White
Write-Host "  DisplayName: caolib.ProjectOpenerExtension" -ForegroundColor White
Write-Host "  PublisherDisplayName: caolib" -ForegroundColor White
Write-Host ""

# 开始构建
Write-Host "开始构建 MSIX 包..." -ForegroundColor Yellow
Write-Host ""

$buildStartTime = Get-Date

try {
    dotnet build $ProjectFile `
        --configuration $Configuration `
        -p:Platform=$Platform `
        -p:GenerateAppxPackageOnBuild=true `
        --verbosity minimal
    
    if ($LASTEXITCODE -ne 0) {
        throw "构建失败，退出码: $LASTEXITCODE"
    }
    
    $buildEndTime = Get-Date
    $buildDuration = ($buildEndTime - $buildStartTime).TotalSeconds
    
    Write-Host ""
    Write-Host "✅ 构建成功！" -ForegroundColor Green
    Write-Host "   耗时: $([math]::Round($buildDuration, 1)) 秒" -ForegroundColor Gray
    Write-Host ""
    
    # 查找生成的 MSIX 包
    Write-Host "查找 MSIX 包..." -ForegroundColor Yellow
    
    $msixFiles = Get-ChildItem -Path "$ProjectDir\AppPackages" -Recurse -Filter "*.msix" -ErrorAction SilentlyContinue | 
        Where-Object { $_.Name -notlike "*Debug*" -and $_.FullName -notlike "*\obj\*" }
    
    if ($msixFiles) {
        Write-Host ""
        Write-Host "📦 找到 $($msixFiles.Count) 个 MSIX 包：" -ForegroundColor Green
        Write-Host ""
        
        foreach ($msix in $msixFiles) {
            $sizeMB = [math]::Round($msix.Length / 1MB, 2)
            Write-Host "  文件名: $($msix.Name)" -ForegroundColor Cyan
            Write-Host "  大小: $sizeMB MB" -ForegroundColor White
            Write-Host "  路径: $($msix.Directory.FullName)" -ForegroundColor Gray
            Write-Host ""
        }
        
        # 显示第一个包的完整路径
        $mainMsix = $msixFiles[0]
        Write-Host "============================================" -ForegroundColor Cyan
        Write-Host "  构建完成！" -ForegroundColor Green
        Write-Host "============================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "📤 MSIX 包位置：" -ForegroundColor Yellow
        Write-Host "   $($mainMsix.FullName)" -ForegroundColor White
        Write-Host ""
        Write-Host "下一步：" -ForegroundColor Yellow
        Write-Host "  1. 打开 https://partner.microsoft.com/dashboard" -ForegroundColor White
        Write-Host "  2. 进入应用提交页面" -ForegroundColor White
        Write-Host "  3. 在'程序包'部分上传此 MSIX 文件" -ForegroundColor White
        Write-Host ""
        Write-Host "⚠️  注意：本地签名失败是正常的，Microsoft Store 会重新签名" -ForegroundColor Yellow
        Write-Host ""
        
    } else {
        Write-Host "⚠️  警告：未找到 MSIX 包文件" -ForegroundColor Yellow
        Write-Host "   检查构建日志以获取更多信息" -ForegroundColor Gray
        Write-Host ""
    }
    
} catch {
    Write-Host ""
    Write-Host "❌ 构建失败！" -ForegroundColor Red
    Write-Host "   错误: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "提示：" -ForegroundColor Yellow
    Write-Host "  1. 确保已安装 .NET 9 SDK" -ForegroundColor White
    Write-Host "  2. 检查 Package.appxmanifest 配置是否正确" -ForegroundColor White
    Write-Host "  3. 查看上方的详细错误信息" -ForegroundColor White
    Write-Host ""
    exit 1
}

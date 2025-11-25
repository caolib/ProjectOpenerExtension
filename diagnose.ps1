# ProjectOpenerExtension 诊断脚本
# 用于诊断配置文件和编辑器检测问题

$ErrorActionPreference = "Continue"

Write-Host "====== ProjectOpenerExtension 诊断工具 ======" -ForegroundColor Cyan
Write-Host ""

# 1. 系统信息
Write-Host "[1] 系统信息" -ForegroundColor Yellow
Write-Host "  Windows 版本: $([Environment]::OSVersion.Version)"
Write-Host "  PowerShell 版本: $($PSVersionTable.PSVersion)"
Write-Host "  当前用户: $env:USERNAME"
Write-Host ""

# 2. 环境变量检查
Write-Host "[2] 环境变量" -ForegroundColor Yellow
Write-Host "  USERPROFILE: $env:USERPROFILE"
Write-Host "  LOCALAPPDATA: $env:LOCALAPPDATA"
Write-Host "  APPDATA: $env:APPDATA"
Write-Host "  PACKAGE_FAMILY_NAME: $env:PACKAGE_FAMILY_NAME"
Write-Host ""

# 3. 配置文件检查
Write-Host "[3] 配置文件位置" -ForegroundColor Yellow

# 检查 MSIX 虚拟化路径
$package = Get-AppxPackage -Name "*ProjectOpenerExtension*" -ErrorAction SilentlyContinue
if ($package) {
    $msixConfig = "$env:LOCALAPPDATA\Packages\$($package.PackageFamilyName)\LocalCache\Local\ProjectOpenerExtension\editors.json"
    Write-Host "  MSIX 配置 (虚拟化): $msixConfig"
} else {
    $msixConfig = "$env:LOCALAPPDATA\ProjectOpenerExtension\editors.json"
    Write-Host "  MSIX 配置 (标准): $msixConfig"
}

if (Test-Path $msixConfig) {
    $fileInfo = Get-Item $msixConfig
    Write-Host "    ✓ 文件存在" -ForegroundColor Green
    Write-Host "    大小: $($fileInfo.Length) bytes"
    Write-Host "    创建时间: $($fileInfo.CreationTime)"
    Write-Host "    修改时间: $($fileInfo.LastWriteTime)"
    
    try {
        $content = Get-Content $msixConfig -Raw | ConvertFrom-Json
        Write-Host "    编辑器数量: $($content.Count)" -ForegroundColor Green
        foreach ($editor in $content) {
            Write-Host "      - $($editor.Name) ($($editor.EditorType))" -ForegroundColor Cyan
        }
    } catch {
        Write-Host "    ✗ JSON 解析失败: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "    ✗ 文件不存在" -ForegroundColor Red
    $dir = Split-Path $msixConfig
    if (Test-Path $dir) {
        Write-Host "    目录存在,但文件缺失" -ForegroundColor Yellow
        Write-Host "    目录内容:"
        Get-ChildItem $dir | ForEach-Object {
            Write-Host "      - $($_.Name)"
        }
    } else {
        Write-Host "    目录也不存在" -ForegroundColor Red
    }
}

Write-Host ""

$standaloneConfig = "$env:USERPROFILE\.config\ProjectOpenerExtension\editors.json"
Write-Host "  独立配置: $standaloneConfig"
if (Test-Path $standaloneConfig) {
    $fileInfo = Get-Item $standaloneConfig
    Write-Host "    ✓ 文件存在" -ForegroundColor Green
    Write-Host "    大小: $($fileInfo.Length) bytes"
    Write-Host "    创建时间: $($fileInfo.CreationTime)"
    Write-Host "    修改时间: $($fileInfo.LastWriteTime)"
    
    try {
        $content = Get-Content $standaloneConfig -Raw | ConvertFrom-Json
        Write-Host "    编辑器数量: $($content.Count)" -ForegroundColor Green
        foreach ($editor in $content) {
            Write-Host "      - $($editor.Name) ($($editor.EditorType))" -ForegroundColor Cyan
        }
    } catch {
        Write-Host "    ✗ JSON 解析失败: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "    ✗ 文件不存在" -ForegroundColor Red
}

Write-Host ""

# 4. 编辑器检测
Write-Host "[4] 编辑器检测" -ForegroundColor Yellow

# VS Code 检测
$vscodePaths = @(
    "$env:LOCALAPPDATA\Programs\Microsoft VS Code\Code.exe",
    "$env:ProgramFiles\Microsoft VS Code\Code.exe",
    "${env:ProgramFiles(x86)}\Microsoft VS Code\Code.exe"
)

Write-Host "  VS Code:"
$vscodeFound = $false
foreach ($path in $vscodePaths) {
    if (Test-Path $path) {
        Write-Host "    ✓ 找到: $path" -ForegroundColor Green
        $vscodeFound = $true
        
        # 检查版本
        try {
            $version = (Get-Item $path).VersionInfo.FileVersion
            Write-Host "      版本: $version"
        } catch {}
        
        break
    }
}
if (-not $vscodeFound) {
    Write-Host "    ✗ 未找到 VS Code 可执行文件" -ForegroundColor Red
    Write-Host "      已检查路径:"
    foreach ($path in $vscodePaths) {
        Write-Host "        - $path"
    }
}

# VS Code Storage 检测
$vscodeStorage = "$env:APPDATA\Code\User\globalStorage\storage.json"
Write-Host "  VS Code Storage: $vscodeStorage"
if (Test-Path $vscodeStorage) {
    Write-Host "    ✓ 文件存在" -ForegroundColor Green
    $fileInfo = Get-Item $vscodeStorage
    Write-Host "    大小: $($fileInfo.Length) bytes"
    Write-Host "    修改时间: $($fileInfo.LastWriteTime)"
    
    # 尝试读取项目数量
    try {
        $storage = Get-Content $vscodeStorage -Raw | ConvertFrom-Json
        if ($storage.profileAssociations.workspaces) {
            $projectCount = ($storage.profileAssociations.workspaces | Get-Member -MemberType NoteProperty).Count
            Write-Host "    项目数量: $projectCount" -ForegroundColor Green
        }
    } catch {
        Write-Host "    警告: 无法解析 storage.json" -ForegroundColor Yellow
    }
} else {
    Write-Host "    ✗ 文件不存在" -ForegroundColor Red
}

Write-Host ""

# IntelliJ IDEA 检测
$jetbrainsPath = "$env:LOCALAPPDATA\JetBrains"
Write-Host "  JetBrains IDE:"
if (Test-Path $jetbrainsPath) {
    Write-Host "    ✓ 配置目录存在: $jetbrainsPath" -ForegroundColor Green
    
    $ideaDirs = Get-ChildItem $jetbrainsPath -Directory -Filter "IntelliJIdea*" -ErrorAction SilentlyContinue
    if ($ideaDirs) {
        Write-Host "    找到 IntelliJ IDEA 配置:"
        foreach ($dir in $ideaDirs) {
            Write-Host "      - $($dir.Name)" -ForegroundColor Cyan
            
            $recentProjects = Join-Path $dir.FullName "options\recentProjects.xml"
            if (Test-Path $recentProjects) {
                Write-Host "        ✓ recentProjects.xml 存在" -ForegroundColor Green
            } else {
                Write-Host "        ✗ recentProjects.xml 不存在" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "    未找到 IntelliJ IDEA 配置目录" -ForegroundColor Yellow
    }
} else {
    Write-Host "    ✗ JetBrains 配置目录不存在" -ForegroundColor Red
}

Write-Host ""

# 5. MSIX 包检查
Write-Host "[5] MSIX 包状态" -ForegroundColor Yellow
$package = Get-AppxPackage -Name "*ProjectOpenerExtension*" -ErrorAction SilentlyContinue
if ($package) {
    Write-Host "  ✓ MSIX 包已安装" -ForegroundColor Green
    Write-Host "    包名: $($package.Name)"
    Write-Host "    版本: $($package.Version)"
    Write-Host "    PackageFamilyName: $($package.PackageFamilyName)"
    Write-Host "    安装位置: $($package.InstallLocation)"
    Write-Host "    架构: $($package.Architecture)"
} else {
    Write-Host "  ✗ MSIX 包未安装" -ForegroundColor Red
    Write-Host "    这可能是开发环境或使用 EXE 安装"
}

Write-Host ""

# 6. PowerToys 检查
Write-Host "[6] PowerToys 状态" -ForegroundColor Yellow
$powertoysProcess = Get-Process -Name "PowerToys" -ErrorAction SilentlyContinue
if ($powertoysProcess) {
    Write-Host "  ✓ PowerToys 正在运行" -ForegroundColor Green
    Write-Host "    进程 ID: $($powertoysProcess.Id)"
    Write-Host "    路径: $($powertoysProcess.Path)"
} else {
    Write-Host "  ✗ PowerToys 未运行" -ForegroundColor Yellow
}

Write-Host ""

# 7. 建议操作
Write-Host "[7] 建议操作" -ForegroundColor Yellow

$issues = @()

if (-not (Test-Path $msixConfig) -and -not (Test-Path $standaloneConfig)) {
    $issues += "配置文件不存在"
    Write-Host "  ⚠ 配置文件缺失 - 运行 'Fix-Config' 修复" -ForegroundColor Yellow
}

if (-not $vscodeFound -and -not (Test-Path $jetbrainsPath)) {
    $issues += "未检测到任何编辑器"
    Write-Host "  ⚠ 未检测到编辑器 - 请确保已安装 VS Code 或 JetBrains IDE" -ForegroundColor Yellow
}

if ($issues.Count -eq 0) {
    Write-Host "  ✓ 未发现明显问题" -ForegroundColor Green
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# 提供快速修复选项
Write-Host "快速修复选项:" -ForegroundColor Cyan
Write-Host "  1. 创建示例配置文件"
Write-Host "  2. 重新检测编辑器"
Write-Host "  3. 保存诊断报告"
Write-Host "  0. 退出"
Write-Host ""

$choice = Read-Host "请选择操作 (0-3)"

switch ($choice) {
    "1" {
        Write-Host "`n创建示例配置文件..." -ForegroundColor Yellow
        
        # 决定使用哪个路径
        $targetPath = if ($package) { $msixConfig } else { $standaloneConfig }
        $targetDir = Split-Path $targetPath
        
        # 创建目录
        if (-not (Test-Path $targetDir)) {
            New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
        }
        
        # 创建示例配置
        $sampleConfig = @()
        
        # 如果找到 VS Code,添加配置
        if ($vscodeFound) {
            $vscodePath = $vscodePaths | Where-Object { Test-Path $_ } | Select-Object -First 1
            $sampleConfig += @{
                Name = "VS Code"
                Enabled = $true
                Icon = ""
                ExecutablePath = $vscodePath
                ProjectPath = $vscodeStorage
                EditorType = "vscode"
            }
        }
        
        # 如果找到 JetBrains,添加配置
        if (Test-Path $jetbrainsPath) {
            $sampleConfig += @{
                Name = "IntelliJ IDEA"
                Enabled = $true
                Icon = ""
                ExecutablePath = "C:\Program Files\JetBrains\IntelliJ IDEA\bin\idea64.exe"
                ProjectPath = $jetbrainsPath
                EditorType = "jetbrains"
            }
        }
        
        # 如果没有检测到任何编辑器,创建空配置
        if ($sampleConfig.Count -eq 0) {
            Write-Host "未检测到编辑器,创建空配置" -ForegroundColor Yellow
        }
        
        # 保存配置
        $json = $sampleConfig | ConvertTo-Json -Depth 10
        $json | Out-File $targetPath -Encoding UTF8
        
        if (Test-Path $targetPath) {
            Write-Host "✓ 配置文件已创建: $targetPath" -ForegroundColor Green
        } else {
            Write-Host "✗ 配置文件创建失败" -ForegroundColor Red
        }
    }
    
    "2" {
        Write-Host "`n重新运行编辑器检测..." -ForegroundColor Yellow
        Write-Host "请重启 PowerToys 以应用更改" -ForegroundColor Cyan
    }
    
    "3" {
        Write-Host "`n保存诊断报告..." -ForegroundColor Yellow
        $reportPath = "$env:TEMP\ProjectOpener-Diagnostic-$(Get-Date -Format 'yyyyMMdd-HHmmss').txt"
        
        # 重新运行诊断并保存
        & $PSCommandPath *> $reportPath
        
        Write-Host "✓ 诊断报告已保存到: $reportPath" -ForegroundColor Green
        Start-Process notepad.exe $reportPath
    }
    
    default {
        Write-Host "退出诊断工具" -ForegroundColor Gray
    }
}

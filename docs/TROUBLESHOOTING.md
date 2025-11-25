# 故障排除指南 / Troubleshooting Guide

## 常见问题 / Common Issues

### 1. 本地构建的 MSIX 包无法安装

**错误信息**: `无法验证此应用包的发布者证书 (0x800B010A)`

**原因**: 本地构建使用测试证书签名,Windows 无法验证

**解决方案**:

#### 选项 A: 使用热重载模式开发 (推荐)
```powershell
# 开发时使用热重载,无需每次重新安装
.\hot-reload.ps1
```

#### 选项 B: 信任开发证书
```powershell
# 1. 找到证书文件
$certFile = "ProjectOpenerExtension_TemporaryKey.pfx"

# 2. 导入到受信任的根证书存储
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($certFile, "test123")
$store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "LocalMachine")
$store.Open("ReadWrite")
$store.Add($cert)
$store.Close()
```

#### 选项 C: 从微软商店安装
- 生产环境使用从微软商店下载的版本,已正确签名

### 2. 从微软商店安装后配置文件未创建

**问题**: 只创建了文件夹,但没有 `editors.json` 文件

**原因**: 
- MSIX 应用运行在沙箱环境中
- 文件系统访问可能受限
- 编辑器检测可能失败

**诊断步骤**:

1. **检查配置文件位置**:
   ```powershell
   # MSIX 应用的配置路径
   $configPath = "$env:LOCALAPPDATA\ProjectOpenerExtension\editors.json"
   
   # 检查文件是否存在
   Test-Path $configPath
   
   # 检查目录内容
   Get-ChildItem (Split-Path $configPath)
   ```

2. **查看调试日志**:
   - 使用 [DebugView](https://learn.microsoft.com/en-us/sysinternals/downloads/debugview) 查看实时日志
   - 搜索 `[ProjectOpener]` 相关日志

3. **手动创建配置文件**:
   ```powershell
   # 创建配置目录
   $configDir = "$env:LOCALAPPDATA\ProjectOpenerExtension"
   New-Item -ItemType Directory -Path $configDir -Force
   
   # 创建示例配置文件
   $config = @(
       @{
           Name = "VS Code"
           Enabled = $true
           Icon = ""
           ExecutablePath = "$env:LOCALAPPDATA\Programs\Microsoft VS Code\Code.exe"
           ProjectPath = "$env:APPDATA\Code\User\globalStorage\storage.json"
           EditorType = "vscode"
       }
   ) | ConvertTo-Json -Depth 10
   
   # 保存配置
   $config | Out-File "$configDir\editors.json" -Encoding UTF8
   ```

### 3. 手动创建配置后仍无法识别项目

**可能原因**:

1. **编辑器路径不正确**
   ```powershell
   # 验证 VS Code 可执行文件路径
   $vscodePaths = @(
       "$env:LOCALAPPDATA\Programs\Microsoft VS Code\Code.exe",
       "$env:ProgramFiles\Microsoft VS Code\Code.exe"
   )
   
   foreach ($path in $vscodePaths) {
       if (Test-Path $path) {
           Write-Host "找到 VS Code: $path"
       }
   }
   ```

2. **Storage.json 路径不正确**
   ```powershell
   # 验证 VS Code storage.json 路径
   $storagePath = "$env:APPDATA\Code\User\globalStorage\storage.json"
   
   if (Test-Path $storagePath) {
       Write-Host "Storage 文件存在"
       # 查看内容
       Get-Content $storagePath | ConvertFrom-Json
   } else {
       Write-Host "Storage 文件不存在: $storagePath"
   }
   ```

3. **MSIX 应用缺少文件系统访问权限**
   
   从 Windows 设置中授予权限:
   - 打开 **设置** > **隐私和安全性** > **文件系统**
   - 找到 **ProjectOpenerExtension**
   - 开启 **文件系统访问** 权限

### 4. 配置文件路径说明

根据安装方式,配置文件位置不同:

| 安装方式            | 配置文件路径                                                |
| ------------------- | ----------------------------------------------------------- |
| **MSIX (微软商店)** | `%LOCALAPPDATA%\ProjectOpenerExtension\editors.json`        |
| **独立 EXE 安装**   | `%USERPROFILE%\.config\ProjectOpenerExtension\editors.json` |
| **开发调试**        | `%USERPROFILE%\.config\ProjectOpenerExtension\editors.json` |

### 5. 完整的配置示例

**VS Code**:
```json
{
  "Name": "VS Code",
  "Enabled": true,
  "Icon": "",
  "ExecutablePath": "C:\\Users\\YourUsername\\AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe",
  "ProjectPath": "C:\\Users\\YourUsername\\AppData\\Roaming\\Code\\User\\globalStorage\\storage.json",
  "EditorType": "vscode"
}
```

**IntelliJ IDEA**:
```json
{
  "Name": "IntelliJ IDEA",
  "Enabled": true,
  "Icon": "",
  "ExecutablePath": "C:\\Program Files\\JetBrains\\IntelliJ IDEA\\bin\\idea64.exe",
  "ProjectPath": "C:\\Users\\YourUsername\\AppData\\Local\\JetBrains",
  "EditorType": "jetbrains"
}
```

### 6. 获取详细诊断信息

使用 PowerShell 脚本收集诊断信息:

```powershell
# 诊断脚本
$report = @"
====== ProjectOpenerExtension 诊断报告 ======
时间: $(Get-Date)

1. 系统信息:
   - Windows 版本: $([Environment]::OSVersion.Version)
   - PowerShell 版本: $($PSVersionTable.PSVersion)

2. 配置文件检查:
"@

# 检查 MSIX 配置
$msixConfig = "$env:LOCALAPPDATA\ProjectOpenerExtension\editors.json"
$report += "`n   - MSIX 配置: $(if (Test-Path $msixConfig) { '✓ 存在' } else { '✗ 不存在' })"
if (Test-Path $msixConfig) {
    $report += "`n     大小: $((Get-Item $msixConfig).Length) bytes"
}

# 检查独立配置
$standaloneConfig = "$env:USERPROFILE\.config\ProjectOpenerExtension\editors.json"
$report += "`n   - 独立配置: $(if (Test-Path $standaloneConfig) { '✓ 存在' } else { '✗ 不存在' })"
if (Test-Path $standaloneConfig) {
    $report += "`n     大小: $((Get-Item $standaloneConfig).Length) bytes"
}

# 检查编辑器
$report += "`n`n3. 编辑器检查:"
$vscodePath = "$env:LOCALAPPDATA\Programs\Microsoft VS Code\Code.exe"
$report += "`n   - VS Code: $(if (Test-Path $vscodePath) { "✓ $vscodePath" } else { '✗ 未找到' })"

$vscodeStorage = "$env:APPDATA\Code\User\globalStorage\storage.json"
$report += "`n   - VS Code Storage: $(if (Test-Path $vscodeStorage) { '✓ 存在' } else { '✗ 不存在' })"

# 检查 MSIX 包
$report += "`n`n4. MSIX 包:"
$package = Get-AppxPackage -Name "*ProjectOpenerExtension*" -ErrorAction SilentlyContinue
if ($package) {
    $report += "`n   - 包名: $($package.Name)"
    $report += "`n   - 版本: $($package.Version)"
    $report += "`n   - 安装位置: $($package.InstallLocation)"
} else {
    $report += "`n   ✗ 未安装 MSIX 包"
}

$report += "`n`n============================================"

# 输出报告
Write-Host $report
$report | Out-File "$env:TEMP\ProjectOpener-Diagnostic.txt"
Write-Host "`n诊断报告已保存到: $env:TEMP\ProjectOpener-Diagnostic.txt"
```

## 联系支持

如果以上方法都无法解决问题,请:

1. 运行上述诊断脚本
2. 收集 DebugView 日志
3. 在 [GitHub Issues](https://github.com/caolib/ProjectOpenerExtension/issues) 提交问题
4. 附上诊断信息和日志

## 开发者调试

如果你是开发者,可以:

1. **启用详细日志**:
   - 使用 DebugView 查看 Debug.WriteLine 输出
   - 搜索 `[ProjectOpener]` 和 `[EditorDetection]` 标签

2. **手动测试配置加载**:
   ```csharp
   // 在代码中添加断点
   var configPath = GetConfigFilePath();
   var editors = LoadEditors();
   ```

3. **测试编辑器检测**:
   ```csharp
   var detected = EditorDetectionService.DetectInstalledEditors();
   foreach (var editor in detected) {
       Debug.WriteLine($"Detected: {editor.Name}");
   }
   ```

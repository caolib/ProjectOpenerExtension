# 修复说明 - Configuration & MSIX Issues

## 修复的问题

### 1. MSIX 应用配置文件路径问题

**问题**: 从微软商店安装后,只创建文件夹但没有创建配置文件

**根本原因**:
- MSIX 应用运行在沙箱环境
- 使用 `Environment.SpecialFolder.UserProfile` 在 MSIX 中可能被虚拟化
- 没有智能检测 MSIX vs 独立模式

**修复内容**:
- ✅ 添加 MSIX 环境检测 (通过 `PACKAGE_FAMILY_NAME` 环境变量)
- ✅ MSIX 模式使用 `LocalApplicationData`
- ✅ 独立模式使用 `UserProfile\.config`
- ✅ 增强的错误处理和详细日志
- ✅ 添加 `broadFileSystemAccess` 权限到 Package.appxmanifest

**代码变更**:
```csharp
// 修改前
var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
var configPath = Path.Combine(userProfile, ".config", "ProjectOpenerExtension", "editors.json");

// 修改后
var packageFamilyName = Environment.GetEnvironmentVariable("PACKAGE_FAMILY_NAME");
var isMsix = !string.IsNullOrEmpty(packageFamilyName);

if (isMsix) {
    var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    configPath = Path.Combine(localAppData, "ProjectOpenerExtension", "editors.json");
} else {
    var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    configPath = Path.Combine(userProfile, ".config", "ProjectOpenerExtension", "editors.json");
}
```

### 2. 配置文件初始化增强

**问题**: 配置文件创建失败但没有详细错误信息

**修复内容**:
- ✅ 添加详细的 Debug.WriteLine 日志
- ✅ 多重异常处理 (UnauthorizedAccessException, IOException)
- ✅ 文件创建验证
- ✅ 创建后立即验证文件存在性和大小

**日志示例**:
```
[ProjectOpener] ========== 开始初始化配置文件 ==========
[ProjectOpener] 目标路径: C:\Users\...\LocalApplicationData\ProjectOpenerExtension\editors.json
[ProjectOpener] 配置目录: C:\Users\...\LocalApplicationData\ProjectOpenerExtension
[ProjectOpener] ✓ 目录已存在
[ProjectOpener] 开始检测编辑器...
[ProjectOpener] ✓ 检测到 1 个编辑器
[ProjectOpener]   - VS Code (vscode)
[ProjectOpener] ✓ 配置文件创建成功!
[ProjectOpener]   文件大小: 256 字节
```

### 3. 编辑器检测增强

**问题**: 编辑器检测失败但没有诊断信息

**修复内容**:
- ✅ 添加详细的检测日志
- ✅ 检查更多 VS Code 安装路径 (User installer)
- ✅ 注册表搜索错误处理
- ✅ 每个检测步骤都有日志输出

### 4. 文件系统权限

**问题**: MSIX 应用无法访问用户文件

**修复内容**:
- ✅ 在 `Package.appxmanifest` 添加 `broadFileSystemAccess` capability
- ✅ 用户需要在 Windows 设置中授予权限

**Manifest 变更**:
```xml
<Capabilities>
  <Capability Name="internetClient" />
  <rescap:Capability Name="runFullTrust" />
  <rescap:Capability Name="broadFileSystemAccess" />  <!-- 新增 -->
</Capabilities>
```

### 5. 本地构建证书问题

**问题**: `无法验证此应用包的发布者证书 (0x800B010A)`

**解决方案**:

**开发环境** (推荐):
```powershell
# 使用热重载,无需重新安装
.\hot-reload.ps1
```

**信任开发证书**:
```powershell
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2("ProjectOpenerExtension_TemporaryKey.pfx", "test123")
$store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "LocalMachine")
$store.Open("ReadWrite")
$store.Add($cert)
$store.Close()
```

**生产环境**:
- 从微软商店安装 (已正确签名)

## 配置文件位置

| 环境                | 路径                                                        |
| ------------------- | ----------------------------------------------------------- |
| **MSIX (微软商店)** | `%LOCALAPPDATA%\ProjectOpenerExtension\editors.json`        |
| **独立 EXE**        | `%USERPROFILE%\.config\ProjectOpenerExtension\editors.json` |
| **开发调试**        | `%USERPROFILE%\.config\ProjectOpenerExtension\editors.json` |

## 新增工具

### 诊断脚本 (`diagnose.ps1`)

全面的诊断工具,检查:
- ✅ 系统信息
- ✅ 环境变量
- ✅ 配置文件位置和内容
- ✅ 编辑器检测
- ✅ MSIX 包状态
- ✅ PowerToys 状态

**使用方法**:
```powershell
.\diagnose.ps1
```

**功能**:
1. 创建示例配置文件
2. 重新检测编辑器
3. 保存诊断报告

### 故障排除文档 (`docs/TROUBLESHOOTING.md`)

详细的问题排查指南,包括:
- 常见问题和解决方案
- 配置文件示例
- PowerShell 诊断脚本
- 手动修复步骤

## 测试步骤

### 本地测试

1. **构建项目**:
   ```powershell
   dotnet build ProjectOpenerExtension/ProjectOpenerExtension.csproj -c Debug -r win-x64 /p:Platform=x64
   ```

2. **使用热重载测试**:
   ```powershell
   .\hot-reload.ps1
   ```

3. **检查日志**:
   - 下载 [DebugView](https://learn.microsoft.com/en-us/sysinternals/downloads/debugview)
   - 搜索 `[ProjectOpener]` 和 `[EditorDetection]`

4. **运行诊断**:
   ```powershell
   .\diagnose.ps1
   ```

### MSIX 测试

1. **构建 MSIX**:
   ```powershell
   .\build-msix.ps1
   ```

2. **安装测试包**:
   ```powershell
   Add-AppxPackage -Path "ProjectOpenerExtension\AppPackages\...\ProjectOpenerExtension_0.0.1.0_x64_Debug.msix"
   ```

3. **授予文件系统权限**:
   - 设置 > 隐私和安全性 > 文件系统
   - 找到 ProjectOpenerExtension
   - 开启文件系统访问

4. **测试功能**:
   - Alt+Space 打开 Command Palette
   - 输入 "Projects"
   - 验证项目列表

## 部署到生产

### 微软商店发布

1. **移除测试证书引用** (如果需要):
   ```xml
   <!-- 在 .csproj 中注释掉 -->
   <!-- <PackageCertificateKeyFile>..\ProjectOpenerExtension_TemporaryKey.pfx</PackageCertificateKeyFile> -->
   <!-- <PackageCertificatePassword>test123</PackageCertificatePassword> -->
   ```

2. **使用商店证书签名**:
   - Partner Center 会自动签名

3. **上传到商店**:
   - 上传 MSIX 包
   - 商店自动处理签名

### WinGet 发布

1. **构建 EXE 安装程序**:
   ```powershell
   .\build-exe.ps1
   ```

2. **更新 WinGet manifest**:
   ```yaml
   # winget/caolib.ProjectOpenerExtension.yaml
   PackageVersion: 0.0.2.0  # 更新版本号
   ```

## 验证清单

- [ ] 配置文件在 MSIX 模式下正确创建
- [ ] 配置文件在独立模式下正确创建
- [ ] VS Code 编辑器正确检测
- [ ] JetBrains 编辑器正确检测
- [ ] 项目列表正确显示
- [ ] 项目可以正确打开
- [ ] 热重载功能正常
- [ ] 诊断脚本正常运行
- [ ] 详细日志可以查看
- [ ] MSIX 包可以安装 (生产环境)
- [ ] 文件系统权限提示正常

## 后续改进建议

1. **自动权限请求**: 在首次运行时提示用户授予文件系统权限
2. **配置 UI**: 添加图形界面配置编辑器
3. **更多编辑器**: 添加 VSCodium, Cursor, Windsurf 等的自动检测
4. **云同步**: 支持配置文件云同步
5. **项目过滤**: 添加项目过滤和搜索功能

## 联系信息

- **GitHub**: https://github.com/caolib/ProjectOpenerExtension
- **Issues**: https://github.com/caolib/ProjectOpenerExtension/issues

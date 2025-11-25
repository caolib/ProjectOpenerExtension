# 修复总结 - MSIX 配置文件创建与证书签名问题

## 已解决的问题

### 1. ✅ 本地构建 MSIX 签名失败
**错误**: `APPX1204: Failed to sign. SignTool Error: An unexpected internal error has occurred`

**根本原因**: 
- 打包工具无法直接使用密码保护的 PFX 文件签名
- 证书未导入到当前用户证书存储

**解决方案**:
1. 将测试证书导入到当前用户证书存储:
   ```powershell
   $certPath='ProjectOpenerExtension_TemporaryKey.pfx'
   $pwd=ConvertTo-SecureString 'test123' -AsPlainText -Force
   Import-PfxCertificate -FilePath $certPath -CertStoreLocation Cert:\CurrentUser\My -Password $pwd -Exportable
   Import-Certificate -FilePath 'ProjectOpenerExtension.cer' -CertStoreLocation Cert:\CurrentUser\Root
   ```

2. 在 `.csproj` 中添加 `PackageCertificateThumbprint`:
   ```xml
   <PackageCertificateThumbprint>96A3BD23C4EBE41DEAEECC3BDDAE995B31C56BC8</PackageCertificateThumbprint>
   ```

**结果**: ✅ `.\hot-reload.ps1` 现在可以成功构建并安装 MSIX 包

---

### 2. ✅ MSIX 应用配置文件创建失败
**问题**: 从微软商店安装后只创建文件夹,没有创建配置文件

**根本原因**:
- 原代码使用 `PACKAGE_FAMILY_NAME` 环境变量检测 MSIX,但该变量在某些情况下为空
- MSIX 应用的 `LocalApplicationData` 会被虚拟化到特殊路径

**解决方案**:
增强 MSIX 环境检测,使用三种方法:

```csharp
private static bool IsMsixPackage()
{
    // 方法1: 检查 PACKAGE_FAMILY_NAME 环境变量
    var packageFamilyName = Environment.GetEnvironmentVariable("PACKAGE_FAMILY_NAME");
    if (!string.IsNullOrEmpty(packageFamilyName)) return true;

    // 方法2: 检查进程是否在 WindowsApps 目录下运行
    var processPath = Process.GetCurrentProcess().MainModule?.FileName;
    if (!string.IsNullOrEmpty(processPath) && processPath.Contains(@"\WindowsApps\")) return true;

    // 方法3: 检查是否存在 AppxManifest.xml
    var manifestPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppxManifest.xml");
    if (File.Exists(manifestPath)) return true;

    return false;
}
```

**实际路径**:
- **MSIX 应用**: `%LOCALAPPDATA%\Packages\{PackageFamilyName}\LocalCache\Local\ProjectOpenerExtension\editors.json`
- **独立应用**: `%USERPROFILE%\.config\ProjectOpenerExtension\editors.json`

**结果**: ✅ 配置文件现在能在 MSIX 虚拟化路径中正确创建

---

### 3. ✅ VS Code 编辑器检测增强
**问题**: 标准路径未找到 VS Code (用户可能使用 portable 版本或其他安装方式)

**解决方案**:
扩展检测路径列表:

```csharp
var possiblePaths = new[]
{
    Path.Combine(localAppData, "Programs", "Microsoft VS Code", "Code.exe"),
    Path.Combine(programFiles, "Microsoft VS Code", "Code.exe"),
    Path.Combine(programFilesX86, "Microsoft VS Code", "Code.exe"),
    // 用户安装路径
    Path.Combine(userProfile, "AppData", "Local", "Programs", "Microsoft VS Code", "Code.exe"),
    // Scoop 包管理器
    Path.Combine(userProfile, "scoop", "apps", "vscode", "current", "Code.exe"),
    // Portable 版本
    Path.Combine(userProfile, "Downloads", "VSCode-win32-x64", "Code.exe"),
    Path.Combine(userProfile, "VSCode", "Code.exe")
};
```

**结果**: ✅ 能检测到更多 VS Code 安装方式

---

## 验证结果

### 当前状态 (经诊断脚本验证)

```
✓ MSIX 包已安装
✓ 配置文件已创建: 
  路径: C:\Users\caolib\AppData\Local\Packages\caolib.ProjectOpenerExtension_8wekyb3d8bbwe\
        LocalCache\Local\ProjectOpenerExtension\editors.json
  大小: 278 bytes
  编辑器数量: 1
    - VS Code (vscode)

✓ VS Code Storage 文件存在
  项目数量: 63

✓ PowerToys 正在运行
```

### 功能测试
- [x] 热重载构建成功
- [x] MSIX 包签名成功
- [x] 配置文件自动创建
- [x] VS Code 检测成功
- [x] 项目路径正确 (storage.json)

---

## 修改的文件

1. **ProjectOpenerExtension.csproj**
   - 添加 `PackageCertificateThumbprint` 支持证书存储签名

2. **Services/DynamicSettingsManager.cs**
   - 新增 `IsMsixPackage()` 方法 (多重检测)
   - 更新 `GetConfigFilePath()` 使用新检测方法
   - 更新 `GetSettingsFilePath()` 使用新检测方法

3. **Services/EditorDetectionService.cs**
   - 扩展 VS Code 检测路径列表
   - 添加详细调试日志

4. **diagnose.ps1**
   - 更新配置文件检测逻辑,支持 MSIX 虚拟化路径

---

## 使用指南

### 开发环境

1. **导入证书** (首次):
   ```powershell
   $certPath='ProjectOpenerExtension_TemporaryKey.pfx'
   $pwd=ConvertTo-SecureString 'test123' -AsPlainText -Force
   Import-PfxCertificate -FilePath $certPath -CertStoreLocation Cert:\CurrentUser\My -Password $pwd
   Import-Certificate -FilePath 'ProjectOpenerExtension.cer' -CertStoreLocation Cert:\CurrentUser\Root
   ```

2. **热重载开发**:
   ```powershell
   .\hot-reload.ps1
   ```

3. **诊断问题**:
   ```powershell
   .\diagnose.ps1
   ```

### 微软商店发布

1. **配置文件位置**:
   - 商店版本会自动使用 MSIX 虚拟化路径
   - 配置文件: `%LOCALAPPDATA%\Packages\{PackageFamilyName}\LocalCache\Local\ProjectOpenerExtension\editors.json`

2. **文件系统权限**:
   - 已在 `Package.appxmanifest` 中添加 `broadFileSystemAccess`
   - 用户需在 Windows 设置中授予权限:
     - 设置 > 隐私和安全性 > 文件系统
     - 找到 ProjectOpenerExtension
     - 开启文件系统访问

3. **商店提交**:
   - 移除 `.csproj` 中的 `PackageCertificateKeyFile` 和 `PackageCertificatePassword`
   - Partner Center 会自动处理签名

---

## 测试清单

- [x] 本地构建 MSIX 包成功
- [x] 热重载安装成功
- [x] MSIX 环境正确检测
- [x] 配置文件自动创建
- [x] VS Code 检测成功
- [x] 配置文件路径正确 (虚拟化)
- [x] 诊断脚本显示正确状态
- [ ] PowerToys Command Palette 显示项目列表 (需用户手动测试)
- [ ] 项目可以正常打开 (需用户手动测试)

---

## 下一步建议

### 用户测试
1. 按 `Alt+Space` 打开 PowerToys Command Palette
2. 输入 "Projects" 查看项目列表
3. 点击项目验证是否能正常打开

### 如果项目不显示
1. 重启 PowerToys
2. 运行 `.\diagnose.ps1` 查看详细状态
3. 检查 VS Code Storage 文件:
   ```powershell
   Get-Content "$env:APPDATA\Code\User\globalStorage\storage.json" | ConvertFrom-Json | 
     Select-Object -ExpandProperty profileAssociations | 
     Select-Object -ExpandProperty workspaces | 
     Get-Member -MemberType NoteProperty | Select-Object -First 5 Name
   ```

### 增强功能 (可选)
- [ ] 添加更多编辑器自动检测 (VSCodium, Cursor, Windsurf)
- [ ] 配置文件 UI 编辑器
- [ ] 首次运行向导
- [ ] 自动权限请求提示

---

## 相关文档

- [docs/TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md) - 详细故障排除指南
- [docs/FIX_SUMMARY.md](docs/FIX_SUMMARY.md) - 完整修复文档
- [README.md](README.md) - 项目说明

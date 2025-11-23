# 快速发布指南

## 准备工作

1. **安装必需工具**
   ```powershell
   # .NET 9 SDK
   winget install Microsoft.DotNet.SDK.9
   
   # Inno Setup
   winget install JRSoftware.InnoSetup
   
   # WingetCreate
   winget install Microsoft.WingetCreate
   
   # GitHub CLI
   winget install GitHub.cli
   ```

2. **验证安装**
   ```powershell
   dotnet --version
   Test-Path "${env:ProgramFiles(x86)}\Inno Setup 6\iscc.exe"
   wingetcreate --version
   gh --version
   ```

## 本地构建测试

```powershell
# 构建 EXE 安装程序
.\build-exe.ps1 -Version "0.0.1.0"

# 查看输出
Get-ChildItem "ProjectOpenerExtension\bin\Release\installer\" -File

# 本地测试安装
.\ProjectOpenerExtension\bin\Release\installer\ProjectOpenerExtension-Setup-0.0.1.0-x64.exe /SILENT

# 验证注册
Get-Item "HKCU:\SOFTWARE\Classes\CLSID\{bfe7c130-2e5c-49db-86b4-1c178ea8527b}"

# 在命令面板测试：Alt+Space -> 输入 "Projects"
```

## 发布流程

### 1. 推送代码并触发 GitHub Actions

```powershell
git add .
git commit -m "准备发布 v0.0.1.0"
git push

# 手动触发 workflow
gh workflow run release-extension.yml `
  --ref main `
  -f version="0.0.1.0" `
  -f release_notes="首次发布：支持多编辑器、JSON 配置、自动检测"
```

### 2. 等待 GitHub Actions 完成

- 访问 GitHub Actions 页面查看构建状态
- 构建完成后会自动创建 Release
- 下载链接会在 Release Assets 中

### 3. 获取安装程序 URL

```powershell
# 获取最新 release 的下载链接
gh release view --json assets --jq '.assets[].browser_download_url'
```

复制两个 URL：
- `https://github.com/caolib/ProjectOpenerExtension/releases/download/v0.0.1.0/ProjectOpenerExtension-Setup-0.0.1.0-x64.exe`
- `https://github.com/caolib/ProjectOpenerExtension/releases/download/v0.0.1.0/ProjectOpenerExtension-Setup-0.0.1.0-arm64.exe`

### 4. 提交到 WinGet（首次手动）

```powershell
# 使用 wingetcreate 创建清单
wingetcreate new `
  "https://github.com/caolib/ProjectOpenerExtension/releases/download/v0.0.1.0/ProjectOpenerExtension-Setup-0.0.1.0-x64.exe" `
  "https://github.com/caolib/ProjectOpenerExtension/releases/download/v0.0.1.0/ProjectOpenerExtension-Setup-0.0.1.0-arm64.exe"
```

#### 交互式提示回答：

- **PackageIdentifier**: 保持自动检测（caolib.ProjectOpenerExtension）
- **Publisher**: caolib
- **PackageName**: Project Opener Extension
- **License**: MIT
- **Tags**: **务必添加** `windows-commandpalette-extension`（必需标签！）
- 其他可选字段：选择 **No**
- 最后确认提交：选择 **Yes**

### 5. 验证发布

```powershell
# 等待 WinGet PR 合并（1-3 天）

# 合并后测试安装
winget search ProjectOpener
winget install caolib.ProjectOpenerExtension

# 在命令面板验证
# Alt+Space -> 输入 "Projects"
```

## 更新版本

后续版本可以使用自动化 workflow：

```powershell
# 更新版本号后推送
git tag v0.0.2.0
git push --tags

# GitHub Actions 会自动：
# 1. 构建新版本
# 2. 创建 Release
# 3. 提交 WinGet PR（如果配置了 update-winget.yml）
```

## 常见问题

### Q: 构建失败 "Inno Setup not found"

检查安装路径：
```powershell
Test-Path "${env:ProgramFiles(x86)}\Inno Setup 6\iscc.exe"
```

如果不存在，重新安装 Inno Setup 6。

### Q: WinGet PR 验证失败

常见原因：
1. 缺少 `windows-commandpalette-extension` 标签
2. SHA256 哈希不匹配（wingetcreate 自动计算，一般不会出错）
3. 下载 URL 无效

修复后在 PR 中更新清单文件。

### Q: 安装后命令面板找不到扩展

检查注册表：
```powershell
Get-Item "HKCU:\SOFTWARE\Classes\CLSID\{bfe7c130-2e5c-49db-86b4-1c178ea8527b}"
```

如果不存在，手动运行：
```powershell
& "C:\Program Files\ProjectOpenerExtension\ProjectOpenerExtension.exe" -RegisterProcessAsComServer
```

## 下一步

- 添加自动化 WinGet 更新 workflow
- 配置 GitHub Releases 自动部署
- 收集用户反馈改进功能

## 参考

- [完整发布文档](RELEASE.md)
- [PowerToys 官方指南](https://learn.microsoft.com/zh-cn/windows/powertoys/command-palette/publish-extension)

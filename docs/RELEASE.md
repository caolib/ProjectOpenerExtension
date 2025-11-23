# 发布到 WinGet

本文档说明如何将 Project Opener Extension 发布到 Windows Package Manager (WinGet)。

根据 [PowerToys 官方文档](https://learn.microsoft.com/zh-cn/windows/powertoys/command-palette/publish-extension)，命令面板扩展应该发布 **EXE 安装程序**到 WinGet，而不是 MSIX 包。

## 前提条件

- [GitHub CLI](https://cli.github.com/)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Inno Setup 6](https://jrsoftware.org/isdl.php)
- [WingetCreate](https://github.com/microsoft/winget-create)
  ```powershell
  winget install Microsoft.WingetCreate
  ```

## 发布步骤

### 1. 准备项目文件

#### 更新 .csproj 文件

在 `ProjectOpenerExtension.csproj` 的 `<PropertyGroup>` 中：

1. 删除：`<PublishProfile>win-$(Platform).pubxml</PublishProfile>`
2. 添加：`<WindowsPackageType>None</WindowsPackageType>`

#### 获取 CLSID

打开 `ProjectOpenerExtension.cs`，找到类声明上方的 `[Guid("...")]` 属性，这个 GUID 就是 CLSID。

### 2. 创建构建脚本

### 2. 创建构建脚本

在项目根目录创建 `build-exe.ps1`（已提供模板在 `winget/build-exe.ps1.template`）：

```powershell
param(
    [string]$ExtensionName = "ProjectOpenerExtension",
    [string]$Configuration = "Release",
    [string]$Version = "0.0.1.0",
    [string[]]$Platforms = @("x64", "arm64")
)

# 构建逻辑...
```

在项目根目录创建 `setup-template.iss`（已提供模板在 `winget/setup-template.iss.template`）：

```ini
#define AppVersion "0.0.1.0"

[Setup]
AppId={{YOUR_CLSID_HERE}}
AppName=Project Opener Extension
AppVersion={#AppVersion}
# ...更多配置
```

### 3. 本地测试构建

```powershell
# 验证工具已安装
dotnet --version
Test-Path "${env:ProgramFiles(x86)}\Inno Setup 6\iscc.exe"

# 构建 EXE 安装程序
.\build-exe.ps1 -Version "0.0.1.0"

# 验证输出
Get-ChildItem "bin\Release\installer\" -File
```

应该看到两个文件：
- `ProjectOpenerExtension-Setup-0.0.1.0-x64.exe`
- `ProjectOpenerExtension-Setup-0.0.1.0-arm64.exe`

### 4. 设置 GitHub Actions 自动构建

创建 `.github/workflows/release-extension.yml`（已提供模板）：

```yaml
name: Build and Release Extension

on:
  workflow_dispatch:
    inputs:
      version:
        description: 'Version number (e.g., 0.0.1.0)'
        required: false
      release_notes:
        description: 'Release notes'
        required: false
        
env:
  EXTENSION_NAME: ProjectOpenerExtension
  # ...更多配置
```

### 5. 触发首次发布

```powershell
# 推送代码到 GitHub
git add .
git commit -m "Prepare for WinGet release"
git push

# 手动触发 GitHub Action
gh workflow run release-extension.yml `
  --ref main `
  -f version="0.0.1.0" `
  -f release_notes="Initial release of Project Opener Extension"
```

GitHub Actions 会自动：
- 构建 x64 和 ARM64 安装程序
- 创建 GitHub Release
- 上传安装程序到 Release Assets

### 6. 提交到 WinGet

#### 首次提交（手动）

```powershell
# 获取 GitHub Release 中的安装程序 URL
# 右键点击 .exe 文件 -> 复制链接地址

# 使用 wingetcreate 创建清单
wingetcreate new `
  "https://github.com/caolib/ProjectOpenerExtension/releases/download/v0.0.1.0/ProjectOpenerExtension-Setup-0.0.1.0-x64.exe" `
  "https://github.com/caolib/ProjectOpenerExtension/releases/download/v0.0.1.0/ProjectOpenerExtension-Setup-0.0.1.0-arm64.exe"
```

根据提示输入包信息：
- **PackageIdentifier**: 自动检测（保持默认）
- **Publisher**: caolib
- **PackageName**: Project Opener Extension
- **License**: MIT
- **Tags**: 务必添加 `windows-commandpalette-extension`（命令面板扩展的必需标签）

```yaml
Tags:
- windows-commandpalette-extension
- powertoys
- vscode
- jetbrains
```

确认所有信息后，选择 **Yes** 提交到 winget-pkgs。

#### 后续更新（自动化）

创建 `.github/workflows/update-winget.yml`（已提供模板）实现自动更新：

```yaml
name: Update WinGet

on:
  release:
    types: [published]
    
jobs:
  update-winget:
    runs-on: windows-latest
    steps:
      - name: Update WinGet manifest
        run: |
          wingetcreate update caolib.ProjectOpenerExtension `
            --version ${{ steps.release.outputs.VERSION }} `
            --urls "$x64Url|x64" "$arm64Url|arm64" `
            --token ${{ secrets.GITHUB_TOKEN }} `
            --submit
```

### 7. 验证发布

等待 WinGet PR 合并后（通常 1-3 天），用户可以：

```powershell
# 搜索扩展
winget search ProjectOpener

# 安装扩展
winget install caolib.ProjectOpenerExtension

# 在命令面板中自动发现
# Alt+Space -> 输入项目名称
```

## 重要配置

## 重要配置

### 必需标签

命令面板使用特殊标签来发现扩展，**必须**在所有 locale 清单中添加：

```yaml
Tags:
- windows-commandpalette-extension  # 必需！命令面板发现标签
- powertoys
- vscode
- jetbrains
- project-manager
```

### Windows App SDK 依赖

在 `installer.yaml` 中声明依赖：

```yaml
Dependencies:
  PackageDependencies:
  - PackageIdentifier: Microsoft.WindowsAppRuntime.1.6
```

## 构建验证清单

发布前检查：

- ✅ .NET 9 SDK 已安装
- ✅ Inno Setup 6 已安装
- ✅ `build-exe.ps1` 和 `setup-template.iss` 已配置
- ✅ 本地构建成功生成 x64 和 ARM64 安装程序
- ✅ GitHub Actions 工作流已配置
- ✅ 首次 Release 创建成功
- ✅ WinGet 清单包含 `windows-commandpalette-extension` 标签

## 文件模板

所有必需的模板文件已在 `winget/` 目录中提供：

- `build-exe.ps1.template` - 构建脚本模板
- `setup-template.iss.template` - Inno Setup 配置模板
- `.github/workflows/release-extension.yml.template` - GitHub Actions 工作流
- `.github/workflows/update-winget.yml.template` - WinGet 自动更新工作流

## 常见问题

### Q: 为什么发布 EXE 而不是 MSIX？

根据 PowerToys 官方文档，命令面板扩展应该使用 EXE 安装程序发布到 WinGet，这样可以：
- 正确注册 COM 组件
- 支持自动发现
- 简化安装流程

### Q: 如何验证安装程序？

```powershell
# 本地测试安装
.\ProjectOpenerExtension-Setup-0.0.1.0-x64.exe /SILENT

# 检查注册表
Get-Item "HKCU:\SOFTWARE\Classes\CLSID\{YOUR_CLSID}"

# 在命令面板测试
# Alt+Space -> 输入项目名称
```

### Q: WinGet PR 被拒绝怎么办？

常见原因：
- 缺少 `windows-commandpalette-extension` 标签
- SHA256 哈希不匹配
- 安装程序 URL 无效

检查 PR 评论中的验证错误，修复后更新 PR。

## 参考资料

- [PowerToys 命令面板发布指南](https://learn.microsoft.com/zh-cn/windows/powertoys/command-palette/publish-extension)
- [winget-pkgs 贡献指南](https://github.com/microsoft/winget-pkgs/blob/master/CONTRIBUTING.md)
- [Inno Setup 文档](https://jrsoftware.org/ishelp/)
- [WingetCreate 文档](https://github.com/microsoft/winget-create)

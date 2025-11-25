# Project Opener Extension for PowerToys

[English](../README.md) | 中文文档

一个 PowerToys 命令面板扩展，用于快速访问多个代码编辑器的最近项目。

## 功能特性

- 🚀 快速访问：`Alt+Space` → 输入 "Projects"
- 🔍 实时搜索过滤项目
- 🎯 多编辑器支持：VS Code 系列 & JetBrains 全家桶
- ⚙️ 基于 JSON 的配置文件
- 🎨 自动从可执行文件提取图标或使用自定义图片
- 🔄 首次运行自动检测已安装的编辑器

## 安装

从微软商店下载

![](https://github.com/microsoft/PowerToys/blob/main/doc/images/readme/StoreBadge-light.png)

## 快速开始

1. 按 `Alt+Space` 打开命令面板
2. 输入 "Projects" 查看最近项目
3. 点击打开项目，或右键选择其他操作

## 配置说明

**配置文件位置**：`%USERPROFILE%\.config\ProjectOpenerExtension\editors.json`

首次运行时，扩展会自动创建配置文件并检测已安装的编辑器。

### 配置示例

```json
[
  {
    "Name": "VS Code",
    "Enabled": true,
    "Icon": "",
    "ExecutablePath": "C:\\Program Files\\Microsoft VS Code\\Code.exe",
    "ProjectPath": "C:\\Users\\{用户名}\\AppData\\Roaming\\Code\\User\\globalStorage\\storage.json",
    "EditorType": "vscode"
  },
  {
    "Name": "IntelliJ IDEA",
    "Enabled": true,
    "Icon": "",
    "ExecutablePath": "C:\\Program Files\\JetBrains\\IntelliJ IDEA\\bin\\idea64.exe",
    "ProjectPath": "C:\\Users\\{用户名}\\AppData\\Roaming\\JetBrains",
    "EditorType": "jetbrains"
  }
]
```

### 配置字段说明

- **Name**：编辑器显示名称
- **Enabled**：`true` 启用 / `false` 禁用
- **Icon**：图标路径（留空则自动从可执行文件提取）
- **ExecutablePath**：编辑器可执行文件的完整路径
- **ProjectPath**：
  - VS Code 系列：`storage.json` 文件路径
  - JetBrains 系列：JetBrains 配置根目录路径
- **EditorType**：`"vscode"` 或 `"jetbrains"`

### 图标选项

1. **自动提取**（推荐）：
   ```json
   "Icon": ""
   ```

2. **自定义图片**：
   ```json
   "Icon": "C:\\Users\\{用户名}\\Pictures\\icons\\vscode.png"
   ```

3. **使用 Emoji**：
   ```json
   "Icon": "📝"
   ```

## 添加自定义编辑器

编辑配置文件，添加新的编辑器条目：

```json
{
  "Name": "我的编辑器",
  "Enabled": true,
  "Icon": "C:\\path\\to\\icon.png",
  "ExecutablePath": "C:\\path\\to\\editor.exe",
  "ProjectPath": "C:\\path\\to\\storage\\or\\config\\directory",
  "EditorType": "vscode"
}
```

**注意**：
- VS Code 类编辑器：`EditorType` 设为 `"vscode"`，`ProjectPath` 指向 `storage.json` 文件
- JetBrains 类编辑器：`EditorType` 设为 `"jetbrains"`，`ProjectPath` 指向配置根目录

## 故障排除

如果遇到问题：

1. **运行诊断工具**：
   ```powershell
   .\diagnose.ps1
   ```

2. **检查配置文件位置**：
   - MSIX 安装: `%LOCALAPPDATA%\ProjectOpenerExtension\editors.json`
   - 独立安装: `%USERPROFILE%\.config\ProjectOpenerExtension\editors.json`

3. **查看详细指南**: [TROUBLESHOOTING.md](TROUBLESHOOTING.md)

常见问题：
- 配置文件未创建 → 运行 `diagnose.ps1` 选择选项 1
- 项目未识别 → 检查配置文件中的编辑器路径
- MSIX 包无法安装 → 开发时使用热重载或信任证书

### 项目没有显示

1. 检查编辑器在配置文件中是否启用
2. 验证 `ProjectPath` 路径是否正确
3. VS Code：确保 `storage.json` 文件存在
4. JetBrains：确保目录包含版本文件夹（如 `IntelliJIdea2024.1`）
5. MSIX 应用：在 Windows 设置中授予文件系统访问权限

## 从源码构建

```bash
git clone https://github.com/caolib/ProjectOpenerExtension.git
cd ProjectOpenerExtension
```

使用 Visual Studio 2022+ 打开 `ProjectOpenerExtension.sln` 并构建。

开发时使用热重载：
```powershell
.\hot-reload.ps1
```

## 许可证

MIT License - 详见 [LICENSE](../LICENSE)

# Project Opener Extension for PowerToys

[English](.././README.md) | 中文文档

一个 PowerToys 命令面板扩展，用于快速搜索并打开vscode系列和jetbrains系列编辑器的最近项目

![image-20251125165500000](./image-20251125165500000.png)

## 安装

<a href="https://apps.microsoft.com/detail/9ng967hcnh55?referrer=appbadge&mode=direct"> 	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/> </a>

## 配置说明

首次运行时，需要进入拓展设置界面设置配置文件的路径，你可以讲下面的配置复制到一个`editors.json`文件保存，然后在拓展设置界面设置为这个文件的路径

### 配置示例

```json
[
  {
    "Name": "VS Code",
    "Enabled": true,
    "Icon": "",
    "ExecutablePath": "C:/.../Code.exe",
    "ProjectPath": "C:/Users/{UserName}/AppData/Roaming/Code/User/globalStorage/storage.json",
    "EditorType": "vscode"
  },
  {
    "Name": "Trae",
    "Enabled": true,
    "Icon": "",
    "ExecutablePath": "C:/.../Trae.exe",
    "ProjectPath": "C:/.../globalStorage/storage.json",
    "EditorType": "vscode"
  },
  {
    "Name": "IntelliJ IDEA",
    "Enabled": true,
    "Icon": "",
    "ExecutablePath": "C:/.../IntelliJ IDEA/bin/idea64.exe",
    "ProjectPath": "C:/Users/{UserName}/AppData/Roaming/JetBrains",
    "EditorType": "jetbrains"
  }
]
```

### 配置字段说明

- **Name**：编辑器显示名称
- **Enabled**：`true` 启用 / `false` 禁用
- **Icon**：图标路径（显示的图标，留空则使用exe文件的图标）
- **ExecutablePath**：编辑器可执行文件的完整路径
- **ProjectPath**：
  - VS Code 系列：`storage.json` 文件路径
  - JetBrains 系列：JetBrains 配置根目录路径（Android Studio 的路径类似C:/Users/{用户名}/AppData/Roaming/Google）
- **EditorType**：`"vscode"` 或 `"jetbrains"`

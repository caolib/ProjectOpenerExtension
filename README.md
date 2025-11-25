# Project Opener Extension for PowerToys

English | [中文文档](./docs/README_zh.md)

A PowerToys Command Palette extension that lets you quickly search and open recent projects from the VS Code family and JetBrains family of editors.

![image-20251125165500000](./docs/image-20251125165500000.png)

## Installation

<a href="https://apps.microsoft.com/detail/9ng967hcnh55?referrer=appbadge&mode=direct"> 	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/> </a>

## Configuration

On first run, you need to open the extension settings page to set the path of the configuration file. You can copy the configuration below into a file named `editors.json`, save it, and then set its path in the extension settings.

### Sample Configuration

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
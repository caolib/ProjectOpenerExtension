# Microsoft Store 受限功能说明

## runFullTrust 功能说明

### 英文版本（推荐）：

**Why do you need the runFullTrust capability and how is it used in your product?**

This application is a PowerToys Command Palette extension that requires runFullTrust capability for the following essential functionalities:

1. **COM Server Registration**: The extension needs to register as a COM server to integrate with PowerToys Command Palette, which requires full trust access to the Windows registry (HKEY_CURRENT_USER\SOFTWARE\Classes\CLSID).

2. **File System Access**: The extension reads recent project lists from various code editors (VS Code, JetBrains IDEs) stored in user's AppData directory, which requires unrestricted file system access beyond the standard app container sandbox.

3. **Process Launching**: The extension launches external applications (code editors) with specific project paths as command-line arguments, which requires full trust to execute arbitrary processes.

4. **Dynamic Configuration**: The extension reads and writes JSON configuration files in the user's home directory (%USERPROFILE%\.config\ProjectOpenerExtension\), allowing users to customize editor settings.

Without runFullTrust, the extension cannot:
- Register the required COM interfaces for PowerToys integration
- Access editor storage locations outside the app sandbox
- Launch code editors with project paths
- Provide the core functionality of quick project access

This is a standard requirement for PowerToys Command Palette extensions, as documented in Microsoft's official PowerToys extension development guide: https://learn.microsoft.com/windows/powertoys/command-palette/

---

### 中文版本：

**为何需要使用 runFullTrust 功能，如何在产品中使用？**

本应用程序是一个 PowerToys 命令面板扩展，需要 runFullTrust 功能来实现以下核心功能：

1. **COM 服务器注册**：扩展需要注册为 COM 服务器以集成到 PowerToys 命令面板中，这需要完全信任访问权限来修改 Windows 注册表（HKEY_CURRENT_USER\SOFTWARE\Classes\CLSID）。

2. **文件系统访问**：扩展需要读取存储在用户 AppData 目录中的各种代码编辑器（VS Code、JetBrains IDE）的最近项目列表，这需要超出标准应用容器沙盒的无限制文件系统访问权限。

3. **进程启动**：扩展需要启动外部应用程序（代码编辑器）并传递特定的项目路径作为命令行参数，这需要完全信任权限来执行任意进程。

4. **动态配置**：扩展需要在用户主目录（%USERPROFILE%\.config\ProjectOpenerExtension\）中读写 JSON 配置文件，允许用户自定义编辑器设置。

没有 runFullTrust 权限，扩展将无法：
- 注册 PowerToys 集成所需的 COM 接口
- 访问应用沙盒之外的编辑器存储位置
- 使用项目路径启动代码编辑器
- 提供快速项目访问的核心功能

这是 PowerToys 命令面板扩展的标准要求，详见 Microsoft 官方 PowerToys 扩展开发指南：https://learn.microsoft.com/zh-cn/windows/powertoys/command-palette/

---

## 建议使用的回复（简洁版英文）：

This is a PowerToys Command Palette extension that requires runFullTrust to:

1. Register as a COM server for PowerToys integration (registry access)
2. Read recent projects from code editors' storage (VS Code, JetBrains IDEs) in AppData
3. Launch external code editors with project paths
4. Access user configuration at %USERPROFILE%\.config\

This is a standard requirement for all PowerToys extensions as documented in Microsoft's official guide: https://learn.microsoft.com/windows/powertoys/command-palette/

Without runFullTrust, the extension cannot integrate with PowerToys or access the necessary editor data files.

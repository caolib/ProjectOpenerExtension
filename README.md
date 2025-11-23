# Project Opener Extension for PowerToys

[中文文档](docs/README_zh.md) | English

A PowerToys Command Palette extension for quick access to recent projects from multiple code editors.

## Features

- 🚀 Quick access via `Alt+Space` → type "Projects"
- 🔍 Search projects by name with real-time filtering
- 🎯 Multi-editor support: VS Code family & JetBrains IDEs
- ⚙️ JSON-based configuration
- 🎨 Auto-extract icons from executables or use custom images
- 🔄 Auto-detect installed editors on first run

## Installation

### via WinGet (Recommended)

```powershell
winget install caolib.ProjectOpenerExtension
```

The extension will be automatically available in PowerToys Command Palette after installation.

### Manual Installation

1. Download the installer for your architecture from [Releases](https://github.com/caolib/ProjectOpenerExtension/releases):
   - **x64 (Intel/AMD)**: `ProjectOpenerExtension-Setup-x.x.x.x-x64.exe`
   - **ARM64 (Windows on ARM)**: `ProjectOpenerExtension-Setup-x.x.x.x-arm64.exe`
2. Run the installer
3. The extension will be automatically registered and available in Command Palette

## Quick Start

1. Press `Alt+Space` to open Command Palette
2. Type "Projects" to see your recent projects
3. Click to open, or right-click for more options

## Configuration

**Config file**: `%USERPROFILE%\.config\ProjectOpenerExtension\editors.json`

The extension auto-creates this file with detected editors on first run.

### Example Configuration

```json
[
  {
    "Name": "VS Code",
    "Enabled": true,
    "Icon": "",
    "ExecutablePath": "C:\\Program Files\\Microsoft VS Code\\Code.exe",
    "ProjectPath": "C:\\Users\\{User}\\AppData\\Roaming\\Code\\User\\globalStorage\\storage.json",
    "EditorType": "vscode"
  }
]
```

### Configuration Fields

- **Name**: Display name
- **Enabled**: `true`/`false`
- **Icon**: Path to icon (leave empty for auto-extract from executable)
- **ExecutablePath**: Full path to editor executable
- **ProjectPath**: 
  - VS Code family: path to `storage.json`
  - JetBrains: path to JetBrains config root directory
- **EditorType**: `"vscode"` or `"jetbrains"`

### Supported Editors

- **VS Code family**: VS Code, VSCodium, Cursor, Windsurf
- **JetBrains**: IntelliJ IDEA, WebStorm, PyCharm, GoLand, Rider, CLion, PhpStorm, RubyMine, DataGrip

## Building from Source

```bash
git clone https://github.com/caolib/ProjectOpenerExtension.git
cd ProjectOpenerExtension
```

Open `ProjectOpenerExtension.sln` in Visual Studio 2022+ and build.

For development with hot-reload:
```powershell
.\hot-reload.ps1
```

## Contributing

Issues and pull requests are welcome!

## License

MIT License - see [LICENSE](LICENSE)

# Project Opener Extension for PowerToys

A PowerToys Command Palette extension that provides quick access to your recent projects from various code editors.

## Features

- 🚀 **Quick Access**: Press `Alt+Space` and type "Projects" to instantly view all your recent projects
- 🔍 **Smart Search**: Search projects by name with real-time filtering
- 🎯 **Multi-Editor Support**: Works with VS Code, IntelliJ IDEA, and other JetBrains IDEs
- ⚙️ **Fully Configurable**: JSON-based configuration for easy customization
- 🎨 **Custom Icons**: Support for PNG, ICO, JPG images or auto-extract from executables
- 🔄 **Auto-Detection**: Automatically detects installed editors on first run

## Supported Editors

- **VS Code-based editors**: VS Code, VSCodium, Cursor, Windsurf, and other Electron-based editors
- **JetBrains IDEs**: IntelliJ IDEA, WebStorm, PyCharm, GoLand, Rider, CLion, PhpStorm, RubyMine, DataGrip

## Installation

1. Download the latest `.msix` package from [Releases](https://github.com/caolib/ProjectOpenerExtension/releases)
2. Install the package (Windows may require enabling Developer Mode or sideloading)
3. Open PowerToys Settings
4. Enable the "Project Opener Extension" in the Command Palette extensions list

## Configuration

Configuration file location: `C:\Users\{YourUsername}\.config\ProjectOpenerExtension\editors.json`

### Configuration Format

```json
[
  {
    "Name": "VS Code",
    "Enabled": true,
    "Icon": "",
    "ExecutablePath": "C:\\Program Files\\Microsoft VS Code\\Code.exe",
    "ProjectPath": "C:\\Users\\{Username}\\AppData\\Roaming\\Code\\User\\globalStorage\\storage.json",
    "EditorType": "vscode"
  },
  {
    "Name": "IntelliJ IDEA",
    "Enabled": true,
    "Icon": "",
    "ExecutablePath": "C:\\Program Files\\JetBrains\\IntelliJ IDEA\\bin\\idea64.exe",
    "ProjectPath": "C:\\Users\\{Username}\\AppData\\Roaming\\JetBrains",
    "EditorType": "jetbrains"
  }
]
```

### Configuration Fields

- **Name**: Display name of the editor (e.g., "VS Code", "IntelliJ IDEA")
- **Enabled**: `true` or `false` - whether to show projects from this editor
- **Icon**: Path to custom icon file (PNG/ICO/JPG) or leave empty to auto-extract from executable
  - Example: `"C:\\icons\\vscode.png"` or `""` for auto-detection
- **ExecutablePath**: Full path to the editor's executable file
  - Used to open projects and extract icons if Icon is empty
- **ProjectPath**: 
  - For VS Code-based editors: Path to `storage.json` file
  - For JetBrains IDEs: Path to JetBrains root directory (e.g., `AppData\Roaming\JetBrains`)
- **EditorType**: Either `"vscode"` or `"jetbrains"`

### Icon Options

1. **Auto-extract from executable** (recommended):
   ```json
   "Icon": ""
   ```

2. **Use custom image**:
   ```json
   "Icon": "C:\\Users\\{Username}\\Pictures\\icons\\vscode.png"
   ```

3. **Use emoji**:
   ```json
   "Icon": "📝"
   ```

## Usage

1. Press `Alt+Space` to open PowerToys Command Palette
2. Type "Projects" or start searching for a project name
3. Select a project from the list
4. Right-click for context menu:
   - **Use {Editor} to open**: Open with specific editor
   - **Show in File Explorer**: Open project folder in Explorer

## Adding Custom Editors

To add a custom editor, edit the configuration file and add a new entry:

```json
{
  "Name": "My Custom Editor",
  "Enabled": true,
  "Icon": "C:\\path\\to\\icon.png",
  "ExecutablePath": "C:\\path\\to\\editor.exe",
  "ProjectPath": "C:\\path\\to\\storage\\or\\config\\directory",
  "EditorType": "vscode"
}
```

**Note**: 
- For VS Code-based editors, set `EditorType` to `"vscode"` and `ProjectPath` to the `storage.json` file
- For JetBrains-like IDEs, set `EditorType` to `"jetbrains"` and `ProjectPath` to the configuration root directory

## Troubleshooting

### Projects not showing up

1. Check if the editor is enabled in the configuration file
2. Verify the `ProjectPath` points to the correct location
3. For VS Code: Ensure `storage.json` exists at the specified path
4. For JetBrains: Ensure the directory contains version folders (e.g., `IntelliJIdea2024.1`)

### Configuration file location

The configuration file is located at:
```
C:\Users\{YourUsername}\.config\ProjectOpenerExtension\editors.json
```

You can access it through:
- PowerToys Settings → Project Opener Extension → Click the folder icon next to "Configuration File Location"
- Manually navigate to the path above

### Changes not taking effect

The extension automatically reloads configuration changes. If changes don't appear:
1. Save the configuration file
2. Wait a few seconds
3. Reopen the Command Palette (`Alt+Space`)

## Building from Source

### Prerequisites

- Visual Studio 2022 or later
- .NET 9.0 SDK
- Windows 11 SDK (10.0.26100.0 or later)
- PowerToys Command Palette Extensions SDK

### Build Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/caolib/ProjectOpenerExtension.git
   cd ProjectOpenerExtension
   ```

2. Open `ProjectOpenerExtension.sln` in Visual Studio

3. Build the solution (Ctrl+Shift+B)

4. The package will be generated in:
   ```
   ProjectOpenerExtension\AppPackages\ProjectOpenerExtension_{version}_{platform}_Debug_Test\
   ```

### Hot Reload (Development)

For rapid development, use the included hot-reload script:

```powershell
.\hot-reload.ps1
```

This script will:
- Uninstall the old version
- Build the project
- Install the new version
- Trigger PowerToys to reload the extension

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Built with [PowerToys Command Palette Extensions SDK](https://github.com/microsoft/PowerToys)
- Inspired by the need for quick project access across multiple editors

## Changelog

### v0.0.1 (Initial Release)
- Multi-editor support (VS Code, JetBrains IDEs)
- JSON-based configuration
- Auto-detection of installed editors
- Custom icon support
- Hot-reload configuration changes

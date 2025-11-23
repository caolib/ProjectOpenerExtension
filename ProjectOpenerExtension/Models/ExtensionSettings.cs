// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;

namespace ProjectOpenerExtension.Models;

/// <summary>
/// 编辑器配置
/// </summary>
public class EditorConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string CommandLineArgs { get; set; } = "\"{0}\""; // {0} 会被替换为项目路径
    public string Icon { get; set; } = "📁";
    public EditorType Type { get; set; }
    public bool IsEnabled { get; set; } = true;

    // VSCode 系列编辑器配置
    public string StorageFilePath { get; set; } = string.Empty; // storage.json 路径
    public string StorageJsonPath { get; set; } = "openedPathsList.entries"; // JSON 路径

    // JetBrains 系列编辑器配置
    public string ConfigFolderPattern { get; set; } = string.Empty; // 如 "Rider*"
    public string RecentProjectsFile { get; set; } = "options/recentProjects.xml";
}

/// <summary>
/// 扩展配置
/// </summary>
public class ExtensionSettings
{
    public List<EditorConfig> Editors { get; set; } = new();
    public int MaxRecentProjects { get; set; } = 20;
    public bool GroupByEditor { get; set; } = true;
    public bool ShowLastOpenedTime { get; set; } = true;

    public static ExtensionSettings GetDefault()
    {
        var settings = new ExtensionSettings
        {
            Editors = new List<EditorConfig>()
        };

        // VS Code
        settings.Editors.Add(new EditorConfig
        {
            Id = "vscode",
            Name = "Visual Studio Code",
            ExecutablePath = "code",
            Icon = "📝",
            Type = EditorType.VSCode,
            StorageFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Code", "User", "globalStorage", "storage.json"
            )
        });

        // VS Codium
        settings.Editors.Add(new EditorConfig
        {
            Id = "vscodium",
            Name = "VSCodium",
            ExecutablePath = "codium",
            Icon = "📘",
            Type = EditorType.VSCode,
            IsEnabled = false,
            StorageFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "VSCodium", "User", "globalStorage", "storage.json"
            )
        });

        // Cursor
        settings.Editors.Add(new EditorConfig
        {
            Id = "cursor",
            Name = "Cursor",
            ExecutablePath = "cursor",
            Icon = "🖱️",
            Type = EditorType.VSCode,
            IsEnabled = false,
            StorageFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Cursor", "User", "globalStorage", "storage.json"
            )
        });

        // Windsurf
        settings.Editors.Add(new EditorConfig
        {
            Id = "windsurf",
            Name = "Windsurf",
            ExecutablePath = "windsurf",
            Icon = "🏄",
            Type = EditorType.VSCode,
            IsEnabled = false,
            StorageFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Windsurf", "User", "globalStorage", "storage.json"
            )
        });

        // JetBrains IDEs
        var jetbrainsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "JetBrains"
        );

        settings.Editors.Add(new EditorConfig
        {
            Id = "rider",
            Name = "Rider",
            ExecutablePath = "rider64.exe",
            Icon = "🎯",
            Type = EditorType.Rider,
            ConfigFolderPattern = "Rider*",
            RecentProjectsFile = "options/recentProjects.xml"
        });

        settings.Editors.Add(new EditorConfig
        {
            Id = "intellij",
            Name = "IntelliJ IDEA",
            ExecutablePath = "idea64.exe",
            Icon = "💡",
            Type = EditorType.IntelliJIdea,
            ConfigFolderPattern = "IntelliJIdea*",
            RecentProjectsFile = "options/recentProjects.xml"
        });

        settings.Editors.Add(new EditorConfig
        {
            Id = "webstorm",
            Name = "WebStorm",
            ExecutablePath = "webstorm64.exe",
            Icon = "🌊",
            Type = EditorType.WebStorm,
            ConfigFolderPattern = "WebStorm*",
            RecentProjectsFile = "options/recentProjects.xml"
        });

        settings.Editors.Add(new EditorConfig
        {
            Id = "pycharm",
            Name = "PyCharm",
            ExecutablePath = "pycharm64.exe",
            Icon = "🐍",
            Type = EditorType.PyCharm,
            ConfigFolderPattern = "PyCharm*",
            RecentProjectsFile = "options/recentProjects.xml"
        });

        settings.Editors.Add(new EditorConfig
        {
            Id = "goland",
            Name = "GoLand",
            ExecutablePath = "goland64.exe",
            Icon = "🦫",
            Type = EditorType.GoLand,
            ConfigFolderPattern = "GoLand*",
            RecentProjectsFile = "options/recentProjects.xml"
        });

        settings.Editors.Add(new EditorConfig
        {
            Id = "phpstorm",
            Name = "PhpStorm",
            ExecutablePath = "phpstorm64.exe",
            Icon = "🐘",
            Type = EditorType.PhpStorm,
            ConfigFolderPattern = "PhpStorm*",
            RecentProjectsFile = "options/recentProjects.xml"
        });

        settings.Editors.Add(new EditorConfig
        {
            Id = "clion",
            Name = "CLion",
            ExecutablePath = "clion64.exe",
            Icon = "⚙️",
            Type = EditorType.CLion,
            ConfigFolderPattern = "CLion*",
            RecentProjectsFile = "options/recentProjects.xml"
        });

        settings.Editors.Add(new EditorConfig
        {
            Id = "rubymine",
            Name = "RubyMine",
            ExecutablePath = "rubymine64.exe",
            Icon = "💎",
            Type = EditorType.RubyMine,
            ConfigFolderPattern = "RubyMine*",
            RecentProjectsFile = "options/recentProjects.xml"
        });

        settings.Editors.Add(new EditorConfig
        {
            Id = "datagrip",
            Name = "DataGrip",
            ExecutablePath = "datagrip64.exe",
            Icon = "🗄️",
            Type = EditorType.DataGrip,
            ConfigFolderPattern = "DataGrip*",
            RecentProjectsFile = "options/recentProjects.xml"
        });

        return settings;
    }
}

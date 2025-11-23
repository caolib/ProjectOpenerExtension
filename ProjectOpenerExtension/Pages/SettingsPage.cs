// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ProjectOpenerExtension.Commands;
using ProjectOpenerExtension.Models;
using ProjectOpenerExtension.Services;

namespace ProjectOpenerExtension.Pages;

/// <summary>
/// 设置页面 - 显示和管理编辑器配置
/// </summary>
internal sealed partial class SettingsPage : ListPage
{
    private readonly DynamicSettingsManager _settingsService;

    public SettingsPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Settings";
        PlaceholderText = "Search editors...";

        _settingsService = DynamicSettingsManager.Instance;
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>();
        var editors = _settingsService.GetEditorConfigs();

        // === VS Code 系列编辑器 ===
        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━",
            Subtitle = "VS Code 系列编辑器",
            Section = "VS Code Editors"
        });

        foreach (var editor in editors.Where(e => e.Type == EditorType.VSCode))
        {
            var subtitle = BuildEditorSubtitle(editor);

            items.Add(new ListItem(new NoOpCommand())
            {
                Title = $"{editor.Name}",
                Subtitle = subtitle,
                Section = "VS Code Editors",
                Tags = new[]
                {
                    new Tag { Text = editor.IsEnabled ? "✓ Enabled" : "Disabled" }
                }
            });
        }

        // === JetBrains 系列编辑器 ===
        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━",
            Subtitle = "JetBrains 系列编辑器",
            Section = "JetBrains Editors"
        });

        foreach (var editor in editors.Where(e => e.Type != EditorType.VSCode))
        {
            var subtitle = BuildEditorSubtitle(editor);

            items.Add(new ListItem(new NoOpCommand())
            {
                Title = editor.Name,
                Subtitle = subtitle,
                Section = "JetBrains Editors",
                Tags = new[]
                {
                    new Tag { Text = editor.IsEnabled ? "✓ Enabled" : "Disabled" }
                }
            });
        }

        // === 自定义编辑器说明 ===
        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━",
            Subtitle = "如何添加自定义编辑器",
            Section = "Custom Editors"
        });

        var settingsFolder = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ProjectOpenerExtension");

        items.Add(new ListItem(new OpenFolderCommand(settingsFolder))
        {
            Title = "📂 打开配置文件夹",
            Subtitle = settingsFolder,
            Section = "Custom Editors"
        });

        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "📝 编辑 custom-editors.json",
            Subtitle = "在配置文件夹中创建或编辑 custom-editors.json 文件",
            Section = "Custom Editors"
        });

        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "💡 VS Code 编辑器格式",
            Subtitle = "{\"Id\":\"myeditor\", \"Name\":\"My Editor\", \"Type\":\"vscode\", \"DefaultExecutable\":\"myeditor\"}",
            Section = "Custom Editors"
        });

        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "💡 JetBrains 编辑器格式",
            Subtitle = "{\"Id\":\"myide\", \"Name\":\"My IDE\", \"Type\":\"jetbrains\", \"DefaultExecutable\":\"myide64.exe\"}",
            Section = "Custom Editors"
        });

        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "🔄 重启生效",
            Subtitle = "添加自定义编辑器后，需要重启 PowerToys 使更改生效",
            Section = "Custom Editors"
        });

        return items.ToArray();
    }

    private string BuildEditorSubtitle(EditorConfig editor)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(editor.ExecutablePath))
        {
            parts.Add($"Executable: {editor.ExecutablePath}");
        }

        if (!string.IsNullOrEmpty(editor.StorageFilePath))
        {
            parts.Add($"Storage: {editor.StorageFilePath}");
        }
        else if (!string.IsNullOrEmpty(editor.ConfigFolderPattern))
        {
            parts.Add($"Config Pattern: {editor.ConfigFolderPattern}");
        }

        return parts.Count > 0 ? string.Join(" • ", parts) : "No configuration";
    }
}



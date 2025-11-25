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

namespace ProjectOpenerExtension;

internal sealed partial class ProjectOpenerExtensionPage : ListPage
{
    private readonly VSCodeProjectService _vscodeService;
    private readonly JetBrainsProjectService _jetbrainsService;
    private readonly DynamicSettingsManager _settingsService;

    public ProjectOpenerExtensionPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Recent Projects";
        PlaceholderText = "Search projects...";

        _vscodeService = new VSCodeProjectService();
        _jetbrainsService = new JetBrainsProjectService();
        _settingsService = DynamicSettingsManager.Instance;
    }
    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>();
        var editors = _settingsService.GetEditorConfigs();

        // Get VS Code projects
        var vscodeProjects = _vscodeService.GetRecentProjects();
        if (vscodeProjects.Count > 0)
        {
            foreach (var project in vscodeProjects.OrderByDescending(p => p.LastOpened))
            {
                var editor = editors.Find(e => e.Id == project.SourceEditorId);
                var sectionLabel = editor != null ? $"{editor.Name} Projects" : "VS Code Projects";
                items.Add(CreateProjectListItem(project, sectionLabel, editors));
            }
        }

        // Get JetBrains projects
        var jetbrainsProjects = _jetbrainsService.GetRecentProjects();
        if (jetbrainsProjects.Count > 0)
        {
            foreach (var project in jetbrainsProjects.OrderByDescending(p => p.LastOpened))
            {
                var editor = editors.Find(e => e.Id == project.SourceEditorId);
                var sectionLabel = editor != null ? $"{editor.Name} Projects" : "Other Projects";
                items.Add(CreateProjectListItem(project, sectionLabel, editors));
            }
        }

        if (items.Count == 0)
        {
            // Check if editors are configured
            if (editors.Count == 0)
            {
                items.Add(new ListItem(new NoOpCommand())
                {
                    Title = "First time using? Configure editors first | 首次使用?请先配置编辑器",
                    Subtitle = "Specify the configuration file path in settings and create editors.json file | 请在设置中指定配置文件路径并创建 editors.json 文件"
                });
                items.Add(new ListItem(new NoOpCommand())
                {
                    Title = "Configuration Steps | 配置步骤",
                    Subtitle = "1. Command Palette → Settings (bottom left) → Extensions → Project Opener Extension | 命令面板 → 设置(左下角) → 扩展 → Project Opener Extension"
                });
                items.Add(new ListItem(new NoOpCommand())
                {
                    Title = "",
                    Subtitle = "2. Enter full path in 'Configuration File Path' (e.g., C:\\config\\editors.json) | 在「配置文件路径」中输入完整路径 (如: C:\\config\\editors.json)"
                });
                items.Add(new ListItem(new NoOpCommand())
                {
                    Title = "",
                    Subtitle = "3. Create and edit the file, refer to configuration examples in settings | 创建并编辑该文件，参考设置页面的配置示例"
                });
            }
            else
            {
                items.Add(new ListItem(new NoOpCommand())
                {
                    Title = "No recent projects found",
                    Subtitle = "Open some projects in VS Code or JetBrains IDEs to see them here"
                });
            }
        }

        return items.ToArray();
    }

    private ListItem CreateProjectListItem(ProjectInfo project, string sectionLabel, List<EditorConfig> editors)
    {
        var defaultEditorId = project.AvailableEditorIds.FirstOrDefault() ?? "vscode";
        var defaultCommand = new OpenProjectCommand(project, defaultEditorId);

        var contextCommands = new List<IContextItem>();

        // 为所有启用的编辑器添加打开命令
        var enabledEditors = _settingsService.GetEditorConfigs().Where(e => e.IsEnabled).ToList();
        foreach (var editor in enabledEditors)
        {
            var openCommand = new OpenProjectCommand(project, editor.Id);
            contextCommands.Add(new CommandContextItem(openCommand));
        }

        // 添加在文件资源管理器中显示
        contextCommands.Add(new CommandContextItem(new OpenFolderCommand(project.Path)));

        // 获取编辑器图标并使用图标服务加载
        var sourceEditor = editors.Find(e => e.Id == project.SourceEditorId);
        IconData icon;

        if (sourceEditor != null)
        {
            // 如果设置了图标则使用设置的
            if (!string.IsNullOrEmpty(sourceEditor.Icon))
            {
                icon = IconService.LoadIcon(sourceEditor.Icon);
            }
            // 否则尝试从可执行文件提取图标
            else if (!string.IsNullOrEmpty(sourceEditor.ExecutablePath) && System.IO.File.Exists(sourceEditor.ExecutablePath))
            {
                icon = IconService.LoadIcon(sourceEditor.ExecutablePath);
            }
            else
            {
                icon = IconService.GetDefaultIcon();
            }
        }
        else
        {
            icon = IconService.GetDefaultIcon();
        }

        // 构建副标题，显示可用编辑器数量
        var subtitle = project.Path;
        if (project.AvailableEditorIds.Count > 1)
        {
            var editorNames = project.AvailableEditorIds
                .Select(id => editors.Find(e => e.Id == id)?.Name ?? id)
                .Take(3);
            subtitle = $"{project.Path} • {string.Join(", ", editorNames)}";
            if (project.AvailableEditorIds.Count > 3)
            {
                subtitle += $" +{project.AvailableEditorIds.Count - 3}";
            }
        }

        return new ListItem(defaultCommand)
        {
            Title = project.Name,
            Subtitle = subtitle,
            Icon = new IconInfo(icon),
            Section = sectionLabel,
            Tags = new[]
            {
                new Tag { Text = FormatLastOpened(project.LastOpened) }
            },
            MoreCommands = contextCommands.ToArray()
        };
    }

    private static string FormatLastOpened(DateTime lastOpened)
    {
        var diff = DateTime.Now - lastOpened;

        if (diff.TotalMinutes < 1)
            return "Just now";
        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours}h ago";
        if (diff.TotalDays < 7)
            return $"{(int)diff.TotalDays}d ago";
        if (diff.TotalDays < 30)
            return $"{(int)(diff.TotalDays / 7)}w ago";

        return lastOpened.ToString("MMM dd", System.Globalization.CultureInfo.InvariantCulture);
    }
}


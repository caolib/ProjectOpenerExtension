// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions.Toolkit;
using ProjectOpenerExtension.Models;
using ProjectOpenerExtension.Services;

namespace ProjectOpenerExtension.Commands;

/// <summary>
/// 在指定编辑器中打开项目的命令
/// </summary>
public partial class OpenProjectCommand : InvokableCommand
{
    private readonly ProjectInfo _project;
    private readonly string _editorId;

    public OpenProjectCommand(ProjectInfo project, string editorId)
    {
        _project = project;
        _editorId = editorId;

        var editor = DynamicSettingsManager.Instance.GetEditorConfigs().Find(e => e.Id == editorId);
        if (editor != null)
        {
            Name = $"使用 {editor.Name} 打开";

            // 如果设置了图标则使用设置的，否则从 exe 文件提取
            if (!string.IsNullOrWhiteSpace(editor.Icon))
            {
                Icon = new(editor.Icon);
            }
            else if (!string.IsNullOrWhiteSpace(editor.ExecutablePath) && System.IO.File.Exists(editor.ExecutablePath))
            {
                Icon = new($"{editor.ExecutablePath},0");
            }
            else
            {
                Icon = new("📁");
            }
        }
        else
        {
            Name = "打开";
            Icon = new("📁");
        }
    }

    public override CommandResult Invoke()
    {
        var editor = DynamicSettingsManager.Instance.GetEditorConfigs().Find(e => e.Id == _editorId);
        if (editor == null)
        {
            return CommandResult.Dismiss();
        }

        if (editor.Type == EditorType.VSCode)
        {
            VSCodeProjectService.OpenInEditor(_project.Path, _editorId);
        }
        else
        {
            JetBrainsProjectService.OpenInJetBrainsIDE(_project.Path, _editorId);
        }

        return CommandResult.Dismiss();
    }
}

/// <summary>
/// 在文件资源管理器中打开项目文件夹
/// </summary>
public partial class OpenFolderCommand : InvokableCommand
{
    private readonly string _path;

    public OpenFolderCommand(string path)
    {
        _path = path;
        Name = "在文件资源管理器中显示";
        Icon = new("📂");
    }

    public override CommandResult Invoke()
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{_path}\"",
            UseShellExecute = true
        });
        return CommandResult.Dismiss();
    }
}



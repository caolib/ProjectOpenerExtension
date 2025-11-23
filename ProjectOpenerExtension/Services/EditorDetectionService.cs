// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace ProjectOpenerExtension.Services;

/// <summary>
/// 自动检测系统中已安装的编辑器
/// </summary>
public static class EditorDetectionService
{
    /// <summary>
    /// 检测所有已安装的编辑器
    /// </summary>
    public static List<EditorDefinition> DetectInstalledEditors()
    {
        var editors = new List<EditorDefinition>();

        // 检测 VS Code
        var vscode = DetectVSCode();
        if (vscode != null) editors.Add(vscode);

        // 检测 IntelliJ IDEA
        var idea = DetectIntelliJIDEA();
        if (idea != null) editors.Add(idea);

        return editors;
    }

    /// <summary>
    /// 检测 VS Code
    /// </summary>
    private static EditorDefinition? DetectVSCode()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        // 检查常见安装位置
        var possiblePaths = new[]
        {
            Path.Combine(localAppData, "Programs", "Microsoft VS Code", "Code.exe"),
            Path.Combine(programFiles, "Microsoft VS Code", "Code.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft VS Code", "Code.exe")
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                var storagePath = Path.Combine(appData, "Code", "User", "globalStorage", "storage.json");

                return new EditorDefinition
                {
                    Name = "VS Code",
                    Enabled = true,
                    Icon = "", // 可以后续添加图标路径
                    ExecutablePath = path,
                    ProjectPath = storagePath,
                    EditorType = "vscode"
                };
            }
        }

        // 尝试从注册表查找
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            if (key != null)
            {
                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    using var subKey = key.OpenSubKey(subKeyName);
                    var displayName = subKey?.GetValue("DisplayName") as string;

                    if (displayName != null && displayName.Contains("Visual Studio Code"))
                    {
                        var installLocation = subKey?.GetValue("InstallLocation") as string;
                        if (!string.IsNullOrEmpty(installLocation))
                        {
                            var exePath = Path.Combine(installLocation, "Code.exe");
                            if (File.Exists(exePath))
                            {
                                var storagePath = Path.Combine(appData, "Code", "User", "globalStorage", "storage.json");

                                return new EditorDefinition
                                {
                                    Name = "VS Code",
                                    Enabled = true,
                                    Icon = "",
                                    ExecutablePath = exePath,
                                    ProjectPath = storagePath,
                                    EditorType = "vscode"
                                };
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // 忽略注册表访问错误
        }

        return null;
    }

    /// <summary>
    /// 检测 IntelliJ IDEA
    /// </summary>
    private static EditorDefinition? DetectIntelliJIDEA()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        // 检查常见安装位置
        var possibleBasePaths = new[]
        {
            Path.Combine(programFiles, "JetBrains"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "JetBrains")
        };

        foreach (var basePath in possibleBasePaths)
        {
            if (Directory.Exists(basePath))
            {
                // 查找 IDEA 目录
                var ideaDirs = Directory.GetDirectories(basePath, "IntelliJ IDEA*");
                if (ideaDirs.Length > 0)
                {
                    // 取最新版本（按目录名排序，通常最新版本在最后）
                    Array.Sort(ideaDirs);
                    var ideaDir = ideaDirs[^1];

                    var exePath = Path.Combine(ideaDir, "bin", "idea64.exe");
                    if (File.Exists(exePath))
                    {
                        var configPath = Path.Combine(localAppData, "JetBrains");

                        return new EditorDefinition
                        {
                            Name = "IntelliJ IDEA",
                            Enabled = true,
                            Icon = "",
                            ExecutablePath = exePath,
                            ProjectPath = configPath,
                            EditorType = "jetbrains"
                        };
                    }
                }
            }
        }

        // 尝试从注册表查找
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\JetBrains\IntelliJ IDEA");
            if (key != null)
            {
                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    using var subKey = key.OpenSubKey(subKeyName);
                    var installLocation = subKey?.GetValue("") as string;

                    if (!string.IsNullOrEmpty(installLocation))
                    {
                        var exePath = Path.Combine(installLocation, "bin", "idea64.exe");
                        if (File.Exists(exePath))
                        {
                            var configPath = Path.Combine(localAppData, "JetBrains");

                            return new EditorDefinition
                            {
                                Name = "IntelliJ IDEA",
                                Enabled = true,
                                Icon = "",
                                ExecutablePath = exePath,
                                ProjectPath = configPath,
                                EditorType = "jetbrains"
                            };
                        }
                    }
                }
            }
        }
        catch
        {
            // 忽略注册表访问错误
        }

        return null;
    }
}

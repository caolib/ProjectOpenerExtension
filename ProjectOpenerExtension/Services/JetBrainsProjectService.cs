// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ProjectOpenerExtension.Models;

namespace ProjectOpenerExtension.Services;

/// <summary>
/// JetBrains IDE 最近项目读取服务
/// </summary>
public class JetBrainsProjectService
{
    private readonly DynamicSettingsManager _settingsService;

    public JetBrainsProjectService()
    {
        _settingsService = DynamicSettingsManager.Instance;
    }

    public List<ProjectInfo> GetRecentProjects()
    {
        var projects = new List<ProjectInfo>();
        var jetbrainsEditors = _settingsService.GetEnabledEditors()
            .Where(e => e.Type == EditorType.IntelliJIdea && !string.IsNullOrEmpty(e.ConfigFolderPattern))
            .ToList();

        foreach (var editor in jetbrainsEditors)
        {
            try
            {
                var configPath = editor.ConfigFolderPattern;

                // 如果是目录，搜索所有版本的 IDEA
                if (Directory.Exists(configPath))
                {
                    // 搜索所有 IntelliJ IDEA 版本目录
                    var ideaDirs = Directory.GetDirectories(configPath, "IntelliJIdea*");

                    foreach (var ideaDir in ideaDirs)
                    {
                        var recentProjectsFile = Path.Combine(ideaDir, "options", "recentProjects.xml");
                        if (File.Exists(recentProjectsFile))
                        {
                            Debug.WriteLine($"[JetBrains] 找到配置文件: {recentProjectsFile}");
                            var editorProjects = ParseRecentProjects(recentProjectsFile, editor.Id);

                            foreach (var project in editorProjects)
                            {
                                var existing = projects.FirstOrDefault(p => p.Path == project.Path);
                                if (existing != null)
                                {
                                    if (!existing.AvailableEditorIds.Contains(editor.Id))
                                    {
                                        existing.AvailableEditorIds.Add(editor.Id);
                                    }
                                }
                                else
                                {
                                    projects.Add(project);
                                }
                            }
                        }
                    }
                }
                // 如果直接是文件，直接解析
                else if (File.Exists(configPath))
                {
                    Debug.WriteLine($"[JetBrains] 直接解析文件: {configPath}");
                    var editorProjects = ParseRecentProjects(configPath, editor.Id);
                    projects.AddRange(editorProjects);
                }
                else
                {
                    Debug.WriteLine($"[JetBrains] 路径不存在: {configPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading {editor.Name} projects: {ex.Message}");
            }
        }

        return projects;
    }

    private List<ProjectInfo> ParseRecentProjects(string xmlPath, string editorId)
    {
        var projects = new List<ProjectInfo>();

        try
        {
            var doc = XDocument.Load(xmlPath);
            var entries = doc.Descendants("entry");

            foreach (var entry in entries)
            {
                var keyAttr = entry.Attribute("key");
                if (keyAttr != null)
                {
                    var path = keyAttr.Value.Replace("$USER_HOME$", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                    path = path.Replace('/', Path.DirectorySeparatorChar);

                    if (Directory.Exists(path))
                    {
                        projects.Add(new ProjectInfo
                        {
                            Name = Path.GetFileName(path),
                            Path = path,
                            AvailableEditorIds = new List<string> { editorId },
                            LastOpened = Directory.GetLastWriteTime(path),
                            SourceEditorId = editorId
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error parsing recent projects XML: {ex.Message}");
        }

        return projects;
    }

    public static void OpenInJetBrainsIDE(string projectPath, string editorId)
    {
        try
        {
            var editor = DynamicSettingsManager.Instance.GetEditorConfigs().Find(e => e.Id == editorId);
            if (editor == null)
            {
                Debug.WriteLine($"Editor not found: {editorId}");
                return;
            }

            var args = string.Format(System.Globalization.CultureInfo.InvariantCulture, editor.CommandLineArgs, projectPath);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = editor.ExecutablePath,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening JetBrains IDE: {ex.Message}");
        }
    }
}

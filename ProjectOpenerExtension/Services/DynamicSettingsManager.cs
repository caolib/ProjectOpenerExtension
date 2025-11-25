// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ProjectOpenerExtension.Models;

namespace ProjectOpenerExtension.Services;

/// <summary>
/// 配置管理器 - 完全基于 JSON 配置文件，无硬编码编辑器
/// </summary>
public class DynamicSettingsManager : JsonSettingsManager
{
    private static DynamicSettingsManager _instance;
    private static readonly string _namespace = "projectopener";
    private string _configFilePath;

    private List<EditorDefinition> _editors;
    private TextSetting _configPathSetting;

    public static DynamicSettingsManager Instance => _instance ??= new DynamicSettingsManager();

    public List<EditorDefinition> Editors => _editors;

    private DynamicSettingsManager()
    {
        FilePath = GetSettingsFilePath();

        // 获取用户设置的配置文件路径（从持久化文件读取）
        _configFilePath = GetUserConfigFilePath();
        System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 配置文件路径: {_configFilePath}");

        // 加载编辑器配置（不自动创建）
        _editors = LoadEditors();

        // Add config file path setting (empty on first use)
        _configPathSetting = new TextSetting(
            Namespaced("config_file_path"),
            "Configuration File Path | 配置文件路径",
            "Specify the full path to editors.json configuration file. Create and configure this file on first use.\n指定 editors.json 配置文件的完整路径。首次使用请创建并配置此文件。\n\nFor configuration format and examples, visit:\n配置文件格式和示例请访问:\nhttps://github.com/caolib/ProjectOpenerExtension#configuration",
            string.Empty  // Empty on first use
        );
        Settings.Add(_configPathSetting);

        // 加载设置以显示当前值
        LoadSettings();

        // 监听设置变化
        Settings.SettingsChanged += (s, e) =>
        {
            SaveSettings(); // 保存设置
            var newPath = _configPathSetting.Value;
            if (!string.IsNullOrEmpty(newPath) && newPath != _configFilePath)
            {
                _configFilePath = newPath;
                _editors = LoadEditors();
                System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 配置文件路径已更新: {newPath}");
            }
        };
    }

    /// <summary>
    /// 获取用户设置的配置文件路径(从持久化文件读取)
    /// </summary>
    private string GetUserConfigFilePath()
    {
        try
        {
            var settingsFile = GetSettingsFilePath();
            if (File.Exists(settingsFile))
            {
                var json = File.ReadAllText(settingsFile);
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    var key = Namespaced("config_file_path");
                    if (root.TryGetProperty(key, out JsonElement value))
                    {
                        var path = value.GetString();
                        if (!string.IsNullOrEmpty(path))
                        {
                            return path;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 读取配置路径失败: {ex.Message}");
        }

        // 首次使用时返回空字符串，不提供默认路径
        return string.Empty;
    }

    /// <summary>
    /// 获取默认配置文件路径
    /// </summary>
    private string GetDefaultConfigFilePath()
    {
        bool isMsix = IsMsixPackage();

        if (isMsix)
        {
            try
            {
                var packageFamilyName = GetPackageFamilyName();
                if (!string.IsNullOrEmpty(packageFamilyName))
                {
                    var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    return Path.Combine(
                        localAppData,
                        "Packages",
                        packageFamilyName,
                        "LocalCache",
                        "Local",
                        "ProjectOpenerExtension",
                        "editors.json"
                    );
                }
            }
            catch { }

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ProjectOpenerExtension",
                "editors.json"
            );
        }
        else
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config",
                "ProjectOpenerExtension",
                "editors.json"
            );
        }
    }

    /// <summary>
    /// 获取当前 MSIX 包的 PackageFamilyName
    /// </summary>
    private static string GetPackageFamilyName()
    {
        try
        {
            // 方法1: 环境变量
            var pfn = Environment.GetEnvironmentVariable("PACKAGE_FAMILY_NAME");
            if (!string.IsNullOrEmpty(pfn))
            {
                return pfn;
            }

            // 方法2: 从进程路径解析
            var processPath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(processPath) && processPath.Contains(@"\WindowsApps\"))
            {
                // 路径格式: C:\Program Files\WindowsApps\{PackageFamilyName}\...
                var parts = processPath.Split(new[] { @"\WindowsApps\" }, StringSplitOptions.None);
                if (parts.Length > 1)
                {
                    var packagePath = parts[1];
                    var packageName = packagePath.Split('\\')[0];
                    return packageName;
                }
            }

            // 方法3: 检查 Packages 目录下是否存在匹配的包
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var packagesDir = Path.Combine(localAppData, "Packages");

            if (Directory.Exists(packagesDir))
            {
                var packageDirs = Directory.GetDirectories(packagesDir, "*ProjectOpenerExtension*");
                if (packageDirs.Length > 0)
                {
                    return Path.GetFileName(packageDirs[0]);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProjectOpener] GetPackageFamilyName 失败: {ex.Message}");
        }

        return string.Empty;
    }

    /// <summary>
    /// 从配置文件加载编辑器列表
    /// </summary>
    private List<EditorDefinition> LoadEditors()
    {
        try
        {
            if (string.IsNullOrEmpty(_configFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectOpener] ⚠ 未设置配置文件路径");
                return new List<EditorDefinition>();
            }

            System.Diagnostics.Debug.WriteLine($"[ProjectOpener] LoadEditors: 开始加载配置文件 {_configFilePath}");

            if (!File.Exists(_configFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectOpener] ⚠ 配置文件不存在: {_configFilePath}");
                return new List<EditorDefinition>();
            }

            var json = File.ReadAllText(_configFilePath);
            System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 配置文件内容: {json}");

            var editors = JsonSerializer.Deserialize<List<EditorDefinition>>(json) ?? new List<EditorDefinition>();
            System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 成功加载 {editors.Count} 个编辑器");

            foreach (var editor in editors)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectOpener] - {editor.Name}: Enabled={editor.Enabled}, Type={editor.EditorType}, Path={editor.ProjectPath}");
            }

            return editors;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 加载配置失败: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 堆栈: {ex.StackTrace}");
            return new List<EditorDefinition>();
        }
    }

    /// <summary>
    /// 获取编辑器配置列表（向后兼容）
    /// 每次调用都重新加载配置文件以确保最新
    /// </summary>
    public List<EditorConfig> GetEditorConfigs()
    {
        // 重新加载配置文件
        _editors = LoadEditors();

        var configs = new List<EditorConfig>();

        System.Diagnostics.Debug.WriteLine($"[ProjectOpener] GetEditorConfigs: 共有 {_editors.Count} 个编辑器");

        foreach (var editor in _editors.Where(e => e.Enabled))
        {
            System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 处理编辑器: {editor.Name}, Type: {editor.EditorType}, Path: {editor.ProjectPath}");

            var config = new EditorConfig
            {
                Id = editor.Name.ToLowerInvariant().Replace(" ", "_"),
                Name = editor.Name,
                ExecutablePath = editor.ExecutablePath,
                Icon = editor.Icon,
                IsEnabled = editor.Enabled
            };

            if (editor.EditorType.Equals("vscode", StringComparison.OrdinalIgnoreCase))
            {
                config.Type = EditorType.VSCode;
                config.StorageFilePath = editor.ProjectPath;
                System.Diagnostics.Debug.WriteLine($"[ProjectOpener] VS Code 编辑器 - StorageFilePath: {config.StorageFilePath}");
            }
            else if (editor.EditorType.Equals("jetbrains", StringComparison.OrdinalIgnoreCase))
            {
                config.Type = EditorType.IntelliJIdea;
                config.ConfigFolderPattern = editor.ProjectPath;
                System.Diagnostics.Debug.WriteLine($"[ProjectOpener] JetBrains 编辑器 - ConfigFolderPattern: {config.ConfigFolderPattern}");
            }

            configs.Add(config);
        }

        System.Diagnostics.Debug.WriteLine($"[ProjectOpener] GetEditorConfigs 返回 {configs.Count} 个配置");
        return configs;
    }

    /// <summary>
    /// 检测是否运行在 MSIX 包中
    /// </summary>
    private static bool IsMsixPackage()
    {
        try
        {
            // 方法1: 检查 PACKAGE_FAMILY_NAME 环境变量
            var packageFamilyName = Environment.GetEnvironmentVariable("PACKAGE_FAMILY_NAME");
            if (!string.IsNullOrEmpty(packageFamilyName))
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectOpener] MSIX 检测: PACKAGE_FAMILY_NAME = {packageFamilyName}");
                return true;
            }

            // 方法2: 检查进程是否在 WindowsApps 目录下运行
            var processPath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(processPath) && processPath.Contains(@"\WindowsApps\"))
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectOpener] MSIX 检测: 进程在 WindowsApps 目录 - {processPath}");
                return true;
            }

            // 方法3: 检查是否存在 AppxManifest.xml
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var manifestPath = Path.Combine(appDirectory, "AppxManifest.xml");
            if (File.Exists(manifestPath))
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectOpener] MSIX 检测: 找到 AppxManifest.xml");
                return true;
            }

            System.Diagnostics.Debug.WriteLine($"[ProjectOpener] MSIX 检测: 非 MSIX 环境");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProjectOpener] MSIX 检测失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 获取 PowerToys 设置文件路径
    /// </summary>
    private static string GetSettingsFilePath()
    {
        bool isMsix = IsMsixPackage();

        string settingsFolder;
        if (isMsix)
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            settingsFolder = Path.Combine(localAppData, "ProjectOpenerExtension");
        }
        else
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            settingsFolder = Path.Combine(userProfile, ".config", "ProjectOpenerExtension");
        }

        Directory.CreateDirectory(settingsFolder);
        return Path.Combine(settingsFolder, "powertoys-settings.json");
    }

    private static string Namespaced(string propertyName) => $"{_namespace}.{propertyName}";
}

/// <summary>
/// 编辑器配置定义 - 对应用户配置文件格式
/// </summary>
public class EditorDefinition
{
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public string Icon { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string ProjectPath { get; set; } = string.Empty;
    public string EditorType { get; set; } = string.Empty; // "vscode" or "jetbrains"
}

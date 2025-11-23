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
    private readonly string _configFilePath;

    private List<EditorDefinition> _editors;

    public static DynamicSettingsManager Instance => _instance ??= new DynamicSettingsManager();

    public List<EditorDefinition> Editors => _editors;

    private DynamicSettingsManager()
    {
        FilePath = GetSettingsFilePath();
        _configFilePath = GetConfigFilePath();

        System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 构造函数 - 初始配置文件路径: {_configFilePath}");

        // 初始化配置文件（如果不存在则创建）
        InitializeConfigFile();

        // 获取实际存在的配置文件路径（可能被MSIX重定向）
        var actualConfigPath = FindActualConfigPath();
        System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 构造函数 - 实际配置文件路径: {actualConfigPath}");

        // 使用实际路径
        _configFilePath = actualConfigPath;

        // 加载编辑器配置
        _editors = LoadEditors();

        // 添加配置文件路径显示
        var fileExistsInfo = File.Exists(_configFilePath) ? "✓ 配置文件已存在" : "✗ 配置文件不存在";

        // 获取配置文件所在目录
        var configDirectory = Path.GetDirectoryName(_configFilePath) ?? _configFilePath;

        var configPathInfo = new TextSetting(
            Namespaced("config_path"),
            "编辑器配置文件",
            $"{fileExistsInfo}\n\n配置文件位置:\n{_configFilePath}\n\n提示：\n- 直接用文本编辑器打开上述文件进行编辑\n- 或在文件资源管理器中打开以下目录:\n{configDirectory}",
            configDirectory  // 传递目录路径，点击文件夹图标会打开目录
        );
        Settings.Add(configPathInfo);

        // 加载设置
        LoadSettings();
    }

    /// <summary>
    /// 查找实际的配置文件路径
    /// </summary>
    private string FindActualConfigPath()
    {
        var standardPath = GetConfigFilePath();
        System.Diagnostics.Debug.WriteLine($"[ProjectOpener] FindActualConfigPath - 标准路径: {standardPath}");
        System.Diagnostics.Debug.WriteLine($"[ProjectOpener] FindActualConfigPath - 文件存在: {File.Exists(standardPath)}");

        return standardPath;
    }

    /// <summary>
    /// 初始化配置文件，如果不存在则创建并自动检测编辑器
    /// </summary>
    private void InitializeConfigFile()
    {
        try
        {
            // 确保目录存在
            var directory = Path.GetDirectoryName(_configFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 创建配置目录: {directory}");
            }

            // 如果配置文件不存在，创建并自动检测编辑器
            if (!File.Exists(_configFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 配置文件不存在，开始初始化: {_configFilePath}");

                // 自动检测已安装的编辑器
                var detectedEditors = EditorDetectionService.DetectInstalledEditors();
                System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 检测到 {detectedEditors.Count} 个编辑器");

                // 如果没有检测到任何编辑器，创建空配置
                if (detectedEditors.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[ProjectOpener] 未检测到编辑器，创建空配置文件");
                }

                // 保存到配置文件
                var json = JsonSerializer.Serialize(detectedEditors, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_configFilePath, json);
                System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 配置文件已创建: {_configFilePath}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 配置文件已存在: {_configFilePath}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 初始化配置文件失败: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 堆栈跟踪: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// 从配置文件加载编辑器列表
    /// </summary>
    private List<EditorDefinition> LoadEditors()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProjectOpener] LoadEditors: 开始加载配置文件 {_configFilePath}");

            if (!File.Exists(_configFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectOpener] 配置文件不存在");
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
    /// 打开配置文件
    /// </summary>
    private void OpenConfigFile()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _configFilePath,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open config file: {ex.Message}");
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
    /// 获取启用的编辑器（向后兼容）
    /// </summary>
    public List<EditorConfig> GetEnabledEditors()
    {
        return GetEditorConfigs();
    }

    /// <summary>
    /// 获取配置文件路径 - 使用标准用户目录，避免MSIX虚拟化
    /// </summary>
    private static string GetConfigFilePath()
    {
        // 使用 UserProfile 而不是 LocalApplicationData，避免MSIX虚拟化
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var configPath = Path.Combine(userProfile, ".config", "ProjectOpenerExtension", "editors.json");

        System.Diagnostics.Debug.WriteLine($"[ProjectOpener] GetConfigFilePath - UserProfile: {userProfile}");
        System.Diagnostics.Debug.WriteLine($"[ProjectOpener] GetConfigFilePath - 配置路径: {configPath}");

        return configPath;
    }

    /// <summary>
    /// 获取 PowerToys 设置文件路径
    /// </summary>
    private static string GetSettingsFilePath()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var settingsFolder = Path.Combine(userProfile, ".config", "ProjectOpenerExtension");
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

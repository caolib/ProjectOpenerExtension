// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace ProjectOpenerExtension.Services;

/// <summary>
/// 图标服务 - 从文件加载图标
/// </summary>
public static class IconService
{
    /// <summary>
    /// 从路径加载图标
    /// 支持: .png, .ico, .jpg 文件 (相对路径基于应用程序目录)
    /// 也支持从 .exe 文件提取图标 (格式: "path.exe,index")
    /// </summary>
    public static IconData LoadIcon(string iconPath)
    {
        if (string.IsNullOrEmpty(iconPath))
        {
            return GetDefaultIcon();
        }

        try
        {
            // 如果是相对路径,转换为绝对路径
            var fullPath = Path.IsPathRooted(iconPath)
                ? iconPath
                : Path.Combine(AppContext.BaseDirectory, iconPath);

            // 检查文件是否存在
            if (!File.Exists(fullPath))
            {
                System.Diagnostics.Debug.WriteLine($"Icon file not found: {fullPath}, using default");
                return GetDefaultIcon();
            }

            var extension = Path.GetExtension(fullPath).ToLowerInvariant();

            // 对于图片文件,直接使用路径
            if (extension == ".png" || extension == ".ico" || extension == ".jpg" || extension == ".jpeg")
            {
                return new IconData(fullPath);
            }

            // 对于可执行文件,从 exe 提取图标 (使用索引 0)
            if (extension == ".exe" || extension == ".dll")
            {
                return new IconData($"{fullPath},0");
            }

            System.Diagnostics.Debug.WriteLine($"Unsupported icon file format: {extension}, using default");
            return GetDefaultIcon();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading icon from {iconPath}: {ex.Message}");
            return GetDefaultIcon();
        }
    }

    /// <summary>
    /// 获取默认图标 (当无法加载自定义图标时使用)
    /// </summary>
    public static IconData GetDefaultIcon()
    {
        // 使用默认的应用程序图标
        return new IconData("📦");
    }
}

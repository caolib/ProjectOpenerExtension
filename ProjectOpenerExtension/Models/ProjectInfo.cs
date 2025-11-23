// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace ProjectOpenerExtension.Models;

/// <summary>
/// 编辑器类型
/// </summary>
public enum EditorType
{
    VSCode,
    VisualStudio,
    Rider,
    WebStorm,
    IntelliJIdea,
    PyCharm,
    GoLand,
    PhpStorm,
    RubyMine,
    CLion,
    DataGrip,
    Fleet,
}

/// <summary>
/// 项目信息
/// </summary>
public class ProjectInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public List<string> AvailableEditorIds { get; set; } = new List<string>(); // 改为编辑器ID列表
    public DateTime LastOpened { get; set; }
    public string SourceEditorId { get; set; } = string.Empty; // 来源编辑器ID
}

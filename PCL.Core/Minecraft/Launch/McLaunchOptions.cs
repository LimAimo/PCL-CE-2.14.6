using System;
using System.Collections.Generic;

namespace PCL.Core.Minecraft.Launch;

/// <summary>
/// Minecraft 启动选项。
/// </summary>
public sealed class McLaunchOptions
{
    /// <summary>
    /// 强制指定启动的实例。为 null 时使用当前选中实例。
    /// </summary>
    public Instance.McInstance? Instance { get; set; }

    /// <summary>
    /// 强制指定启动后进入的服务器 IP。为 null 时使用实例设置。
    /// </summary>
    public string? ServerIp { get; set; }

    /// <summary>
    /// 指定启动后进入的存档名称。为 null 时不指定。
    /// </summary>
    public string? WorldName { get; set; }

    /// <summary>
    /// 额外的启动参数。
    /// </summary>
    public List<string> ExtraArgs { get; } = new();

    /// <summary>
    /// 是否为"测试游戏"按钮启动（显示实时日志）。
    /// </summary>
    public bool IsTest { get; set; }

    /// <summary>
    /// 将启动脚本保存到该路径，然后取消实际启动。为 null 时不保存。
    /// </summary>
    public string? SaveBatchPath { get; set; }
}

/// <summary>
/// 启动状态。
/// </summary>
public enum LaunchState
{
    Idle,
    Preparing,
    Launching,
    Running,
    Failed,
    Finished
}

using System;
using PCL.Core.App.Essentials;
using PCL.Core.UI;
using PCL.Core.UI.Animation.UIAccessProvider;
using PCL.Core.Utils.OS;

namespace PCL.Core.App;

/// <summary>
/// 平台服务定位器。<br/>
/// 由框架特定实现（如 PCL.Core.Wpf）在应用程序初始化阶段设置，
/// 供 PCL.Core 中的静态方法调用以访问平台相关能力（消息框、剪贴板、文件对话框等）。
/// </summary>
public static class Platform
{
    /// <summary>
    /// 消息框服务。
    /// </summary>
    public static IMessageBoxService MessageBox { get; set; } = null!;

    /// <summary>
    /// 应用程序打包资源提供者。
    /// </summary>
    public static IResourceProvider Resources { get; set; } = null!;

    /// <summary>
    /// 窗口激活服务。
    /// </summary>
    public static IWindowActivationService WindowActivation { get; set; } = null!;

    /// <summary>
    /// 剪贴板服务。
    /// </summary>
    public static IClipboardService Clipboard { get; set; } = null!;

    /// <summary>
    /// 文件对话框服务。
    /// </summary>
    public static IFileDialogService FileDialogs { get; set; } = null!;

    /// <summary>
    /// 应用程序资源字典。
    /// </summary>
    public static IResourceDictionary ResourceDictionary { get; set; } = null!;

    /// <summary>
    /// UI 线程访问提供者。
    /// </summary>
    public static IUIAccessProvider UIAccess { get; set; } = null!;
}

using System;
using System.Windows;
using PCL.Core.App;
using PCL.Core.App.Essentials;
using PCL.Core.UI;
using PCL.Core.UI.Animation.UIAccessProvider;
using PCL.Core.Utils.OS;

namespace PCL.Core.App.Essentials;

/// <summary>
/// WPF 平台服务初始化器。<br/>
/// 在 <see cref="ApplicationService"/> 创建 Application 实例后调用，
/// 将所有 WPF 实现注册到 <see cref="Platform"/> 服务定位器。
/// </summary>
public static class WpfPlatformInitializer
{
    /// <summary>
    /// 使用指定的 WPF <see cref="Application"/> 实例初始化所有平台服务。
    /// </summary>
    /// <param name="app">WPF Application 实例</param>
    public static void Initialize(Application app)
    {
        ArgumentNullException.ThrowIfNull(app);

        Platform.UIAccess = new WpfUIAccessProvider(app.Dispatcher);
        Platform.MessageBox = new WpfMessageBoxService();
        Platform.Resources = new WpfResourceProvider();
        Platform.WindowActivation = new WpfWindowActivationService();
        Platform.Clipboard = new WpfClipboardService();
        Platform.FileDialogs = new WpfFileDialogService();
        Platform.ResourceDictionary = new WpfResourceDictionary();
    }
}

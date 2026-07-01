using System;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using PCL.Core.App;
using PCL.Core.App.Essentials;
using PCL.Core.UI;
using PCL.Core.UI.Animation.UIAccessProvider;
using PCL.Core.Utils.OS;

namespace PCL.Core.App.Essentials;

/// <summary>
/// WinUI 3 平台服务初始化器。<br/>
/// 在 Application 实例创建后将所有 WinUI 实现注册到 <see cref="Platform"/> 服务定位器。
/// </summary>
public static class WinUIPlatformInitializer
{
    public static void Initialize(Application app)
    {
        ArgumentNullException.ThrowIfNull(app);
        var dispatcherQueue = DispatcherQueue.GetForCurrentThread()
            ?? throw new InvalidOperationException("无法获取 DispatcherQueue");
        Platform.UIAccess = new WinUIUIAccessProvider(dispatcherQueue);
        Platform.MessageBox = new WinUIMessageBoxService();
        Platform.Resources = new WinUIResourceProvider();
        Platform.WindowActivation = new WinUIWindowActivationService();
        Platform.Clipboard = new WinUIClipboardService();
        Platform.FileDialogs = new WinUIFileDialogService();
        Platform.ResourceDictionary = new WinUIResourceDictionary();
    }
}

using System;
using System.Windows;
using System.Windows.Threading;
using PCL.Core.App.Essentials;

namespace PCL.Core.App.Essentials;

/// <summary>
/// WPF 窗口激活服务实现。<br/>
/// 封装主窗口的 Dispatcher 切换 + WindowState/Topmost/Activate 逻辑，
/// 替代 RpcService 中直接操作 WPF Window 的代码。
/// </summary>
public sealed class WpfWindowActivationService : IWindowActivationService
{
    /// <inheritdoc/>
    public void ActivateMainWindow()
    {
        var app = Application.Current;
        if (app == null) return;
        app.Dispatcher.BeginInvoke(() =>
        {
            var window = app.MainWindow;
            if (window == null) return;
            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;
            if (!window.Topmost)
            {
                window.Topmost = true;
                window.Topmost = false;
            }
            window.Activate();
        }, DispatcherPriority.Send);
    }
}

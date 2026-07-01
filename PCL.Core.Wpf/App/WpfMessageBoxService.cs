using System;
using System.Windows;
using PCL.Core.App.Essentials;

namespace PCL.Core.App.Essentials;

/// <summary>
/// WPF 消息框服务实现。<br/>
/// 处理线程亲和性：若调用方不在 UI 线程，自动切换到 UI 线程执行。
/// </summary>
public sealed class WpfMessageBoxService : IMessageBoxService
{
    /// <inheritdoc/>
    public void Show(string message, string caption, bool isError)
    {
        var image = isError ? MessageBoxImage.Error : MessageBoxImage.Information;
        var app = Application.Current;
        if (app?.Dispatcher?.CheckAccess() == false)
        {
            app.Dispatcher.Invoke(() => System.Windows.MessageBox.Show(message, caption, MessageBoxButton.OK, image));
        }
        else
        {
            System.Windows.MessageBox.Show(message, caption, MessageBoxButton.OK, image);
        }
    }
}

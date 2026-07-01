using System;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PCL.Core.App.IoC;
using PCL.Core.App.Essentials;

namespace PCL.Core.App.Essentials;

/// <summary>
/// WinUI 3 消息框服务实现，使用 <see cref="ContentDialog"/>。
/// </summary>
public sealed class WinUIMessageBoxService : IMessageBoxService
{
    public void Show(string message, string caption, bool isError)
    {
        var host = Lifecycle.CurrentApplication as WinUIApplicationHost;
        var app = host?.Application;
        if (app == null)
        {
            // 应用尚未启动，无法显示对话框
            Console.Error.WriteLine($"[{caption}] {message}");
            return;
        }
        var dispatcher = DispatcherQueue.GetForCurrentThread()
            ?? throw new InvalidOperationException("无法获取 DispatcherQueue");
        _ = dispatcher.TryEnqueue(async () =>
        {
            var dialog = new ContentDialog
            {
                Title = caption,
                Content = message,
                CloseButtonText = "确定",
                XamlRoot = host?.MainWindow is WinUIWindowHost wh
                    ? wh.Window.Content?.XamlRoot
                    : null
            };
            if (isError)
            {
                dialog.DefaultButton = ContentDialogButton.Close;
            }
            await dialog.ShowAsync();
        });
    }
}

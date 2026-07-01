using System;
using System.Windows;
using PCL.Core.App.Essentials;

namespace PCL.Core.App.Essentials;

/// <summary>
/// WPF 窗口宿主实现，包装 <see cref="Window"/>。
/// </summary>
public sealed class WpfWindowHost : IWindowHost
{
    private readonly Window _window;

    public WpfWindowHost(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
    }

    /// <summary>
    /// 底层 WPF <see cref="Window"/> 实例，供 PCL.Core.Wpf 内部使用。
    /// </summary>
    public Window Window => _window;

    /// <inheritdoc/>
    public void Show() => _window.Show();
}

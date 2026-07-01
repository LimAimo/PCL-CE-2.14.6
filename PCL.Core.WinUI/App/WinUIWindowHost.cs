using System;
using Microsoft.UI.Xaml;
using PCL.Core.App.Essentials;

namespace PCL.Core.App.Essentials;

/// <summary>
/// WinUI 3 窗口宿主实现，包装 <see cref="Window"/>。
/// </summary>
public sealed class WinUIWindowHost : IWindowHost
{
    private readonly Window _window;

    public WinUIWindowHost(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
    }

    /// <summary>
    /// 底层 WinUI <see cref="Window"/> 实例，供 PCL.Core.WinUI 内部使用。
    /// </summary>
    public Window Window => _window;

    /// <inheritdoc/>
    public void Show() => _window.Activate();
}

using System;
using System.Collections.Generic;
using System.Windows.Interop;
using PCL.Core.Utils.OS;

namespace PCL.Core.Utils.OS;

/// <summary>
/// WPF 窗口句柄提供者实现，封装 <see cref="HwndSource"/>。<br/>
/// 将 <see cref="WindowHook"/> 委托适配为 <see cref="HwndSourceHook"/> 委托。
/// </summary>
public sealed class WpfWindowHandleProvider : IWindowHandleProvider
{
    private readonly HwndSource _hwndSource;
    private readonly Dictionary<WindowHook, HwndSourceHook> _hookWrappers = new();

    public WpfWindowHandleProvider(HwndSource hwndSource)
    {
        _hwndSource = hwndSource ?? throw new ArgumentNullException(nameof(hwndSource));
    }

    /// <inheritdoc/>
    public IntPtr Handle => _hwndSource.Handle;

    /// <inheritdoc/>
    public void AddHook(WindowHook hook)
    {
        HwndSourceHook wrapper = (hwnd, msg, wParam, lParam, ref handled) =>
            hook(hwnd, msg, wParam, lParam, ref handled);
        _hookWrappers[hook] = wrapper;
        _hwndSource.AddHook(wrapper);
    }

    /// <inheritdoc/>
    public void RemoveHook(WindowHook hook)
    {
        if (_hookWrappers.TryGetValue(hook, out var wrapper))
        {
            _hwndSource.RemoveHook(wrapper);
            _hookWrappers.Remove(hook);
        }
    }
}

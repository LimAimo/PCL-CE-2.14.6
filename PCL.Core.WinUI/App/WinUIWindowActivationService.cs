using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using PCL.Core.App.Essentials;
using PCL.Core.App.IoC;
using WinRT.Interop;

namespace PCL.Core.App.Essentials;

/// <summary>
/// WinUI 3 窗口激活服务实现。<br/>
/// 使用 Win32 API 实现最小化还原与置前逻辑，因为 WinUI Window 没有直接的 Topmost/WindowState API。
/// </summary>
public sealed partial class WinUIWindowActivationService : IWindowActivationService
{
    public void ActivateMainWindow()
    {
        var host = Lifecycle.CurrentApplication as WinUIApplicationHost;
        var window = (host?.MainWindow as WinUIWindowHost)?.Window;
        if (window == null) return;

        var hwnd = WindowNative.GetWindowHandle(window);
        // 若最小化则还原
        if (IsIconic(hwnd))
        {
            ShowWindow(hwnd, SW_RESTORE);
        }
        // 置前并激活
        SetForegroundWindow(hwnd);
    }

    private const int SW_RESTORE = 9;

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsIconic(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(IntPtr hWnd);
}

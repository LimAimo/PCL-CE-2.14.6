using System;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace PCL.Core.Utils.OS;

public static partial class WindowInterop
{
    // ReSharper disable InconsistentNaming UnusedMember.Local

    // DWM 外边缘结构定义
    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS { public int leftWidth, rightWidth, topHeight, bottomHeight; }

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmIsCompositionEnabled([MarshalAs(UnmanagedType.Bool)] out bool pfEnabled);

    // Win32 矩形结构定义
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int left; public int top; public int right; public int bottom; }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    // MONITOR_DPI_TYPE enum
    private enum MONITOR_DPI_TYPE {
        MDT_EFFECTIVE_DPI = 0,
        MDT_ANGULAR_DPI = 1,
        MDT_RAW_DPI = 2,
        MDT_DEFAULT = MDT_EFFECTIVE_DPI
    }

    [LibraryImport("user32.dll")]
    private static partial IntPtr MonitorFromWindow(IntPtr hWnd, uint dwFlags);

    // Get the primary monitor handle
    private const int MONITOR_DEFAULTTOPRIMARY = 1;

    [LibraryImport("shcore.dll", EntryPoint = "GetDpiForMonitor")]
    private static partial int GetDpiForMonitor(
        IntPtr hMonitor,
        MONITOR_DPI_TYPE dpiType,
        out uint dpiX,
        out uint dpiY
    );

    // DWM 窗口属性 (Windows 11 Mica/Backdrop)
    private enum DWMWINDOWATTRIBUTE
    {
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        DWMWA_SYSTEMBACKDROP_TYPE = 38,
        DWMWA_MICA_EFFECT = 1029
    }

    /// <summary>
    /// 系统背景类型 (Windows 11 22H2+)
    /// </summary>
    public enum DwmSystemBackdropType
    {
        /// <summary>自动（跟随系统）</summary>
        Auto = 0,
        /// <summary>无背景效果</summary>
        None = 1,
        /// <summary>Mica 云母效果</summary>
        Mica = 2,
        /// <summary>Acrylic 亚克力效果</summary>
        Acrylic = 3,
        /// <summary>Mica Alt 云母变体（颜色更深）</summary>
        MicaAlt = 4
    }

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

    // ReSharper enable InconsistentNaming UnusedMember.Local

    /// <summary>
    /// 检测 DWM 组合是否可用
    /// </summary>
    public static bool IsCompositionEnabled()
    {
        var hResult = DwmIsCompositionEnabled(out var enabled);
        return hResult != 0 ? throw new Win32Exception(hResult, "Failed to check DWM status") : enabled;
    }

    /// <summary>
    /// 设置 DWM 窗口边框到客户区域的扩展大小
    /// </summary>
    public static void ExtendFrameIntoClientArea(
        IntPtr hWnd, int marginLeft, int marginTop, int marginRight, int marginBottom)
    {
        MARGINS margins = new()
        {
            leftWidth = marginLeft,
            rightWidth = marginRight,
            topHeight = marginTop,
            bottomHeight = marginBottom
        };
        if (!IsCompositionEnabled()) return;
        var hResult = DwmExtendFrameIntoClientArea(hWnd, ref margins);
        if (hResult != 0) throw new Win32Exception(hResult, "Failed to extend frame into client area");
    }

    /// <summary>
    /// See <see cref="ExtendFrameIntoClientArea(IntPtr, int, int, int, int)"/>
    /// </summary>
    public static void ExtendFrameIntoClientArea(IntPtr hWnd, int margin)
        => ExtendFrameIntoClientArea(hWnd, margin, margin, margin, margin);

    /// <summary>
    /// 获取 Win32 窗口矩形定义
    /// </summary>
    public static (int Left, int Top, int Right, int Bottom) GetWindowRectangle(IntPtr hWnd)
    {
        var hResult = GetWindowRect(hWnd, out var rect);
        return hResult ? (rect.left, rect.top, rect.right, rect.bottom)
            : throw new Win32Exception("Failed to get window rectangle");
    }

    /// <summary>
    /// 获取 Win32 窗口位置与大小
    /// </summary>
    public static (int X, int Y, int Width, int Height) ToWindowBounds(
        this (int Left, int Top, int Right, int Bottom) rect)
    {
        var (l, t, r, b) = rect;
        var x = l;
        var y = t;
        var width = r - l;
        var height = b - t;
        return (x, y, width, height);
    }

    /// <summary>
    /// 获取指定屏幕的系统 DPI
    /// </summary>
    /// <param name="hWnd">位于指定屏幕上的任意窗口句柄，默认指定主屏</param>
    public static int GetSystemDpi(IntPtr hWnd = 0) {
        // Get the monitor handle
        var hMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTOPRIMARY);
        // 0 is S_OK
        var hr = GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out var dpiX, out _);
        if (hr == 0)
            return (int)dpiX;
        // fallback to default DPI (96)
        return 96;
    }

    /// <summary>
    /// 设置窗口的系统背景类型（Mica/Acrylic），需要 Windows 11 22H2+。
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <param name="backdropType">背景类型</param>
    /// <returns>是否设置成功</returns>
    public static bool SetSystemBackdropType(IntPtr hWnd, DwmSystemBackdropType backdropType)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22621)) return false;
        var value = (int)backdropType;
        var hr = DwmSetWindowAttribute(hWnd, (int)DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, ref value, sizeof(int));
        return hr == 0;
    }

    /// <summary>
    /// 设置窗口是否使用沉浸式深色模式（影响 Mica 的颜色）。
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <param name="useDarkMode">是否使用深色模式</param>
    /// <returns>是否设置成功</returns>
    public static bool SetImmersiveDarkMode(IntPtr hWnd, bool useDarkMode)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18985)) return false;
        var value = useDarkMode ? 1 : 0;
        var hr = DwmSetWindowAttribute(hWnd, (int)DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
        return hr == 0;
    }

    /// <summary>
    /// 检测当前系统是否支持 Mica 背景效果。
    /// </summary>
    public static bool IsMicaSupported() => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22621);
}

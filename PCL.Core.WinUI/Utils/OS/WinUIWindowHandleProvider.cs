using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PCL.Core.Utils.OS;

namespace PCL.Core.Utils.OS;

/// <summary>
/// WinUI 3 窗口句柄提供者实现。<br/>
/// WinUI 没有内置的消息钩子机制，通过 Win32 <c>SetWindowSubclass</c> 实现。
/// </summary>
public sealed partial class WinUIWindowHandleProvider : IWindowHandleProvider, IDisposable
{
    private IntPtr _hwnd;
    private readonly Dictionary<WindowHook, SUBCLASSPROC> _hookWrappers = new();
    private bool _disposed;

    public WinUIWindowHandleProvider(IntPtr hwnd)
    {
        _hwnd = hwnd;
        if (hwnd == IntPtr.Zero)
            throw new ArgumentException("窗口句柄不能为零", nameof(hwnd));
    }

    public IntPtr Handle => _hwnd;

    public void AddHook(WindowHook hook)
    {
        var wrapper = new SUBCLASSPROC(SubclassProc);
        _hookWrappers[hook] = wrapper;
        SetWindowSubclass(_hwnd, wrapper, (uint)_hookWrappers.Count, IntPtr.Zero);
    }

    public void RemoveHook(WindowHook hook)
    {
        if (_hookWrappers.TryGetValue(hook, out var wrapper))
        {
            RemoveWindowSubclass(_hwnd, wrapper);
            _hookWrappers.Remove(hook);
        }
    }

    private IntPtr SubclassProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData)
    {
        var handled = false;
        foreach (var pair in _hookWrappers)
        {
            var result = pair.Key(hwnd, (int)msg, wParam, lParam, ref handled);
            if (handled) return result;
        }
        return DefSubclassProc(hwnd, msg, wParam, lParam);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var pair in _hookWrappers)
        {
            RemoveWindowSubclass(_hwnd, pair.Value);
        }
        _hookWrappers.Clear();
    }

    private delegate IntPtr SUBCLASSPROC(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData);

    [LibraryImport("comctl32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowSubclass(IntPtr hwnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass, IntPtr dwRefData);

    [LibraryImport("comctl32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RemoveWindowSubclass(IntPtr hwnd, SUBCLASSPROC pfnSubclass);

    [LibraryImport("comctl32.dll")]
    private static partial IntPtr DefSubclassProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);
}

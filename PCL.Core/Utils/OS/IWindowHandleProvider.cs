using System;

namespace PCL.Core.Utils.OS;

/// <summary>
/// 窗口句柄提供者抽象，封装 <see cref="System.Windows.Interop.HwndSource"/> 的消息钩子能力。
/// </summary>
public interface IWindowHandleProvider
{
    /// <summary>
    /// 窗口句柄。
    /// </summary>
    IntPtr Handle { get; }

    /// <summary>
    /// 添加窗口消息处理钩子。
    /// </summary>
    void AddHook(WindowHook hook);

    /// <summary>
    /// 移除窗口消息处理钩子。
    /// </summary>
    void RemoveHook(WindowHook hook);
}

/// <summary>
/// 窗口消息处理委托，签名兼容 <see cref="System.Windows.Interop.HwndSourceHook"/>。
/// </summary>
/// <param name="hwnd">窗口句柄</param>
/// <param name="msg">消息 ID</param>
/// <param name="wParam">WParam</param>
/// <param name="lParam">LParam</param>
/// <param name="handled">是否已处理</param>
/// <returns>返回值</returns>
public delegate IntPtr WindowHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);

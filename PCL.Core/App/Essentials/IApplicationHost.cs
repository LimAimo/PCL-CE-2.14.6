using System;

namespace PCL.Core.App.Essentials;

/// <summary>
/// 应用程序容器抽象，替代 <see cref="System.Windows.Application"/>。<br/>
/// 由框架特定实现（如 PCL.Core.Wpf）提供，在 <see cref="IoC.LifecycleState.BeforeLoading"/> 阶段设置到
/// <see cref="IoC.Lifecycle.CurrentApplication"/>。
/// </summary>
public interface IApplicationHost
{
    /// <summary>
    /// 主窗口宿主。在 <see cref="IoC.LifecycleState.WindowCreating"/> 阶段由
    /// <see cref="MainWindowService"/> 设置。
    /// </summary>
    IWindowHost? MainWindow { get; set; }

    /// <summary>
    /// 启动应用程序消息循环并阻塞，直到应用程序关闭。
    /// </summary>
    /// <returns>应用程序退出状态码</returns>
    int Run();
}

/// <summary>
/// 窗口宿主抽象，替代 <see cref="System.Windows.Window"/> 的最小子集。
/// </summary>
public interface IWindowHost
{
    /// <summary>
    /// 显示窗口。
    /// </summary>
    void Show();
}

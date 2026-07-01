using System;
using Microsoft.UI.Xaml;
using PCL.Core.App.Essentials;
using PCL.Core.App.IoC;

namespace PCL.Core.App.Essentials;

/// <summary>
/// WinUI 3 应用程序容器服务。<br/>
/// 在生命周期 <see cref="LifecycleState.BeforeLoading"/> 阶段创建 Application 实例并设置到
/// <see cref="Lifecycle.CurrentApplication"/>。<br/>
/// 与 WPF 版不同，WinUI 3 的消息循环由 <c>Application.Start</c> 管理，
/// 因此 <see cref="WinUIApplicationHost.Run"/> 仅返回 0。
/// </summary>
[LifecycleService(LifecycleState.BeforeLoading, Priority = int.MinValue)]
[LifecycleScope("application", "应用程序", false)]
public sealed partial class WinUIApplicationService
{
    public static Func<Application>? Loading { private get; set; }

    /// <summary>
    /// 在 Application 实例创建后调用的初始化钩子。
    /// </summary>
    public static Action<Application>? OnApplicationCreated { private get; set; }

    [LifecycleStart]
    private static void _Start()
    {
        Context.Debug("正在初始化 WinUI 3 应用程序容器");
        var app = Loading!.Invoke();
        Lifecycle.CurrentApplication = new WinUIApplicationHost(app);
        OnApplicationCreated?.Invoke(app);
        Loading = null;
        OnApplicationCreated = null;
        Context.Trace("应用程序容器初始化完毕");
    }

    [LifecycleStop]
    private static void _Stop()
    {
        if (Lifecycle.IsForceShutdown)
        {
            Context.Warn("已指定强制关闭，跳过 WinUI 标准关闭流程");
            return;
        }
        // WinUI 3 的关闭通过关闭主窗口或 Environment.Exit 实现
        Context.Debug("正在关闭应用程序");
        var host = Lifecycle.CurrentApplication as WinUIApplicationHost;
        var window = (host?.MainWindow as WinUIWindowHost)?.Window;
        window?.Close();
    }
}

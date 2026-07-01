using System;
using Microsoft.UI.Xaml;
using PCL.Core.App.Essentials;
using PCL.Core.App.IoC;

namespace PCL.WinUI;

/// <summary>
/// PCL CE WinUI 3 版程序入口。
/// </summary>
public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // 单文件发布模式下，必须设置 Windows App Runtime 基目录
        Environment.SetEnvironmentVariable("MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY", AppContext.BaseDirectory);
        Console.WriteLine("Welcome to Plain Craft Launcher 2 Community Edition (WinUI 3)!");
        // 注册 Application 和 MainWindow 的工厂
        WinUIApplicationService.Loading = () => new App();
        WinUIApplicationService.OnApplicationCreated = app =>
        {
            // 在 Application 创建后注册平台服务
            WinUIPlatformInitializer.Initialize(app);
        };
        WinUIMainWindowService.Loading = () => new MainWindow();
        // 启动 WinUI 3 应用程序
        // Application.Start 会在内部创建 STA 线程，然后在回调中初始化生命周期
        Application.Start((_) =>
        {
            // 通过 Lifecycle.OnInitialize 启动完整生命周期
            // Lifecycle.OnInitialize → ApplicationService._Start → 创建 App 实例
            // → OnLoading → MainWindowService.Start → 创建窗口
            Lifecycle.OnInitialize();
        });
    }
}

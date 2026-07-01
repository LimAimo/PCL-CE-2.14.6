using System;
using Microsoft.UI.Xaml;
using PCL.Core.App.Essentials;
using PCL.Core.App.IoC;

namespace PCL.Core.App.Essentials;

/// <summary>
/// WinUI 3 主窗口服务。<br/>
/// 在生命周期 <see cref="LifecycleState.WindowCreating"/> 阶段创建主窗口并显示。
/// </summary>
[LifecycleService(LifecycleState.WindowCreating, Priority = int.MaxValue)]
public sealed class WinUIMainWindowService : GeneralService
{
    public static Func<Window>? Loading { private get; set; }

    private static LifecycleContext? _context;
    private static LifecycleContext Context => _context!;
    private WinUIMainWindowService() : base("window", "主窗体", false) { _context = ServiceContext; }

    public override void Start()
    {
        Context.Debug("正在初始化 WinUI 3 窗体");
        var window = Loading!.Invoke();
        window.Activated += (_, e) =>
        {
            // 首次激活时触发 WindowCreated 状态
            if (e.WindowActivationState != WindowActivationState.Deactivated)
            {
                Lifecycle.OnWindowCreated();
            }
        };
        Lifecycle.CurrentApplication.MainWindow = new WinUIWindowHost(window);
        window.Activate();
        Context.Trace("窗体创建完毕");
    }
}

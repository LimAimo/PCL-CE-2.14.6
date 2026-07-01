using System;
using Microsoft.UI.Xaml;
using PCL.Core.App.Essentials;

namespace PCL.Core.App.Essentials;

/// <summary>
/// WinUI 3 应用程序容器实现，包装 <see cref="Application"/>。
/// </summary>
public sealed class WinUIApplicationHost : IApplicationHost
{
    private readonly Application _application;

    public WinUIApplicationHost(Application application)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));
    }

    /// <summary>
    /// 底层 WinUI <see cref="Application"/> 实例，供 PCL.Core.WinUI 内部使用。
    /// </summary>
    public Application Application => _application;

    /// <inheritdoc/>
    public IWindowHost? MainWindow { get; set; }

    /// <inheritdoc/>
    public int Run()
    {
        // WinUI 应用通过 Application.Start 启动消息循环，此处不直接执行 Run。
        // 实际启动由 WinUIApplicationService 在 OnInitialize 阶段调用 Application.Start 完成。
        // 此处返回 0 占位，真正的退出码由 Application.Exit 处理。
        return 0;
    }
}

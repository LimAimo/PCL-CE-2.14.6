using System;
using System.Windows;
using PCL.Core.App.Essentials;

namespace PCL.Core.App.Essentials;

/// <summary>
/// WPF 应用程序容器实现，包装 <see cref="Application"/>。
/// </summary>
public sealed class WpfApplicationHost : IApplicationHost
{
    private readonly Application _application;

    public WpfApplicationHost(Application application)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));
    }

    /// <summary>
    /// 底层 WPF <see cref="Application"/> 实例，供 PCL.Core.Wpf 内部使用。
    /// </summary>
    public Application Application => _application;

    /// <inheritdoc/>
    public IWindowHost? MainWindow { get; set; }

    /// <inheritdoc/>
    public int Run() => _application.Run();
}

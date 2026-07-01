using System;
using Microsoft.UI.Xaml;
using PCL.Core.App.Essentials;
using PCL.Core.App.IoC;

namespace PCL.WinUI;

/// <summary>
/// WinUI 3 应用程序定义。
/// </summary>
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // 触发加载阶段
        Lifecycle.OnLoading();
    }
}

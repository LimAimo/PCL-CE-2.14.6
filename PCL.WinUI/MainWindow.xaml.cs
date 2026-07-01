using System;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT;
using WinRT.Interop;

namespace PCL.WinUI;

/// <summary>
/// WinUI 3 主窗口，使用原生 NavigationView 导航与 Mica 系统背景。
/// 主题跟随系统，不设置自定义 RequestedTheme。
/// </summary>
public sealed partial class MainWindow : Window
{
    private MicaController? _micaController;
    private SystemBackdropConfiguration? _backdropConfiguration;

    public MainWindow()
    {
        InitializeComponent();
        // 设置窗口尺寸（WinUI 3 Window 不支持在 XAML 中设置）
        try
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            var appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(hwnd));
            appWindow.Resize(new Windows.Graphics.SizeInt32(960, 640));
            appWindow.SetIcon("Assets/AppIcon.ico");
        }
        catch { /* 尺寸/图标加载失败不影响运行 */ }
        // 应用 Mica 背景
        ApplyMicaBackdrop();
        // 默认选中启动页
        NavView.SelectedItem = NavView.MenuItems[0];
    }

    /// <summary>
    /// 应用 Mica 系统背景。
    /// </summary>
    private void ApplyMicaBackdrop()
    {
        if (!MicaController.IsSupported()) return;

        _backdropConfiguration = new SystemBackdropConfiguration
        {
            IsInputActive = true,
            // 主题跟随系统
            Theme = SystemBackdropTheme.Default
        };

        _micaController = new MicaController { Kind = MicaKind.Base };
        _micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
        _micaController.SetSystemBackdropConfiguration(_backdropConfiguration);

        Activated += OnWindowActivated;
    }

    private void OnWindowActivated(object sender, WindowActivatedEventArgs args)
    {
        if (_backdropConfiguration != null)
        {
            _backdropConfiguration.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }
    }

    /// <summary>
    /// NavigationView 导航切换。
    /// </summary>
    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            Type? pageType = tag switch
            {
                "Launch" => typeof(Pages.LaunchPage),
                "Download" => typeof(Pages.DownloadPage),
                "Setup" => typeof(Pages.SetupPage),
                _ => null
            };
            if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
            {
                ContentFrame.Navigate(pageType, null, args.RecommendedNavigationTransitionInfo);
            }
        }
    }
}

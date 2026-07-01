using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PCL.Core.App;
using PCL.Core.Minecraft;

namespace PCL.WinUI.Pages;

/// <summary>
/// 设置页，使用 WinUI 3 原生控件展示和编辑 PCL.Core 配置项。
/// </summary>
public sealed partial class SetupPage : Page
{
    public SetupPage()
    {
        InitializeComponent();
        Loaded += SetupPage_Loaded;
        Unloaded += SetupPage_Unloaded;
    }

    private void SetupPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Java 列表
        RefreshJavaList();

        // 启动设置
        MemoryBox.Value = Config.Launch.CustomMemorySize;
        JvmArgsBox.Text = Config.Launch.JvmArgs ?? "";
        WindowWidthBox.Value = Config.Launch.GameWindowWidth;
        WindowHeightBox.Value = Config.Launch.GameWindowHeight;

        // 下载设置
        DownloadThreadBox.Value = Config.Download.ThreadLimit;
        DownloadSpeedBox.Value = Config.Download.SpeedLimit;

        // 网络设置
        DoHSwitch.IsOn = Config.Network.EnableDoH;

        // 注册变更事件
        MemoryBox.ValueChanged += (_, _) => Config.Launch.CustomMemorySize = (int)MemoryBox.Value;
        JvmArgsBox.LostFocus += (_, _) => Config.Launch.JvmArgs = JvmArgsBox.Text;
        WindowWidthBox.ValueChanged += (_, _) => Config.Launch.GameWindowWidth = (int)WindowWidthBox.Value;
        WindowHeightBox.ValueChanged += (_, _) => Config.Launch.GameWindowHeight = (int)WindowHeightBox.Value;
        DownloadThreadBox.ValueChanged += (_, _) => Config.Download.ThreadLimit = (int)DownloadThreadBox.Value;
        DownloadSpeedBox.ValueChanged += (_, _) => Config.Download.SpeedLimit = (int)DownloadSpeedBox.Value;
        DoHSwitch.Toggled += (_, _) => Config.Network.EnableDoH = DoHSwitch.IsOn;
    }

    private void SetupPage_Unloaded(object sender, RoutedEventArgs e)
    {
        // 配置项在 SetValue 时已自动持久化，无需手动保存
    }

    private void RefreshJavaList()
    {
        JavaComboBox.Items.Clear();
        try
        {
            var mgr = JavaService.JavaManager;
            if (mgr == null) return;
            var list = mgr.GetSortedJavaList();
            var selected = Config.Launch.SelectedJava;
            foreach (var entry in list)
            {
                var item = new ComboBoxItem
                {
                    Content = $"{entry.Installation.Brand} {entry.Installation.Version} ({entry.Installation.JavaExePath})",
                    Tag = entry.Installation.JavaExePath
                };
                JavaComboBox.Items.Add(item);
                if (!string.IsNullOrEmpty(selected) && entry.Installation.JavaExePath == selected)
                {
                    JavaComboBox.SelectedItem = item;
                }
            }
            JavaComboBox.SelectionChanged += (_, _) =>
            {
                if (JavaComboBox.SelectedItem is ComboBoxItem sel && sel.Tag is string path)
                {
                    Config.Launch.SelectedJava = path;
                }
            };
        }
        catch { /* Java 服务未就绪 */ }
    }
}

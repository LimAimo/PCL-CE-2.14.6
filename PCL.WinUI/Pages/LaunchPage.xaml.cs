using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PCL.Core.App;
using PCL.Core.Minecraft;
using PCL.Core.Minecraft.Instance;
using PCL.Core.Minecraft.Launch;

namespace PCL.WinUI.Pages;

/// <summary>
/// 启动页，展示当前实例、Java 状态并提供启动入口。
/// </summary>
public sealed partial class LaunchPage : Page
{
    public LaunchPage()
    {
        InitializeComponent();
        Loaded += LaunchPage_Loaded;
    }

    private void LaunchPage_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshInstanceInfo();
        RefreshJavaInfo();
        LaunchService.StateChanged += OnLaunchStateChanged;
    }

    private void RefreshInstanceInfo()
    {
        var instance = InstanceService.CurrentInstance;
        if (instance != null)
        {
            InstanceName.Text = instance.Name;
            InstancePath.Text = instance.Path;
            GameFolder.Text = instance.GameFolder;
        }
        else
        {
            InstanceName.Text = "未选择实例";
            InstancePath.Text = "";
            GameFolder.Text = States.Game.SelectedFolder ?? "未设置";
        }
    }

    private void RefreshJavaInfo()
    {
        try
        {
            var mgr = JavaService.JavaManager;
            if (mgr == null || !mgr.ExistAnyJava())
            {
                JavaInfo.Text = "未检测到 Java";
                return;
            }
            var list = mgr.GetSortedJavaList();
            var first = list.FirstOrDefault();
            JavaInfo.Text = first != null
                ? $"{first.Installation.Brand} {first.Installation.Version}"
                : "未检测到 Java";
        }
        catch
        {
            JavaInfo.Text = "Java 服务未就绪";
        }
    }

    private void OnLaunchStateChanged(LaunchState state, string message)
    {
        _ = DispatcherQueue.TryEnqueue(() =>
        {
            LaunchButton.Content = state switch
            {
                LaunchState.Preparing => "准备中...",
                LaunchState.Launching => "启动中...",
                LaunchState.Running => "运行中",
                LaunchState.Failed => "启动失败",
                LaunchState.Finished => "已完成",
                _ => "启动游戏"
            };
            LaunchButton.IsEnabled = state is LaunchState.Idle or LaunchState.Finished or LaunchState.Failed;
        });
    }

    private async void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        if (InstanceService.CurrentInstance == null)
        {
            await ShowDialog("无法启动", "请先选择一个游戏实例。");
            return;
        }
        await LaunchService.LaunchAsync();
    }

    private async void SelectInstanceButton_Click(object sender, RoutedEventArgs e)
    {
        var folders = InstanceService.GetGameFolders();
        if (folders.Count == 0)
        {
            await ShowDialog("未设置游戏文件夹", "请先在设置中添加游戏文件夹。");
            return;
        }

        // 显示实例选择对话框
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = "选择实例",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary
        };

        var list = new ListView { SelectionMode = ListViewSelectionMode.Single };
        foreach (var inst in InstanceService.Instances)
        {
            list.Items.Add(new { Name = inst.Name, Info = inst.DisplayInfo, Instance = inst });
        }
        if (InstanceService.Instances.Count > 0)
        {
            list.SelectedIndex = 0;
        }
        dialog.Content = list;

        var primaryBtn = new Button { Content = "选择" };
        dialog.PrimaryButtonText = "选择";
        dialog.PrimaryButtonClick += (s, e) =>
        {
            if (list.SelectedItem != null)
            {
                dynamic selected = list.SelectedItem;
                InstanceService.SelectInstance(selected.Instance.Name);
                RefreshInstanceInfo();
            }
            else
            {
                e.Cancel = true;
            }
        };

        await dialog.ShowAsync();
    }

    private async System.Threading.Tasks.Task ShowDialog(string title, string content)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = title,
            Content = content,
            CloseButtonText = "确定"
        };
        await dialog.ShowAsync();
    }
}

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PCL.Core.App;
using PCL.Core.App.Tasks;
using PCL.Core.IO.Download;
using PCL.Core.Minecraft.Install;
using PCL.Core.Minecraft.Resources;
using PCL.Core.Minecraft.Versions;

namespace PCL.WinUI.Pages;

/// <summary>
/// 下载页，提供 Minecraft 版本安装、Mod/资源包搜索与下载。
/// </summary>
public sealed partial class DownloadPage : Page
{
    public ObservableCollection<DownloadTaskItem> Tasks { get; } = new();
    public Visibility TaskListVisibility => Tasks.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

    private string _currentCategory = "minecraft";
    private CancellationTokenSource? _searchCts;

    public DownloadPage()
    {
        InitializeComponent();
        Loaded += DownloadPage_Loaded;
        Unloaded += DownloadPage_Unloaded;
    }

    private void DownloadPage_Loaded(object sender, RoutedEventArgs e)
    {
        SyncTasks();
        TaskCenter.Tasks.CollectionChanged += Tasks_CollectionChanged;
    }

    private void DownloadPage_Unloaded(object sender, RoutedEventArgs e)
    {
        TaskCenter.Tasks.CollectionChanged -= Tasks_CollectionChanged;
        _searchCts?.Cancel();
    }

    private void Tasks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        SyncTasks();
    }

    private void SyncTasks()
    {
        Tasks.Clear();
        foreach (var task in TaskCenter.Tasks)
        {
            Tasks.Add(DownloadTaskItem.From(task));
        }
        Bindings.Update();
    }

    private void CategorySelector_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (sender.SelectedItem is SelectorBarItem item && item.Tag is string tag)
        {
            _currentCategory = tag;
            // 切换类别时清空结果
            ResultList.Visibility = Visibility.Collapsed;
            VersionList.Visibility = Visibility.Collapsed;
            EmptyState.Visibility = Visibility.Visible;

            if (tag == "minecraft")
            {
                SearchBox.PlaceholderText = "加载版本列表...";
                _ = LoadVersionsAsync();
            }
            else
            {
                SearchBox.PlaceholderText = "搜索资源...";
            }
        }
    }

    private async Task LoadVersionsAsync()
    {
        LoadingState.Visibility = Visibility.Visible;
        EmptyState.Visibility = Visibility.Collapsed;
        ResultList.Visibility = Visibility.Collapsed;
        VersionList.Visibility = Visibility.Collapsed;

        try
        {
            var versions = await VersionManifestService.GetVersionsAsync();
            var releaseVersions = versions.Where(v => v.IsRelease).Take(50).ToList();
            VersionList.Items.Clear();
            foreach (var v in releaseVersions)
            {
                VersionList.Items.Add(new VersionItem
                {
                    Id = v.Id,
                    Type = v.Type,
                    ReleaseTime = v.ReleaseTime?.Substring(0, 10) ?? ""
                });
            }
            LoadingState.Visibility = Visibility.Collapsed;
            VersionList.Visibility = Visibility.Visible;
        }
        catch
        {
            LoadingState.Visibility = Visibility.Collapsed;
            EmptyState.Visibility = Visibility.Visible;
        }
    }

    private void SearchBox_SubmitQuery(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (_currentCategory == "minecraft")
        {
            _ = LoadVersionsAsync();
        }
        else
        {
            _ = SearchAsync(args.QueryText);
        }
    }

    private async Task SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return;

        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        LoadingState.Visibility = Visibility.Visible;
        EmptyState.Visibility = Visibility.Collapsed;
        ResultList.Visibility = Visibility.Collapsed;
        VersionList.Visibility = Visibility.Collapsed;

        try
        {
            var results = await ModrinthClient.SearchAsync(query, _currentCategory, limit: 30, token: token);
            ResultList.Items.Clear();
            foreach (var hit in results.Hits)
            {
                ResultList.Items.Add(new SearchResultItem
                {
                    Title = hit.Title,
                    Description = hit.Description,
                    IconUrl = hit.IconUrl,
                    Downloads = $"{hit.Downloads:N0} 次下载",
                    ProjectId = hit.ProjectId,
                    Slug = hit.Slug
                });
            }
            LoadingState.Visibility = Visibility.Collapsed;
            ResultList.Visibility = ResultList.Items.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            EmptyState.Visibility = ResultList.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (OperationCanceledException)
        {
            // 搜索被取消，忽略
        }
        catch
        {
            LoadingState.Visibility = Visibility.Collapsed;
            EmptyState.Visibility = Visibility.Visible;
        }
    }

    private async void ResultList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is SearchResultItem item)
        {
            // 获取版本列表并下载
            try
            {
                var versions = await ModrinthClient.GetVersionsAsync(item.ProjectId);
                if (versions.Count == 0)
                {
                    await ShowDialog("无可用版本", "该资源暂无可下载的版本。");
                    return;
                }

                // 选择最新版本
                var version = versions[0];
                var file = version.Files.Find(f => f.Primary) ?? version.Files.FirstOrDefault();
                if (file == null)
                {
                    await ShowDialog("无可用文件", "该版本暂无可下载的文件。");
                    return;
                }

                // 确定下载路径
                var gameFolder = States.Game.SelectedFolder ?? "";
                if (string.IsNullOrEmpty(gameFolder))
                {
                    await ShowDialog("未设置游戏文件夹", "请先在设置中配置游戏文件夹。");
                    return;
                }

                var targetDir = _currentCategory switch
                {
                    "mod" => System.IO.Path.Combine(gameFolder, "mods"),
                    "modpack" => gameFolder,
                    "resourcepack" => System.IO.Path.Combine(gameFolder, "resourcepacks"),
                    "shader" => System.IO.Path.Combine(gameFolder, "shaderpacks"),
                    _ => gameFolder
                };
                var targetPath = System.IO.Path.Combine(targetDir, file.Filename);

                // 提交下载
                NDlScheduler.Submit(file.Url, targetPath, $"{item.Title} {version.VersionNumber}");
                SyncTasks();
            }
            catch (Exception ex)
            {
                await ShowDialog("下载失败", ex.Message);
            }
        }
    }

    private async void VersionList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is VersionItem item)
        {
            var gameFolder = States.Game.SelectedFolder ?? "";
            if (string.IsNullOrEmpty(gameFolder))
            {
                await ShowDialog("未设置游戏文件夹", "请先在设置中配置游戏文件夹。");
                return;
            }

            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = "安装 Minecraft",
                Content = $"确定要安装 Minecraft {item.Id} 到\n{gameFolder}？",
                PrimaryButtonText = "安装",
                CloseButtonText = "取消"
            };
            dialog.PrimaryButtonClick += async (s, e) =>
            {
                dialog.Hide();
                try
                {
                    await InstallService.InstallAsync(item.Id, gameFolder);
                    _ = DispatcherQueue.TryEnqueue(() => SyncTasks());
                }
                catch (Exception ex)
                {
                    _ = DispatcherQueue.TryEnqueue(async () => await ShowDialog("安装失败", ex.Message));
                }
            };
            await dialog.ShowAsync();
        }
    }

    private async Task ShowDialog(string title, string content)
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

/// <summary>
/// 搜索结果项（UI 包装）。
/// </summary>
public sealed class SearchResultItem
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? IconUrl { get; set; }
    public string Downloads { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string Slug { get; set; } = "";
}

/// <summary>
/// 版本列表项（UI 包装）。
/// </summary>
public sealed class VersionItem
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string ReleaseTime { get; set; } = "";
}

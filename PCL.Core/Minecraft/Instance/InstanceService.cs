using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PCL.Core.App.IoC;

namespace PCL.Core.Minecraft.Instance;

/// <summary>
/// 游戏实例管理服务。<br/>
/// 扫描游戏文件夹下的版本列表，提供实例选择与查询能力。
/// </summary>
[LifecycleService(LifecycleState.Loaded, Priority = 100)]
[LifecycleScope("instance", "游戏实例管理", false)]
public sealed partial class InstanceService
{
    private static List<McInstance> _instances = new();
    private static string _currentGameFolder = "";

    /// <summary>
    /// 当前游戏文件夹下的所有实例列表（按名称排序）。
    /// </summary>
    public static IReadOnlyList<McInstance> Instances => _instances;

    /// <summary>
    /// 当前选中的实例（可能为 null）。
    /// </summary>
    public static McInstance? CurrentInstance { get; private set; }

    /// <summary>
    /// 当前游戏文件夹路径。
    /// </summary>
    public static string CurrentGameFolder => _currentGameFolder;

    [LifecycleStart]
    private static void _Start()
    {
        // 从配置加载当前游戏文件夹
        _currentGameFolder = App.States.Game.SelectedFolder ?? "";
        if (!string.IsNullOrEmpty(_currentGameFolder) && Directory.Exists(_currentGameFolder))
        {
            RefreshInstances();
            SelectInstance(App.States.Game.SelectedInstance);
        }
    }

    [LifecycleStop]
    private static void _Stop()
    {
        _instances.Clear();
        CurrentInstance = null;
        _currentGameFolder = "";
    }

    /// <summary>
    /// 设置当前游戏文件夹并刷新实例列表。
    /// </summary>
    public static void SetGameFolder(string folder)
    {
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return;
        _currentGameFolder = folder;
        App.States.Game.SelectedFolder = folder;
        RefreshInstances();
        SelectInstance(App.States.Game.SelectedInstance);
    }

    /// <summary>
    /// 刷新当前游戏文件夹下的实例列表。
    /// </summary>
    public static void RefreshInstances()
    {
        _instances.Clear();
        if (string.IsNullOrEmpty(_currentGameFolder) || !Directory.Exists(_currentGameFolder))
            return;

        var versionsDir = Path.Combine(_currentGameFolder, "versions");
        if (!Directory.Exists(versionsDir)) return;

        foreach (var dir in Directory.GetDirectories(versionsDir))
        {
            var name = Path.GetFileName(dir);
            var jsonPath = Path.Combine(dir, name + ".json");
            if (!File.Exists(jsonPath)) continue;
            try
            {
                _instances.Add(new McInstance(name, _currentGameFolder));
            }
            catch { /* 跳过无法加载的实例 */ }
        }

        _instances = _instances.OrderBy(i => i.Name).ToList();
    }

    /// <summary>
    /// 选择当前实例。
    /// </summary>
    public static McInstance? SelectInstance(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            CurrentInstance = _instances.FirstOrDefault();
        }
        else
        {
            CurrentInstance = _instances.FirstOrDefault(i => i.Name == name)
                ?? _instances.FirstOrDefault();
        }

        if (CurrentInstance != null)
        {
            App.States.Game.SelectedInstance = CurrentInstance.Name;
        }

        return CurrentInstance;
    }

    /// <summary>
    /// 获取所有游戏文件夹列表。
    /// </summary>
    public static List<string> GetGameFolders()
    {
        var foldersStr = App.States.Game.Folders ?? "";
        if (string.IsNullOrEmpty(foldersStr)) return new List<string>();
        return foldersStr.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    /// <summary>
    /// 添加游戏文件夹。
    /// </summary>
    public static void AddGameFolder(string folder)
    {
        if (string.IsNullOrEmpty(folder)) return;
        var folders = GetGameFolders();
        if (!folders.Contains(folder))
        {
            folders.Add(folder);
            App.States.Game.Folders = string.Join("|", folders);
        }
    }

    /// <summary>
    /// 移除游戏文件夹。
    /// </summary>
    public static void RemoveGameFolder(string folder)
    {
        if (string.IsNullOrEmpty(folder)) return;
        var folders = GetGameFolders();
        folders.Remove(folder);
        App.States.Game.Folders = string.Join("|", folders);
    }
}

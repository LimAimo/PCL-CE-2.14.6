using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using PCL.Core.App;
using PCL.Core.App.Tasks;

namespace PCL.Core.IO.Download;

/// <summary>
/// 下载调度器，管理下载任务队列，控制并发数。
/// </summary>
public class NDlScheduler
{
    private static readonly SemaphoreSlim _semaphore = new(Math.Max(1, App.Config.Download.ThreadLimit));
    private static readonly ConcurrentDictionary<string, NDlTask> _tasks = new();

    /// <summary>
    /// 最大并发下载数。
    /// </summary>
    public static int MaxConcurrency => Math.Max(1, App.Config.Download.ThreadLimit);

    /// <summary>
    /// 提交下载任务并注册到任务中心。
    /// </summary>
    /// <param name="task">下载任务</param>
    /// <param name="start">是否立即开始</param>
    public static void Submit(NDlTask task, bool start = true)
    {
        var key = task.Title + "_" + Guid.NewGuid();
        _tasks[key] = task;
        TaskCenter.Register(task, start);
    }

    /// <summary>
    /// 提交下载任务（便捷方法）。
    /// </summary>
    /// <param name="url">下载 URL</param>
    /// <param name="targetPath">目标路径</param>
    /// <param name="displayName">显示名称</param>
    /// <param name="start">是否立即开始</param>
    /// <returns>下载任务实例</returns>
    public static NDlTask Submit(string url, string targetPath, string? displayName = null, bool start = true)
    {
        var task = new NDlTask(url, targetPath, displayName);
        Submit(task, start);
        return task;
    }

    /// <summary>
    /// 提交下载任务并等待完成。
    /// </summary>
    public static async Task<NDlTask> SubmitAsync(string url, string targetPath, string? displayName = null)
    {
        var task = new NDlTask(url, targetPath, displayName);
        await task.ExecuteAsync();
        return task;
    }
}

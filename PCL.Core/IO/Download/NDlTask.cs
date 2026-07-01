using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PCL.Core.App.Tasks;
using PCL.Core.IO.Net;

namespace PCL.Core.IO.Download;

/// <summary>
/// 下载任务，实现 <see cref="ITask"/>、<see cref="ITaskProgressive"/>、<see cref="ITaskCancelable"/>。
/// </summary>
public class NDlTask : ITask, ITaskProgressive, ITaskCancelable
{
    private readonly string _url;
    private readonly string _targetPath;
    private readonly string? _displayName;
    private CancellationTokenSource? _cts;
    private long _totalBytes;
    private long _downloadedBytes;

    /// <summary>
    /// 任务标题。
    /// </summary>
    public string Title => _displayName ?? Path.GetFileName(_targetPath);

    /// <summary>
    /// 总字节数（0 表示未知）。
    /// </summary>
    public long TotalBytes => _totalBytes;

    /// <summary>
    /// 已下载字节数。
    /// </summary>
    public long DownloadedBytes => _downloadedBytes;

    /// <summary>
    /// 下载进度（0.0~1.0）。
    /// </summary>
    public double Progress => _totalBytes > 0 ? (double)_downloadedBytes / _totalBytes : 0;

    public event TaskStateEvent? StateChanged;
    public event TaskProgressEvent? ProgressChanged;

    /// <summary>
    /// 创建下载任务。
    /// </summary>
    /// <param name="url">下载 URL</param>
    /// <param name="targetPath">目标文件路径</param>
    /// <param name="displayName">显示名称（可选）</param>
    public NDlTask(string url, string targetPath, string? displayName = null)
    {
        _url = url ?? throw new ArgumentNullException(nameof(url));
        _targetPath = targetPath ?? throw new ArgumentNullException(nameof(targetPath));
        _displayName = displayName;
    }

    public async Task ExecuteAsync(CancellationToken cancelToken = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
        var token = _cts.Token;

        StateChanged?.Invoke(TaskState.Running, "正在下载...");
        try
        {
            var client = NetworkService.GetClient();
            // 确保目录存在
            var dir = Path.GetDirectoryName(_targetPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            using var response = await client.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();
            _totalBytes = response.Content.Headers.ContentLength ?? 0;

            await using var contentStream = await response.Content.ReadAsStreamAsync(token);
            await using var fileStream = new FileStream(_targetPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync: true);

            var buffer = new byte[81920];
            int read;
            while ((read = await contentStream.ReadAsync(buffer, token)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, read), token);
                _downloadedBytes += read;
                if (_totalBytes > 0)
                {
                    ProgressChanged?.Invoke(Progress);
                }
            }

            StateChanged?.Invoke(TaskState.Success, "下载完成");
        }
        catch (OperationCanceledException)
        {
            StateChanged?.Invoke(TaskState.Canceled, "已取消");
            // 删除不完整的文件
            try { if (File.Exists(_targetPath)) File.Delete(_targetPath); } catch { }
        }
        catch (Exception ex)
        {
            StateChanged?.Invoke(TaskState.Failed, ex.Message);
            // 删除不完整的文件
            try { if (File.Exists(_targetPath)) File.Delete(_targetPath); } catch { }
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    public void Cancel()
    {
        _cts?.Cancel();
    }
}

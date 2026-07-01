using System;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using PCL.Core.UI.Animation.UIAccessProvider;

namespace PCL.Core.UI.Animation.UIAccessProvider;

/// <summary>
/// WinUI 3 UI 线程访问提供者实现，使用 <see cref="DispatcherQueue"/> + <see cref="CompositionTarget"/>。
/// </summary>
public sealed class WinUIUIAccessProvider : IUIAccessProvider
{
    private readonly DispatcherQueue _dispatcherQueue;
    private event EventHandler? _frameTick;

    public WinUIUIAccessProvider(DispatcherQueue dispatcherQueue)
    {
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        // WinUI 3 的渲染帧事件通过 CompositionTarget.Rendering 提供
        CompositionTarget.Rendering += OnRendering;
    }

    private void OnRendering(object? sender, object e)
    {
        _frameTick?.Invoke(sender, EventArgs.Empty);
    }

    public bool CheckAccess()
    {
        return _dispatcherQueue.HasThreadAccess;
    }

    public void Invoke(Action action)
    {
        if (CheckAccess())
        {
            action();
            return;
        }
        _dispatcherQueue.TryEnqueue(() => action());
    }

    public Task InvokeAsync(Action action)
    {
        if (CheckAccess())
        {
            action();
            return Task.CompletedTask;
        }
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = _dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                action();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }

    public Task<T> InvokeAsync<T>(Func<T> func)
    {
        if (CheckAccess())
        {
            return Task.FromResult(func());
        }
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = _dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                tcs.SetResult(func());
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }

    public Task InvokeAsync(Func<Task> func)
    {
        if (CheckAccess())
        {
            return func();
        }
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = _dispatcherQueue.TryEnqueue(async () =>
        {
            try
            {
                await func();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }

    public Task<T> InvokeAsync<T>(Func<Task<T>> func)
    {
        if (CheckAccess())
        {
            return func();
        }
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = _dispatcherQueue.TryEnqueue(async () =>
        {
            try
            {
                tcs.SetResult(await func());
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }

    public event EventHandler FrameTick
    {
        add => _frameTick += value;
        remove => _frameTick -= value;
    }
}

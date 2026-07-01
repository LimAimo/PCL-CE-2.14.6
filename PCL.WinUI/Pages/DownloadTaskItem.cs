using System;
using PCL.Core.App.Tasks;

namespace PCL.WinUI.Pages;

/// <summary>
/// TaskModel 的 UI 包装类，避免 XAML 编译器直接处理 required/init-only 属性。
/// </summary>
public sealed class DownloadTaskItem
{
    public string Title { get; set; } = "";
    public string StateText { get; set; } = "";
    public string StateMessage { get; set; } = "";
    public double Progress { get; set; }
    public bool SupportProgress { get; set; }

    public static DownloadTaskItem From(TaskModel model)
    {
        return new DownloadTaskItem
        {
            Title = model.Title,
            StateText = model.State.ToString(),
            StateMessage = model.StateMessage,
            Progress = model.Progress,
            SupportProgress = model.SupportProgress
        };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using PCL.Core.App.IoC;
using PCL.Core.UI;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace PCL.Core.UI;

/// <summary>
/// WinUI 3 文件对话框服务实现，使用 <see cref="Windows.Storage.Pickers"/>。
/// </summary>
public sealed class WinUIFileDialogService : IFileDialogService
{
    public string SelectSaveFile(string title, string fileName, string? fileFilter = null, string? initialDirectory = null)
    {
        var picker = new FileSavePicker
        {
            SuggestedFileName = fileName
        };
        // 解析文件过滤器 "描述|*.ext1;*.ext2|描述2|*.ext3"
        if (!string.IsNullOrEmpty(fileFilter))
        {
            var parts = fileFilter.Split('|');
            for (int i = 0; i + 1 < parts.Length; i += 2)
            {
                var exts = parts[i + 1].Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var ext in exts)
                {
                    var cleanExt = ext.TrimStart('*').TrimStart('.');
                    picker.FileTypeChoices[parts[i]] = new List<string> { cleanExt.StartsWith('.') ? cleanExt : "." + cleanExt };
                }
            }
        }
        else
        {
            picker.FileTypeChoices.Add("所有文件", new List<string> { "." });
        }
        InitializeWithWindow.Initialize(picker, GetMainWindowHandle());
        var file = picker.PickSaveFileAsync().GetAwaiter().GetResult();
        return file?.Path ?? "";
    }

    public string[] SelectFiles(string fileFilter, string title = "选择文件", string? initialDirectory = null, bool allowMultiSelect = true)
    {
        var picker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.List
        };
        // 解析文件过滤器
        if (!string.IsNullOrEmpty(fileFilter))
        {
            var parts = fileFilter.Split('|');
            for (int i = 1; i < parts.Length; i += 2)
            {
                var exts = parts[i].Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var ext in exts)
                {
                    var cleanExt = ext.TrimStart('*').TrimStart('.');
                    picker.FileTypeFilter.Add(cleanExt.StartsWith('.') ? cleanExt : "." + cleanExt);
                }
            }
        }
        else
        {
            picker.FileTypeFilter.Add("*");
        }
        if (allowMultiSelect) picker.CommitButtonText = "选择";
        InitializeWithWindow.Initialize(picker, GetMainWindowHandle());
        var files = allowMultiSelect
            ? picker.PickMultipleFilesAsync().GetAwaiter().GetResult()
            : picker.PickSingleFileAsync().GetAwaiter().GetResult() is { } single ? new[] { single } : new Windows.Storage.StorageFile[0];
        return files.Select(f => f.Path).ToArray();
    }

    public string SelectFolder(string title = "选择文件夹", string? initialDirectory = null)
    {
        var picker = new FolderPicker
        {
            ViewMode = PickerViewMode.List
        };
        picker.FileTypeFilter.Add("*");
        InitializeWithWindow.Initialize(picker, GetMainWindowHandle());
        var folder = picker.PickSingleFolderAsync().GetAwaiter().GetResult();
        return folder is null ? "" : folder.Path + "\\";
    }

    private static IntPtr GetMainWindowHandle()
    {
        var host = Lifecycle.CurrentApplication as App.Essentials.WinUIApplicationHost;
        var window = (host?.MainWindow as App.Essentials.WinUIWindowHost)?.Window;
        return window is null ? IntPtr.Zero : WindowNative.GetWindowHandle(window);
    }
}

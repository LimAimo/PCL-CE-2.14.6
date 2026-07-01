using System;
using System.Collections.Generic;
using System.Linq;
using PCL.Core.Utils.OS;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace PCL.Core.Utils.OS;

/// <summary>
/// WinUI 3 剪贴板服务实现，使用 <see cref="Windows.ApplicationModel.DataTransfer.Clipboard"/>。
/// </summary>
public sealed class WinUIClipboardService : IClipboardService
{
    public string[]? GetFiles()
    {
        var content = Clipboard.GetContent();
        if (!content.Contains(StandardDataFormats.StorageItems)) return null;
        var items = content.GetStorageItemsAsync().AsTask().GetAwaiter().GetResult();
        if (items.Count == 0) return null;
        return items.Select(i => i.Path).ToArray();
    }

    public void SetFiles(string[] paths)
    {
        var data = new DataPackage();
        // WinUI 的 DataPackage 不直接支持文件路径数组，需要构造 StorageItems
        var items = new List<IStorageItem>();
        foreach (var path in paths)
        {
            if (System.IO.Directory.Exists(path))
            {
                var folder = StorageFolder.GetFolderFromPathAsync(path).AsTask().GetAwaiter().GetResult();
                items.Add(folder);
            }
            else if (System.IO.File.Exists(path))
            {
                var file = StorageFile.GetFileFromPathAsync(path).AsTask().GetAwaiter().GetResult();
                items.Add(file);
            }
        }
        data.SetStorageItems(items);
        Clipboard.SetContent(data);
        Clipboard.Flush();
    }
}

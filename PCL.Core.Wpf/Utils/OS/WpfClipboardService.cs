using System;
using System.Windows;
using PCL.Core.Utils.OS;

namespace PCL.Core.Utils.OS;

/// <summary>
/// WPF 剪贴板服务实现。<br/>
/// 封装 <see cref="System.Windows.Clipboard"/> 的文件拖放操作。
/// </summary>
public sealed class WpfClipboardService : IClipboardService
{
    /// <inheritdoc/>
    public string[]? GetFiles()
    {
        var dataObject = Clipboard.GetDataObject();
        if (dataObject == null || !dataObject.GetDataPresent(DataFormats.FileDrop))
            return null;
        var data = dataObject.GetData(DataFormats.FileDrop);
        return data as string[];
    }

    /// <inheritdoc/>
    public void SetFiles(string[] paths)
    {
        var dataObject = new DataObject();
        dataObject.SetData(DataFormats.FileDrop, paths);
        Clipboard.SetDataObject(dataObject);
    }
}

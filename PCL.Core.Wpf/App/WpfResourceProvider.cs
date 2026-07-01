using System;
using System.IO;
using System.Windows;
using PCL.Core.App.Essentials;

namespace PCL.Core.App.Essentials;

/// <summary>
/// WPF 打包资源提供者实现。<br/>
/// 封装 <see cref="Application.GetResourceStream"/> 和 pack URI 解析。
/// </summary>
public sealed class WpfResourceProvider : IResourceProvider
{
    /// <inheritdoc/>
    public Stream? GetResourceStream(string path)
    {
        var uri = new Uri($"pack://application:,,,/{path}", UriKind.Absolute);
        var resourceInfo = Application.GetResourceStream(uri);
        return resourceInfo?.Stream;
    }
}

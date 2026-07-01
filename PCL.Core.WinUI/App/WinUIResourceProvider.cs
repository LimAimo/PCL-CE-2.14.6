using System;
using System.IO;
using PCL.Core.App;
using Windows.ApplicationModel.Resources;
using Windows.Storage;

namespace PCL.Core.App.Essentials;

/// <summary>
/// WinUI 3 资源提供者实现。<br/>
/// WinUI 3 没有类似 WPF 的 pack URI 资源流机制，对于打包资源使用 ResourceManager，
/// 对于文件系统资源使用 StorageFile。
/// </summary>
public sealed class WinUIResourceProvider : IResourceProvider
{
    private static readonly ResourceLoader _resourceLoader = new();

    public Stream? GetResourceStream(string path)
    {
        // WinUI 3 中资源访问方式与 WPF 不同：
        // 1. 字符串资源通过 ResourceLoader 获取（不适合返回 Stream）
        // 2. 文件资源（如 jar、ps）通常作为 Content 文件随应用打包
        // 此处尝试从应用安装目录读取作为 Content 的文件
        try
        {
            var packagePath = Windows.ApplicationModel.Package.Current.InstalledPath;
            var fullPath = Path.Combine(packagePath, path.Replace('/', '\\'));
            if (File.Exists(fullPath))
            {
                return File.OpenRead(fullPath);
            }
            // 尝试 ms-appx:// 协议
            return null;
        }
        catch
        {
            return null;
        }
    }
}

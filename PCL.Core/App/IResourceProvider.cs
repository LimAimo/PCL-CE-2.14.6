using System.IO;

namespace PCL.Core.App;

/// <summary>
/// 应用程序打包资源提供者抽象，替代 <see cref="System.Windows.Application.GetResourceStream(System.Uri)"/>。
/// </summary>
public interface IResourceProvider
{
    /// <summary>
    /// 获取程序打包资源的输入流。该资源必须声明为 <c>Resource</c> 类型。
    /// </summary>
    /// <param name="path">资源路径，例如 "Resources/java-wrapper.jar"</param>
    /// <returns>资源输入流，若资源不存在则为 <c>null</c></returns>
    Stream? GetResourceStream(string path);
}

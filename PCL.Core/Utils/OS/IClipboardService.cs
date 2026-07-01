namespace PCL.Core.Utils.OS;

/// <summary>
/// 剪贴板服务抽象，替代 <see cref="System.Windows.Clipboard"/> 和 <see cref="System.Windows.DataObject"/>。
/// 仅暴露项目中实际使用的文件拖放相关操作。
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// 从剪贴板获取文件路径列表，若剪贴板中不包含文件则返回 <c>null</c>。
    /// </summary>
    /// <returns>文件路径数组，或 <c>null</c></returns>
    string[]? GetFiles();

    /// <summary>
    /// 将文件路径列表设置到剪贴板。
    /// </summary>
    /// <param name="paths">文件路径数组</param>
    void SetFiles(string[] paths);
}

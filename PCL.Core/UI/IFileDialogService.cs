namespace PCL.Core.UI;

/// <summary>
/// 文件对话框服务抽象，替代 <see cref="Microsoft.Win32.OpenFileDialog"/>、
/// <see cref="Microsoft.Win32.SaveFileDialog"/> 和 <see cref="Microsoft.Win32.OpenFolderDialog"/>。
/// </summary>
public interface IFileDialogService
{
    /// <summary>
    /// 显示保存文件对话框。
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="fileName">默认文件名</param>
    /// <param name="fileFilter">文件格式过滤器，例如 "常用图片文件|*.png;*.jpg"</param>
    /// <param name="initialDirectory">初始目录</param>
    /// <returns>用户选择的完整文件路径，如果取消则返回空字符串</returns>
    string SelectSaveFile(string title, string fileName, string? fileFilter = null, string? initialDirectory = null);

    /// <summary>
    /// 显示打开文件对话框，可选择多个文件。
    /// </summary>
    /// <param name="fileFilter">文件格式过滤器</param>
    /// <param name="title">对话框标题</param>
    /// <param name="initialDirectory">初始目录</param>
    /// <param name="allowMultiSelect">是否允许选择多个文件</param>
    /// <returns>用户选择的文件路径数组，如果取消则返回空数组</returns>
    string[] SelectFiles(string fileFilter, string title = "选择文件", string? initialDirectory = null, bool allowMultiSelect = true);

    /// <summary>
    /// 显示文件夹选择对话框。
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="initialDirectory">初始目录</param>
    /// <returns>用户选择的文件夹路径（以 \ 结尾），如果取消则返回空字符串</returns>
    string SelectFolder(string title = "选择文件夹", string? initialDirectory = null);
}

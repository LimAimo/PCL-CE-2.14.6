using PCL.Core.App;
using PCL.Core.Logging;

namespace PCL.Core.UI;

using System;
using System.IO;

/// <summary>
/// 提供文件和文件夹对话框相关的实用方法。<br/>
/// 通过 <see cref="Platform.FileDialogs"/> 转发到框架特定实现。
/// </summary>
public static class SystemDialogs {
    /// <summary>
    /// 显示保存文件对话框，要求用户选择保存位置。
    /// </summary>
    /// <param name="title">对话框标题。</param>
    /// <param name="fileName">默认文件名。</param>
    /// <param name="fileFilter">文件格式过滤器，例如 "常用图片文件|*.png;*.jpg"，默认为 null。</param>
    /// <param name="initialDirectory">初始目录，默认为 null。</param>
    /// <returns>用户选择的完整文件路径，如果取消则返回空字符串。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="title"/> 或 <paramref name="fileName"/> 为 null 时抛出。</exception>
    public static string SelectSaveFile(string title, string fileName, string? fileFilter = null, string? initialDirectory = null) {
        LogWrapper.Info("Dialog", $"打开保存文件对话框：{title}");
        var selectedPath = Platform.FileDialogs.SelectSaveFile(title, fileName, fileFilter, initialDirectory);
        if (string.IsNullOrEmpty(selectedPath)) {
            LogWrapper.Info("Dialog", "选择文件被取消");
            return "";
        }
        LogWrapper.Info("Dialog", $"选择文件返回：{selectedPath}");
        return selectedPath;
    }

    /// <summary>
    /// 显示打开文件对话框，要求用户选择单个文件。
    /// </summary>
    /// <param name="fileFilter">文件格式过滤器，例如 <c>常用图片文件|*.png;*.jpg</c></param>
    /// <param name="title">对话框标题</param>
    /// <param name="initialDirectory">初始目录，默认由系统决定</param>
    /// <returns>用户选择的完整文件路径，如果取消则返回空字符串</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="fileFilter"/> 或 <paramref name="title"/> 为 <c>null</c> 时抛出</exception>
    public static string SelectFile(string fileFilter, string title, string? initialDirectory = null) {
        var result = SelectFiles(fileFilter, title, initialDirectory, false);
        return result.Length == 0 ? "" : result[0];
    }

    /// <summary>
    /// 显示打开文件对话框，要求用户选择文件。
    /// </summary>
    /// <param name="fileFilter">文件格式过滤器，例如 <c>常用图片文件|*.png;*.jpg</c></param>
    /// <param name="title">对话框标题，默认为 "选择文件"</param>
    /// <param name="initialDirectory">初始目录，默认由系统决定</param>
    /// <param name="allowMultiSelect">是否允许选择多个文件，默认允许</param>
    /// <returns>用户选择的文件路径数组，如果取消则返回空数组</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="fileFilter"/> 或 <paramref name="title"/> 为 <c>null</c> 时抛出</exception>
    public static string[] SelectFiles(string fileFilter, string title = "选择文件", string? initialDirectory = null, bool allowMultiSelect = true) {
        var num = allowMultiSelect ? "多" : "单";
        LogWrapper.Info("Dialog", $"打开选择{num}个文件对话框: {title}");
        var selectedFiles = Platform.FileDialogs.SelectFiles(fileFilter, title, initialDirectory, allowMultiSelect);
        if (selectedFiles.Length == 0) {
            LogWrapper.Info("Dialog", "选择文件被取消");
            return [];
        }
        LogWrapper.Info("Dialog", $"选择{num}个文件返回: {string.Join(",", selectedFiles)}");
        return selectedFiles;
    }

    /// <summary>
    /// 显示文件夹选择对话框，要求用户选择一个文件夹。
    /// </summary>
    /// <param name="title">对话框标题，默认为 "选择文件夹"</param>
    /// <param name="initialDirectory">初始目录，默认为桌面</param>
    /// <returns>用户选择的文件夹路径（以 \ 结尾），如果取消则返回空字符串</returns>
    public static string SelectFolder(string title = "选择文件夹", string? initialDirectory = null) {
        LogWrapper.Info("Dialog", $"打开选择文件夹对话框: {title}");
        var selectedPath = Platform.FileDialogs.SelectFolder(title, initialDirectory);
        if (string.IsNullOrEmpty(selectedPath)) {
            LogWrapper.Info("Dialog", "选择文件夹被取消");
            return "";
        }
        LogWrapper.Info("Dialog", $"选择文件夹返回: {selectedPath}");
        return selectedPath;
    }
}

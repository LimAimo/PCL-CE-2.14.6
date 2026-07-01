using System;
using System.IO;
using Microsoft.Win32;
using PCL.Core.UI;

namespace PCL.Core.UI;

/// <summary>
/// WPF 文件对话框服务实现。<br/>
/// 封装 <see cref="SaveFileDialog"/>、<see cref="OpenFileDialog"/> 和 <see cref="OpenFolderDialog"/>。
/// </summary>
public sealed class WpfFileDialogService : IFileDialogService
{
    /// <inheritdoc/>
    public string SelectSaveFile(string title, string fileName, string? fileFilter = null, string? initialDirectory = null)
    {
        var fileDialog = new SaveFileDialog
        {
            AddExtension = true,
            Title = title,
            FileName = fileName,
            Filter = fileFilter ?? "所有文件|*.*",
            InitialDirectory = !string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory) ? initialDirectory : null
        };

        var result = fileDialog.ShowDialog();
        if (result != true) return "";

        var selectedPath = fileDialog.FileName;
        return string.IsNullOrEmpty(selectedPath) ? "" : Path.GetFullPath(selectedPath);
    }

    /// <inheritdoc/>
    public string[] SelectFiles(string fileFilter, string title = "选择文件", string? initialDirectory = null, bool allowMultiSelect = true)
    {
        var fileDialog = new OpenFileDialog
        {
            AddExtension = true,
            CheckFileExists = true,
            Filter = fileFilter,
            Multiselect = allowMultiSelect,
            Title = title,
            ValidateNames = true,
            InitialDirectory = !string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory) ? initialDirectory : null
        };

        var result = fileDialog.ShowDialog();
        if (result != true) return [];

        string[] selectedFiles = fileDialog.FileNames;
        return selectedFiles.Length == 0 ? [] : Array.ConvertAll(selectedFiles, Path.GetFullPath);
    }

    /// <inheritdoc/>
    public string SelectFolder(string title = "选择文件夹", string? initialDirectory = null)
    {
        var folderDialog = new OpenFolderDialog
        {
            Title = title,
            InitialDirectory = initialDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Multiselect = false
        };

        var result = folderDialog.ShowDialog();
        if (result != true) return "";

        var selectedPath = folderDialog.FolderName;
        if (string.IsNullOrEmpty(selectedPath)) return "";

        var normalizedPath = Path.GetFullPath(selectedPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return normalizedPath;
    }
}

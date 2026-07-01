namespace PCL.Core.App.Essentials;

/// <summary>
/// 消息框服务抽象，替代 <see cref="System.Windows.MessageBox.Show(string, string, System.Windows.MessageBoxButton, System.Windows.MessageBoxImage)"/>。
/// </summary>
public interface IMessageBoxService
{
    /// <summary>
    /// 显示一个模态消息框。
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="caption">窗口标题</param>
    /// <param name="isError">是否为错误图标</param>
    void Show(string message, string caption, bool isError);
}

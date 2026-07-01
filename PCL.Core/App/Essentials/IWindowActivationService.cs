namespace PCL.Core.App.Essentials;

/// <summary>
/// 窗口激活服务抽象。<br/>
/// 封装将主窗口从最小化/后台状态激活到前台的逻辑，
/// 替代 RpcService 中直接操作 <see cref="System.Windows.Window"/> 的 WindowState/Topmost/Activate 调用。
/// </summary>
public interface IWindowActivationService
{
    /// <summary>
    /// 激活主窗口：若最小化则还原，并置于前台。
    /// </summary>
    void ActivateMainWindow();
}

namespace PCL.Core.UI;

/// <summary>
/// 应用程序资源字典抽象，替代 <see cref="System.Windows.Application.Resources"/>。
/// 仅暴露项目中实际使用的颜色/画刷设置操作。
/// </summary>
public interface IResourceDictionary
{
    /// <summary>
    /// 将 scRGB 颜色设置到资源字典中的指定键。
    /// </summary>
    /// <param name="key">资源键</param>
    /// <param name="alpha">Alpha 通道（scRGB，通常 0~1）</param>
    /// <param name="red">红色通道（scRGB 线性，通常 0~1）</param>
    /// <param name="green">绿色通道（scRGB 线性，通常 0~1）</param>
    /// <param name="blue">蓝色通道（scRGB 线性，通常 0~1）</param>
    void SetScColor(string key, float alpha, float red, float green, float blue);

    /// <summary>
    /// 将 scRGB 纯色画刷设置到资源字典中的指定键。
    /// </summary>
    /// <param name="key">资源键</param>
    /// <param name="alpha">Alpha 通道（scRGB，通常 0~1）</param>
    /// <param name="red">红色通道（scRGB 线性，通常 0~1）</param>
    /// <param name="green">绿色通道（scRGB 线性，通常 0~1）</param>
    /// <param name="blue">蓝色通道（scRGB 线性，通常 0~1）</param>
    void SetScColorBrush(string key, float alpha, float red, float green, float blue);
}

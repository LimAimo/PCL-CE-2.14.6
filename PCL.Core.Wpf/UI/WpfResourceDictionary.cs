using System;
using System.Windows;
using System.Windows.Media;
using PCL.Core.UI;

namespace PCL.Core.UI;

/// <summary>
/// WPF 资源字典实现。<br/>
/// 将 scRGB 颜色应用到 <see cref="Application.Resources"/>，
/// 替代 CatColorResource.Apply() 中直接操作 WPF 资源字典的代码。
/// </summary>
public sealed class WpfResourceDictionary : IResourceDictionary
{
    /// <inheritdoc/>
    public void SetScColor(string key, float alpha, float red, float green, float blue)
    {
        var color = Color.FromScRgb(alpha, red, green, blue);
        Application.Current.Resources[key] = color;
    }

    /// <inheritdoc/>
    public void SetScColorBrush(string key, float alpha, float red, float green, float blue)
    {
        var color = Color.FromScRgb(alpha, red, green, blue);
        Application.Current.Resources[key] = new SolidColorBrush(color);
    }
}

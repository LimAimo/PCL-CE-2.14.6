using System;
using System.Windows.Media;
using PCL.Core.UI;

namespace PCL.Core.UI;

/// <summary>
/// <see cref="NColor"/> 的 WPF 扩展方法，提供与 WPF <see cref="Color"/>/<see cref="SolidColorBrush"/> 的互转。<br/>
/// 替代 NColor 中原有的 WPF 构造函数。
/// </summary>
public static class NColorWpfExtensions
{
    /// <summary>
    /// 将 <see cref="NColor"/> 转换为 WPF <see cref="Color"/>（使用 sRGB byte 通道）。
    /// </summary>
    public static Color ToColor(this NColor color)
    {
        return Color.FromArgb((byte)color.A, (byte)color.R, (byte)color.G, (byte)color.B);
    }

    /// <summary>
    /// 将 <see cref="NColor"/> 转换为 WPF <see cref="SolidColorBrush"/>。
    /// </summary>
    public static SolidColorBrush ToSolidColorBrush(this NColor color)
    {
        return new SolidColorBrush(color.ToColor());
    }

    /// <summary>
    /// 从 WPF <see cref="Color"/> 创建 <see cref="NColor"/>。
    /// </summary>
    public static NColor ToNColor(this Color color)
    {
        return new NColor(color.R, color.G, color.B, color.A);
    }

    /// <summary>
    /// 从 WPF <see cref="SolidColorBrush"/> 创建 <see cref="NColor"/>。
    /// </summary>
    public static NColor ToNColor(this SolidColorBrush brush)
    {
        return brush.Color.ToNColor();
    }

    /// <summary>
    /// 从 WPF <see cref="Brush"/> 创建 <see cref="NColor"/>（仅支持 <see cref="SolidColorBrush"/>）。
    /// </summary>
    public static NColor ToNColor(this Brush brush)
    {
        return ((SolidColorBrush)brush).ToNColor();
    }
}

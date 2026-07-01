using System.Windows.Media;
using PCL.Core.UI.Theme;

namespace PCL.Core.UI.Theme;

/// <summary>
/// <see cref="LabColor"/> 的 WPF 扩展方法，提供与 WPF <see cref="Color"/> 的互转（使用 scRGB 线性通道）。<br/>
/// 替代 LabColor 中原有的 <c>FromWpfColor</c>/<c>ToWpfColor</c> 静态方法和 implicit operator。
/// </summary>
public static class LabColorWpfExtensions
{
    /// <summary>
    /// 从 WPF <see cref="Color"/> 创建 <see cref="LabColor"/>（使用 scRGB 线性通道）。
    /// </summary>
    public static LabColor FromWpfColor(this Color color)
    {
        return LabColor.FromRgb(color.ScR, color.ScG, color.ScB, color.ScA);
    }

    /// <summary>
    /// 将 <see cref="LabColor"/> 转换为 WPF <see cref="Color"/>（使用 scRGB 线性通道）。
    /// </summary>
    public static Color ToWpfColor(this LabColor lab)
    {
        var (a, r, g, b) = lab.ScRgb;
        return Color.FromScRgb(a, r, g, b);
    }
}

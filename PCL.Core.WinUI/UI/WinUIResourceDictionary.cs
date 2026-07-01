using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using PCL.Core.UI;
using Windows.UI;

namespace PCL.Core.UI;

/// <summary>
/// WinUI 3 资源字典实现，使用 <see cref="Microsoft.UI.Xaml.Application.Resources"/>。
/// </summary>
public sealed class WinUIResourceDictionary : IResourceDictionary
{
    public void SetScColor(string key, float alpha, float red, float green, float blue)
    {
        var color = ColorHelper.FromScRgb(alpha, red, green, blue);
        Application.Current.Resources[key] = color;
    }

    public void SetScColorBrush(string key, float alpha, float red, float green, float blue)
    {
        var color = ColorHelper.FromScRgb(alpha, red, green, blue);
        Application.Current.Resources[key] = new SolidColorBrush(color);
    }
}

/// <summary>
/// WinUI 颜色辅助类。<br/>
/// WinUI 的 <see cref="Windows.UI.Color"/> 没有 FromScRgb 方法，需手动从 scRGB 转换为 sRGB。
/// </summary>
internal static class ColorHelper
{
    public static Color FromScRgb(float alpha, float red, float green, float blue)
    {
        // scRGB 线性 → sRGB gamma 转换
        return Windows.UI.Color.FromArgb(
            (byte)(Math.Clamp(alpha, 0f, 1f) * 255f),
            ToSRgbByte(red),
            ToSRgbByte(green),
            ToSRgbByte(blue)
        );
    }

    private static byte ToSRgbByte(float linear)
    {
        linear = Math.Clamp(linear, 0f, 1f);
        // 标准 IEC 61966-2-1 sRGB 转换
        var srgb = linear <= 0.0031308f
            ? linear * 12.92f
            : 1.055f * MathF.Pow(linear, 1.0f / 2.4f) - 0.055f;
        return (byte)Math.Clamp(srgb * 255f + 0.5f, 0f, 255f);
    }
}

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PCL.Core.Minecraft.Instance;

/// <summary>
/// 表示一个 Minecraft 游戏实例（版本）。
/// </summary>
public sealed class McInstance
{
    /// <summary>
    /// 实例名称（版本文件夹名）。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 实例文件夹路径（如 .minecraft\versions\1.20.1）。
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// 游戏根文件夹路径（如 .minecraft）。
    /// </summary>
    public string GameFolder { get; }

    /// <summary>
    /// 版本 JSON 文件路径。
    /// </summary>
    public string JsonPath => System.IO.Path.Combine(Path, Name + ".json");

    /// <summary>
    /// 版本 JAR 文件路径（可能不存在）。
    /// </summary>
    public string JarPath => System.IO.Path.Combine(Path, Name + ".jar");

    /// <summary>
    /// 版本 JSON 中声定的版本 ID。
    /// </summary>
    public string? VersionId { get; private set; }

    /// <summary>
    /// 版本类型（release/snapshot/modified/...）。
    /// </summary>
    public string? Type { get; private set; }

    /// <summary>
    /// 父版本名称（用于 Forge/Fabric 等模组加载器）。
    /// </summary>
    public string? InheritsFrom { get; private set; }

    /// <summary>
    /// 主类名。
    /// </summary>
    public string? MainClass { get; private set; }

    /// <summary>
    /// 资源索引版本。
    /// </summary>
    public string? AssetsIndex { get; private set; }

    /// <summary>
    /// 所需 Java 主版本号（0 表示未指定）。
    /// </summary>
    public int JavaMajorVersion { get; private set; }

    /// <summary>
    /// 实例是否存在（版本 JSON 文件存在）。
    /// </summary>
    public bool Exists => File.Exists(JsonPath);

    /// <summary>
    /// 是否为自定义实例（有 inheritsFrom 或非标准类型）。
    /// </summary>
    public bool IsCustom => !string.IsNullOrEmpty(InheritsFrom) || Type == "modified";

    /// <summary>
    /// 版本描述信息（用于显示）。
    /// </summary>
    public string DisplayInfo
    {
        get
        {
            if (!string.IsNullOrEmpty(InheritsFrom))
                return $"{InheritsFrom} (modified)";
            return VersionId ?? Name;
        }
    }

    public McInstance(string name, string gameFolder)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        GameFolder = gameFolder ?? throw new ArgumentNullException(nameof(gameFolder));
        Path = System.IO.Path.Combine(gameFolder, "versions", name);
        LoadInfo();
    }

    /// <summary>
    /// 从版本 JSON 加载实例信息。
    /// </summary>
    private void LoadInfo()
    {
        try
        {
            if (!File.Exists(JsonPath)) return;
            var json = File.ReadAllText(JsonPath);
            var info = JsonSerializer.Deserialize<VersionJsonInfo>(json);
            if (info == null) return;
            VersionId = string.IsNullOrEmpty(info.Id) ? Name : info.Id;
            Type = info.Type ?? "release";
            InheritsFrom = info.InheritsFrom;
            MainClass = info.MainClass;
            AssetsIndex = info.Assets;
            JavaMajorVersion = info.JavaVersion?.MajorVersion ?? 0;
        }
        catch
        {
            // JSON 解析失败时使用默认值
            VersionId = Name;
            Type = "modified";
        }
    }

    /// <summary>
    /// 获取继承链上的原始版本名（递归查找 inheritsFrom）。
    /// </summary>
    public string GetRootVersion()
    {
        if (string.IsNullOrEmpty(InheritsFrom)) return Name;
        try
        {
            var parentPath = System.IO.Path.Combine(GameFolder, "versions", InheritsFrom, InheritsFrom + ".json");
            if (File.Exists(parentPath))
            {
                var parent = new McInstance(InheritsFrom, GameFolder);
                return parent.GetRootVersion();
            }
        }
        catch { /* 忽略递归错误 */ }
        return InheritsFrom;
    }

    public override string ToString() => DisplayInfo;

    /// <summary>
    /// 版本 JSON 反序列化模型（仅提取需要的字段）。
    /// </summary>
    private sealed class VersionJsonInfo
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("type")] public string? Type { get; set; }
        [JsonPropertyName("mainClass")] public string? MainClass { get; set; }
        [JsonPropertyName("inheritsFrom")] public string? InheritsFrom { get; set; }
        [JsonPropertyName("assets")] public string? Assets { get; set; }
        [JsonPropertyName("javaVersion")] public JavaVersionInfo? JavaVersion { get; set; }
    }

    private sealed class JavaVersionInfo
    {
        [JsonPropertyName("component")] public string? Component { get; set; }
        [JsonPropertyName("majorVersion")] public int MajorVersion { get; set; }
    }
}

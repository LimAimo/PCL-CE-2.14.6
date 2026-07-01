using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PCL.Core.Minecraft.Launch;

/// <summary>
/// Minecraft 版本 JSON 模型（仅提取启动所需字段）。
/// </summary>
internal sealed class VersionJson
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("mainClass")] public string? MainClass { get; set; }
    [JsonPropertyName("inheritsFrom")] public string? InheritsFrom { get; set; }
    [JsonPropertyName("assets")] public string? Assets { get; set; }
    [JsonPropertyName("libraries")] public List<LibraryJson>? Libraries { get; set; }
    [JsonPropertyName("arguments")] public ArgumentsJson? Arguments { get; set; }
    [JsonPropertyName("minecraftArguments")] public string? MinecraftArguments { get; set; }
    [JsonPropertyName("javaVersion")] public JavaVersionJson? JavaVersion { get; set; }
    [JsonPropertyName("minimumLauncherVersion")] public int MinimumLauncherVersion { get; set; }
    [JsonPropertyName("releaseTime")] public string? ReleaseTime { get; set; }
    [JsonPropertyName("time")] public string? Time { get; set; }
    [JsonPropertyName("downloads")] public DownloadsJson? Downloads { get; set; }
}

internal sealed class LibraryJson
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("downloads")] public LibraryDownloadsJson? Downloads { get; set; }
    [JsonPropertyName("rules")] public List<RuleJson>? Rules { get; set; }
    [JsonPropertyName("natives")] public Dictionary<string, string>? Natives { get; set; }
}

internal sealed class ArgumentsJson
{
    [JsonPropertyName("game")] public List<object>? Game { get; set; }
    [JsonPropertyName("jvm")] public List<object>? Jvm { get; set; }
}

internal sealed class JavaVersionJson
{
    [JsonPropertyName("component")] public string? Component { get; set; }
    [JsonPropertyName("majorVersion")] public int MajorVersion { get; set; }
}

internal sealed class RuleJson
{
    [JsonPropertyName("action")] public string? Action { get; set; }
    [JsonPropertyName("os")] public OsJson? Os { get; set; }
}

internal sealed class OsJson
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("arch")] public string? Arch { get; set; }
}

internal sealed class LibraryDownloadsJson
{
    [JsonPropertyName("artifact")] public ArtifactDownloadJson? Artifact { get; set; }
    [JsonPropertyName("classifiers")] public Dictionary<string, ArtifactDownloadJson>? Classifiers { get; set; }
}

internal sealed class ArtifactDownloadJson
{
    [JsonPropertyName("path")] public string? Path { get; set; }
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("size")] public long Size { get; set; }
    [JsonPropertyName("sha1")] public string? Sha1 { get; set; }
}

internal sealed class DownloadsJson
{
    [JsonPropertyName("client")] public ClientDownloadJson? Client { get; set; }
}

internal sealed class ClientDownloadJson
{
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("size")] public long Size { get; set; }
    [JsonPropertyName("sha1")] public string? Sha1 { get; set; }
}

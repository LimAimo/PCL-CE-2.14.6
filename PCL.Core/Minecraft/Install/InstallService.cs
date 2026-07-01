using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using PCL.Core.App;
using PCL.Core.App.Tasks;
using PCL.Core.IO.Download;
using PCL.Core.IO.Net;
using PCL.Core.Minecraft.Versions;

namespace PCL.Core.Minecraft.Install;

/// <summary>
/// Minecraft 版本安装服务。
/// </summary>
public static class InstallService
{
    /// <summary>
    /// 安装 Minecraft 版本。
    /// </summary>
    /// <param name="versionId">版本 ID（如 "1.20.1"）</param>
    /// <param name="gameFolder">游戏文件夹路径</param>
    /// <param name="token">取消令牌</param>
    public static async Task InstallAsync(string versionId, string gameFolder, CancellationToken token = default)
    {
        var versionJson = await VersionManifestService.GetVersionJsonAsync(versionId, token);
        if (string.IsNullOrEmpty(versionJson))
            throw new InvalidOperationException($"无法获取版本 {versionId} 的信息");

        // 解析版本详情
        var versionInfo = JsonSerializer.Deserialize<InstallVersionInfo>(versionJson)
            ?? throw new InvalidOperationException("版本 JSON 解析失败");

        // 创建版本目录
        var versionDir = Path.Combine(gameFolder, "versions", versionId);
        Directory.CreateDirectory(versionDir);

        // 保存版本 JSON
        var jsonPath = Path.Combine(versionDir, versionId + ".json");
        await File.WriteAllTextAsync(jsonPath, versionJson, token);

        // 下载客户端 JAR
        if (versionInfo.Downloads?.Client?.Url != null)
        {
            var jarPath = Path.Combine(versionDir, versionId + ".jar");
            await NDlScheduler.SubmitAsync(versionInfo.Downloads.Client.Url, jarPath, $"Minecraft {versionId}");
        }

        // 下载资源索引
        if (!string.IsNullOrEmpty(versionInfo.Assets))
        {
            await DownloadAssetIndexAsync(gameFolder, versionInfo.Assets, token);
        }

        // 下载库文件
        if (versionInfo.Libraries != null)
        {
            await DownloadLibrariesAsync(gameFolder, versionInfo.Libraries, token);
        }
    }

    /// <summary>
    /// 下载资源索引文件。
    /// </summary>
    private static async Task DownloadAssetIndexAsync(string gameFolder, string assetsIndex, CancellationToken token)
    {
        var indexUrl = $"https://launchermeta.mojang.com/v1/packages/{assetsIndex}/{assetsIndex}.json";
        var indexPath = Path.Combine(gameFolder, "assets", "indexes", assetsIndex + ".json");
        var indexDir = Path.GetDirectoryName(indexPath);
        if (!string.IsNullOrEmpty(indexDir)) Directory.CreateDirectory(indexDir);
        await NDlScheduler.SubmitAsync(indexUrl, indexPath, $"资源索引 {assetsIndex}");
    }

    /// <summary>
    /// 下载库文件。
    /// </summary>
    private static async Task DownloadLibrariesAsync(string gameFolder, List<InstallLibraryInfo> libraries, CancellationToken token)
    {
        var librariesDir = Path.Combine(gameFolder, "libraries");
        foreach (var lib in libraries)
        {
            if (lib.Downloads?.Artifact?.Path == null || lib.Downloads.Artifact.Url == null) continue;

            // 检查规则
            if (!CheckRules(lib.Rules)) continue;

            var libPath = Path.Combine(librariesDir, lib.Downloads.Artifact.Path);
            if (File.Exists(libPath)) continue; // 已存在则跳过

            await NDlScheduler.SubmitAsync(lib.Downloads.Artifact.Url, libPath, $"库 {lib.Name}");
        }
    }

    /// <summary>
    /// 检查库的规则是否允许当前平台。
    /// </summary>
    private static bool CheckRules(List<InstallRuleInfo>? rules)
    {
        if (rules == null || rules.Count == 0) return true;
        bool allowed = false;
        foreach (var rule in rules)
        {
            bool osMatch = rule.Os?.Name == null || rule.Os.Name == "windows";
            if (osMatch)
            {
                allowed = rule.Action == "allow";
            }
        }
        return allowed;
    }
}

/// <summary>
/// 安装用版本信息（仅提取下载所需字段）。
/// </summary>
internal sealed class InstallVersionInfo
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("assets")] public string? Assets { get; set; }
    [JsonPropertyName("libraries")] public List<InstallLibraryInfo>? Libraries { get; set; }
    [JsonPropertyName("downloads")] public InstallDownloadsInfo? Downloads { get; set; }
}

internal sealed class InstallLibraryInfo
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("downloads")] public InstallLibDownloadsInfo? Downloads { get; set; }
    [JsonPropertyName("rules")] public List<InstallRuleInfo>? Rules { get; set; }
}

internal sealed class InstallLibDownloadsInfo
{
    [JsonPropertyName("artifact")] public InstallArtifactInfo? Artifact { get; set; }
}

internal sealed class InstallArtifactInfo
{
    [JsonPropertyName("path")] public string? Path { get; set; }
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("size")] public long Size { get; set; }
    [JsonPropertyName("sha1")] public string? Sha1 { get; set; }
}

internal sealed class InstallDownloadsInfo
{
    [JsonPropertyName("client")] public InstallClientInfo? Client { get; set; }
}

internal sealed class InstallClientInfo
{
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("size")] public long Size { get; set; }
    [JsonPropertyName("sha1")] public string? Sha1 { get; set; }
}

internal sealed class InstallRuleInfo
{
    [JsonPropertyName("action")] public string? Action { get; set; }
    [JsonPropertyName("os")] public InstallOsInfo? Os { get; set; }
}

internal sealed class InstallOsInfo
{
    [JsonPropertyName("name")] public string? Name { get; set; }
}

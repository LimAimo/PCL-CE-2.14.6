using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using PCL.Core.IO.Net;

namespace PCL.Core.Minecraft.Versions;

/// <summary>
/// Mojang 版本清单服务，获取 Minecraft 版本列表。
/// </summary>
public static class VersionManifestService
{
    private const string ManifestUrl = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
    private static List<VersionEntry>? _cachedVersions;
    private static (string release, string snapshot)? _latest;

    /// <summary>
    /// 获取版本列表（带缓存）。
    /// </summary>
    public static async Task<List<VersionEntry>> GetVersionsAsync(bool forceRefresh = false, CancellationToken token = default)
    {
        if (_cachedVersions != null && !forceRefresh) return _cachedVersions;

        var client = NetworkService.GetClient();
        var json = await client.GetStringAsync(ManifestUrl, token);
        var manifest = JsonSerializer.Deserialize<ManifestJson>(json);

        _latest = manifest?.Latest != null
            ? (manifest.Latest.Release ?? "", manifest.Latest.Snapshot ?? "")
            : ("", "");
        _cachedVersions = manifest?.Versions ?? new List<VersionEntry>();

        return _cachedVersions;
    }

    /// <summary>
    /// 获取最新版本信息。
    /// </summary>
    public static async Task<(string release, string snapshot)> GetLatestAsync(CancellationToken token = default)
    {
        if (_latest == null) await GetVersionsAsync(token: token);
        return _latest ?? ("", "");
    }

    /// <summary>
    /// 获取指定版本的详细 JSON（包含下载链接）。
    /// </summary>
    public static async Task<string?> GetVersionJsonAsync(string versionId, CancellationToken token = default)
    {
        var versions = await GetVersionsAsync(token: token);
        var entry = versions.Find(v => v.Id == versionId);
        if (entry?.Url == null) return null;

        var client = NetworkService.GetClient();
        return await client.GetStringAsync(entry.Url, token);
    }

    /// <summary>
    /// 版本条目。
    /// </summary>
    public sealed class VersionEntry
    {
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        [JsonPropertyName("type")] public string Type { get; set; } = "";
        [JsonPropertyName("url")] public string? Url { get; set; }
        [JsonPropertyName("time")] public string? Time { get; set; }
        [JsonPropertyName("releaseTime")] public string? ReleaseTime { get; set; }

        public bool IsRelease => Type == "release";
        public bool IsSnapshot => Type == "snapshot";
    }

    private sealed class ManifestJson
    {
        [JsonPropertyName("latest")] public LatestJson? Latest { get; set; }
        [JsonPropertyName("versions")] public List<VersionEntry>? Versions { get; set; }
    }

    private sealed class LatestJson
    {
        [JsonPropertyName("release")] public string? Release { get; set; }
        [JsonPropertyName("snapshot")] public string? Snapshot { get; set; }
    }
}

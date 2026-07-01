using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using PCL.Core.IO.Net;
using PCL.Core.Minecraft.ResourceProject.Modrinth;

namespace PCL.Core.Minecraft.Resources;

/// <summary>
/// Modrinth API 客户端，提供资源搜索与查询功能。
/// </summary>
public static class ModrinthClient
{
    private const string BaseUrl = "https://api.modrinth.com/v2";

    /// <summary>
    /// 搜索 Modrinth 资源。
    /// </summary>
    /// <param name="query">搜索关键字</param>
    /// <param name="projectType">资源类型（mod/modpack/resourcepack/shader）</param>
    /// <param name="gameVersion">游戏版本过滤（可选）</param>
    /// <param name="limit">返回数量上限</param>
    /// <param name="offset">偏移量</param>
    /// <param name="token">取消令牌</param>
    public static async Task<ModrinthSearchResult> SearchAsync(
        string query,
        string? projectType = null,
        string? gameVersion = null,
        int limit = 20,
        int offset = 0,
        CancellationToken token = default)
    {
        var client = NetworkService.GetClient();
        var queryBuilder = HttpUtility.ParseQueryString(string.Empty);
        queryBuilder["query"] = query;
        queryBuilder["limit"] = limit.ToString();
        queryBuilder["offset"] = offset.ToString();
        queryBuilder["index"] = "relevance";

        var facets = new List<List<string>>();
        if (!string.IsNullOrEmpty(projectType))
            facets.Add(new List<string> { $"project_type:{projectType}" });
        if (!string.IsNullOrEmpty(gameVersion))
            facets.Add(new List<string> { $"versions:{gameVersion}" });
        if (facets.Count > 0)
            queryBuilder["facets"] = JsonSerializer.Serialize(facets);

        var url = $"{BaseUrl}/search?{queryBuilder}";
        var result = await client.GetFromJsonAsync<ModrinthSearchResult>(url, token);
        return result ?? new ModrinthSearchResult();
    }

    /// <summary>
    /// 获取项目详情。
    /// </summary>
    public static async Task<ModrinthProject?> GetProjectAsync(string idOrSlug, CancellationToken token = default)
    {
        var client = NetworkService.GetClient();
        return await client.GetFromJsonAsync<ModrinthProject>($"{BaseUrl}/project/{idOrSlug}", token);
    }

    /// <summary>
    /// 获取项目版本列表。
    /// </summary>
    public static async Task<List<ModrinthVersion>> GetVersionsAsync(string idOrSlug, CancellationToken token = default)
    {
        var client = NetworkService.GetClient();
        var versions = await client.GetFromJsonAsync<List<ModrinthVersion>>($"{BaseUrl}/project/{idOrSlug}/version", token);
        return versions ?? new List<ModrinthVersion>();
    }
}

/// <summary>
/// Modrinth 搜索结果。
/// </summary>
public sealed class ModrinthSearchResult
{
    [JsonPropertyName("hits")] public List<ModrinthSearchHit> Hits { get; set; } = new();
    [JsonPropertyName("offset")] public int Offset { get; set; }
    [JsonPropertyName("limit")] public int Limit { get; set; }
    [JsonPropertyName("total_hits")] public int TotalHits { get; set; }
}

/// <summary>
/// Modrinth 搜索结果条目。
/// </summary>
public sealed class ModrinthSearchHit
{
    [JsonPropertyName("project_id")] public string ProjectId { get; set; } = "";
    [JsonPropertyName("project_type")] public string ProjectType { get; set; } = "";
    [JsonPropertyName("slug")] public string Slug { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("icon_url")] public string? IconUrl { get; set; }
    [JsonPropertyName("downloads")] public int Downloads { get; set; }
    [JsonPropertyName("follows")] public int Follows { get; set; }
    [JsonPropertyName("versions")] public List<string> Versions { get; set; } = new();
    [JsonPropertyName("latest_version")] public string? LatestVersion { get; set; }
    [JsonPropertyName("author")] public string Author { get; set; } = "";
}

/// <summary>
/// Modrinth 版本信息。
/// </summary>
public sealed class ModrinthVersion
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("version_number")] public string VersionNumber { get; set; } = "";
    [JsonPropertyName("game_versions")] public List<string> GameVersions { get; set; } = new();
    [JsonPropertyName("loaders")] public List<string> Loaders { get; set; } = new();
    [JsonPropertyName("files")] public List<ModrinthFile> Files { get; set; } = new();
    [JsonPropertyName("date_published")] public string DatePublished { get; set; } = "";
    [JsonPropertyName("version_type")] public string VersionType { get; set; } = "";
}

/// <summary>
/// Modrinth 版本文件。
/// </summary>
public sealed class ModrinthFile
{
    [JsonPropertyName("url")] public string Url { get; set; } = "";
    [JsonPropertyName("filename")] public string Filename { get; set; } = "";
    [JsonPropertyName("primary")] public bool Primary { get; set; }
    [JsonPropertyName("size")] public long Size { get; set; }
}

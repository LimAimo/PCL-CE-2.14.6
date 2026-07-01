using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using PCL.Core.App;
using PCL.Core.IO.Net;
using PCL.Core.Minecraft.ResourceProject.Curseforge;

namespace PCL.Core.Minecraft.Resources;

/// <summary>
/// Curseforge API 客户端，提供资源搜索与查询功能。
/// </summary>
public static class CurseforgeClient
{
    private const string BaseUrl = "https://api.curseforge.com/v1";
    private const int MinecraftGameId = 432;

    private static HttpClient GetClient()
    {
        var client = NetworkService.GetClient();
        if (!client.DefaultRequestHeaders.Contains("X-API-Key"))
        {
            client.DefaultRequestHeaders.Add("X-API-Key", Secrets.CurseForgeAPIKey);
        }
        return client;
    }

    /// <summary>
    /// 搜索 Curseforge 资源。
    /// </summary>
    /// <param name="query">搜索关键字</param>
    /// <param name="classId">资源分类 ID（6=Mod, 4471=整合包, 12=资源包, 6552=光影包）</param>
    /// <param name="gameVersion">游戏版本过滤</param>
    /// <param name="pageSize">返回数量</param>
    /// <param name="token">取消令牌</param>
    public static async Task<CurseforgeSearchResult> SearchAsync(
        string query,
        int? classId = null,
        string? gameVersion = null,
        int pageSize = 20,
        CancellationToken token = default)
    {
        var client = GetClient();
        var queryBuilder = HttpUtility.ParseQueryString(string.Empty);
        queryBuilder["gameId"] = MinecraftGameId.ToString();
        queryBuilder["searchFilter"] = query;
        queryBuilder["pageSize"] = pageSize.ToString();
        if (classId.HasValue) queryBuilder["classId"] = classId.Value.ToString();
        if (!string.IsNullOrEmpty(gameVersion)) queryBuilder["gameVersion"] = gameVersion;
        queryBuilder["sortField"] = "2"; // 按流行度排序

        var url = $"{BaseUrl}/mods/search?{queryBuilder}";
        var result = await client.GetFromJsonAsync<CurseforgeSearchResult>(url, token);
        return result ?? new CurseforgeSearchResult();
    }

    /// <summary>
    /// 获取项目详情。
    /// </summary>
    public static async Task<CurseforgeProject?> GetProjectAsync(int modId, CancellationToken token = default)
    {
        var client = GetClient();
        var result = await client.GetFromJsonAsync<CurseforgeProjectResponse>($"{BaseUrl}/mods/{modId}", token);
        return result?.data;
    }

    /// <summary>
    /// 获取项目文件列表。
    /// </summary>
    public static async Task<List<CurseforgeFile>> GetFilesAsync(int modId, string? gameVersion = null, CancellationToken token = default)
    {
        var client = GetClient();
        var queryBuilder = HttpUtility.ParseQueryString(string.Empty);
        if (!string.IsNullOrEmpty(gameVersion)) queryBuilder["gameVersion"] = gameVersion;
        var qs = queryBuilder.ToString();
        var url = string.IsNullOrEmpty(qs)
            ? $"{BaseUrl}/mods/{modId}/files"
            : $"{BaseUrl}/mods/{modId}/files?{qs}";
        var result = await client.GetFromJsonAsync<CurseforgeFilesResponse>(url, token);
        return result?.Data ?? new List<CurseforgeFile>();
    }

    // Curseforge 分类 ID 常量
    public static class ClassIds
    {
        public const int Mod = 6;
        public const int Modpack = 4471;
        public const int ResourcePack = 12;
        public const int ShaderPack = 6552;
        public const int World = 17;
        public const int DataPack = 6945;
    }
}

/// <summary>
/// Curseforge 搜索结果。
/// </summary>
public sealed class CurseforgeSearchResult
{
    [JsonPropertyName("data")] public List<CurseforgeProject> Data { get; set; } = new();
    [JsonPropertyName("pagination")] public CurseforgePagination? Pagination { get; set; }
}

/// <summary>
/// Curseforge 分页信息。
/// </summary>
public sealed class CurseforgePagination
{
    [JsonPropertyName("index")] public int Index { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
    [JsonPropertyName("resultCount")] public int ResultCount { get; set; }
    [JsonPropertyName("totalCount")] public int TotalCount { get; set; }
}

/// <summary>
/// Curseforge 文件列表响应。
/// </summary>
internal sealed class CurseforgeFilesResponse
{
    [JsonPropertyName("data")] public List<CurseforgeFile> Data { get; set; } = new();
}

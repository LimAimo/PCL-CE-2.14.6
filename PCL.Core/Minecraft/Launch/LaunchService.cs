using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PCL.Core.App;
using PCL.Core.App.Tasks;
using PCL.Core.Minecraft.Instance;
using PCL.Core.Minecraft.Java;

// 类型别名
using LauncherVisibility = PCL.Core.App.LauncherVisibility;

namespace PCL.Core.Minecraft.Launch;

/// <summary>
/// Minecraft 启动服务。<br/>
/// 负责拼装启动命令并启动游戏进程。
/// </summary>
public static class LaunchService
{
    /// <summary>
    /// 当前是否正在启动。
    /// </summary>
    public static bool IsLaunching { get; private set; }

    /// <summary>
    /// 当前启动状态。
    /// </summary>
    public static LaunchState State { get; private set; } = LaunchState.Idle;

    /// <summary>
    /// 启动状态变更事件。
    /// </summary>
    public static event Action<LaunchState, string>? StateChanged;

    private static void SetState(LaunchState state, string message = "")
    {
        State = state;
        IsLaunching = state == LaunchState.Preparing || state == LaunchState.Launching;
        StateChanged?.Invoke(state, message);
    }

    /// <summary>
    /// 启动 Minecraft。
    /// </summary>
    /// <param name="options">启动选项，为 null 时使用默认选项</param>
    /// <returns>是否成功开始启动</returns>
    public static async Task<bool> LaunchAsync(McLaunchOptions? options = null)
    {
        if (IsLaunching)
        {
            SetState(LaunchState.Failed, "已有游戏正在启动中");
            return false;
        }

        options ??= new McLaunchOptions();
        var instance = options.Instance ?? InstanceService.CurrentInstance;
        if (instance == null || !instance.Exists)
        {
            SetState(LaunchState.Failed, "未选择有效的游戏实例");
            return false;
        }

        try
        {
            SetState(LaunchState.Preparing, "正在准备启动环境...");

            // 选择 Java
            var java = await SelectJavaAsync(instance);
            if (java == null)
            {
                SetState(LaunchState.Failed, "未找到合适的 Java 运行时");
                return false;
            }

            SetState(LaunchState.Preparing, "正在解析版本信息...");

            // 解析版本 JSON 链
            var versionChain = ResolveVersionChain(instance);
            if (versionChain.Count == 0)
            {
                SetState(LaunchState.Failed, "无法解析版本信息");
                return false;
            }

            SetState(LaunchState.Preparing, "正在拼装启动命令...");

            // 拼装启动命令
            var command = BuildLaunchCommand(java, instance, versionChain, options);

            // 保存批处理（如果需要）
            if (!string.IsNullOrEmpty(options.SaveBatchPath))
            {
                await File.WriteAllTextAsync(options.SaveBatchPath, command);
                SetState(LaunchState.Finished, "启动脚本已保存");
                return true;
            }

            SetState(LaunchState.Launching, "正在启动游戏...");

            // 启动进程
            var processInfo = new ProcessStartInfo
            {
                FileName = java.Installation.JavaExePath,
                Arguments = command,
                UseShellExecute = false,
                CreateNoWindow = false,
                WorkingDirectory = instance.GameFolder
            };

            var process = new Process { StartInfo = processInfo };
            process.Start();

            SetState(LaunchState.Running, $"游戏已启动 (PID: {process.Id})");

            // 根据配置处理启动器可见性
            HandleLauncherVisibility();

            return true;
        }
        catch (Exception ex)
        {
            SetState(LaunchState.Failed, $"启动失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 选择适合实例的 Java。
    /// </summary>
    private static async Task<JavaEntry?> SelectJavaAsync(McInstance instance)
    {
        var mgr = JavaService.JavaManager;
        if (mgr == null || !mgr.ExistAnyJava()) return null;

        // 优先使用实例配置的 Java
        var instanceJava = App.Config.Instance.SelectedJava[instance.Path];
        if (!string.IsNullOrEmpty(instanceJava))
        {
            var entry = mgr.Get(instanceJava);
            if (entry != null && entry.IsEnabled) return entry;
        }

        // 其次使用全局配置的 Java
        var globalJava = App.Config.Launch.SelectedJava;
        if (!string.IsNullOrEmpty(globalJava))
        {
            var entry = mgr.Get(globalJava);
            if (entry != null && entry.IsEnabled) return entry;
        }

        // 自动选择
        var minVersion = instance.JavaMajorVersion > 0 ? new Version(instance.JavaMajorVersion, 0) : new Version(8, 0);
        var maxVersion = new Version(int.MaxValue, 0);
        var suitable = await mgr.SelectSuitableJavaAsync(minVersion, maxVersion);
        return suitable.FirstOrDefault();
    }

    /// <summary>
    /// 解析版本继承链（从当前实例到根版本）。
    /// </summary>
    private static List<VersionJson> ResolveVersionChain(McInstance instance)
    {
        var chain = new List<VersionJson>();
        var current = instance;
        var visited = new HashSet<string>();

        while (current != null && current.Exists && visited.Add(current.Name))
        {
            var json = LoadVersionJson(current);
            if (json == null) break;
            chain.Add(json);

            if (string.IsNullOrEmpty(json.InheritsFrom)) break;
            current = new McInstance(json.InheritsFrom, instance.GameFolder);
        }

        return chain;
    }

    /// <summary>
    /// 加载版本 JSON。
    /// </summary>
    private static VersionJson? LoadVersionJson(McInstance instance)
    {
        try
        {
            if (!File.Exists(instance.JsonPath)) return null;
            var content = File.ReadAllText(instance.JsonPath);
            return JsonSerializer.Deserialize<VersionJson>(content);
        }
        catch { return null; }
    }

    /// <summary>
    /// 拼装启动命令。
    /// </summary>
    private static string BuildLaunchCommand(
        JavaEntry java,
        McInstance instance,
        List<VersionJson> versionChain,
        McLaunchOptions options)
    {
        var rootVersion = versionChain[^1];
        var args = new List<string>();

        // JVM 参数
        var memorySize = App.Config.Launch.CustomMemorySize;
        var jvmArgs = App.Config.Launch.JvmArgs ?? "";
        args.Add($"-Xmx{Math.Max(memorySize, 1)}G");
        args.Add($"-Xms{Math.Max(memorySize / 2, 1)}G");
        if (!string.IsNullOrWhiteSpace(jvmArgs))
            args.AddRange(jvmArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // 类路径
        var classpath = BuildClasspath(instance, versionChain);
        args.Add($"-cp \"{classpath}\"");

        // 主类
        var mainClass = versionChain[0].MainClass ?? rootVersion.MainClass ?? "net.minecraft.client.main.Main";
        args.Add(mainClass);

        // 游戏参数
        args.Add($"--username {GetUsername()}");
        args.Add($"--version \"{instance.Name}\"");
        args.Add($"--gameDir \"{instance.GameFolder}\"");
        args.Add($"--assetsDir \"{Path.Combine(instance.GameFolder, "assets")}\"");
        var assetsIndex = versionChain[0].Assets ?? rootVersion.Assets ?? instance.Name;
        args.Add($"--assetsIndex {assetsIndex}");
        args.Add($"--uuid {GetUuid()}");
        args.Add($"--accessToken {GetAccessToken()}");
        args.Add("--userType mojang");

        // 服务器 IP
        var serverIp = options.ServerIp ?? App.Config.Instance.ServerToEnter[instance.Path];
        if (!string.IsNullOrEmpty(serverIp))
        {
            args.Add($"--server {serverIp.Split(':')[0]}");
            if (serverIp.Contains(':'))
                args.Add($"--port {serverIp.Split(':')[1]}");
        }

        // 额外参数
        if (options.ExtraArgs.Count > 0)
            args.AddRange(options.ExtraArgs);

        // 版本 JSON 中的额外游戏参数
        foreach (var version in versionChain)
        {
            if (version.Arguments?.Game != null)
            {
                foreach (var arg in version.Arguments.Game)
                {
                    if (arg is string s && !string.IsNullOrEmpty(s))
                        args.Add(s);
                }
            }
        }

        return string.Join(" ", args);
    }

    /// <summary>
    /// 构建类路径（classpath）。
    /// </summary>
    private static string BuildClasspath(McInstance instance, List<VersionJson> versionChain)
    {
        var libraries = new List<string>();

        // 从根版本到子版本收集库
        for (int i = versionChain.Count - 1; i >= 0; i--)
        {
            if (versionChain[i].Libraries == null) continue;
            foreach (var lib in versionChain[i].Libraries!)
            {
                var path = ResolveLibraryPath(instance.GameFolder, lib);
                if (!string.IsNullOrEmpty(path) && !libraries.Contains(path))
                    libraries.Add(path);
            }
        }

        // 添加版本 JAR
        foreach (var version in versionChain)
        {
            var jarPath = Path.Combine(instance.GameFolder, "versions", version.Id ?? "", (version.Id ?? "") + ".jar");
            if (File.Exists(jarPath))
                libraries.Add(jarPath);
        }

        return string.Join(System.IO.Path.PathSeparator, libraries);
    }

    /// <summary>
    /// 解析库文件路径。
    /// </summary>
    private static string? ResolveLibraryPath(string gameFolder, LibraryJson lib)
    {
        if (lib.Name == null) return null;
        // 格式: group:artifact:version
        var parts = lib.Name.Split(':');
        if (parts.Length < 3) return null;
        var groupPath = parts[0].Replace('.', System.IO.Path.DirectorySeparatorChar);
        var artifact = parts[1];
        var version = parts[2];
        var fileName = $"{artifact}-{version}.jar";
        var fullPath = Path.Combine(gameFolder, "libraries", groupPath, artifact, version, fileName);
        return File.Exists(fullPath) ? fullPath : null;
    }

    private static string GetUsername()
    {
        var name = App.States.Game.LegacyProfile.LoginLegacyName;
        return string.IsNullOrEmpty(name) ? "Player" : name;
    }

    private static string GetUuid()
    {
        var uuid = App.States.Game.LegacyProfile.AuthUuid;
        return string.IsNullOrEmpty(uuid) ? "00000000-0000-0000-0000-000000000000" : uuid;
    }

    private static string GetAccessToken()
    {
        return "0"; // 离线模式令牌
    }

    private static void HandleLauncherVisibility()
    {
        var visibility = App.Config.Launch.LauncherVisibility;
        switch (visibility)
        {
            case LauncherVisibility.ExitImmediately:
                // 退出启动器
                break;
            case LauncherVisibility.HideAndExit:
            case LauncherVisibility.HideAndReopen:
                // 隐藏启动器
                break;
            default:
                // 不做任何操作
                break;
        }
    }
}

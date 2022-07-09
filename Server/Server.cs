using BepInEx;
using Server.Emulator.EagleTcpPatches;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

// ReSharper disable All

namespace Server;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Server : BaseUnityPlugin
{
    public static BepInEx.Logging.ManualLogSource logger;
    public static Emulator.Database.Database Database;
    public static Emulator.PlaceholderServerData PlaceholderServerData;
    public static OsuManiaLoader.Loader ManiaBeatmapsLoader;
    public static List<string> MustImplement = new();
    public static bool Debug = false;
    private static Process _process;
    private static bool ShuttingDown = false;
    private static bool ManiaLoaderDisabled = true;

    private void Awake()
    {
        _instance = this;
        logger = Logger;
        Database = new Emulator.Database.Database();
        PlaceholderServerData = new();
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        GeneralPatches.TipHelperInputPatches.Hook();
        HookManager.Instance.Create();
        var currentVersion = new AutoUpdater.Tag(PluginInfo.PLUGIN_VERSION);

        var releasesApiLink = "https://api.github.com/repos/Invaxion-Server-Emulator/invaxion-server-emulator/releases";
        var headers = new Dictionary<string, string> { { "Accept", "application/vnd.github.v3+json" } };
        var checkUpdateReq = new Networking.Request(releasesApiLink, headers);
        StartCoroutine(checkUpdateReq.Download(((request, success) =>
        {
            if (success)
            {
                AutoUpdater.GithubReleases.Release NewVersion = null;
                var releases = LitJson.JsonMapper.ToObject<AutoUpdater.GithubReleases.Release[]>(request._www.text);
                foreach (var release in releases)
                {
                    var releaseVersion = new AutoUpdater.Tag(release.tag_name);
                    if (releaseVersion > currentVersion)
                    {
                        NewVersion = release;
                    }
                }
                if (NewVersion != null)
                {
                    var releaseVersion = new AutoUpdater.Tag(NewVersion.tag_name);
                    logger.LogInfo($"A new version is available on GitHub! (v{releaseVersion})");
                    void ShowUpdateDialog()
                    {
                        if (DiscordRichPresence.GameState.CurrentScene == "MenuScene")
                        {
                            Emulator.Tools.Run.After(1f, () =>
                            {
                                Aquatrax.TipHelper.Instance.InitTipMode(Aquatrax.TipHelper.TipsMode.two);
                                Aquatrax.TipHelper.Instance.InitText($"An update for ServerEmulator has been found ({currentVersion} -> {releaseVersion}), update now?");
                                Aquatrax.TipHelper.Instance.Commit = delegate ()
                                {
                                    AutoUpdater.Update.Procceed(NewVersion);
                                };
                            });
                            DiscordRichPresence.GameEvents.switchScene -= ShowUpdateDialog;
                        }
                    }
                    DiscordRichPresence.GameEvents.switchScene += ShowUpdateDialog;
                }
            }
        })));

        if (!ManiaLoaderDisabled)
        {
            ManiaBeatmapsLoader = new OsuManiaLoader.Loader();
            if (ManiaBeatmapsLoader.BeatmapPacks.Count > 0) Logger.LogInfo($"Loaded {ManiaBeatmapsLoader.BeatmapPacks.Count} osu!mania packs!");
        }

        if (File.Exists(Path.Combine(Path.Combine("INVAXION_Data", "Plugins"), "discord_game_sdk.dll")))
        {
            DiscordRichPresence.Data.Init();
            DiscordRichPresence.GameEventHooks.Hook();
        }

        if (Debug && File.Exists("BepInEx/watch_logs.py"))
        {
            _process = new Process()
            {
                StartInfo =
                {
                    FileName = "python.exe",
                    Arguments = "watch_logs.py",
                    WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "BepInEx"),
                }
            };
            _process.Start();
        }
    }

    private static long timeDelta = TimeHelper.getCurUnixTimeOfSec();

    public static Server Instance
    {
        get => _instance;
    }
    private static Server _instance;

    public void startCoroutine(System.Collections.IEnumerator routine)
    {
        StartCoroutine(routine);
    }

    private void Update()
    {
        if (ShuttingDown)
            return;
        if (TimeHelper.getCurUnixTimeOfSec() - timeDelta >= 5)
        {
            // Updates the presence every 5 seconds
            timeDelta = TimeHelper.getCurUnixTimeOfSec();
            DiscordRichPresence.Data.UpdateActivity();
        }

        DiscordRichPresence.Data.Poll();
    }

    private void OnApplicationQuit()
    {
        ShuttingDown = true;
        DiscordRichPresence.Data._activityManager.ClearActivity(
            (
                result =>
                {
                    // why is a callback required?
                }
            )
        );
        DiscordRichPresence.Data.Poll();

        if (Debug && _process != null)
        {
            _process.Kill();
            _process = null;
        }

        if (!Debug) return;

        var commands = new List<string>();

        foreach (var command in MustImplement)
        {
            if (!commands.Contains(command))
            {
                commands.Add($"{command} (x{MustImplement.Count(command.Equals)})");
            }
        }

        var output = commands.Aggregate("", (current, line) => current + $"{line}\n");
        File.WriteAllText("must-implement.txt", output);
    }
}

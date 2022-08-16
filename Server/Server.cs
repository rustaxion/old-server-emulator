using System;
using BepInEx;
using BepInEx.Configuration;
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
    private static Process _process;
    private static bool ShuttingDown = false;


    // Configuration
    private static ConfigEntry<bool> _EnableManiaLoader;
    private static ConfigEntry<bool> _Debug;
    private static ConfigEntry<bool> _CheckForUpdates;

    public static bool EnableManiaLoader { get => _EnableManiaLoader.Value; }
    public static bool Debug { get => _Debug.Value; }
    public static bool CheckForUpdates { get => _CheckForUpdates.Value; }


    private void Awake()
    {
        _EnableManiaLoader = Config.Bind("General", "EnableManiaLoader", false, "Enables/Disables loading osu!mania beatmaps into the game.");
        _Debug = Config.Bind("General", "EnableDebug", false, "Enables/Disables debugging messages and utils.");
        _CheckForUpdates = Config.Bind("General", "CheckForUpdates", true, "Enables/Disables checking for updates.");

        _instance = this;
        logger = Logger;
        Database = new Emulator.Database.Database();
        PlaceholderServerData = new();
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        GeneralPatches.TipHelperInputPatches.Hook();
        HookManager.Instance.Create();

        if (CheckForUpdates)
        {
            var currentVersion = new AutoUpdater.Tag(PluginInfo.PLUGIN_VERSION);
            AutoUpdater.Update.CheckForUpdate(currentVersion);
        }

        if (EnableManiaLoader)
        {
            ManiaBeatmapsLoader = new OsuManiaLoader.Loader();
            if (ManiaBeatmapsLoader.BeatmapPacks.Count > 0) Logger.LogInfo($"Loaded {ManiaBeatmapsLoader.BeatmapPacks.Count} osu!mania packs!");
        }

        if (File.Exists(Emulator.Tools.Path.Combine("INVAXION_Data", "Plugins", "discord_game_sdk.dll")))
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
            try { DiscordRichPresence.Data.UpdateActivity(); } catch (Exception) {}
        }

        DiscordRichPresence.Data.Poll();
    }

    private void OnApplicationQuit()
    {
        ShuttingDown = true;
        DiscordRichPresence.Data._activityManager.ClearActivity(_ => { });
        DiscordRichPresence.Data.Poll();

        if (Debug && _process != null)
        {
            _process.Kill();
            _process = null;
        }

        if (!Debug) return;

        var commands = new List<string>();
        var filter = new List<string>();

        foreach (var command in MustImplement)
        {
            var cmd = $"{command} (x{MustImplement.Count(command.Equals)})";
            if (!filter.Contains(cmd))
            {
                commands.Add(cmd);
            }
        }

        var output = commands.Aggregate("", (current, line) => current + $"{line}\n");
        File.WriteAllText("must-implement.txt", output);
    }
}

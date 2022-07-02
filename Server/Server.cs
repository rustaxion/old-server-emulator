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
    public static List<string> MustImplement = new();
    public static bool Debug = false;
    private static Process _process;
    private static bool ShuttingDown = false;

    private void Awake()
    {
        _instance = this;
        logger = Logger;
        Database = new Emulator.Database.Database();
        PlaceholderServerData = new();
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        HookManager.Instance.Create();
        DiscordRichPresence.Data.Init();

        if (File.Exists("BepInEx/watch_logs.py") && Debug)
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

        DiscordRichPresence.GameEventHooks.Hook();
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

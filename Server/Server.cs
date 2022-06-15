﻿using Server.Emulator.EagleTcpPatches;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using BepInEx;

// ReSharper disable All

namespace Server;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Server : BaseUnityPlugin
{
    public static BepInEx.Logging.ManualLogSource logger;
    public static Emulator.Database.Database Database;
    public static List<string> MustImplement = new();
    public static bool Debug = true;

    private void Awake()
    {
        logger = Logger;
        Database = new Emulator.Database.Database();
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        
        HookManager.Instance.Create();
        DiscordRichPresence.Data.Init();
    }

    private static long timeDelta = TimeHelper.getCurUnixTimeOfSec();
    private void Update()
    {
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

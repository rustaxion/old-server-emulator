using Server.Emulator.EagleTcpPatches;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;

namespace Server;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Server : BaseUnityPlugin
{
    public static BepInEx.Logging.ManualLogSource logger;
    public static Emulator.Database.Database Database = new();
    public static List<string> MustImplement = new();

    private void Awake()
    {
        Server.logger = Logger;
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        HookManager.Instance.Create();
    }

    private void OnApplicationQuit()
    {
        var output = MustImplement.Aggregate("", (current, line) => current + $"{line}\n");
        File.WriteAllText("must-implement.txt", output);
    }
}

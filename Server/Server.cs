using BepInEx;
using Server.Emulator.EagleTcpPatches;

namespace Server
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Server : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource logger;
        private static bool EnableEmulator = true;
        public static DataBase Database;

        private void Awake()
        {
            Server.logger = Logger;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            
            if (EnableEmulator)
            {
                Database = new DataBase();
                Logger.LogInfo("Local server is enabled!");
                HookManager.Instance.Create();
            }
        }
    }
}
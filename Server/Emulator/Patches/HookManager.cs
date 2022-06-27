using HarmonyLib;

namespace Server.Emulator.Patches;

public class HookManager
{
    private Harmony harmony;
    public void Init()
    {
        harmony = new("Server.Emulator.Patches.HookManager");
        harmony.PatchAll(typeof(ExtensionMethodsPatch));
    }
}
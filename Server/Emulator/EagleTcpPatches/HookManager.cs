using HarmonyLib;
using System;
using System.Reflection;

namespace Server.Emulator.EagleTcpPatches;

public class HookManager
{
    public static HookManager Instance
    {
        get { return _instance ??= new HookManager(); }
    }
    
    private static HookManager _instance;
    
    public void Create()
    {
        try
        {
            Harmony harmonyInstance = new("server-emulator-hook");

            var constructorInfo = AccessTools.Constructor(typeof(EagleTcp), new Type[]
            {
                typeof(EagleTcp.CSocketType),
                typeof(string),
                typeof(uint)
            });
            var methodInfo3 = AccessTools.Method(typeof(EagleTcpHook), "DotCtorTranspiler");
            harmonyInstance.Patch(constructorInfo, transpiler: new HarmonyMethod(methodInfo3));

            var methodInfo4 = AccessTools.Method(typeof(EagleTcp), "IsConnected");
            var methodInfo5 = AccessTools.Method(typeof(EagleTcpHook), "IsConnectedTranspiler");
            harmonyInstance.Patch(methodInfo4, transpiler: new HarmonyMethod(methodInfo5));

            var methodInfo6 = AccessTools.Method(typeof(EagleTcp), "Disconnect");
            var methodInfo7 = AccessTools.Method(typeof(EagleTcpHook), "DisconnectTranspiler");
            harmonyInstance.Patch(methodInfo6, transpiler: new HarmonyMethod(methodInfo7));

            var methodInfo8 = AccessTools.Method(typeof(EagleTcp), "SendCmd");
            var methodInfo9 = AccessTools.Method(typeof(EagleTcpHook), "SendCmdTranspiler");
            harmonyInstance.Patch(methodInfo8, transpiler: new HarmonyMethod(methodInfo9));

            var methodInfo10 = AccessTools.Method(typeof(EagleTcp), "ParseCmd");
            var methodInfo11 = AccessTools.Method(typeof(EagleTcpHook), "ParseCmdTranspiler");
            harmonyInstance.Patch(methodInfo10, transpiler: new HarmonyMethod(methodInfo11));
            
            var backToLogin = AccessTools.Method(typeof(Aquatrax.NetManager), "backToLogin");
            var backToLoginPatch = AccessTools.Method(typeof(NetManagerPatch), "backToLoginPatch");
            harmonyInstance.Patch(backToLogin, prefix: new HarmonyMethod(backToLoginPatch));
            
            var hasForbidWord = AccessTools.Method(typeof(Aquatrax.GlobalConfig), "hasForbidWord");
            var hasForbidWordPatch = AccessTools.Method(typeof(GlobalConfigPatches), "hasForbidWord");
            harmonyInstance.Patch(hasForbidWord, prefix: new HarmonyMethod(hasForbidWordPatch));

            ServerLogger.LogInfo("Hooker: All OK!");
        }
        catch (Exception e)
        {
            ServerLogger.LogError(e.ToString());
        }
    }
}

public class NetManagerPatch
{
    public static bool backToLoginPatch()
    {
        return false; // skip the original method
    }
}

public class GlobalConfigPatches
{
    public static bool hasForbidWord(string str, ref bool __result)
    {
        // Allow people to use swear words in the username for fuck's sake.
        __result = false;
        return false; // skip the original method
    }
}
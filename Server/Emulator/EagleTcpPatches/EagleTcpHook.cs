using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Server.Emulator.EagleTcpPatches;

internal class EagleTcpHook
{
    private static readonly Type HookTargetType = typeof(EagleTcp);
    private static readonly Type HookType = typeof(EagleTcpClient);
    
    private static IEnumerable<CodeInstruction> GeneralTranspiler(IEnumerable<CodeInstruction> instructions, MethodInfo method, MethodInfo method2)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Call && instruction.operand == method)
                yield return new CodeInstruction(OpCodes.Call, method2);
            else
                yield return instruction;
        }
    }
    
    public static IEnumerable<CodeInstruction> DotCtorTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var method = AccessTools.Method(HookTargetType, "contectServer");
        var method2 = AccessTools.Method(HookType, "ContectServer");

        return GeneralTranspiler(instructions, method, method2);
    }

    public static IEnumerable<CodeInstruction> IsConnectedTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var method = AccessTools.Method(HookTargetType, "isConnected");
        var method2 = AccessTools.Method(HookType, "IsConnected");

        return GeneralTranspiler(instructions, method, method2);
    }

    public static IEnumerable<CodeInstruction> DisconnectTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var method = AccessTools.Method(HookTargetType, "disconnectServer");
        var method2 = AccessTools.Method(HookType, "DisconnectServer");

        return GeneralTranspiler(instructions, method, method2);
    }

    public static IEnumerable<CodeInstruction> SendCmdTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var method = AccessTools.Method(HookTargetType, "sendCmd");
        var method2 = AccessTools.Method(HookType, "SendCmd");

        return GeneralTranspiler(instructions, method, method2);
    }
    
    public static IEnumerable<CodeInstruction> ParseCmdTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var method = AccessTools.Method(HookTargetType, "parseCmd");
        var method2 = AccessTools.Method(HookType, "ParseCmd");

        return GeneralTranspiler(instructions, method, method2);
    }
}
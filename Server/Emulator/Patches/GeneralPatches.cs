using System.Collections.Generic;
using Aquatrax;
using HarmonyLib;

namespace Server.Emulator.Patches;


[HarmonyPatch]
public class GeneralPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ExtensionMethods), "getRewardInfo")]
    public static void getRewardInfo(uint id, uint type, int count, ref rewardInfo __result)
    {
        if (type != 3) return;
        if (!__result.rewardName.Contains("的物品不存在")) return;
        var charName = GlobalConfig.getInstance().getCharacterNameById(id);
        if (charName.Contains("的人物不存在")) return;

        __result.rewardName = $"Pilot: {charName}, EXP ";
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerCtrler), nameof(PlayerCtrler.Awake))]
    public static void patchIntroVideoStrings(ref List<string> ___videoStrings)
    {
        ___videoStrings.Remove("network_movie.mp4");
    }
}

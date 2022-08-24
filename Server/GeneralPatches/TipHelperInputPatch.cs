using Aquatrax;
using HarmonyLib;
using UnityEngine;

namespace Server.GeneralPatches;


public static class TipHelperInputPatches
{
    private static readonly Harmony _harmony = new Harmony("tiphelper-patches");


    public static bool PatchedUpdate(ref TipHelper __instance)
    {
        // The original function used Input.GetKeyDown() for some reason, so I changed it to Input.GetKeyUp() instead.

        if (InputManager.currentRef.getCurentGroup() == "TipHelper")
        {
            if (Input.GetKeyUp(KeyCode.Escape) || InputDevice.isButtonChecked_Cancel)
            {
                __instance.ButtonCancel();
            }
            else if (Input.GetKeyUp(KeyCode.Return) || InputDevice.isButtonChecked_Submit)
            {
                __instance.ButtonCommit();
            }
        }
        return false;
    }

    public static void Hook()
    {
        var TipHelper__Update = AccessTools.Method(typeof(TipHelper), "Update");
        var TipHelperInputPatches__PatchedUpdate = AccessTools.Method(typeof(TipHelperInputPatches), "PatchedUpdate");
        _harmony.Patch(TipHelper__Update, prefix: new HarmonyMethod(TipHelperInputPatches__PatchedUpdate));
    }
}

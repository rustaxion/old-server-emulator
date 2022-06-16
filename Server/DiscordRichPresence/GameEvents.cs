using HarmonyLib;

namespace Server.DiscordRichPresence;


public delegate void SwitchScene();

public static class GameEvents
{
    public static event SwitchScene switchScene;

    private static void PC_newQuickPlayView()
    {
        GameState.CurrentScene = "PC_newQuickPlayView";
        switchScene?.Invoke();
    }

    private static void PC_newGradeView()
    {
        GameState.CurrentScene = "PC_newGradeView";
        switchScene?.Invoke();
    }

    private static void PC_newPlaneView()
    {
        GameState.CurrentScene = "PC_newPlaneView";
        switchScene?.Invoke();
    }

    private static void GameOverView()
    {
        GameState.CurrentScene = "GameOverView";
        switchScene?.Invoke();
    }

    private static void PlayView()
    {
        GameState.CurrentScene = "PlayView";
        switchScene?.Invoke();
    }

    private static void SwitchScene(ref string ____assetPath)
    {
        GameState.CurrentScene = ____assetPath;
        switchScene?.Invoke();
    }
}

public static class GameEventHooks
{
    private static readonly Harmony _harmony = new Harmony("game-events");
    public static void Hook()
    {
        var PC_newQuickPlayView = AccessTools.Method(typeof(Aquatrax.PC_newQuickPlayView), "Start");
        var PC_newQuickPlayViewPatch = AccessTools.Method(typeof(GameEvents), "PC_newQuickPlayView");
        _harmony.Patch(PC_newQuickPlayView, prefix: new HarmonyMethod(PC_newQuickPlayViewPatch));

        var PC_newGradeView = AccessTools.Method(typeof(Aquatrax.PC_newGradeView), "Start");
        var PC_newGradeViewPatch = AccessTools.Method(typeof(GameEvents), "PC_newGradeView");
        _harmony.Patch(PC_newGradeView, prefix: new HarmonyMethod(PC_newGradeViewPatch));

        var PC_newPlaneView = AccessTools.Method(typeof(Aquatrax.PC_newPlaneView), "Open");
        var PC_newPlaneViewPatch = AccessTools.Method(typeof(GameEvents), "PC_newPlaneView");
        _harmony.Patch(PC_newPlaneView, prefix: new HarmonyMethod(PC_newPlaneViewPatch));

        var GameOverView = AccessTools.Method(typeof(Aquatrax.TalkingDataService), "onPlayFaild");
        var GameOverViewPatch = AccessTools.Method(typeof(GameEvents), "GameOverView");
        _harmony.Patch(GameOverView, prefix: new HarmonyMethod(GameOverViewPatch));

        var PlayView = AccessTools.Method(typeof(Aquatrax.PlayData), "Start");
        var PlayViewPatch = AccessTools.Method(typeof(GameEvents), "PlayView");
        _harmony.Patch(PlayView, prefix: new HarmonyMethod(PlayViewPatch));

        var switchScene = AccessTools.Method(typeof(Aquatrax.SwitchScene), "StartLoading");
        var switchScenePatch = AccessTools.Method(typeof(GameEvents), "SwitchScene");
        _harmony.Patch(switchScene, prefix: new HarmonyMethod(switchScenePatch));
    }
}

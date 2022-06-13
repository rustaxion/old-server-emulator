namespace Server.DiscordRichPresence;

public class GameState
{
    public static bool IsPaused => Aquatrax.ShortCut.PlayController.isPauseEnable && Aquatrax.ShortCut.PlayController.isPaused;
    public static Aquatrax.MusicInfoData CurrentSong => Aquatrax.GlobalConfig.getInstance().getMusicInfoById(Aquatrax.ShortCut.SelectData.MusicId);
}
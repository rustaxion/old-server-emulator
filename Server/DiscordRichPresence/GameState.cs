namespace Server.DiscordRichPresence;

public class GameState
{
    public static bool IsPaused
    {
        get
        {
            try
            {
                return Aquatrax.ShortCut.PlayController.isPauseEnable && Aquatrax.ShortCut.PlayController.isPaused;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }

    public static Aquatrax.MusicInfoData CurrentSong
    {
        get
        {
            try
            {
                return Aquatrax.GlobalConfig.getInstance().getMusicInfoById(Aquatrax.ShortCut.SelectData.MusicId);
            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }

    public static string CurrentScene;
}
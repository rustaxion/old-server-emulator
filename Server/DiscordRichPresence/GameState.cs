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
    public static Aquatrax.AQDiffLevel Difficulty
    {
        get
        {
            return Aquatrax.ShortCut.SelectData.Level;

        }
    }
    public static string keyCount
    {
        get
        {
            return Aquatrax.ShortCut.SelectData.keyCount switch
            {
                Aquatrax.eKeyMode.key4 => "4Key",
                Aquatrax.eKeyMode.key6 => "6Key",
                Aquatrax.eKeyMode.key8 => "8Key",
                _ => "Unknown Key"
            };
        }
    }

    public static string CurrentScene;
}
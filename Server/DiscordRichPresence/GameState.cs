using System.Collections.Generic;

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
        get { return Aquatrax.ShortCut.SelectData.Level; }
    }
    public static int DifficultyNumber
    {
        get
        {
            var level = Aquatrax.ShortCut.SelectData.Level.ToString().ToLower();
            var keys = Aquatrax.ShortCut.SelectData.keyCount.ToString();
            var difficulyKey = keys + "_" + level + "_diff";
            var difficulyDict = new Dictionary<string, int>();
            difficulyDict.Add("key4_standard_diff", CurrentSong.key4_easy_diff);
            difficulyDict.Add("key4_hard_diff", CurrentSong.key4_normal_diff);
            difficulyDict.Add("key4_trinity_diff", CurrentSong.key4_hard_diff);
            difficulyDict.Add("key6_standard_diff", CurrentSong.key6_easy_diff);
            difficulyDict.Add("key6_hard_diff", CurrentSong.key6_normal_diff);
            difficulyDict.Add("key6_trinity_diff", CurrentSong.key6_hard_diff);
            difficulyDict.Add("key8_standard_diff", CurrentSong.key8_easy_diff);
            difficulyDict.Add("key8_hard_diff", CurrentSong.key8_normal_diff);
            difficulyDict.Add("key8_trinity_diff", CurrentSong.key8_hard_diff);
            if (difficulyDict.ContainsKey(difficulyKey))
            {
                return difficulyDict[difficulyKey];
            }
            else
            {
                return 0;
            }
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

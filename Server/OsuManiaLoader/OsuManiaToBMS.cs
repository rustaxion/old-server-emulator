using System.Collections.Generic;

namespace Server.OsuManiaLoader;

public class OsuManiaToBMS
{
    private readonly static Dictionary<int, int> maniaNoteToChannel = new() {
        {0, 16},
        {1, 11},
        {2, 12},
        {3, 13},
        {4, 14},
        {5, 15},
        {6, 18},
        {7, 19}
    };

    private readonly static Dictionary<int, int> maniaLnToChannel = new() {
        {0, 56},
        {1, 51},
        {2, 52},
        {3, 53},
        {4, 54},
        {5, 55},
        {6, 58},
        {7, 59}
    };

    private static List<string> CreateHeader(OsuMania beatmap)
    {
        var header = new List<string> {
            "", "*---------------------- HEADER FIELD", "",
            "#PLAYER 1", $"#GENRE {beatmap.creator}",
            $"#TITLE {beatmap.titleUnicode}", $"#SUBTITLE {beatmap.version}",
            $"#ARTIST {beatmap.artistUnicode}", "#BPM {calcuateTheBPM}",
            "#DIFFICULTY 5", "#RANK 3", "", "*---------------------- EXPANSION FIELD",
            "", "*---------------------- MAIN DATA FIELD", "", ""
        };

        return header;
    }

    public static void Convert(OsuMania beatmap)
    {
        var buffer = CreateHeader(beatmap);
    }
}
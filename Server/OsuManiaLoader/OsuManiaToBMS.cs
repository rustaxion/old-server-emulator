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

    private static List<string> CreateHeader(Beatmap beatmap)
    {
        var header = new List<string> {
            "", "*---------------------- HEADER FIELD", "",
            "#PLAYER 1", $"#GENRE {beatmap.Metadata.Creator}",
            $"#TITLE {beatmap.Metadata.TitleUnicode}", $"#SUBTITLE {beatmap.version}",
            $"#ARTIST {beatmap.Metadata.ArtistUnicode}", "#BPM {calcuateTheBPM}",
            "#DIFFICULTY 5", "#RANK 3", "", "*---------------------- EXPANSION FIELD",
            "", "*---------------------- MAIN DATA FIELD", "", ""
        };

        return header;
    }

    public static void Convert(Beatmap beatmap)
    {
        var buffer = CreateHeader(beatmap);
    }
}
using Server.Emulator.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.OsuManiaLoader;


public class Difficulty
{
    public string HPDrainRate;
    public string CircleSize;
    public string OverallDifficulty;
    public string ApproachRate;
    public string SliderMultiplier;
    public string SliderTickRate;
}

public class Editor
{
    public string DistanceSpacing;
    public string BeatDivisor;
    public string GridSize;
    public string TimelineZoom;
    public string Bookmarks;
}

public class General
{
    public string AudioFilename;
    public string AudioLeadIn;
    public string PreviewTime;
    public string Countdown;
    public string SampleSet;
    public string StackLeniency;
    public string Mode;
    public string LetterboxInBreaks;
    public string SpecialStyle;
    public string WidescreenStoryboard;
}

public class Metadata
{
    public string Title;
    public string TitleUnicode;
    public string Artist;
    public string ArtistUnicode;
    public string Creator;
    public string Version;
    public string Source;
    public string Tags;
    public string BeatmapID;
    public string BeatmapSetID;
}

public class Beatmap
{
    public int version;
    public General General;
    public Editor Editor;
    public Metadata Metadata;
    public Difficulty Difficulty;
    public string[] Events;
    public string[] TimingPoints;
    public string[] HitObjects;
}


public static class Base36
{
    public static char[] alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToArray();
    public static string Encode(int number)
    {
        var output = new List<char>();

        if (0 <= number && number < alphabet.Length)
        {
            output.Insert(0, alphabet[number]);
        }
        else
        {
            while (number != 0)
            {
                number = Math.DivRem(number, alphabet.Length, out int i);
                output.Insert(0, alphabet[i]);
            }
        }

        return string.Join("", output.Select(ch => ch.ToString()).ToArray()).PadLeft(2, '0');
    }
}

public class BeatmapParser
{
    public static Beatmap Parse(string beatmapContent)
    {
        var lines = beatmapContent.Trim().Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith("//"))
            .ToList();

        var version = Convert.ToInt32(lines.Pop(0).Split('v').Last());
        var sections = new Dictionary<string, List<string>>();
        string curSection = null;

        foreach (var line in lines)
        {
            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                curSection = line.Substring(1, line.Length - 2);
                sections[curSection] = new List<string>();
            }
            else
            {
                sections[curSection!].Add(line.Trim());
            }
        }

        var parsedSections = new Dictionary<string, Dictionary<string, string>>();

        foreach (var section in sections.Keys)
        {
            if (new string[] { "HitObjects", "TimingPoints", "Events" }.Contains(section)) continue;

            parsedSections[section] = sections[section].ToDictionary(line => line.Split(':')[0].Trim(), line => line.Split(':')[1].Trim());
        }

        var beatmap = LitJson.JsonMapper.ToObject<Beatmap>(LitJson.JsonMapper.ToJson(parsedSections));
        beatmap.version = version;
        beatmap.TimingPoints = sections["TimingPoints"].ToArray();
        beatmap.HitObjects = sections["HitObjects"].ToArray();
        beatmap.Events = sections["Events"].ToArray();
        return beatmap;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.OsuManiaLoader;

internal class TmpNote
{
    public int Key;
    public int Action;
    public int Time;
    public int BarIndex;
    public int NodeIndex;
}

internal class InvaxionTrack
{
    public Dictionary<int, InvaxionNode> Nodes;
}

internal class InvaxionNode
{
    public int Action;
}

internal class InvaxionBar
{
    public Dictionary<int, InvaxionTrack> Tracks;
}


public class Difficulty
{
    public string KeyMode;
    public string Diff;
}


public class OsuMania
{
    public string AudioFilename;
    public int AudioLeadIn;
    public bool SpecialStyle;
    public string SampleSet;
    public int PreviewTime;
    public string BMS;
    public string INVAXION;

    public Difficulty Difficulty;

    // Metadata
    public string Title;
    public string TitleUnicode;
    public string Artist;
    public string ArtistUnicode;
    public string Creator;
    public string Version;
    public string Source;
    public int BeatmapId;
    public int BeatmapSetId;
    public string StageBg;
    public int BeatDivisor;

    public int KeyCount;
    public int OverallDifficulty;

    public List<(string, float)> FloatBpms = new();
    public List<OsuTimingPoint> TimingPoints = new();
    public List<OsuHitObject> HitObjects = new();
    public List<OsuObj> Objects;
    public List<string> SampleFilenames = new();
    public Dictionary<string, string> FilenameToSample = new();
    public List<OsuTimingPoint> NoneInheritedTp = new();

    public void ParseFloatBpm(float bpm)
    {
        if (FloatBpms.Any(e => Math.Abs(e.Item2 - bpm) < 0.1)) return;
        FloatBpms.Add((Stuff.GetCurrentHsCount(FloatBpms.Count + 1), bpm));
    }
}

public abstract class OsuObj
{
    public int Time;
    public int SortType;
}

public class OsuTimingPoint : OsuObj
{
    public float MsPerBeat;
    public int Meter;
    public int SampleSet;
    public int SampleIndex;
    public int Volume;
    public bool Inherited;
    public bool KiaiMode;
    public long MsPerMeasure;

    public OsuTimingPoint()
    {
        SortType = 0;
    }

    public override string ToString()
    {
        return $"{Time}ms (at {MsPerBeat}ms per beat)";
    }

    public static bool operator ==(OsuTimingPoint left, OsuTimingPoint right)
    {
        if (left == null) return false; if (right == null) return false;
        if (!left.MsPerBeat.Equals(right.MsPerBeat)) return false;
        if (!left.Meter.Equals(right.Meter)) return false;
        return left.SampleSet.Equals(right.SampleSet) && left.SampleIndex.Equals(right.SampleIndex);
    }

    public static bool operator !=(OsuTimingPoint left, OsuTimingPoint right)
    {
        return !(left == right);
    }

    public override bool Equals(object right)
    {
        if (right == null) return false;
        if (right.GetType() != typeof(OsuTimingPoint)) return false;
        return this == (OsuTimingPoint)right;
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public abstract class OsuHitObject : OsuObj
{
    public long TimeValue;
    public bool NewCombo;
    public int x;
    public OsuTimingPoint TimingPoint;

    public OsuHitObject()
    {
        SortType = 1;
    }

    internal string Name = "HitObject";

    public override string ToString()
    {
        return $"{Name}: t={Time} | c={x}";
    }

    public abstract int GetTypeValue();
}

public class OsuManiaNote : OsuHitObject
{
    public OsuManiaNote()
    {
        Name = "ManiaNote";
    }

    public override int GetTypeValue()
    {
        return NewCombo ? 5 : 1;
    }
}

public class OsuManiaLongNote : OsuHitObject
{
    public int EndTime;

    public OsuManiaLongNote(int endTime = -1)
    {
        Name = "ManiaNote (long)";
        if (endTime > -1)
            EndTime = endTime;
    }

    public override int GetTypeValue()
    {
        return NewCombo ? 132 : 128;
    }
}

public static class Base36
{
    private static readonly char[] Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToArray();
    public static string Encode(int number)
    {
        var output = new List<char>();

        if (0 <= number && number < Alphabet.Length) output.Insert(0, Alphabet[number]);
        else while (number != 0)
        {
            number = Math.DivRem(number, Alphabet.Length, out var i);
            output.Insert(0, Alphabet[i]);
        }

        return string.Join("", output.Select(ch => ch.ToString()).ToArray()).PadLeft(2, '0');
    }
}

internal static class Stuff
{
    public static string GetCurrentHsCount(int sampleNum)
    {
        var ret = Base36.Encode(sampleNum);
        return ret.Length > 2 || ret == "ZZ" ? "" : ret;
    }

    public static float CalculateBpm(OsuTimingPoint timingPoint)
    {
        int GetNthDecimal(float number, int n)
        {
            return (int)(number * Math.Pow(10, n)) % 10;
        }

        var bpm = (float)(1.0 / (timingPoint.MsPerBeat / 1000.0 / 60.0));
        int count = 0, nCount = 0;

        for (var i = 0; i < 5; i++)
        {
            switch (GetNthDecimal(bpm, i))
            {
                case 0:
                    {
                        count++;
                        break;
                    }
                case 9:
                    {
                        nCount++;
                        break;
                    }
            }
        }
        if (count == 4) return Convert.ToInt32(bpm);
        if (nCount == 4) return Convert.ToInt32(Math.Round(bpm));

        return (float)(Convert.ToInt32(bpm * Math.Pow(10, 4)) / 10000.0);
    }
}
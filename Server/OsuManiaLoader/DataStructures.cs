using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.OsuManiaLoader;

public class OsuMania
{
    public string audioFilename;
    public int audioLeadIn;
    public bool specialStyle;
    public string sampleSet;
    public int previewTime;

    // Metadata
    public string title;
    public string titleUnicode;
    public string artist;
    public string artistUnicode;
    public string creator;
    public string version;
    public string source;
    public string beatmapId;
    public string stageBg;

    public int keyCount;
    public float od; // OverallDifficulty

    public List<(string, float)> floatBpm = new();
    public List<OsuTimingPoint> timingPoints = new();
    public List<OsuHitObject> hitObjects = new();
    public List<OsuObj> objects;
    public List<string> sampleFilenames = new();
    public Dictionary<string, string> filenameToSample = new();
    public List<OsuTimingPoint> noneInheritedTP = new();

    public void ParseFloatBPM(float bpm)
    {
        foreach (var e in floatBpm)
        {
            if (e.Item2 == bpm)
            {
                return;
            }
        }
        floatBpm.Add((Stuff.GetCurrentHsCount(floatBpm.Count + 1), bpm));
    }
}

public abstract class OsuObj
{
    public long time;
    public int sortType;
}

public class OsuTimingPoint : OsuObj
{
    public float msPerBeat;
    public int meter;
    public int sampleSet;
    public int sampleIndex;
    public int volume;
    public bool inherited;
    public bool kiaiMode;

    public long msPerMeasure;

    public OsuTimingPoint()
    {
        this.sortType = 0;
    }

    public override string ToString()
    {
        return $"{time}ms (at {msPerBeat}ms per beat)";
    }

    public static bool operator ==(OsuTimingPoint left, OsuTimingPoint right)
    {
        if (!left.msPerBeat.Equals(right.msPerBeat))
        {
            return false;
        }
        if (!left.meter.Equals(right.meter))
        {
            return false;
        }
        if (!left.sampleSet.Equals(right.sampleSet))
        {
            return false;
        }
        if (!left.sampleIndex.Equals(right.sampleIndex))
        {
            return false;
        }
        return true;
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
    public long timeValue;
    public bool newCombo;
    public int maniaColumn;
    public OsuTimingPoint timingPoint;

    public OsuHitObject()
    {
        this.sortType = 1;
    }

    internal string name = "HitObject";

    public override string ToString()
    {
        return $"{name}: t={time} | c={maniaColumn}";
    }

    public abstract int GetTypeValue();
}

public class OsuManiaNote : OsuHitObject
{
    public OsuManiaNote()
    {
        this.name = "ManiaNote";
    }

    public override int GetTypeValue()
    {
        return this.newCombo ? 5 : 1;
    }
}

public class OsuManiaLongNote : OsuHitObject
{
    public int endTime;

    public OsuManiaLongNote(int endTime = -1)
    {
        this.name = "ManiaNote (long)";
        if (endTime > -1)
            this.endTime = endTime;
    }

    public override int GetTypeValue()
    {
        return this.newCombo ? 132 : 128;
    }
}

public class BMSMeasure
{
    public string measureNumber;
    public List<BMSMainDataLine> lines = new();

    public override string ToString()
    {
        var ret = "";

        foreach (var line in lines)
        {
            ret += $"#{measureNumber}{line.channel}:{line.data}\n";
        }

        return ret;
    }

    public void CreateDataLine(string channel, int bits, List<(int, object)> locations)
    {
        var chars = new Dictionary<int, string>();
        var locations_ = locations.Select(locations => locations.Item1).ToArray();

        foreach (var loc in locations)
        {
            if (typeof(OsuHitObject).IsInstanceOfType((loc.Item2)))
            {
                chars[loc.Item1] = "ZZ";
            }
            else
            {
                chars[loc.Item1] = loc.Item2 as string;
            }
        }

        lines.Add(new(channel, bits, chars, locations_, measureNumber));
    }

    public void CreateMeasureLengthChange(int numOfBeats)
    {
        lines.Add(new("02", 1, new() { { 0, numOfBeats.ToString() } }, new int[] { 0 }, measureNumber));
    }

    public void CreateBpmChangeLine(int bpm)
    {
        lines.Add(new("03", 1, new() { { 0, bpm.ToString("X4").ToUpper() } }, new int[] { 0 }, measureNumber));
    }

    public void CreateBpmExtendedChangeLine(float BPM, (string, float)[] BPMs)
    {
        (string, float) tuple = (null, 0);
        foreach (var bpm in BPMs)
        {
            if (bpm.Item2 == BPM)
            {
                tuple = bpm;
            }
        }
        lines.Add(new("08", 1, new() { { 0, tuple.Item1.ToString() } }, new int[] { 0 }, measureNumber));
    }
}

public class BMSMainDataLine
{
    public BMSMainDataLine(string channel, int bits, Dictionary<int, string> characters, int[] locations, string measureNumber)
    {
        this.channel = channel;
        this.data = BuildData(bits, characters, locations);
        this.measureNumber = measureNumber;
    }

    public string channel;
    public string data;
    public string measureNumber;

    private string BuildData(int bits, Dictionary<int, string> characters, int[] locations)
    {
        var output = "";
        int index = 0;

        for (var i = 0; i < bits; i++)
        {
            if (i == locations[index])
            {
                output += characters[i];
                if (index <= locations.Length)
                {
                    index++;
                }
            }
            else
            {
                output += "00";
            }
        }

        return output;
    }

    public override string ToString()
    {
        return $"#{measureNumber}{channel}:{data}\n";
    }
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

internal static class Stuff
{
    public static string GetCurrentHsCount(int sampleNum)
    {
        var ret = Base36.Encode(sampleNum);
        return ret.Length > 2 ? "" : ret;
    }

    public static float CalculateBPM(OsuTimingPoint timingPoint)
    {
        int GetNthDecimal(float number, int n)
        {
            return System.Convert.ToInt32(number * System.Math.Pow(10, n)) % 10;
        }

        float bpm = 1 / ((timingPoint.msPerBeat / 1000) / 60);
        int count = 0, ncount = 0;

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
                        ncount++;
                        break;
                    }
            }
        }
        if (count == 4)
        {
            return System.Convert.ToInt32(bpm);
        }
        else if (ncount == 4)
        {
            return System.Convert.ToInt32(System.Math.Round(bpm));
        }
        else
        {
            return (float)(System.Convert.ToInt32(bpm * System.Math.Pow(10, 4)) / 1000.0);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.OsuManiaLoader;

public class OsuMania
{
    public string AudioFilename;
    public int AudioLeadIn;
    public bool SpecialStyle;
    public string SampleSet;
    public int PreviewTime;

    // Metadata
    public string Title;
    public string TitleUnicode;
    public string Artist;
    public string ArtistUnicode;
    public string Creator;
    public string Version;
    public string Source;
    public string BeatmapId;
    public string StageBg;

    public int KeyCount;
    public float OverallDifficulty;

    public List<(string, float)> FloatBpms = new();
    public List<OsuTimingPoint> TimingPoints = new();
    public List<OsuHitObject> HitObjects = new();
    public List<OsuObj> Objects;
    public List<string> SampleFilenames = new();
    public Dictionary<string, string> FilenameToSample = new();
    public List<OsuTimingPoint> NoneInheritedTp = new();

    public void ParseFloatBPM(float bpm)
    {
        foreach (var e in FloatBpms)
        {
            if (Math.Abs(e.Item2 - bpm) < 0.1)
            {
                return;
            }
        }
        FloatBpms.Add((Stuff.GetCurrentHsCount(FloatBpms.Count + 1), bpm));
    }
}

public abstract class OsuObj
{
    public long Time;
    public int SortType;
}

public class OsuTimingPoint : OsuObj
{
    public int MsPerBeat;
    public int Meter;
    public int SampleSet;
    public int SampleIndex;
    public int Volume;
    public bool Inherited;
    public bool KiaiMode;

    public long MsPerMeasure;

    public OsuTimingPoint()
    {
        this.SortType = 0;
    }

    public override string ToString()
    {
        return $"{Time}ms (at {MsPerBeat}ms per beat)";
    }

    public static bool operator ==(OsuTimingPoint left, OsuTimingPoint right)
    {
        if (left == null) return false;
        if (right == null) return false;
        if (!left.MsPerBeat.Equals(right.MsPerBeat))
        {
            return false;
        }
        if (!left.Meter.Equals(right.Meter))
        {
            return false;
        }
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
    public int ManiaColumn;
    public OsuTimingPoint TimingPoint;

    public OsuHitObject()
    {
        this.SortType = 1;
    }

    internal string Name = "HitObject";

    public override string ToString()
    {
        return $"{Name}: t={Time} | c={ManiaColumn}";
    }

    public abstract int GetTypeValue();
}

public class OsuManiaNote : OsuHitObject
{
    public OsuManiaNote()
    {
        this.Name = "ManiaNote";
    }

    public override int GetTypeValue()
    {
        return this.NewCombo ? 5 : 1;
    }
}

public class OsuManiaLongNote : OsuHitObject
{
    public int EndTime;

    public OsuManiaLongNote(int endTime = -1)
    {
        this.Name = "ManiaNote (long)";
        if (endTime > -1)
            this.EndTime = endTime;
    }

    public override int GetTypeValue()
    {
        return this.NewCombo ? 132 : 128;
    }
}

public class Fraction
{
    // https://gist.github.com/fidelsoto/b4c0f14b800c58e137ad5757f35cacd6
    public float Numerator;
    public float Denominator;

    public Fraction(float numerator, float denominator)
    {
        this.Numerator = numerator;
        this.Denominator = denominator;
    }

    public Fraction(Fraction fraction)
    {
        Numerator = fraction.Numerator;
        Denominator = fraction.Denominator;
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;

        var other = (Fraction)obj;
        return Math.Abs(Numerator - other.Numerator) < 0.1 && Math.Abs(Denominator - other.Denominator) < 0.1;
    }

    public static bool operator ==(Fraction f1, Fraction f2)
    {
        return f1.Equals(f2);
    }

    public static bool operator ==(Fraction f1, int f2)
    {
        return f1.Equals(new Fraction(f2, 1));
    }

    public static bool operator !=(Fraction f1, Fraction f2)
    {
        return !(f1 == f2);
    }

    public static bool operator !=(Fraction f1, int f2)
    {
        return !(f1 == f2);
    }

    public static Fraction operator *(Fraction f1, float f2)
    {
        var f3 = new Fraction(f1);
        f3.Numerator *= f2;
        return f3;
    }

    public static Fraction operator /(Fraction f1, float f2)
    {
        var f3 = new Fraction(f1);
        f3.Numerator /= f2;
        f3.Denominator /= f2;
        return f3;
    }

    public static Fraction operator +(Fraction f1, float f2)
    {
        var f3 = new Fraction(f1.Numerator, f1.Denominator);
        f3.Numerator += f2 * f3.Denominator;
        return f3;
    }

    public static Fraction operator -(Fraction f1, float f2)
    {
        var f3 = new Fraction(f1.Numerator, f1.Denominator);
        f3.Numerator -= f2 * f3.Denominator;
        return f3;
    }

    public static float operator +(float f1, Fraction f2)
    {
        return f1 + f2.ToFloat();
    }

    public static float operator -(float f1, Fraction f2)
    {
        return f1 - f2.ToFloat();
    }

    public static float operator +(int f1, Fraction f2)
    {
        return f1 + f2.ToFloat();
    }

    public static float operator -(int f1, Fraction f2)
    {
        return f1 - f2.ToFloat();
    }

    public static Fraction operator +(Fraction f1, Fraction f2)
    {
        var a = f1.Denominator > f2.Denominator ? f1 : f2;
        var b = f1.Denominator < f2.Denominator ? f1 : f2;
        var difference = a.Denominator / b.Denominator;

        return new Fraction(a.Numerator + (b.Numerator * difference), a.Denominator);
    }

    public static Fraction operator -(Fraction f1, Fraction f2)
    {
        var a = f1.Denominator > f2.Denominator ? f1 : f2;
        var b = f1.Denominator < f2.Denominator ? f1 : f2;
        var difference = a.Denominator / b.Denominator;

        return new Fraction(a.Numerator - (b.Numerator * difference), a.Denominator);
    }

    public static bool operator >(Fraction f1, Fraction f2)
    {
        return (f1.Numerator / f1.Denominator) > (f2.Numerator / f2.Denominator);
    }

    public static bool operator <(Fraction f1, Fraction f2)
    {
        return (f1.Numerator / f1.Denominator) < (f2.Numerator / f2.Denominator);
    }

    public static bool operator <=(Fraction f1, Fraction f2)
    {
        return (f1.Numerator / f1.Denominator) <= (f2.Numerator / f2.Denominator);
    }

    public static bool operator >=(Fraction f1, Fraction f2)
    {
        return (f1.Numerator / f1.Denominator) >= (f2.Numerator / f2.Denominator);
    }

    public static bool operator >(Fraction f1, float f2)
    {
        return (f1.Numerator / f1.Denominator) > f2;
    }

    public static bool operator <(Fraction f1, float f2)
    {
        return (f1.Numerator / f1.Denominator) < f2;
    }

    public static bool operator <=(Fraction f1, float f2)
    {
        return (f1.Numerator / f1.Denominator) <= f2;
    }

    public static bool operator >=(Fraction f1, float f2)
    {
        return (f1.Numerator / f1.Denominator) >= f2;
    }

    public static bool operator >(Fraction f1, int f2)
    {
        return (f1.Numerator / f1.Denominator) > f2;
    }

    public static bool operator <(Fraction f1, int f2)
    {
        return (f1.Numerator / f1.Denominator) < f2;
    }

    public static bool operator <=(Fraction f1, int f2)
    {
        return (f1.Numerator / f1.Denominator) <= f2;
    }

    public static bool operator >=(Fraction f1, int f2)
    {
        return (f1.Numerator / f1.Denominator) >= f2;
    }

    public override int GetHashCode()
    {
        // i dunno, some weird stuff, its not like we care
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return Numerator + "/" + Denominator;
    }

    public float ToFloat()
    {
        return Numerator / Denominator;
    }

    //Helper function, simplifies a fraction.
    public Fraction Simplify()
    {
        for (var divideBy = Denominator; divideBy > 0; divideBy--)
        {
            var divisible = (Math.Abs((int)(Numerator / divideBy) * divideBy - Numerator) < 0.1) &&
                (Math.Abs((int)(Denominator / divideBy) * divideBy - Denominator) < 0.1);

            if (!divisible) continue;

            Numerator /= divideBy;
            Denominator /= divideBy;
        }
        return this;
    }
}

public class BmsMeasure
{
    public string MeasureNumber;
    public List<BMSMainDataLine> Lines = new();

    public override string ToString()
    {
        return Lines.Aggregate("", (current, line) => current + $"#{MeasureNumber}{line.Channel}:{line.Data}\n");
    }

    public void CreateDataLine(string channel, int bits, List<(int, object)> locations)
    {
        var chars = new Dictionary<int, string>();
        var locations_ = locations.Select(loc => loc.Item1).ToArray();

        foreach (var loc in locations)
        {
            if ((loc.Item2) is OsuHitObject)
            {
                chars[loc.Item1] = "ZZ";
            }
            else
            {
                chars[loc.Item1] = loc.Item2 as string;
            }
        }

        Lines.Add(new(channel, bits, chars, locations_, MeasureNumber));
    }

    public void CreateMeasureLengthChange(float numOfBeats)
    {
        Lines.Add(new("02", 1, new() { { 0, numOfBeats.ToString() } }, new int[] { 0 }, MeasureNumber));
    }

    public void CreateBpmChangeLine(int bpm)
    {
        Lines.Add(new("03", 1, new() { { 0, bpm.ToString("X4").ToUpper() } }, new int[] { 0 }, MeasureNumber));
    }

    public void CreateBpmExtendedChangeLine(float BPM, (string, float)[] BPMs)
    {
        (string, float) tuple = (null, 0);
        foreach (var bpm in BPMs)
        {
            if (Math.Abs(bpm.Item2 - BPM) < 0.1)
            {
                tuple = bpm;
            }
        }
        Lines.Add(new("08", 1, new() { { 0, tuple.Item1.ToString() } }, new int[] { 0 }, MeasureNumber));
    }
}

public class BMSMainDataLine
{
    public BMSMainDataLine(string channel, int bits, Dictionary<int, string> characters, int[] locations, string measureNumber)
    {
        this.Channel = channel;
        this.Data = BuildData(bits, characters, locations);
        this.MeasureNumber = measureNumber;
    }

    public string Channel;
    public string Data;
    public string MeasureNumber;

    private static string BuildData(int bits, Dictionary<int, string> characters, int[] locations)
    {
        var output = "";
        var index = 0;

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
        return $"#{MeasureNumber}{Channel}:{Data}\n";
    }
}

public static class Base36
{
    public static char[] Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToArray();
    public static string Encode(int number)
    {
        var output = new List<char>();

        if (0 <= number && number < Alphabet.Length)
        {
            output.Insert(0, Alphabet[number]);
        }
        else
        {
            while (number != 0)
            {
                number = Math.DivRem(number, Alphabet.Length, out var i);
                output.Insert(0, Alphabet[i]);
            }
        }

        return string.Join("", output.Select(ch => ch.ToString()).ToArray()).PadLeft(2, '0');
    }
}

internal static class Stuff
{
    public static float GreatestCommonDenominator(float a, float b)
    {
        while (a != 0 && b != 0)
        {
            if (a > b)
                a %= b;
            else
                b %= a;
        }

        return a == 0 ? b : a;
    }

    public static string GetCurrentHsCount(int sampleNum)
    {
        var ret = Base36.Encode(sampleNum);
        return ret.Length > 2 ? "" : ret;
    }

    public static float CalculateBpm(OsuTimingPoint timingPoint)
    {
        int GetNthDecimal(float number, int n)
        {
            return Convert.ToInt32(number * Math.Pow(10, n)) % 10;
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

        return (float)(Convert.ToInt32(bpm * Math.Pow(10, 4)) / 1000.0);
    }
}
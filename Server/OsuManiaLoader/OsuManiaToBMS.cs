using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.OsuManiaLoader;

public class OsuManiaToBms
{
    private static readonly Dictionary<int, int> ManiaNoteToChannel = new() {
        {0, 16}, {1, 11}, {2, 12}, {3, 13}, {4, 14}, {5, 15}, {6, 18}, {7, 19}
    };

    private static readonly Dictionary<int, int> ManiaLnToChannel = new() {
        {0, 56}, {1, 51}, {2, 52}, {3, 53}, {4, 54}, {5, 55}, {6, 58}, {7, 59}
    };

    private static readonly Dictionary<float, Fraction> MsToInverseNoteValues = new();

    private static void InitializeMtnv()
    {
        MsToInverseNoteValues.Clear();
    }

    private static void AddToMtnv(float key, Fraction value)
    {
        MsToInverseNoteValues[key] = value;
        MsToInverseNoteValues[key - 1] = value;
        MsToInverseNoteValues[key + 1] = value;
    }

    public static void Clear()
    {
        MsToInverseNoteValues.Clear();
    }

    private static List<string> CreateHeader(OsuMania beatmap)
    {
        return new List<string> {
            "", "*---------------------- HEADER FIELD", "",
            "#PLAYER 1", $"#GENRE {beatmap.Creator}",
            $"#TITLE {beatmap.Title}", $"#SUBTITLE {beatmap.Version}",
            $"#ARTIST {beatmap.Artist}", $"#BPM {(int)Stuff.CalculateBpm(beatmap.TimingPoints[0])}",
            $"#DIFFICULTY {beatmap.OverallDifficulty}", "#RANK 3", "", "*---------------------- EXPANSION FIELD",
            "", "*---------------------- MAIN DATA FIELD", "", ""
        };
    }

    public static string Convert(OsuMania beatmap)
    {
        Clear();
        var buffer = CreateHeader(beatmap);
        var (measureOffset, startTime, _buffer) = GetMusicStartTime(beatmap);
        buffer.AddRange(_buffer);
        buffer.Add("\n");
        buffer.AddRange(GetNextMeasure(beatmap, measureOffset, (int)startTime));

        return string.Join("\n", buffer.ToArray());
    }

    private static Fraction WrapExpansion(float n, float msPerMeasure)
    {
        (float, Fraction) Expander(float n, float msPerMeasure, float meter, float end)
        {
            bool IsWithinOffset(float num, Fraction sum, float offset)
            {
                return (sum + offset) * msPerMeasure > System.Convert.ToInt32(msPerMeasure * num) - 1 &&
                    (sum + offset) * msPerMeasure < System.Convert.ToInt32(msPerMeasure * num) + 2;
            }
            var done = false;
            var denominator = meter;
            var sum = new Fraction(1, meter);

            while ((sum + new Fraction(1, denominator)).ToFloat() < n)
            {
                sum += new Fraction(1, denominator);
            }

            for (var i = 0; i < 6; i++)
            {
                if (IsWithinOffset(n, sum, 0) || Math.Abs(Math.Round(n, 5) - Math.Round(sum.ToFloat(), 5)) < 0.1)
                {
                    done = true;
                    break;
                }
                if (sum > n)
                {
                    sum -= 1 / denominator;
                    denominator *= 2;
                }
                else if (sum < n)
                {
                    sum += 1 / denominator;
                    denominator *= 2;
                }
            }

            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            while (!done)
            {
                if (IsWithinOffset(n, sum, 0))
                    break;
                var prevErr = Math.Abs(n - sum);
                if (sum > n)
                {
                    if (IsWithinOffset(n, sum, -1 / end))
                    {
                        sum -= 1 / end;
                        break;
                    }
                    else if (sum - 1 / end < Math.Abs(n - prevErr - 1 / end))
                        break;
                    sum -= 1 / end;
                }
                else if (sum < n)
                {
                    if (IsWithinOffset(n, sum, 1 / end))
                    {
                        sum += 1 / end;
                        break;
                    }
                    else if (sum - 1 / end > Math.Abs(n - (prevErr + 1 / end)))
                        break;
                    sum += 1 / end;
                }
            }

            return (Math.Abs(n - sum), sum);
        }

        var err2 = Expander(n, msPerMeasure, 4, 128);
        var err3 = Expander(n, msPerMeasure, 3, 192);
        var err = err2.Item1 < err3.Item1 ? err2 : err3;
        var timeValue = err.Item2;

        if (timeValue == 1) return new Fraction(0, 1);
        if (timeValue != 0) AddToMtnv(timeValue.ToFloat() * msPerMeasure, timeValue);

        return err.Item2;
    }

    private static (int, float, List<string>) GetMusicStartTime(OsuMania beatmap)
    {
        var firstObj = beatmap.Objects[0];
        var firstTiming = beatmap.TimingPoints[0];
        var msPerMeasure = firstTiming.Meter * firstTiming.MsPerBeat;
        var buffer = new List<string>();

        var useObj = false;
        float startTime = default;

        if (firstObj.Time < firstTiming.Time)
        {
            startTime = firstObj.Time;
            useObj = true;
        }
        else
        {
            startTime = firstTiming.Time;
        }

        if (useObj)
        {
            var i = 0;
            while (beatmap.Objects[i].Time < firstTiming.Time) i++;
            startTime = beatmap.Objects[i].Time;
        }

        while (startTime - msPerMeasure > 0)
        {
            startTime -= msPerMeasure;
        }

        var startTimeOffset = startTime > 0 ? msPerMeasure - startTime : System.Math.Abs(startTime);
        var musicStartsAt001 = (firstObj.Time + msPerMeasure) < msPerMeasure;
        var stoFraction = startTimeOffset / msPerMeasure;
        var timeValueRatio = WrapExpansion(stoFraction, msPerMeasure);
        BmsMeasure bmsMeasure = default;
        int measureStart;

        if (timeValueRatio == 0 && musicStartsAt001)
        {
            bmsMeasure = new() { MeasureNumber = "001" };
            measureStart = 1;
            bmsMeasure.CreateDataLine("01", (int)timeValueRatio.Denominator, new() { ((int)timeValueRatio.Numerator, "01") });
        }
        else
        {
            if (musicStartsAt001)
            {
                bmsMeasure = new() { MeasureNumber = "001" };
                measureStart = 1;
            }
            else
            {
                bmsMeasure = new() { MeasureNumber = "002" };
                if (timeValueRatio == 0)
                {
                    measureStart = 0;
                }
                else
                {
                    measureStart = 1;
                }
                bmsMeasure.CreateDataLine("01", (int)timeValueRatio.Denominator, new() { ((int)timeValueRatio.Numerator, "01") });
            }
        }
        var measureOffset = measureStart;

        if (beatmap.Objects[0].Time > startTime)
        {
            while (!(startTime >= beatmap.Objects[0].Time))
            {
                startTime += msPerMeasure;
                measureOffset += 1;
            }
        }

        if (firstTiming.Meter != 4)
        {
            if (bmsMeasure.MeasureNumber != "000")
            {
                var bmsMeasure0 = new BmsMeasure { MeasureNumber = "000" };
                bmsMeasure0.CreateMeasureLengthChange(firstTiming.Meter / 4);
                buffer.AddRange(bmsMeasure0.Lines.Select(line => line.ToString()));
            }
            bmsMeasure.CreateMeasureLengthChange(firstTiming.Meter / 4);
        }

        buffer.AddRange(bmsMeasure.Lines.Select(line => line.ToString()));

        if (firstTiming.Meter != 4)
        {
            for (var i = 1; i < measureOffset; i++)
            {
                bmsMeasure = new BmsMeasure { MeasureNumber = i.ToString("D3") };
                bmsMeasure.CreateMeasureLengthChange(firstTiming.Meter / 4);
                buffer.AddRange(bmsMeasure.Lines.Select(line => line.ToString()));
            }
        }

        var firstMeasureTime = startTime;
        if (firstObj.Time < firstMeasureTime - 1)
        {
            measureOffset -= 1;
            firstMeasureTime -= msPerMeasure;
        }

        if (timeValueRatio != 0 || musicStartsAt001) return (measureOffset, firstMeasureTime, buffer);

        while (!IsWithin2Ms(startTime, measureOffset * msPerMeasure))
        {
            measureOffset++;
        }

        return (measureOffset, firstMeasureTime, buffer);
    }

    private static bool IsWithin2Ms(float base_, float n)
    {
        Console.WriteLine($"isWithin2Ms:: {Math.Round(base_, 2)}, {n}, ({Math.Round(base_, 2)-2} <= {n}) && ({Math.Round(base_, 2)+2} >= {n})");
        return base_ - 2 <= n && n <= base_ + 2;
    }

    private static List<string> GetNextMeasure(OsuMania beatmap, int startingMeasure, int startingMs)
    {
        void AddToMeasure(Dictionary<int, List<OsuObj>> currentMeasure, OsuObj hitObj)
        {
            if (hitObj is OsuTimingPoint)
            {
                if (currentMeasure.ContainsKey(0))
                {
                    currentMeasure[0].Add(hitObj);
                }
                else
                {
                    currentMeasure[0] = new() { hitObj };
                }
            }
            else
            {
                var column = ((OsuHitObject)hitObj).ManiaColumn;
                var bmsColumn = (hitObj is OsuManiaNote ? ManiaNoteToChannel : ManiaLnToChannel)[column];
                if (currentMeasure.ContainsKey(bmsColumn))
                {
                    currentMeasure[bmsColumn].Add(hitObj);
                }
                else
                {
                    currentMeasure[bmsColumn] = new() { hitObj };
                }
            }
        }

        var currentMeasure = new Dictionary<int, List<OsuObj>>();

        var currentTimeInMs = startingMs;
        var truncateMeasure = false;
        var firstTiming = beatmap.NoneInheritedTp[0];
        var msPerMeasure = firstTiming.MsPerBeat * firstTiming.Meter;
        var measureNumber = startingMeasure;
        var mostRecentTp = firstTiming;
        BmsMeasure bmsMeasure;
        var i = 0;

        var buffer = new List<string>();

        while (i < beatmap.Objects.Count)
        {
            var hitObj = beatmap.Objects[i];

            if (hitObj.Time < currentTimeInMs + msPerMeasure - 1 && !truncateMeasure)
            {
                if (hitObj is OsuTimingPoint tp)
                {
                    if (IsWithin2Ms(currentTimeInMs, tp.Time))
                    {
                        mostRecentTp = tp;
                        msPerMeasure = tp.MsPerBeat * tp.Meter;
                        currentTimeInMs = (int)tp.Time;
                        AddToMeasure(currentMeasure, tp);
                    }
                    else
                    {
                        truncateMeasure = true;
                    }
                }
                else
                {
                    AddToMeasure(currentMeasure, hitObj);
                }
            }
            else
            {
                if (truncateMeasure)
                {
                    var numerator = hitObj.Time - currentTimeInMs;
                    if (hitObj.Time - currentTimeInMs < 0) numerator += msPerMeasure;
                    var truncationFraction = numerator / msPerMeasure;

                    var truncationFloat = WrapExpansion(truncationFraction, msPerMeasure).ToFloat();
                    bmsMeasure = CreateMeasure(beatmap, currentMeasure, mostRecentTp, currentTimeInMs, measureNumber.ToString("D3"), truncationFloat);
                    truncateMeasure = false;

                    buffer.AddRange(bmsMeasure.Lines.Select(line => line.ToString()));

                    InitializeMtnv();
                    measureNumber++;
                    mostRecentTp = (OsuTimingPoint)hitObj;
                    msPerMeasure = mostRecentTp.MsPerBeat * mostRecentTp.Meter;
                    currentTimeInMs = (int)hitObj.Time;
                    currentMeasure = new();
                    AddToMeasure(currentMeasure, hitObj);

                    i++;
                    continue;
                }
                else
                {
                    bmsMeasure = CreateMeasure(beatmap, currentMeasure, mostRecentTp, currentTimeInMs, measureNumber.ToString("D3"), 0);
                }

                buffer.AddRange(bmsMeasure.Lines.Select(line => line.ToString()));

                while (!IsWithin2Ms(currentTimeInMs, hitObj.Time))
                {
                    Console.WriteLine("A");
                    measureNumber++;
                    currentTimeInMs += msPerMeasure;

                    if (hitObj.Time >= currentTimeInMs + msPerMeasure - 1) continue;
                    if (hitObj is OsuTimingPoint obj)
                    {
                        if (!IsWithin2Ms(currentTimeInMs, obj.Time))
                        {
                            truncateMeasure = true;
                        }
                        else
                        {
                            mostRecentTp = obj;
                            InitializeMtnv();
                            msPerMeasure = obj.MsPerBeat * obj.Meter;
                            currentTimeInMs = (int)obj.Time;
                        }
                    }
                    
                    currentMeasure = new();
                    AddToMeasure(currentMeasure, hitObj);
                    break;
                }

                if (measureNumber > 999)
                {
                    throw new Exception("Exceeded 999 measures");
                }
            }
            if (!truncateMeasure) i++;
        }
        bmsMeasure = CreateMeasure(beatmap, currentMeasure, mostRecentTp, currentTimeInMs, measureNumber.ToString("D3"), 0);
        foreach (var line in bmsMeasure.Lines)
        {
            buffer.Add(line.ToString());
        }

        return buffer;
    }

    private static BmsMeasure CreateMeasure(OsuMania beatmap, Dictionary<int, List<OsuObj>> currentMeasure,
        OsuTimingPoint timingPoint, float measureStart, string measureNumber, float measureTruncation)
    {
        int GetNumeratorWithGcd(Fraction fraction, float gcd)
        {
            if (Math.Abs(fraction.Denominator - gcd) < 0.1) return System.Convert.ToInt32(fraction.Numerator);
            if (fraction.Denominator < gcd)
            {
                fraction *= 2;
                return GetNumeratorWithGcdNotRec(fraction, gcd);
            }

            if (fraction.Denominator % 3 == 0)
            {
                fraction /= 3;
                fraction = new Fraction((int)fraction.Numerator, (int)fraction.Denominator);
            }
            else
            {
                fraction /= 4;
                fraction = new Fraction((int)fraction.Numerator, (int)fraction.Denominator);
                fraction *= 3;
                fraction = new Fraction((int)fraction.Numerator, (int)fraction.Denominator);
            }

            return GetNumeratorWithGcdNotRec(fraction, gcd);
        }

        int GetNumeratorWithGcdNotRec(Fraction fraction, float gcd)
        {
            while (true)
            {
                if (Math.Abs(fraction.Denominator - gcd) < 0.1) break;
                if (fraction.Denominator < gcd) fraction *= 2;
                else
                {
                    if (fraction.Denominator % 3 == 0)
                    {
                        fraction /= 3;
                        fraction = new Fraction((int)fraction.Numerator, (int)fraction.Denominator);
                    }
                    else
                    {
                        fraction /= 4;
                        fraction = new Fraction((int)fraction.Numerator, (int)fraction.Denominator);
                        fraction *= 3;
                        fraction = new Fraction((int)fraction.Numerator, (int)fraction.Denominator);
                    }
                    break;
                }
            }
            return System.Convert.ToInt32(fraction.Numerator);
        }

        if (currentMeasure.Count == 0)
        {
            return null;
        }

        var bmsMeasure = new BmsMeasure { MeasureNumber = measureNumber };
        if (timingPoint.Meter != 4)
        {
            bmsMeasure.CreateMeasureLengthChange(timingPoint.Meter / 4);
            InitializeMtnv();
        }
        else if (timingPoint.Meter != 0)
            bmsMeasure.CreateMeasureLengthChange(measureTruncation);

        var msPerMeasure = timingPoint.Meter * timingPoint.MsPerBeat;

        foreach (var key in currentMeasure.Keys)
        {
            var denoms = new List<float>();
            var locations = new List<(Fraction, OsuObj)>();
            var locations_ = new List<(int, OsuObj)>();

            foreach (var note in currentMeasure[key])
            {
                var timeValueMs = (float)Math.Round(Math.Abs(measureStart - note.Time), 5);
                Fraction timeValueRatio;
                if (IsWithin2Ms(timeValueMs, 0))
                    timeValueRatio = new Fraction(0, 1);
                else if (MsToInverseNoteValues.ContainsKey(System.Convert.ToInt32(timeValueMs)))
                    timeValueRatio = MsToInverseNoteValues[System.Convert.ToInt32(timeValueMs)];
                else timeValueRatio = WrapExpansion(timeValueMs / msPerMeasure, msPerMeasure);

                denoms.Add(timeValueRatio.Denominator);
                locations.Add((timeValueRatio, note));
            }

            switch (key)
            {
                case 0 when !((OsuTimingPoint)currentMeasure[key][0]).Inherited:
                    {
                        var newBpm = Stuff.CalculateBpm((OsuTimingPoint)currentMeasure[key][0]);
                        if (newBpm <= 255 && Math.Abs(System.Convert.ToInt32(newBpm) - newBpm) < 0.1)
                        {
                            bmsMeasure.CreateBpmChangeLine(System.Convert.ToInt32(newBpm));
                        }
                        else
                        {
                            bmsMeasure.CreateBpmExtendedChangeLine(newBpm, beatmap.FloatBpms.ToArray());
                        }

                        break;
                    }
                case 1:
                    {
                        locations_ = locations_.OrderBy(loc => loc.Item1).ToList();
                        for (var i = 0; i < locations.Count; i++)
                        {
                            bmsMeasure.CreateDataLine(key.ToString().PadLeft(2, '0'), (int)locations_[i].Item1,
                                new() { ((int)locations_[i].Item1, locations_[i].Item2) });
                        }

                        break;
                    }
                default:
                    {
                        float gcd = 0;
                        var first = true;
                        foreach (var denom in denoms)
                        {
                            if (first)
                            {
                                first = false;
                                gcd = denom;
                                continue;
                            }

                            // ReSharper disable once PossibleLossOfFraction
                            gcd = System.Convert.ToInt32(gcd * denom) /
                                  System.Convert.ToInt32(Stuff.GreatestCommonDenominator(gcd, denom));
                        }

                        locations_.AddRange(locations.Select(list_ => (GetNumeratorWithGcdNotRec(list_.Item1, gcd), list_.Item2)));
                        bmsMeasure.CreateDataLine(key.ToString().PadLeft(2, '0'), (int)gcd,
                            locations_.OrderBy(loc => loc.Item1).Select((loc) => (loc.Item1, (object)loc.Item2))
                                .ToList()
                        );
                        break;
                    }
            }
        }

        return bmsMeasure;
    }
}
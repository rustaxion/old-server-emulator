using System.Collections.Generic;

namespace Server.OsuManiaLoader;

public class OsuManiaToBMS
{
    private readonly static Dictionary<int, int> maniaNoteToChannel = new() {
        {0, 16}, {1, 11}, {2, 12}, {3, 13}, {4, 14}, {5, 15}, {6, 18}, {7, 19}
    };

    private readonly static Dictionary<int, int> maniaLnToChannel = new() {
        {0, 56}, {1, 51}, {2, 52}, {3, 53}, {4, 54}, {5, 55}, {6, 58}, {7, 59}
    };

    private readonly static Dictionary<float, float> msToInverseNoteValues = new();

    private static void addToMtnv(float key, float value)
    {
        msToInverseNoteValues[key] = value;
        msToInverseNoteValues[key - 1] = value;
        msToInverseNoteValues[key + 1] = value;
    }

    public static void Clear()
    {
        msToInverseNoteValues.Clear();
    }

    private static List<string> CreateHeader(OsuMania beatmap)
    {
        return new List<string> {
            "", "*---------------------- HEADER FIELD", "",
            "#PLAYER 1", $"#GENRE {beatmap.creator}",
            $"#TITLE {beatmap.titleUnicode}", $"#SUBTITLE {beatmap.version}",
            $"#ARTIST {beatmap.artistUnicode}", $"#BPM {Stuff.CalculateBPM(beatmap.timingPoints[0])}",
            $"#DIFFICULTY {beatmap.od}", "#RANK 3", "", "*---------------------- EXPANSION FIELD",
            "", "*---------------------- MAIN DATA FIELD", "", ""
        };
    }

    public static void Convert(OsuMania beatmap)
    {
        Clear();
        var buffer = CreateHeader(beatmap);
        (var measureOffset, var startTime, var _buffer) = getMusicStartTime(beatmap);
        buffer.AddRange(_buffer);
        buffer.Add("\n");
        // TODO: Actually finish this, 2 more methods remain to be ported.
    }

    private static Fraction WrapExpansion(float n, float msPerMeasure)
    {
        (float, Fraction) expander(float n, float msPerMeasure, float meter, float end)
        {
            bool isWithinOffset(float num, Fraction sum, float offset)
            {
                return (sum + offset) * msPerMeasure > System.Convert.ToInt32(msPerMeasure * num) - 1 &&
                    (sum + offset) * msPerMeasure < System.Convert.ToInt32(msPerMeasure * num) + 2;
            }
            var done = false;
            var denominator = meter;
            var sum = new Fraction(1, meter);

            while (sum + new Fraction(1, denominator) < n)
                sum += 1 / denominator;

            for (var i = 0; i < 6; i++)
            {
                if (isWithinOffset(n, sum, 0) || System.Math.Round(n, 5) == System.Math.Round(sum.ToFloat(), 5))
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

            while (!done)
            {
                if (isWithinOffset(n, sum, 0))
                    break;
                var prevErr = System.Math.Abs(n - sum);
                if (sum > n)
                {
                    if (isWithinOffset(n, sum, -1 / end))
                    {
                        sum -= 1 / end;
                        break;
                    }
                    else if (sum - 1 / end < System.Math.Abs(n - prevErr - 1 / end))
                        break;
                    sum -= 1 / end;
                }
                else if (sum < n)
                {
                    if (isWithinOffset(n, sum, 1 / end))
                    {
                        sum += 1 / end;
                        break;
                    }
                    else if (sum - 1 / end > System.Math.Abs(n - (prevErr + 1 / end)))
                        break;
                    sum += 1 / end;
                }
            }

            return (System.Math.Abs(n - sum), sum);
        }

        var err2 = expander(n, msPerMeasure, 4, 128);
        var err3 = expander(n, msPerMeasure, 3, 192);
        var err = err2.Item1 < err3.Item1 ? err2 : err3;
        var timeValue = err.Item2;

        if (timeValue == 1) return new Fraction(0, 1);
        if (timeValue != 0) addToMtnv(timeValue.ToFloat() * msPerMeasure, timeValue.ToFloat());

        return err.Item2;
    }

    private static (int, float, List<string>) getMusicStartTime(OsuMania beatmap)
    {
        var firstObj = beatmap.objects[0];
        var firstTiming = beatmap.timingPoints[0];
        var msPerMeasure = firstTiming.meter * firstTiming.msPerBeat;
        var buffer = new List<string>();

        var useObj = false;
        float startTime = default;

        if (firstObj.time < firstTiming.time)
        {
            startTime = firstObj.time;
            useObj = true;
        }
        else
        {
            startTime = firstTiming.time;
        }

        if (useObj)
        {
            var i = 0;
            while (beatmap.objects[i].time < firstTiming.time) i++;
            startTime = beatmap.objects[i].time;
        }

        while (startTime - msPerMeasure > 0)
        {
            startTime -= msPerMeasure;
        }

        var startTimeOffset = startTime > 0 ? msPerMeasure - startTime : System.Math.Abs(startTime);
        var musicStartsAt001 = (firstObj.time + msPerMeasure) < msPerMeasure;
        var stoFraction = startTimeOffset / msPerMeasure;
        var timeValueRatio = WrapExpansion(stoFraction, msPerMeasure);
        BMSMeasure bmsMeasure = default;
        int measureStart = 0;

        if (timeValueRatio == 0 && musicStartsAt001)
        {
            bmsMeasure = new() { measureNumber = "001" };
            measureStart = 1;
            bmsMeasure.CreateDataLine("01", (int)timeValueRatio.denominator, new() { ((int)timeValueRatio.numerator, "01") });
        }
        else
        {
            if (musicStartsAt001)
            {
                bmsMeasure = new() { measureNumber = "001" };
                measureStart = 1;
            }
            else
            {
                bmsMeasure = new() { measureNumber = "002" };
                if (timeValueRatio == 0)
                {
                    measureStart = 0;
                }
                else
                {
                    measureStart = 1;
                }
                bmsMeasure.CreateDataLine("01", (int)timeValueRatio.denominator, new() { ((int)timeValueRatio.numerator, "01") });
            }
        }
        var measureOffset = measureStart;

        if (beatmap.objects[0].time > startTime)
        {
            while (!(startTime >= beatmap.objects[0].time))
            {
                startTime += msPerMeasure;
                measureOffset += 1;
            }
        }

        if (firstTiming.meter != 4)
        {
            if (bmsMeasure.measureNumber != "000")
            {
                var bmsMeasure0 = new BMSMeasure { measureNumber = "000" };
                bmsMeasure0.CreateMeasureLengthChange(firstTiming.meter / 4);
                foreach (var line in bmsMeasure0.lines)
                {
                    buffer.Add(line.ToString());
                }
            }
            bmsMeasure.CreateMeasureLengthChange(firstTiming.meter / 4);
        }

        foreach (var line in bmsMeasure.lines)
        {
            buffer.Add(line.ToString());
        }

        if (firstTiming.meter != 4)
        {
            for (var i = 1; i < measureOffset; i++)
            {
                bmsMeasure = new BMSMeasure { measureNumber = i.ToString("D3") };
                bmsMeasure.CreateMeasureLengthChange(firstTiming.meter / 4);
                foreach (var line in bmsMeasure.lines)
                {
                    buffer.Add(line.ToString());
                }
            }
        }

        var firstMeasureTime = startTime;
        if (firstObj.time < firstMeasureTime - 1)
        {
            measureOffset -= 1;
            firstMeasureTime -= msPerMeasure;
        }

        if (timeValueRatio == 0 && !musicStartsAt001)
        {
            while (!isWithin2Ms(startTime, measureOffset * msPerMeasure))
                measureOffset++;
        }

        return (measureOffset, firstMeasureTime, buffer);
    }

    private static bool isWithin2Ms(float base_, float n)
    {
        return base_ - 2 <= n && n <= base_ + 2;
    }
}
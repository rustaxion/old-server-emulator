using System;
using System.Linq;

namespace Server.OsuManiaLoader;

public static class OsuBeatmapReader
{
    public static OsuMania Parse(string content)
    {
        var beatmap = new OsuMania();
        var latestTpIndex = 0;

        void HeaderHitObjects(string line)
        {
            OsuManiaLongNote lnBuffer = null;
            var lineSeperated = line.Split(',');
            if (lineSeperated.Length < 4)
            {
                throw new Exception("HitObject Error: Invalid Syntax");
            }

            var hitObjType = lineSeperated[3];
            var hitObjectArg = lineSeperated.Last().Split(':');
            OsuHitObject hitObj;

            if (hitObjType == "1" || hitObjType == "5")
            {
                hitObj = new OsuManiaNote();
                if (hitObjType == "5")
                {
                    hitObj.NewCombo = true;
                }
            }
            else if (new[] { "2", "6", "8", "12" }.Contains(hitObjType)) return;
            else if (hitObjType == "132" || hitObjType == "128")
            {
                hitObj = new OsuManiaLongNote(int.Parse(hitObjectArg[0]));
                lnBuffer = new OsuManiaLongNote(int.Parse(hitObjectArg[0]));

                if (hitObjType == "128")
                {
                    hitObj.NewCombo = true;
                }
            }
            else throw new Exception($"HitObject Error: type {hitObjType} is not found in `{line}`");

            hitObj.x = int.Parse(lineSeperated[0]);
            hitObj.Time = int.Parse(lineSeperated[2]);

            if (latestTpIndex < beatmap.TimingPoints.Count - 1 &&
                beatmap.TimingPoints[latestTpIndex + 1].Time <= hitObj.Time)
            {
                latestTpIndex++;
            }

            hitObj.TimingPoint = beatmap.TimingPoints[latestTpIndex];
            beatmap.HitObjects.Add(hitObj);

            if (lnBuffer == null) return;

            lnBuffer.Time = ((OsuManiaLongNote)hitObj).EndTime;
            lnBuffer.EndTime = ((OsuManiaLongNote)hitObj).EndTime;
            lnBuffer.x = hitObj.x;
            beatmap.HitObjects.Add(lnBuffer);
        }

        void HeaderTimingPoints(string line)
        {
            float GetMsPerBeat(float ms)
            {
                if (ms >= 0) return ms;
                foreach (var tp in beatmap.TimingPoints)
                {
                    if (!tp.Inherited)
                    {
                        return Math.Abs(ms) / 100 * tp.MsPerBeat;
                    }
                }

                throw new Exception("Non inherited BPM not found. Timing points are broken");
            }

            var lineSeperated = line.Split(',');
            if (lineSeperated.Length != 8)
            {
                throw new Exception("TimingPoint Error: Invalid Syntax");
            }

            var tp = new OsuTimingPoint();
            tp.Time = int.Parse(lineSeperated[0]);
            tp.Inherited = lineSeperated[6] == "0";
            tp.Meter = int.Parse(lineSeperated[2]);
            tp.SampleSet = int.Parse(lineSeperated[3]);
            tp.SampleIndex = int.Parse(lineSeperated[4]);
            tp.Volume = int.Parse(lineSeperated[5]);
            tp.KiaiMode = lineSeperated[7] != "0";

            if (beatmap.TimingPoints.Count > 0 && tp.Time <= beatmap.TimingPoints.Last().Time + 2 && !tp.Inherited)
                beatmap.TimingPoints.RemoveAt(beatmap.TimingPoints.Count - 1);

            if (tp.Inherited)
            {
                tp.MsPerBeat = GetMsPerBeat(float.Parse(lineSeperated[1]));
                var prevTp = beatmap.TimingPoints.Last();

                if (beatmap.TimingPoints.Count > 0 && tp.Time <= prevTp.Time + 1 &&
                    tp.SampleSet != prevTp.SampleSet && tp.SampleIndex != prevTp.SampleIndex)
                {
                    beatmap.TimingPoints.RemoveAt(beatmap.TimingPoints.Count - 1);
                }
            }
            else
            {
                tp.MsPerBeat = float.Parse(lineSeperated[1]);
                var bpm = Stuff.CalculateBpm(tp);

                if (Math.Abs(bpm - (int)bpm) > 0.1 || bpm > 255)
                    beatmap.ParseFloatBpm(bpm);

                if (beatmap.TimingPoints.Count > 0 && tp.Time <= beatmap.NoneInheritedTp.Last().Time + 1)
                    beatmap.NoneInheritedTp.RemoveAt(beatmap.NoneInheritedTp.Count - 1);
                beatmap.NoneInheritedTp.Add(tp);
            }

            beatmap.TimingPoints.Add(tp);
        }

        void HeaderDifficulty(string line)
        {
            var lineProperty = line.Split(':');

            switch (lineProperty[0])
            {
                case "CircleSize":
                {
                    beatmap.KeyCount = int.Parse(lineProperty[1]);
                    break;
                }
                case "OverallDifficulty":
                {
                    beatmap.OverallDifficulty = float.Parse(lineProperty[1]);
                    break;
                }
                case "SliderTickRate":
                case "HPDrainRate":
                case "SliderMultiplier":
                case "ApproachRate":
                {
                    break;
                }
                default:
                {
                    throw new Exception("Attribute Error: " + lineProperty[0] + " not found");
                }
            }
        }

        void HeaderEditor(string line)
        {
            var lineProperty = line.Split(':');

            switch (lineProperty[0])
            {
                case "BeatDivisor":
                {
                    beatmap.BeatDivisor = int.Parse(lineProperty[1].Trim());
                    break;
                }
            }
        }

        void HeaderGeneral(string line)
        {
            var lineProperty = line.Split(':');

            switch (lineProperty[0])
            {
                case "AudioFilename":
                {
                    beatmap.AudioFilename = lineProperty[1].Trim();
                    break;
                }
                case "AudioLeadIn":
                {
                    beatmap.AudioLeadIn = int.Parse(lineProperty[1]);
                    break;
                }
                case "PreviewTime":
                {
                    beatmap.PreviewTime = int.Parse(lineProperty[1]);
                    break;
                }
                case "SampleSet":
                {
                    beatmap.SampleSet = lineProperty[1];
                    break;
                }
                case "Mode":
                {
                    if (int.Parse(lineProperty[1]) != 3)
                        throw new Exception("Beatmap is not an o!m beatmap!");
                    break;
                }
                case "SpecialStyle":
                {
                    beatmap.SpecialStyle = lineProperty[1] == "1";
                    break;
                }
            }
        }

        void HeaderMetadata(string line)
        {
            var lineProperty = line.Split(':');

            switch (lineProperty[0])
            {
                case "Title":
                {
                    beatmap.Title = lineProperty[1].Trim().Replace("/", "").Replace("\\", "");
                    break;
                }
                case "TitleUnicode":
                {
                    beatmap.TitleUnicode = lineProperty[1].Trim();
                    break;
                }
                case "Artist":
                {
                    beatmap.Artist = lineProperty[1].Trim();
                    break;
                }
                case "ArtistUnicode":
                {
                    beatmap.ArtistUnicode = lineProperty[1].Trim();

                    break;
                }
                case "Creator":
                {
                    beatmap.Creator = lineProperty[1].Trim();
                    break;
                }
                case "Version":
                {
                    beatmap.Version = lineProperty[1].Trim();
                    break;
                }
                case "Source":
                {
                    beatmap.Source = lineProperty[1].Trim();
                    break;
                }
                case "BeatmapID":
                {
                    beatmap.BeatmapId = int.Parse(lineProperty[1].Trim());
                    break;
                }
                case "BeatmapSetID":
                {
                    beatmap.BeatmapSetId = int.Parse(lineProperty[1].Trim());
                    break;
                }
            }
        }

        bool isEmpty(string line) => line.Trim().Length == 0;
        bool isComment(string line) => line.StartsWith("//");
        bool isSectionHeader(string line) => line.Trim().StartsWith("[") && line.Trim().EndsWith("]");

        var currentSection = "FileFormat";

        foreach (var line in content.Split('\n'))
        {
            if (isEmpty(line) || isComment(line)) continue;
            if (isSectionHeader(line))
            {
                var split = line.Trim().ToList();
                split.RemoveAt(0);
                split.RemoveAt(split.Count - 1);
                currentSection = String.Join("", split.Select(e => e.ToString()).ToArray());
                continue;
            }

            switch (currentSection)
            {
                case "General":
                {
                    HeaderGeneral(line);
                    break;
                }
                case "Metadata":
                {
                    HeaderMetadata(line);
                    break;
                }
                case "Difficulty":
                {
                    HeaderDifficulty(line);
                    break;
                }
                case "TimingPoints":
                {
                    HeaderTimingPoints(line);
                    break;
                }
                case "HitObjects":
                {
                    HeaderHitObjects(line);
                    break;
                }
                case "Editor":
                {
                    HeaderEditor(line);
                    break;
                }
                case "Events":
                case "FileFormat":
                case "Colours":
                {
                    break;
                }
                default:
                {
                    throw new Exception("Header Error: " + currentSection + " not found");
                }
            }
        }

        beatmap.Objects = new();
        beatmap.Objects.AddRange(beatmap.HitObjects.Select(e => (OsuObj)e));
        beatmap.Objects.AddRange(beatmap.NoneInheritedTp.Select(e => (OsuObj)e));

        beatmap.Objects = beatmap.Objects.OrderBy(obj => obj.Time).ToList();

        return beatmap;
    }
}
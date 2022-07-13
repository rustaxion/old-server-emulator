using System;
using System.Linq;

namespace Server.OsuManiaLoader;

public class OsuBeatmapReader
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
            OsuHitObject hitObj = null;

            if (hitObjType == "1" || hitObjType == "5")
            {
                hitObj = new OsuManiaNote();
                if (hitObjType == "5")
                {
                    hitObj.newCombo = true;
                }
            }
            else if (new string[] { "2", "6", "8", "12" }.Contains(hitObjType)) return;
            else if (hitObjType == "132" || hitObjType == "128")
            {
                hitObj = new OsuManiaLongNote(int.Parse(hitObjectArg[0]));
                lnBuffer = new OsuManiaLongNote(int.Parse(hitObjectArg[0]));

                if (hitObjType == "128")
                {
                    hitObj.newCombo = true;
                }
            }
            else throw new Exception($"HitObject Error: type {hitObjType} is not found in `{line}`");

            if (beatmap.keyCount == 7)
            {
                hitObj.maniaColumn = lineSeperated[0] != "0" ? (int.Parse(lineSeperated[0]) / (512 / beatmap.keyCount)) + 1 : 0;
            }
            else if (beatmap.keyCount == 8)
            {
                hitObj.maniaColumn = (int.Parse(lineSeperated[0]) / (512 / beatmap.keyCount)) + 1;
            }
            hitObj.time = int.Parse(lineSeperated[2]);

            if (latestTpIndex < beatmap.timingPoints.Count - 1 && beatmap.timingPoints[latestTpIndex + 1].time <= hitObj.time)
            {
                latestTpIndex += 1;
            }

            hitObj.timingPoint = beatmap.timingPoints[latestTpIndex];
            beatmap.hitObjects.Add(hitObj);

            if (lnBuffer != null)
            {
                lnBuffer.time = hitObj.time;
                lnBuffer.endTime = ((OsuManiaLongNote)hitObj).endTime;
                lnBuffer.maniaColumn = hitObj.maniaColumn;
                beatmap.hitObjects.Add(lnBuffer);
            }
        }
        void HeaderTimingPoints(string line)
        {
            float GetMsPerBeat(float ms)
            {
                if (ms >= 0) return ms;
                foreach (var tp in beatmap.timingPoints)
                {
                    if (!tp.inherited)
                    {
                        return System.Math.Abs(ms / 100) * tp.msPerBeat;
                    }
                }
                throw new System.Exception("Non inherited BPM not found. Timing points are broken");
            }
            var lineSeperated = line.Split(',');
            if (lineSeperated.Length != 8)
            {
                throw new Exception("TimingPoint Error: Invalid Syntax");
            }

            var tp = new OsuTimingPoint();
            tp.time = Convert.ToInt64(float.Parse(lineSeperated[0]));
            tp.inherited = lineSeperated[6] == "0";
            tp.meter = int.Parse(lineSeperated[2]);
            tp.sampleSet = int.Parse(lineSeperated[3]);
            tp.sampleIndex = int.Parse(lineSeperated[4]);
            tp.volume = int.Parse(lineSeperated[5]);
            tp.kiaiMode = lineSeperated[7] != "0";

            if (beatmap.timingPoints.Count > 0 && tp.time <= beatmap.timingPoints.Last().time + 2 && !tp.inherited)
                beatmap.timingPoints.RemoveAt(beatmap.timingPoints.Count - 1);

            if (tp.inherited)
            {
                tp.msPerBeat = GetMsPerBeat(float.Parse(lineSeperated[1]));
                var prevTp = beatmap.timingPoints.Last();

                if (beatmap.timingPoints.Count > 0 && tp.time <= prevTp.time + 1 &&
                    tp.sampleSet != prevTp.sampleSet && tp.sampleIndex != prevTp.sampleIndex)
                {
                    beatmap.timingPoints.RemoveAt(beatmap.timingPoints.Count - 1);
                }
            }
            else
            {
                tp.msPerBeat = float.Parse(lineSeperated[1]);
                var bpm = Stuff.CalculateBPM(tp);

                if (bpm != Convert.ToInt32(bpm) || bpm > 255)
                    beatmap.ParseFloatBPM(bpm);

                if (beatmap.timingPoints.Count > 0 && tp.time <= beatmap.noneInheritedTP.Last().time + 1)
                    beatmap.noneInheritedTP.RemoveAt(beatmap.noneInheritedTP.Count - 1);
                beatmap.noneInheritedTP.Add(tp);
            }
            beatmap.timingPoints.Add(tp);
        }
        void HeaderDifficulty(string line)
        {
            var lineProperty = line.Split(':');

            switch (lineProperty[0])
            {
                case "CircleSize":
                    {
                        beatmap.keyCount = int.Parse(lineProperty[1]);
                        break;
                    }
                case "OverallDifficulty":
                    {
                        beatmap.od = float.Parse(lineProperty[1]);
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
        void HeaderGeneral(string line)
        {
            var lineProperty = line.Split(':');

            switch (lineProperty[0])
            {
                case "AudioFilename":
                    {
                        beatmap.audioFilename = lineProperty[1].Trim();
                        break;
                    }
                case "AudioLeadIn":
                    {
                        beatmap.audioLeadIn = int.Parse(lineProperty[1]);
                        break;
                    }
                case "PreviewTime":
                    {
                        beatmap.previewTime = int.Parse(lineProperty[1]);
                        break;
                    }
                case "SampleSet":
                    {
                        beatmap.sampleSet = lineProperty[1];
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
                        beatmap.specialStyle = lineProperty[1] == "1";
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
                        beatmap.title = lineProperty[1].Trim().Replace("/", "").Replace("\\", "");
                        break;
                    }
                case "TitleUnicode":
                    {
                        beatmap.titleUnicode = lineProperty[1].Trim();
                        break;
                    }
                case "Artist":
                    {
                        beatmap.artist = lineProperty[1].Trim();
                        break;
                    }
                case "ArtistUnicode":
                    {
                        beatmap.artistUnicode = lineProperty[1].Trim();

                        break;
                    }
                case "Creator":
                    {
                        beatmap.creator = lineProperty[1].Trim();
                        break;
                    }
                case "Version":
                    {
                        beatmap.version = lineProperty[1].Trim();
                        break;
                    }
                case "Source":
                    {
                        beatmap.source = lineProperty[1].Trim();
                        break;
                    }
                case "BeatmapID":
                    {
                        beatmap.beatmapId = lineProperty[1].Trim();
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
                case "Events":
                case "Editor":
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

        beatmap.objects = new();
        beatmap.objects.AddRange(beatmap.hitObjects.Select(e => (OsuObj)e));
        beatmap.objects.AddRange(beatmap.noneInheritedTP.Select(e => (OsuObj)e));

        beatmap.objects = beatmap.objects.OrderBy(obj => obj.time).ToList();

        return beatmap;
    }
}

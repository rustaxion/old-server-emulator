using System;
using SharpCompress.Archive.Zip;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aquatrax;

namespace Server.OsuManiaLoader;

public class OsuManiaBeatmapPack
{
    public int PackId;
    public string PackName;
    public string PackFile;
    public List<OsuMania> Beatmaps = new();
    public string[] BackgroundImages;

    public int MinBpm;
    public int MaxBpm;
}

public class Loader
{
    private static readonly string OszDir = Path.Combine(Directory.GetCurrentDirectory(), "osu!mania_beatmaps");
    public List<OsuManiaBeatmapPack> BeatmapPacks = new();
    public Dictionary<string, MusicInfoData> MusicDic = new();

    public Loader()
    {
        SongPatches.Inject();
        var utf8 = new UTF8Encoding();

        if (!Directory.Exists(OszDir))
        {
            Directory.CreateDirectory(OszDir);
        }

        foreach (var osuFile in Directory.GetFiles(OszDir, "*.osz"))
        {
            try
            {
                var pack = new OsuManiaBeatmapPack
                {
                    PackName = osuFile.Substring(0, osuFile.Length - 4).Replace("\\", "/").Split('/').Last(),
                    PackFile = osuFile,
                };

                Logger.LogInfo($"Beatmap {pack.PackName} found!");
                var archive = ZipArchive.Open(new MemoryStream(File.ReadAllBytes(osuFile)));

                foreach (var entry in archive.Entries)
                {
                    if (entry.IsDirectory || !entry.FilePath.EndsWith(".osu")) continue;
                    var bytes = new byte[entry.Size];
                    var _ = entry.OpenEntryStream().Read(bytes, 0, (int)entry.Size);
                    var textContent = utf8.GetString(bytes);

                    pack.Beatmaps.Add(OsuBeatmapReader.Parse(textContent));
                }

                if (pack.Beatmaps.Count <= 0) continue;

                BeatmapPacks.Add(pack);
                pack.PackId = pack.Beatmaps.Select(btm => btm.BeatmapSetId).First();
                pack.BackgroundImages = pack.Beatmaps.Select(btm => btm.BackgroundImage).Where(i => i != null).ToArray();
                var BPMs = pack.Beatmaps.Select(btm => (int)Stuff.CalculateBpm(btm.TimingPoints[0])).ToArray();
                
                pack.MaxBpm = BPMs.Max();
                pack.MinBpm = BPMs.Min();
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                Logger.LogError(e.StackTrace);
                Logger.LogError($"Failed to load `{osuFile}`");
            }
        }

        foreach (var pack in BeatmapPacks)
        {
            Logger.LogInfo($"Beatmap pack `{pack.PackName}` is now loading!");
            try
            {
                var saltedId = (pack.PackId + SongPatches.Salt).ToString();
                MusicDic[saltedId] = new MusicInfoData
                {
                    id = int.Parse(saltedId),
                    composer = pack.Beatmaps[0].Artist,
                    name = pack.Beatmaps[0].TitleUnicode + " (o!m)",
                    quality = 0,
                    maxBPM = pack.MaxBpm,
                    minBPM = pack.MinBpm,
                    songType = 1
                };

                foreach (var beatmap in pack.Beatmaps)
                {
                    new ManiaToInvaxion(beatmap).Convert(out var map, out var audioFill);
                    beatmap.INVAXION = map;

                    var diff = beatmap.OverallDifficulty;
                    var notes = beatmap.HitObjects.Count;

                    switch (beatmap.KeyCount)
                    {
                        case 4:
                        {
                            if (MusicDic[saltedId].key4_easy_diff == 0)
                            {
                                MusicDic[saltedId].key4_easy_diff = diff;
                                MusicDic[saltedId].key4_easy_note = notes;
                                beatmap.Difficulty = new() { Diff = "standard", KeyMode = "4k" };
                            }
                            else if (MusicDic[saltedId].key4_normal_diff == 0)
                            {
                                MusicDic[saltedId].key4_normal_diff = diff;
                                MusicDic[saltedId].key4_normal_note = notes;
                                beatmap.Difficulty = new() { Diff = "hard", KeyMode = "4k" };
                            }
                            else if (MusicDic[saltedId].key4_hard_diff == 0)
                            {
                                MusicDic[saltedId].key4_hard_diff = diff;
                                MusicDic[saltedId].key4_hard_note = notes;
                                beatmap.Difficulty = new() { Diff = "trinity", KeyMode = "4k" };
                            }

                            break;
                        }
                        case 5:
                        case 6:
                        {
                            if (MusicDic[saltedId].key6_easy_diff == 0)
                            {
                                MusicDic[saltedId].key6_easy_diff = diff;
                                MusicDic[saltedId].key6_easy_note = notes;
                                beatmap.Difficulty = new() { Diff = "standard", KeyMode = "6k" };
                            }
                            else if (MusicDic[saltedId].key6_normal_diff == 0)
                            {
                                MusicDic[saltedId].key6_normal_diff = diff;
                                MusicDic[saltedId].key6_normal_note = notes;
                                beatmap.Difficulty = new() { Diff = "hard", KeyMode = "6k" };
                            }
                            else if (MusicDic[saltedId].key6_hard_diff == 0)
                            {
                                MusicDic[saltedId].key6_hard_diff = diff;
                                MusicDic[saltedId].key6_hard_note = notes;
                                beatmap.Difficulty = new() { Diff = "trinity", KeyMode = "6k" };
                            }

                            break;
                        }
                        case 7:
                        case 8:
                        {
                            if (MusicDic[saltedId].key8_easy_diff == 0)
                            {
                                MusicDic[saltedId].key8_easy_diff = diff;
                                MusicDic[saltedId].key8_easy_note = notes;
                                beatmap.Difficulty = new() { Diff = "standard", KeyMode = "8k" };
                            }
                            else if (MusicDic[saltedId].key8_normal_diff == 0)
                            {
                                MusicDic[saltedId].key8_normal_diff = diff;
                                MusicDic[saltedId].key8_normal_note = notes;
                                beatmap.Difficulty = new() { Diff = "hard", KeyMode = "8k" };
                            }
                            else if (MusicDic[saltedId].key8_hard_diff == 0)
                            {
                                MusicDic[saltedId].key8_hard_diff = diff;
                                MusicDic[saltedId].key8_hard_note = notes;
                                beatmap.Difficulty = new() { Diff = "trinity", KeyMode = "8k" };
                            }

                            break;
                        }
                    }
                }

                Logger.LogInfo($"Beatmap pack `{pack.PackName}` loaded!");
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to load the beatmap pack `{pack.PackName}!` ({e.Message})");
                Logger.LogError(e.StackTrace);
            }
        }
    }
}
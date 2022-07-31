using SharpCompress.Archive.Zip;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Server.OsuManiaLoader;

public class OsuManiaBeatmapPack
{
    public string packName;
    public string packFile;
    public List<OsuMania> Beatmaps = new();
}

public class Loader
{
    private static readonly string OszDir = Path.Combine(Directory.GetCurrentDirectory(), "osu!mania_beatmaps");
    public List<OsuManiaBeatmapPack> BeatmapPacks = new();

    public Loader()
    {
        if (!Server.EnableManiaLoader) return;
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
                    packName = osuFile.Substring(0, osuFile.Length - 4).Replace("\\", "/").Split('/').Last(),
                    packFile = osuFile,
                };

                Logger.LogInfo($"Beatmap {pack.packName} found!");
                var archive = ZipArchive.Open(new MemoryStream(File.ReadAllBytes(osuFile)));

                foreach (var entry in archive.Entries)
                {
                    if (entry.IsDirectory || !entry.FilePath.EndsWith(".osu")) continue;
                    var bytes = new byte[entry.Size];
                    var _ = entry.OpenEntryStream().Read(bytes, 0, (int)entry.Size);
                    var textContent = utf8.GetString(bytes);

                    pack.Beatmaps.Add(OsuBeatmapReader.Parse(textContent));
                }

                if (pack.Beatmaps.Count > 0)
                {
                    BeatmapPacks.Add(pack);
                }
            }
            catch (System.Exception e)
            {
                foreach (var pack in BeatmapPacks)
                {
                    foreach (var bmp in pack.Beatmaps)
                    {
                        foreach (var tp in bmp.TimingPoints)
                        {
                            Logger.LogError(tp.MsPerBeat.ToString());
                        }
                    }
                }
                Logger.LogError(e.Message);
                Logger.LogError(e.StackTrace);
                Logger.LogError($"Failed to load `{osuFile}`");
            }
        }

        foreach (var pack in BeatmapPacks)
        {
            Logger.LogInfo($"Beatmap pack `{pack.packName}` is now loading!");
            foreach (var beatmap in pack.Beatmaps)
            {
                var BMS = OsuManiaToBms.Convert(beatmap);
            }
            Logger.LogInfo($"Beatmap pack `{pack.packName}` loaded!");
        }
    }
}
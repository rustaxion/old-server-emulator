using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using Aquatrax;
using HarmonyLib;
using LitJson;
using Server.Emulator.Tools;
using SharpCompress.Archive.Zip;
using UnityEngine;
using Path = System.IO.Path;

namespace Server.OsuManiaLoader;

// Partially stolen from: https://github.com/MoeGrid/InvaxionCustomBeatmap/tree/9c90c9937c5c940b4a5f981aecf6ea20f0ba0ff7/Plugin
// ReSharper disable once InconsistentNaming
public static class SongPatches
{
    public const long
        Salt = 46978; // int(0x6d616e6961 / 10000000) // python -c "print('int(0x' + b'mania'.hex(), '/ 10000000)')"

    private static readonly Harmony _harmony = new Harmony("song-injector");

    // ReSharper disable once InconsistentNaming
    public static void classifyMusicInfoList_Patch(ref GlobalConfig __instance)
    {
        var dic = Traverse.Create(__instance).Field<Dictionary<string, MusicInfoData>>("musicInfoDict").Value;
        dic.AddAll(Server.ManiaBeatmapsLoader.MusicDic);
    }

    // ReSharper disable once InconsistentNaming
    public static bool GetCoverSprite_Patch(string path, ref Sprite __result)
    {
        var paths = path.Split('_');
        var songId = int.Parse(paths[0]);
        var pack = Server.ManiaBeatmapsLoader.BeatmapPacks.FirstOrDefault(pack => pack.PackId + Salt == songId);
        if (pack == null) return true;

        var bgDir = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("osu!mania_beatmaps", "Backgrounds!"));
        if (!Directory.Exists(bgDir)) Directory.CreateDirectory(bgDir);
        var bgPath = Path.Combine(bgDir, $"{pack.PackId}_{pack.PackName}_bg.jpg");

        var found = File.Exists(bgPath);

        if (!found)
        {
            var backupImg = "";
            if (pack.BackgroundImages.Length != 0)
                backupImg = pack.BackgroundImages.OrderBy(n => Guid.NewGuid()).ToArray()[0];

            var archive = ZipArchive.Open(new MemoryStream(File.ReadAllBytes(pack.PackFile)));
            foreach (var entry in archive.Entries)
            {
                var name = Path.GetFileNameWithoutExtension(entry.FilePath.Replace('\\', '/').Split('/').Last())
                    .ToLower();
                if (entry.IsDirectory || name != "background" && name != "bg") continue;

                var bytes = new byte[entry.Size];
                var _ = entry.OpenEntryStream().Read(bytes, 0, (int)entry.Size);

                File.WriteAllBytes(bgPath, bytes);
                found = true;
                break;
            }

            if (!found && backupImg != "")
            {
                foreach (var entry in archive.Entries)
                {
                    var name = entry.FilePath.Replace('\\', '/').Split('/').Last();
                    if (name != backupImg) continue;

                    var bytes = new byte[entry.Size];
                    var _ = entry.OpenEntryStream().Read(bytes, 0, (int)entry.Size);

                    File.WriteAllBytes(bgPath, bytes);
                    found = true;
                    break;
                }
            }
        }

        if (!found) return true;

        var www = new WWW("file:///" + bgPath);
        while (!www.isDone && www.error == null)
            Thread.Sleep(10);

        var texture = www.GetTexture(true);
        __result = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f),
            texture.width / 3.333f);
        return false;
    }

    class PlayDataHook
    {
        public static IEnumerable<CodeInstruction> LoadPlayMusicTranspiler(ILGenerator il,
            IEnumerable<CodeInstruction> instructions)
        {
            var tmp = new List<CodeInstruction>();
            foreach (var i in instructions)
            {
                tmp.Add(i);
                if (i.opcode == OpCodes.Call && i.operand.ToString().Contains("GetMainAsset"))
                {
                    var call = AccessTools.Method(typeof(PlayDataHook), nameof(LoadPlayMusicHook));
                    var field = AccessTools.Field(typeof(PlayData), "MusicID");
                    tmp.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    tmp.Add(new CodeInstruction(OpCodes.Ldfld, field));
                    tmp.Add(new CodeInstruction(OpCodes.Call, call));
                }
            }

            return tmp;
        }

        public static AudioClip LoadPlayMusicHook(AudioClip audioClip, string id)
        {
            var pack = Server.ManiaBeatmapsLoader.BeatmapPacks.FirstOrDefault(pack =>
                pack.PackId + Salt == int.Parse(id));
            if (pack == null) return audioClip;

            var audioDir = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("osu!mania_beatmaps", "Audio!"));
            if (!Directory.Exists(audioDir)) Directory.CreateDirectory(audioDir);

            var songPath = Path.Combine(audioDir, $"{pack.PackId}_{pack.PackName}_{pack.Beatmaps[0].AudioFilename}");
            var audioType = pack.Beatmaps[0].AudioFilename.Split('.').Last().ToLower() switch
            {
                "mp3" => AudioType.MPEG,
                "wav" => AudioType.WAV,
                "ogg" => AudioType.OGGVORBIS
            };

            var found = File.Exists(songPath);

            if (!found)
            {
                var archive = ZipArchive.Open(new MemoryStream(File.ReadAllBytes(pack.PackFile)));
                foreach (var entry in archive.Entries)
                {
                    if (entry.IsDirectory || !entry.FilePath.EndsWith(pack.Beatmaps[0].AudioFilename)) continue;
                    var bytes = new byte[entry.Size];
                    var _ = entry.OpenEntryStream().Read(bytes, 0, (int)entry.Size);
                    File.WriteAllBytes(songPath, bytes);

                    found = true;
                }
            }

            if (!found) return audioClip;

            var www = new WWW("file:///" + songPath);
            while (!www.isDone && www.error == null)
                Thread.Sleep(10);

            return www.GetAudioClip(true, true, audioType);
        }
    }

    // ReSharper disable once InconsistentNaming
    public static bool ReadOneMusicMap_Patch(string musicMap, int flag, ref MazicData __instance)
    {
        try
        {
            var strs = musicMap.Split('_');
            var pack = Server.ManiaBeatmapsLoader.BeatmapPacks.FirstOrDefault(pack =>
                pack.PackId + Salt == int.Parse(strs[0]));
            if (strs.Length == 4 && pack != null)
            {
                foreach (var beatmap in pack.Beatmaps.Where(i => i.Difficulty != null))
                {
                    if (beatmap.Difficulty.KeyMode != strs[1] ||
                        beatmap.Difficulty.Diff != strs[2]) continue;

                    __instance.ParseText(0, beatmap.INVAXION);
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogInfo("ERROR " + e.Message);
            Logger.LogInfo(e.StackTrace);
        }

        return true;
    }

    public static void Inject()
    {
        var classifyMusicInfoList =
            AccessTools.Method(typeof(GlobalConfig), nameof(GlobalConfig.classifyMusicInfoList));
        var classifyMusicInfoListPatch = AccessTools.Method(typeof(SongPatches), nameof(classifyMusicInfoList_Patch));
        _harmony.Patch(classifyMusicInfoList, prefix: new HarmonyMethod(classifyMusicInfoListPatch));

        var getCoverSprite = AccessTools.Method(typeof(ExtensionMethods), nameof(ExtensionMethods.getCoverSprite));
        var getCoverSpritePatch = AccessTools.Method(typeof(SongPatches), nameof(GetCoverSprite_Patch));
        _harmony.Patch(getCoverSprite, prefix: new HarmonyMethod(getCoverSpritePatch));

        var loadPlayMusic = AccessTools.Method(typeof(PlayData), nameof(PlayData.LoadPlayMusic));
        var loadPlayMusicTranspiler =
            AccessTools.Method(typeof(PlayDataHook), nameof(PlayDataHook.LoadPlayMusicTranspiler));
        _harmony.Patch(loadPlayMusic, transpiler: new HarmonyMethod(loadPlayMusicTranspiler));

        var readOneMusicMap = AccessTools.Method(typeof(MazicData), nameof(MazicData.ReadOneMusicMap));
        var readOneMusicMapPrefix = AccessTools.Method(typeof(SongPatches), nameof(ReadOneMusicMap_Patch));
        _harmony.Patch(readOneMusicMap, new HarmonyMethod(readOneMusicMapPrefix));
    }
}
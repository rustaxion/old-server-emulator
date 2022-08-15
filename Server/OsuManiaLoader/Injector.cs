using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using Aquatrax;
using HarmonyLib;
using Server.Emulator.Tools;
using SharpCompress.Archive.Zip;
using UnityEngine;
using Path = System.IO.Path;

namespace Server.OsuManiaLoader;

public static class SongPatches
{
    public const long
        Salt = 46978; // int(0x6d616e6961 / 10000000) // python -c "print('int(0x' + b'mania'.hex(), '/ 10000000)')"

    private static readonly Harmony _harmony = new Harmony("song-injector");

    // ReSharper disable once InconsistentNaming
    public static void getQuickPlayMusicList_Patch(ref GlobalConfig __instance)
    {
        var dic = __instance.musicInfoDict;
        dic.AddAll(Server.ManiaBeatmapsLoader.MusicDic);

        foreach (var key in Server.ManiaBeatmapsLoader.MusicDic.Keys)
        {
            if (!__instance.freeMusicList.Contains(int.Parse(key)))
            {
                __instance.freeMusicList.Add(int.Parse(key));
            }
        }
    }

    public static bool GetCoverSprite_Patch(string path, ref Sprite __result)
    {
        var paths = path.Split(' ');
        if (paths.Length <= 1) return true;
        var songId = int.Parse(paths[0]);
        if (Server.ManiaBeatmapsLoader.BeatmapPacks.Any(pack => pack.PackId + Salt == songId))
        {
            // TODO: See if cropping the bg image is worthwhile
            __result = new Sprite();
            return false;
        }

        return true;
    }

    class PlayDataHook
    {
        public static IEnumerable<CodeInstruction> LoadPlayMusicTranspiler(ILGenerator il,
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> tmp = new List<CodeInstruction>();
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
            if (File.Exists(songPath))
            {
                var www = new WWW("file:///" + songPath);
                while (!www.isDone && www.error == null)
                    Thread.Sleep(10);
                
                return www.GetAudioClip(true, true,
                    pack.Beatmaps[0].AudioFilename.Split('.').Last().ToLower() switch
                    {
                        "mp3" => AudioType.MPEG,
                        "wav" => AudioType.WAV,
                        "ogg" => AudioType.OGGVORBIS
                    }
                );
            }

            var archive = ZipArchive.Open(new MemoryStream(File.ReadAllBytes(pack.PackFile)));
            foreach (var entry in archive.Entries)
            {
                if (entry.IsDirectory || !entry.FilePath.EndsWith(pack.Beatmaps[0].AudioFilename)) continue;
                var bytes = new byte[entry.Size];
                var _ = entry.OpenEntryStream().Read(bytes, 0, (int)entry.Size);
                
                File.WriteAllBytes(songPath, bytes);

                var www = new WWW("file:///" + songPath);
                while (!www.isDone && www.error == null)
                    Thread.Sleep(10);

                return www.GetAudioClip(true, true,
                    pack.Beatmaps[0].AudioFilename.Split('.').Last().ToLower() switch
                    {
                        "mp3" => AudioType.MPEG,
                        "wav" => AudioType.WAV,
                        "ogg" => AudioType.OGGVORBIS
                    }
                );
            }

            return audioClip;
        }
    }

    public static bool ReadOneMusicMap_Patch(string musicMap, int flag, ref MazicData __instance)
    {
        try
        {
            var strs = musicMap.Split('_');
            var pack = Server.ManiaBeatmapsLoader.BeatmapPacks.FirstOrDefault(pack =>
                pack.PackId + Salt == int.Parse(strs[0]));
            if (strs.Length == 4 && pack != null)
            {
                foreach (var beatmap in pack.Beatmaps)
                {
                    if (beatmap.Difficulty == null ||
                        beatmap.Difficulty.KeyMode != strs[1] ||
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
        var getQuickPlayMusicList = AccessTools.Method(typeof(GlobalConfig), "getQuickPlayMusicList");
        var getQuickPlayMusicListPatch = AccessTools.Method(typeof(SongPatches), "getQuickPlayMusicList_Patch");
        _harmony.Patch(getQuickPlayMusicList, prefix: new HarmonyMethod(getQuickPlayMusicListPatch));

        var getCoverSprite = AccessTools.Method(typeof(ExtensionMethods), "getCoverSprite");
        var getCoverSpritePatch = AccessTools.Method(typeof(SongPatches), "GetCoverSprite_Patch");
        _harmony.Patch(getCoverSprite, prefix: new HarmonyMethod(getCoverSpritePatch));

        var loadPlayMusic = AccessTools.Method(typeof(PlayData), "LoadPlayMusic");
        var loadPlayMusicTranspiler =
            AccessTools.Method(typeof(PlayDataHook), nameof(PlayDataHook.LoadPlayMusicTranspiler));
        _harmony.Patch(loadPlayMusic, transpiler: new HarmonyMethod(loadPlayMusicTranspiler));

        var readOneMusicMap = AccessTools.Method(typeof(MazicData), nameof(MazicData.ReadOneMusicMap));
        var readOneMusicMapPrefix = AccessTools.Method(typeof(SongPatches), nameof(ReadOneMusicMap_Patch));
        _harmony.Patch(readOneMusicMap, new HarmonyMethod(readOneMusicMapPrefix));
    }
}
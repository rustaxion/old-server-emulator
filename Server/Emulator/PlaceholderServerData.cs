using System.Collections.Generic;
using Server.Emulator.Tools;
using System.Linq;
using System;
using cometScene;

namespace Server.Emulator;

public class PlaceholderServerData
{
    public Ret_Event_Info GetEventInfo()
    {
        var info = new Ret_Event_Info();
        info.newPlayer = new() { loginDay = 7 };
        info.getStamina = new() { isGet = 0 };
        info.levelGift = new() { getList = { 5, 2 } };
        info.bili = new()
        {
            beginTime = 1530374400,
            endTime = 1893427200,
            id = 0,
            list =
            {
                new SpecialEventOneData
                {
                    index = 1,
                    type = 4,
                    condition = 1,
                    value = 0,
                    isGet = 1,
                    rewardList = { new() { type = 4, count = 1, id = 12 } }
                },
                new SpecialEventOneData
                {
                    index = 2,
                    type = 4,
                    condition = 1,
                    value = 0,
                    isGet = 1,
                    rewardList = { new() { type = 4, count = 1, id = 8 } }
                },
                new SpecialEventOneData
                {
                    index = 3,
                    type = 4,
                    condition = 1,
                    value = 0,
                    isGet = 1,
                    rewardList = { new() { type = 5, count = 1, id = 50010 } }
                },
                new SpecialEventOneData
                {
                    index = 4,
                    type = 4,
                    condition = 2,
                    value = 0,
                    isGet = 1,
                    rewardList = { new() { type = 5, count = 1, id = 20020 } }
                },
            },
        };
        return info;
    }

    public cometScene.Ret_ShopInfo GetShopInfo()
    {
        var shopInfo = new cometScene.Ret_ShopInfo
        {
            characterList =
            {
                Capacity = 10
            },
            songList =
            {
                Capacity = 10
            },
            themeList =
            {
                Capacity = 2
            },
        };

        shopInfo.characterList.AddRange(
            (
                new int[][]
                {
                    new[] { 20090, 2, 1000, 1 },
                    new[] { 40320, 2, 300, 0 },
                    new[] { 20060, 2, 300, 0 },
                    new[] { 40010, 2, 1888, 2 },
                    new[] { 40090, 2, 300, 0 },
                    new[] { 20050, 2, 1000, 1 },
                    new[] { 30040, 2, 1888, 2 },
                    new[] { 40250, 2, 300, 0 },
                    new[] { 40150, 2, 1000, 1 },
                    new[] { 20040, 2, 1000, 1 }
                }
            ).Select(
                i =>
                    new cometScene.ShopItemInfo
                    {
                        id = i[0],
                        costType = i[1],
                        normalPrice = i[2],
                        discountPrice = i[2],
                        order = i[3],
                        beginSaleTime = 0,
                        discountBeginTime = 0,
                        discountEndTime = 0,
                    }
            )
        );

        shopInfo.songList.AddRange(
            (
                new int[][]
                {
                    new[] { 63204, 2, 200, 200, 0 },
                    new[] { 68008, 2, 200, 200, 2 },
                    new[] { 62005, 2, 200, 200, 0 },
                    new[] { 63103, 2, 200, 200, 0 },
                    new[] { 63123, 2, 200, 200, 0 },
                    new[] { 80002, 2, 200, 200, 2 },
                    new[] { 62006, 2, 200, 200, 0 },
                    new[] { 63122, 2, 200, 200, 0 },
                    new[] { 69008, 2, 200, 200, 2 },
                    new[] { 68108, 2, 200, 200, 0 }
                }
            ).Select(
                i =>
                    new cometScene.ShopItemInfo
                    {
                        id = i[0],
                        costType = i[1],
                        normalPrice = i[2],
                        discountPrice = i[3],
                        order = i[4],
                        beginSaleTime = 0,
                        discountBeginTime = 0,
                        discountEndTime = 0,
                    }
            )
        );

        shopInfo.themeList.AddRange(
            (new [] { new[] { 2, 2, 1000, 1000, 0 }, new[] { 6, 2, 1000, 1000, 0 } }).Select(
                i =>
                    new cometScene.ShopItemInfo
                    {
                        id = i[0],
                        costType = i[1],
                        normalPrice = i[2],
                        discountPrice = i[3],
                        order = i[4],
                        beginSaleTime = 0,
                        discountBeginTime = 0,
                        discountEndTime = 0,
                    }
            )
        );

        return shopInfo;
    }
    
    public Tuple<List<cometScene.SingleSongData>, List<cometScene.SingleSongData>, List<cometScene.SingleSongData>> ArcadeInfoList
    {
        get
        {
            var diffList = new Dictionary<string, uint> { { "easy", 0 }, { "normal", 1 }, { "hard", 2 } };
            var arcadeList = new Tuple<List<cometScene.SingleSongData>, List<cometScene.SingleSongData>, List<cometScene.SingleSongData>>(new(), new(), new());

            try
            {
                foreach (var (songId, song) in Songs.SongData.Select(kvp => (kvp.Key, kvp.Value)))
                {
                    if (song.songType == 0)
                        continue;

                    foreach (var diff in song.IterateDifficulties())
                    {
                        if (diff.Item1 == null)
                            continue;
                        var diffInfo = new cometScene.SingleSongData
                        {
                            songId = song.id,
                            difficulty = diffList[diff.Item2],
                            mode = diff.Item3
                        };
                        if (diff.Item1 is >= 1 and <= 9)
                            arcadeList.Item1.Add(diffInfo);
                        if (diff.Item1 is >= 5 and <= 13)
                            arcadeList.Item2.Add(diffInfo);
                        if ((diff.Item1 is >= 9 and <= 17) || diff.Item1 == 99)
                            arcadeList.Item3.Add(diffInfo);
                    }
                }
            }
            catch (Exception e)
            {
                ServerLogger.LogError("ArcadeInfoList: " + e.Message);
            }

            return arcadeList;
        }
    }
}

public class SongInfo
{
    public uint id;
    public string name;
    public string composer;
    public int songType;
    public int param;
    public uint? key4_easy_diff;
    public uint? key4_normal_diff;
    public uint? key4_hard_diff;
    public uint? key6_easy_diff;
    public uint? key6_normal_diff;
    public uint? key6_hard_diff;
    public uint? key8_normal_diff;
    public uint? key8_hard_diff;
    public int? key4_easy_note;
    public int? key4_normal_note;
    public int? key4_hard_note;
    public int? key6_easy_note;
    public int? key6_normal_note;
    public int? key6_hard_note;
    public int? key8_normal_note;
    public int? key8_hard_note;
    public int? minBPM;
    public int? maxBPM;

    public IEnumerable<Tuple<uint?, string, uint>> IterateDifficulties()
    {
        var diffList = new Dictionary<string, uint?>
        {
            { "key4_easy_diff", key4_easy_diff },
            { "key4_normal_diff", key4_normal_diff },
            { "key4_hard_diff", key4_hard_diff },
            { "key6_easy_diff", key6_easy_diff },
            { "key6_normal_diff", key6_normal_diff },
            { "key6_hard_diff", key6_hard_diff },
            { "key8_normal_diff", key8_normal_diff },
            { "key8_hard_diff", key8_hard_diff }
        };
        var keyNumToId = new Dictionary<char, uint>() { { '4', 1 }, { '6', 2 }, { '8', 3 } };

        foreach (var diff in diffList)
        {
            yield return new Tuple<uint?, string, uint>(diff.Value, diff.Key.Split(new char[] { '_' })[1], keyNumToId[diff.Key[3]]);
        }
    }
}

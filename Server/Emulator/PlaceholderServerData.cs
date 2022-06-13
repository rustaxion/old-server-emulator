using System.Collections.Generic;
using System.Linq;
using LitJson;
using System;
using System.Reflection;
using Server.Emulator.Tools;

namespace Server.Emulator;

public static class PlaceholderServerData
{
    public static int[] SongList = {
        20000, 20001, 20002, 20003, 20005, 20006, 20007, 20008, 20009,
        20010, 20011, 20012, 20013, 20014, 20015, 62001, 62003, 62004,
        62005, 62006, 62007, 62008, 62010, 62011, 62012, 62013, 62016,
        62017, 62018, 62019, 62020, 62021, 62022, 62023, 62024, 62025,
        63001, 63002, 63003, 63004, 63101, 63102, 63103, 63120, 63121,
        63122, 63123, 63204, 64001, 64002, 64003, 64004, 64005, 65006,
        65011, 65012, 65014, 65015, 65016, 65030, 65031, 65032, 65033,
        65034, 65035, 65036, 65037, 65100, 65101, 65102, 66001, 67001,
        67002, 67003, 67004, 68001, 68002, 68003, 68004, 68005, 68006,
        68007, 68008, 68009, 68101, 68102, 68103, 68104, 68105, 68106,
        68107, 68108, 69001, 69002, 69008, 69009, 69012, 69017, 69018,
        69901, 69903, 80000, 80001, 80002, 80003, 80004, 80005, 80006,
        80007, 80008, 80009, 80010, 80011, 80012, 80013, 80014, 80015,
        80016, 80017, 80018, 80019, 80020, 80021, 80023, 80026, 80027,
        80028, 80029, 80030, 80031, 80032, 80033, 80034, 80035, 80036,
        80037, 80038, 80039, 80040, 80041, 80042, 80043, 80044, 80045,
        80046, 81001, 81002, 81003, 81004, 81005, 81015, 82000, 82001,
        82002, 82003, 82004, 82005, 82006, 82007, 82008, 82009, 82010,
        82011, 82012, 82013, 84100, 84101, 84102, 90000
    };

    public static cometScene.Ret_Event_Info EventInfo
    {
       get
       {
          var eventInfo = new cometScene.Ret_Event_Info();
          eventInfo.levelGift.getList.AddRange(new uint[] { 5, 2 });
          eventInfo.getStamina._isGet = 0;
          eventInfo.newPlayer.loginDay = 7;
          eventInfo.weekCheckin = new cometScene.WeekCheckinData
          {
             loginDay = 2,
             rewardList = {
                new cometScene.WeekCheckinRewardData
                {
                   day = 1,
                   reward =
                   {
                      type = 1,
                      id = 0,
                      count = 10,
                   }
                },
                new cometScene.WeekCheckinRewardData
                {
                   day = 2,
                   reward =
                   {
                      type = 3,
                      id = 90002,
                      count = 1,
                   }
                },
                new cometScene.WeekCheckinRewardData
                {
                   day = 3,
                   reward =
                   {
                      type = 3,
                      id = 90001,
                      count = 1,
                   }
                },
                new cometScene.WeekCheckinRewardData
                {
                   day = 4,
                   reward =
                   {
                      type = 1,
                      id = 0,
                      count = 1,
                   }
                },
                new cometScene.WeekCheckinRewardData
                {
                   day = 5,
                   reward =
                   {
                      type = 3,
                      id = 90001,
                      count = 1,
                   }
                },
                new cometScene.WeekCheckinRewardData
                {
                   day = 6,
                   reward =
                   {
                      type = 3,
                      id = 90002,
                      count = 1,
                   }
                },
                new cometScene.WeekCheckinRewardData
                {
                   day = 7,
                   reward =
                   {
                      type = 1,
                      id = 90005,
                      count = 1,
                   }
                },
             }
          };

          eventInfo.bili = new cometScene.SpecialEventData
          {
             beginTime = 1530374400,
             endTime = 1893427200,
             id = 0,
             list =
             {
                new cometScene.SpecialEventOneData
                {
                   index = 1,
                   type = 4,
                   condition = 1,
                   value = 0,
                   isGet = 1,
                   rewardList = { new cometScene.ItemData { type = 4, count = 1, id = 12 } }
                },
                new cometScene.SpecialEventOneData
                {
                   index = 2,
                   type = 4,
                   condition = 1,
                   value = 0,
                   isGet = 1,
                   rewardList = { new cometScene.ItemData { type = 4, count = 1, id = 8 } }
                },
                new cometScene.SpecialEventOneData
                {
                   index = 3,
                   type = 4,
                   condition = 1,
                   value = 0,
                   isGet = 1,
                   rewardList = { new cometScene.ItemData { type = 5, count = 1, id = 50010 } }
                },
                new cometScene.SpecialEventOneData
                {
                   index = 4,
                   type = 4,
                   condition = 2,
                   value = 0,
                   isGet = 1,
                   rewardList = { new cometScene.ItemData { type = 5, count = 1, id = 20020 } }
                },

             },
          };
          return eventInfo;
       }
    }
    
    public static cometScene.CharacterList CharacterList
       {
          get
          {
             var characterList = new cometScene.CharacterList();
             (new uint[]
             {
                 20060, 40090, 40130, 50010, 40010,
                 40040, 10050, 10010, 10020, 20040,
                 40220, 10040, 20050, 30070, 20020,
                 20090, 40320, 20030, 10030, 30050,
                 40150, 40260, 30040, 40330, 40120,
                 20170, 10060, 30090, 40250, 30100,
                 30110, 1006010, 50050, 3004010, 30060
             }).Select(i =>
             {
                 var charData = new cometScene.CharData()
                 {
                     charId = i,
                     level = 1,
                     exp = 0,
                     playCount = 0,
                 };
                 characterList.list.Add(charData);
                 return charData;
             });
             return characterList;
          }
       }

    public static cometScene.Ret_ShopInfo ShopInfo
    {
       get
       {
          var shopInfo = new cometScene.Ret_ShopInfo();
          shopInfo.characterList.AddRange((new int[][]
          {
             new [] { 20090, 2, 1000, 1000, 1 },
             new [] { 40320, 2, 300, 300, 0 },
             new [] { 20060, 2, 300, 300, 0 },
             new [] { 40010, 2, 1888, 1888, 2 },
             new [] { 40090, 2, 300, 300, 0 },
             new [] { 20050, 2, 1000, 1000, 1 },
             new [] { 30040, 2, 1888, 1888, 2 },
             new [] { 40250, 2, 300, 300, 0 },
             new [] { 40150, 2, 1000, 1000, 1 },
             new [] { 20040, 2, 1000, 1000, 1 }
          }).Select(i => new cometScene.ShopItemInfo
          {
             id = i[0],
             costType = i[1],
             normalPrice = i[2],
             discountPrice = i[3],
             order = i[4],
             beginSaleTime = 0,
             discountBeginTime = 0,
             discountEndTime = 0,
          }));
          
          shopInfo.songList.AddRange((new int[][]
          {
             new [] { 63204, 2, 200, 200, 0 },
             new [] { 68008, 2, 200, 200, 2 },
             new [] { 62005, 2, 200, 200, 0 },
             new [] { 63103, 2, 200, 200, 0 },
             new [] { 63123, 2, 200, 200, 0 },
             new [] { 80002, 2, 200, 200, 2 },
             new [] { 62006, 2, 200, 200, 0 },
             new [] { 63122, 2, 200, 200, 0 },
             new [] { 69008, 2, 200, 200, 2 },
             new [] { 68108, 2, 200, 200, 0 }
          }).Select(i => new cometScene.ShopItemInfo
          {
             id = i[0],
             costType = i[1],
             normalPrice = i[2],
             discountPrice = i[3],
             order = i[4],
             beginSaleTime = 0,
             discountBeginTime = 0,
             discountEndTime = 0,
          }));
          
          shopInfo.themeList.AddRange((new int[][]
          {
             new [] { 2, 2, 1000, 1000, 0 },
             new [] { 6, 2, 1000, 1000, 0 }
          }).Select(i => new cometScene.ShopItemInfo
          {
             id = i[0],
             costType = i[1],
             normalPrice = i[2],
             discountPrice = i[3],
             order = i[4],
             beginSaleTime = 0,
             discountBeginTime = 0,
             discountEndTime = 0,
          }));
          return shopInfo;
       }
    }
    
    public static Tuple<List<cometScene.SingleSongData>, List<cometScene.SingleSongData>, List<cometScene.SingleSongData>> ArcadeInfoList
    {
        get
        {
            var diffList = new Dictionary<string, uint> { {"easy", 0 }, { "normal", 1 }, { "hard", 2 } };
            var arcadeList = new Tuple<List<cometScene.SingleSongData>, List<cometScene.SingleSongData>, List<cometScene.SingleSongData>>(new(), new(), new());

            try
            {
               foreach (var (songId, song) in Songs.SongData.Select(kvp => (kvp.Key, kvp.Value)))
               {
                  if (song.songType == 0) continue;

                  foreach (var diff in song.IterateDifficulties())
                  {
                     if (diff.Item1 == null) continue;
                     var diffInfo = new cometScene.SingleSongData { songId = song.id, difficulty = diffList[diff.Item2], mode = diff.Item3 };
                     if (diff.Item1 is >= 1 and <= 9) arcadeList.Item1.Add(diffInfo);
                     if (diff.Item1 is >= 5 and <= 13) arcadeList.Item2.Add(diffInfo);
                     if ((diff.Item1 is >= 9 and <= 17) || diff.Item1 == 99) arcadeList.Item3.Add(diffInfo);
                  }
               }
            } catch (Exception e)
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
         { "key4_easy_diff", key4_easy_diff }, { "key4_normal_diff", key4_normal_diff }, { "key4_hard_diff", key4_hard_diff },
         { "key6_easy_diff", key6_easy_diff }, { "key6_normal_diff", key6_normal_diff }, { "key6_hard_diff", key6_hard_diff },
         { "key8_normal_diff", key8_normal_diff }, { "key8_hard_diff", key8_hard_diff }
      };
      var keyNumToId = new Dictionary<char, uint>() { { '4', 1 }, { '6', 2 }, { '8', 3 } };

      foreach (var diff in diffList)
      {
         yield return new Tuple<uint?, string, uint>(diff.Value, diff.Key.Split(new char[] { '_' })[1], keyNumToId[diff.Key[3]]);
      }
   }
}
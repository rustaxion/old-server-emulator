using LitJson;

namespace Server.Emulator;

public class PlaceholderServerData
{
    public static int[] songList = {
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
    
    public static cometScene.Ret_Event_Info Ret_Event_Info = JsonMapper.ToObject<cometScene.Ret_Event_Info>(@"
         {
            ""levelGift"":{
               ""getList"":[
                  5,
                  2
               ]
            },
            ""getStamina"":{
               ""isGet"":0
            },
            ""newPlayer"":{
             ""loginDay"":7
            },
            ""weekCheckin"":{
             ""loginDay"":2,
               ""rewardList"":[
                  {
                    ""day"": 1,
                    ""reward"": {
                        ""type"": 1,
                        ""count"": 10,
                        ""id"": 0
                    }
                  },
                  {
                     ""day"":7,
                     ""reward"":{
                        ""type"":3,
                        ""count"":1,
                        ""id"":90005
                     }
                  },
                  {
                     ""day"":6,
                     ""reward"":{
                        ""type"":3,
                        ""count"":1,
                        ""id"":90002
                     }
                  },
                  {
                     ""day"":5,
                     ""reward"":{
                        ""type"":3,
                        ""count"":1,
                        ""id"":90001
                     }
                  },
                  {
                     ""day"":4,
                     ""reward"":{
                        ""type"":1,
                        ""count"":10,
                        ""id"":0
                     }
                 },
                  {
                     ""day"":3,
                     ""reward"":{
                        ""type"":3,
                        ""count"":1,
                        ""id"":90001
                     }
                  },
                  {
                     ""day"":2,
                     ""reward"":{
                        ""type"":3,
                        ""count"":1,
                        ""id"":90002
                     }
                  }
               ]
            },
            ""bili"":{
               ""beginTime"":""1530374400"",
               ""endTime"":""1893427200"",
               ""id"":0,
               ""list"":[
                  {
                 ""index"":1,
                     ""type"":4,
                     ""condition"":1,
                     ""value"":0,
                     ""isGet"":1,
                     ""rewardList"":[
                        {
                             ""type"":4,
                             ""count"":1,
                             ""id"":12
                        }
                     ]
                  },
                  {
                 ""index"":4,
                     ""type"":4,
                     ""condition"":2,
                     ""value"":0,
                     ""isGet"":1,
                     ""rewardList"":[
                        {
                     ""type"":5,
                           ""count"":1,
                           ""id"":20020
                        }
                     ]
                  },
                  {
                 ""index"":3,
                     ""type"":4,
                     ""condition"":1,
                     ""value"":0,
                     ""isGet"":1,
                     ""rewardList"":[
                        {
                     ""type"":5,
                           ""count"":1,
                           ""id"":50010
                        }
                     ]
                  },
                  {
                 ""index"":2,
                     ""type"":4,
                     ""condition"":1,
                     ""value"":0,
                     ""isGet"":1,
                     ""rewardList"":[
                        {
                     ""type"":4,
                           ""count"":1,
                           ""id"":8
                        }
                     ]
                  }
               ]
            }
         }
     ");

    public static cometScene.CharacterList
        characterList = JsonMapper.ToObject<cometScene.CharacterList>(@"
      {
         ""list"": [
            {
                ""charId"": 20060,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 40090,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 40130,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 50010,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 40010,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 40040,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 10050,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 10010,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 10020,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 20040,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 40220,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 10040,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 20050,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 30070,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 20020,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 20090,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 40320,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 20030,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 10030,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 6
            },
            {
                ""charId"": 30050,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 40150,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 40260,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 30040,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 40330,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 40120,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 20170,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 10060,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 30090,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 40250,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 30100,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"": 30110,
                ""level"": 30,
                ""exp"": 0,
                ""playCount"": 0
            },
            {
                ""charId"":1006010,
                ""level"":30,
                ""exp"":0,
                ""playCount"":0
            },
            {
                ""charId"":50050,
                ""level"":30,
                ""exp"":0,
                ""playCount"":0
            },
            {
                ""charId"":3004010,
                ""level"":30,
                ""exp"":0,
                ""playCount"":0
            },
            {
                ""charId"":30060,
                ""level"":30,
                ""exp"":0,
                ""playCount"":0
            }
         ]
      }
    ");
}
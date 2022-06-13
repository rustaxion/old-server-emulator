using System.Collections.Generic;
using System.IO;
using LitJson;
using System;
using System.Linq;
using cometScene;
using Server.Emulator.Database;
using Serializer = ProtoBuf.Serializer;

namespace Server.Emulator.Handlers;

public class Gate
{
    private Dictionary<uint, Action<byte[], long>> Handlers = new();
    private readonly Dictionary<uint, string> _modeLink = new()
    {
        { 1, "4k" },
        { 2, "6k" },
        { 3, "8k" }
    };
    
    private readonly Dictionary<uint, string> _difficultyLink = new()
    {
        { 1, "ez" },
        { 2, "nm" },
        { 3, "hd" }
    };
    
    private static cometScene.Ntf_CharacterFullData GetFullCharacterData(types.AccountData account)
    {
        var announcementData = new cometScene.AnnouncementData();
        announcementData.list.Add(new cometScene.AnnouncementOneData
        {
            title = "Operation Announcement",
            content = "<b><color=#ffa500ff>《音灵INVAXION》Closing notice</color></b>\n\t\t  \n\n　　It's been a long wait, guardians of the sound.\n\t\t  \n　　Welcome to the<color=#ffa500ff>《音灵INVAXION》</color>world.",
            picId = 0,
            tag = 1,
        });
        return new cometScene.Ntf_CharacterFullData
        {
            data = new cometScene.CharacterFullData
            {
                baseInfo = new cometScene.PlayerBaseInfo
                {
                    accId = account.accId,
                    charId = account.charId,
                    charName = account.name,
                    headId = account.headId,
                    level = account.level,
                    curExp = account.curExp,
                    maxExp = account.maxExp,
                    guideStep = 7,
                    curCharacterId = account.selectCharId,
                    curThemeId = account.selectThemeId,
                    onlineTime = account.onlineTime,
                    needReqAppReceipt = 0,
                    activePoint = 0,
                    preRankId = 0,
                    guideList = { 9, 8, 7, 6, 5, 4, 3, 2, 1 },
                    country = account.country,
                    preRankId4K = 0,
                    preRankId6K = 0,
                    titleId = account.titleId,
                },
                currencyInfo = account.currencyInfo,
                socialData = new cometScene.SocialData(),
                announcement = announcementData,
                themeList = account.themeList,
                vipInfo = account.vipInfo,
                arcadeData = account.arcadeData,
                team = account.team,
                charList = PlaceholderServerData.CharacterList,
                scoreList = account.scoreList,
                songList = account.songList,
            }
        };
    }

    public Gate()
    {
        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_SingleSongRank, (byte[] msgContent, long sessionId) =>
        {
            // TODO: Finish this, as it is not called right now I'll ignore it.
            ServerLogger.LogInfo("Single song chart.");
            var data = Serializer.Deserialize<cometScene.Req_SingleSongRank>(new MemoryStream(msgContent));
            
            ServerLogger.LogError($"mode: {data.mode}, isWeek: {data.isWeek}, difficulty: {data.difficulty}");
        });
        
        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_ChangeHeadIcon, (byte[] msgContent, long sessionId) =>
        {
            ServerLogger.LogInfo($"Change head Icon.");
            var data = Serializer.Deserialize<cometScene.Req_ChangeHeadIcon>(new MemoryStream(msgContent));
            
            var account = Server.Database.GetAccount(sessionId);
            if (account == null) return;
            account.headId = data.id;
            
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ChangeHeadIcon,
                Data = Index.ObjectToByteArray(new cometScene.Ret_ChangeHeadIcon { id = data.id }),
            });
            
            Server.Database.SaveAll();
        });
        
        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_ChangeCharacter, (byte[] msgContent, long sessionId) =>
        {
            ServerLogger.LogInfo($"Change character.");
            var data = Serializer.Deserialize<cometScene.Req_ChangeCharacter>(new MemoryStream(msgContent));
            
            var account = Server.Database.GetAccount(sessionId);
            if (account == null) return;
            account.selectCharId = data.id;
            
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ChangeCharacter,
                Data = Index.ObjectToByteArray(new cometScene.Ret_ChangeCharacter { id = data.id }),
            });
            
            Server.Database.SaveAll();
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_ChangeTheme, (byte[] msgContent, long sessionId) =>
        {
            ServerLogger.LogInfo($"Change theme.");
            var data = Serializer.Deserialize<cometScene.Req_ChangeTheme>(new MemoryStream(msgContent));
            
            var account = Server.Database.GetAccount(sessionId);
            if (account == null) return;
            account.selectThemeId = data.id;
            
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ChangeTheme,
                Data = Index.ObjectToByteArray(new cometScene.Ret_ChangeTheme { id = data.id }),
            });
            
            Server.Database.SaveAll();
        });
        
        Handlers.Add((uint)cometGate.ParaCmd.ParaCmd_Ret_UserGameTime + 1000, (byte[] msgContent, long sessionId) =>
        {
            ServerLogger.LogInfo($"Reported game time.");
        });
        
        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_BeginSong + 1000, (byte[] msgContent, long sessionId) =>
        {
            ServerLogger.LogInfo($"Start playing song!");
        });
        
        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_FinishSong, (byte[] msgContent, long sessionId) =>
        {
            ServerLogger.LogInfo($"Finished playing song!");
            var data = Serializer.Deserialize<cometScene.Req_FinishSong>(new MemoryStream(msgContent)).data;
            var account = Server.Database.GetAccount(sessionId);
            if (account == null) return;

            account.totalScore = data.totalScore;
            account.total4KScore = data.total4KScore;
            account.total6KScore = data.total6KScore;
            account.total8KScore = data.total8KScore;

            Server.Database.UpdateAccount(account);

            var songInfo = data.playData;

            var scoreList = _modeLink[data.mode] switch
            {
                "4k" => account.scoreList._key4List,
                "6k" => account.scoreList._key6List,
                "8k" => account.scoreList._key8List,
                _ => throw new NotSupportedException($"Mode `{_modeLink[data.mode]} ({data.mode})` not supported."),
            };

            var difficultyList = _difficultyLink[data.difficulty] switch
            {
                "ez" => scoreList._easyList,
                "nm" => scoreList._normalList,
                "hd" => scoreList._hardList,
                _ => throw new NotSupportedException($"Difficulty `{_difficultyLink[data.difficulty]} ({data.difficulty})` not supported."),
            };

            SingleSongInfo InitSingleSongInfo()
            {
                var song = new cometScene.SingleSongInfo() { songId = data.songId, playCount = 0 };
                difficultyList.Add(song);
                return song;
            }

            var singleSongInfo = difficultyList.FirstOrDefault(song => song.songId == data.songId) ?? InitSingleSongInfo();

            singleSongInfo.miss = songInfo.miss;
            if (songInfo.score > singleSongInfo.score) singleSongInfo.score = songInfo.score;
            singleSongInfo.playCount += 1;
            singleSongInfo.isAllMax = (songInfo.maxPercent == 100) ? 1u : 0u;
            singleSongInfo.isFullCombo = (songInfo.miss == 0) ? 1u : 0u;
            
            Server.Database.SaveAll();

            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_FinishSong,
                Data = Index.ObjectToByteArray(new cometScene.Ret_FinishSong()
                {
                    songInfo = singleSongInfo,
                    settleData = new()
                    {
                        changeList = { new() { type = 9, count = 450, id = 0 }, },
                        expData = new() { level = 69, curExp = 0, maxExp = 100 }
                    }
                }),
            });
        });
        
        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_ChangeLanguage, (byte[] msgContent, long sessionId) =>
        {
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ChangeLanguage,
                Data = new byte[0],
            });
        });
        
        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_ShopInfo, (byte[] msgContent, long sessionId) =>
        {
            ServerLogger.LogInfo("Sent shop info.");
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ShopInfo,
                Data = Index.ObjectToByteArray(PlaceholderServerData.ShopInfo),
            });
        });

        Handlers.Add((uint)cometGate.ParaCmd.ParaCmd_LoginGateVerify + 1000, (byte[] msgContent, long sessionId) =>
        {
            var data = Serializer.Deserialize<cometGate.LoginGateVerify>(new MemoryStream(msgContent));
            var accId = data.accId;

            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometGate.MainCmd.MainCmd_Time,
                ParaCmd = (uint)cometGate.ParaCmd.ParaCmd_Ntf_GameTime,
                Data = Index.ObjectToByteArray(new cometGate.Ntf_GameTime()
                {
                    gametime = (uint)TimeHelper.getCurUnixTimeOfSec(),
                }),
            });

            var account = Server.Database.GetAccount(accId);
            if (account == null) return;

            account.sessionId = (uint)sessionId;
            Server.Database.UpdateAccount(account);
            
            var userInfoList = new cometGate.SelectUserInfoList();

            if (account.charId != 0)
            {
                userInfoList.userList.Add(new cometGate.SelectUserInfo
                {
                    charId = (uint)account.charId,
                    accStates = 0,
                });
            }
            
            ServerLogger.LogInfo($"LoginGateVerify: [{((userInfoList.userList.Count > 0) ? ("{ charId: " + account.charId + ", accStates: 0 }") : "") }]");
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometGate.MainCmd.MainCmd_Select,
                ParaCmd = (uint)cometGate.ParaCmd.ParaCmd_SelectUserInfoList,
                Data = Index.ObjectToByteArray(userInfoList),
            });
        });
        
        Handlers.Add((uint)cometGate.ParaCmd.ParaCmd_EnterGame + 1000, (byte[] msgContent, long sessionId) =>
        {
            var account = Server.Database.GetAccount(sessionId);

            ServerLogger.LogInfo("Enter the game: account: " + (account != null ? account.ToString() : "null"));
            if (account == null) return;

            var characterFullData = GetFullCharacterData(account);
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ntf_CharacterFullData,
                Data = Index.ObjectToByteArray(characterFullData),
            });
        });
        
        Handlers.Add((uint)cometGate.ParaCmd.ParaCmd_CreateCharacter + 1000, (byte[] msgContent, long sessionId) =>
        {
            ServerLogger.LogInfo("Creating a new character!");
            var data = Serializer.Deserialize<cometGate.CreateCharacter>(new MemoryStream(msgContent));
            var account = Server.Database.GetAccount(sessionId);

            account.name = data.name;
            account.selectCharId = data.selectCharId;
            account.language = data.language;
            account.country = data.country;
            account.headId = data.selectCharId;
            account.charId = (long)Math.Round((double)(account.accId + 40000000000));
            ServerLogger.LogInfo($"New account id: {account.accId}, new character id: {account.charId}");

            Server.Database.UpdateAccount(account);

            var characterFullData = GetFullCharacterData(account);
            ServerLogger.LogInfo(characterFullData.ToString());
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ntf_CharacterFullData,
                Data = Index.ObjectToByteArray(characterFullData),
            });
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_Event_Info, (byte[] msgContent, long sessionId) =>
        {
            ServerLogger.LogInfo($"Shop information");
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_Event_Info,
                Data = Index.ObjectToByteArray(PlaceholderServerData.EventInfo),
            });
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_Social_PublishDynamics, (byte[] msgContent, long sessionId) =>
        {
            var data = Serializer.Deserialize<cometScene.Req_Social_PublishDynamics>(new MemoryStream(msgContent));
            ServerLogger.LogInfo($"Public Activity");
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_Social_PublishDynamics,
                Data = Index.ObjectToByteArray(JsonMapper.ToObject<cometScene.Ret_Social_PublishDynamics>(
                    @"{ ""contentList"": " + JsonMapper.ToJson(data.contentList) + "}"
                )),
            });
        });
        
        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_Arcade_Info, (byte[] msgContent, long sessionId) =>
        {
            ServerLogger.LogInfo("Arcade Mode");
            var stageList = new List<cometScene.ArcadeStageData>();

            for (var i = 0; i < 3; i++)
            {
                var rnd = new Random();

                var arcadeInfoList = (i switch
                {
                    0 => PlaceholderServerData.ArcadeInfoList.Item1,
                    1 => PlaceholderServerData.ArcadeInfoList.Item2,
                    _ => PlaceholderServerData.ArcadeInfoList.Item3,
                }).OrderBy(x => rnd.Next()).ToArray();
                
                var songList = new List<cometScene.SingleSongData>();
                for (var j = 0; j < 8; j++)
                {
                    songList.Add(arcadeInfoList[j]);
                }
                
                stageList.Add(new cometScene.ArcadeStageData { stageId = (uint)(i + 1) });
                stageList.Last().songList.AddRange(songList);
            }

            var retArcadeInfo = new cometScene.Ret_Arcade_Info();
            retArcadeInfo.stageList.AddRange(stageList);

            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_Arcade_Info,
                Data = Index.ObjectToByteArray(retArcadeInfo),
            });
        });
    }

    public bool Dispatch(uint mainCmd, uint paraCmd, byte[] msgContent, long sessionId)
    {
        if (!Handlers.ContainsKey((uint)(paraCmd + (mainCmd is 1 or 3 ? 1000 : 0)))) return false;

        Handlers[(uint)(paraCmd + (mainCmd is 1 or 3 ? 1000 : 0))](msgContent, sessionId);
        return true;
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LitJson;
using ProtoBuf;

namespace Server.Emulator.Handlers;

public class Gate
{
    private Dictionary<uint, Action<byte[]>> Handlers = new();

    public Gate()
    {
        Handlers.Add(1003, (byte[] msgContent) =>
        {
            ServerLogger.LogInfo($"Reported game time.");
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_ChangeLanguage, (byte[] msgContent) =>
        {
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ChangeLanguage,
                Data = new byte[0],
            });
        });

        Handlers.Add((uint)cometGate.ParaCmd.ParaCmd_LoginGateVerify + 1000, (byte[] msgContent) =>
        {
            var data = Serializer.Deserialize<cometGate.LoginGateVerify>(new MemoryStream(msgContent));
            var accId = data.accId;

            var acc = Server.Database.GetAccount(accId);
            acc.sessionId = Server.Database.Session;
            Server.Database.Save();

            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometGate.MainCmd.MainCmd_Time,
                ParaCmd = (uint)cometGate.ParaCmd.ParaCmd_Ntf_GameTime,
                Data = Index.ObjectToByteArray(new cometGate.Ntf_GameTime()
                {
                    gametime = (uint)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                }),
            });

            var account = Server.Database.GetAccount(accId);
            if (account == null) return;

            var json = @"{ ""userList"": [" +
               (account.charId == "0000000000"
                   ? ""
                   : ("{" + $@"""charId"": ""{account.charId}"", ""accStates"": 0" + "}")
               ) + "]}";
            ServerLogger.LogInfo("LoginGateVerify: " + json.Replace("\n", "").Replace("\r", ""));
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometGate.MainCmd.MainCmd_Select,
                ParaCmd = (uint)cometGate.ParaCmd.ParaCmd_SelectUserInfoList,
                Data = Index.ObjectToByteArray(JsonMapper.ToObject<cometGate.SelectUserInfoList>(json)),
            });
        });
        
        // + 500 to disable it...
        Handlers.Add((uint)cometGate.ParaCmd.ParaCmd_CreateCharacter + 1000 + 500, (byte[] msgContent) =>
        {
            ServerLogger.LogInfo("Creating a new character!");
            var data = Serializer.Deserialize<cometGate.CreateCharacter>(new MemoryStream(msgContent));
            var newCharacter = new DataBase.Datatypes.AccountData
            {
                name = data.name,
                selectCharId = data.selectCharId,
                language = data.language,
                country = data.country,
                headId = data.selectCharId
            };
            newCharacter.charId = Math.Round((double)newCharacter.accId + 40000000000).ToString(CultureInfo.InvariantCulture);
            Server.Database.AddAccount(newCharacter);
            Server.Database.Save();

            var characterFullData = new cometScene.Ntf_CharacterFullData
            {
                data = new cometScene.CharacterFullData
                {
                    baseInfo = new cometScene.PlayerBaseInfo
                    {
                        accId = newCharacter.accId,
                        charId = Convert.ToInt64(newCharacter.charId),
                        charName = newCharacter.name,
                        headId = newCharacter.headId,
                        level = 1,
                        curExp = 0,
                        maxExp = 0,
                        guideStep = 7,
                        curCharacterId = newCharacter.selectCharId,
                        curThemeId = (uint)newCharacter.selectThemeId,
                        onlineTime = 0,
                        needReqAppReceipt = 0,
                        activePoint = 0,
                        preRankId = 0,
                        guideList = { 9, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
                        country = newCharacter.country,
                        preRankId4K = 0,
                        preRankId6K = 0,
                        titleId = 1001,
                    },
                    currencyInfo = new cometScene.PlayerCurrencyInfo
                    {
                        gold = 0,
                        diamond = 0,
                        curStamina = 0,
                        maxStamina = 10,
                        honourPoint = 0,
                    },
                    socialData = new cometScene.SocialData(),
                    announcement = JsonMapper.ToObject<cometScene.AnnouncementData>(@"
                        {
                            ""list"": [
                                    {
                                        ""title"": ""Operation Announcement"",
                                        ""content"": ""<b><color=#ffa500ff>《音灵INVAXION》Closing notice</color></b>\n\t\t  \n\n　　It's been a long wait, guardians of the sound.\n\t\t  \n　　Welcome to the<color=#ffa500ff>《音灵INVAXION》</color>world."",
                                        ""picId"": 0,
                                        ""tag"": 1
                                    },
                            ],
                            ""picList"": []
                        }
                    "),
                    themeList = JsonMapper.ToObject<cometScene.ThemeList>("{ \"list\": " + JsonMapper.ToJson(Enumerable.Range(1, 14).Select(i => new cometScene.ThemeData { themeId = (uint)i}).ToList()) + "}"),
                    vipInfo = new cometScene.PlayerVIPInfo
                    {
                        level = 0,
                        exp = 0,
                        levelUpExp = 100,
                        inSubscription = 0,
                    },
                    arcadeData = new cometScene.ArcadeData
                    {
                        key4List = new cometScene.ArcadeDiffList(),
                        key6List = new cometScene.ArcadeDiffList(),
                        key8List = new cometScene.ArcadeDiffList(),
                    },
                    team = new cometScene.TeamData
                    {
                        teamId = 0,
                        teamName = "",
                        uploadSongCount = 3,
                        canUploadSong = 0,
                    },
                    charList = PlaceholderServerData.characterList,
                    scoreList = new cometScene.ScoreList(),
                    songList = JsonMapper.ToObject<cometScene.SongList>("{ \"list\": " + JsonMapper.ToJson(PlaceholderServerData.songList.Select(i => new cometScene.SongData { songId = (uint)i}).ToList()) +  @", ""favoriteList"": []"  + "}"),
                }
            };
            
            ServerLogger.LogInfo(characterFullData.ToString());
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ntf_CharacterFullData,
                Data = Index.ObjectToByteArray(characterFullData),
            });
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_Event_Info, (byte[] msgContent) =>
        {
            ServerLogger.LogInfo($"Shop information");
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_Event_Info,
                Data = Index.ObjectToByteArray(PlaceholderServerData.Ret_Event_Info),
            });
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_Social_PublishDynamics, (byte[] msgContent) =>
        {
            cometScene.Req_Social_PublishDynamics data = Serializer.Deserialize<cometScene.Req_Social_PublishDynamics>(new MemoryStream(msgContent));
            ServerLogger.LogInfo($"Public Activity");
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_Social_PublishDynamics,
                Data = Index.ObjectToByteArray(JsonMapper.ToObject<cometScene.Ret_Social_PublishDynamics>(
                        @"{ ""contentList"": " + JsonMapper.ToJson(data.contentList) + "}"
                    ).contentList),
            });
        });
    }

    public bool Dispatch(uint mainCmd, uint paraCmd, byte[] msgContent)
    {
        if (!Handlers.ContainsKey((uint)(paraCmd + (mainCmd is 1 or 3 ? 1000 : 0)))) return false;

        Handlers[(uint)(paraCmd + (mainCmd is 1 or 3 ? 1000 : 0))](msgContent);
        return true;
    }
}
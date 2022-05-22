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
        
        Handlers.Add((uint)cometGate.ParaCmd.ParaCmd_CreateCharacter + 1000, (byte[] msgContent) =>
        {
            ServerLogger.LogInfo("Creating a new character!");
            var data = Serializer.Deserialize<cometGate.CreateCharacter>(new MemoryStream(msgContent));
            var account = Server.Database.CreateAccount();

            account.name = data.name;
            account.selectCharId = data.selectCharId;
            account.language = data.language;
            account.country = data.country;
            account.headId = data.selectCharId;
            
            Server.Database.UpdateAccount(account);

            var characterFullData = new cometScene.Ntf_CharacterFullData
            {
                data = new cometScene.CharacterFullData
                {
                    baseInfo = new cometScene.PlayerBaseInfo
                    {
                        accId = account.accId,
                        charId = Convert.ToInt64(account.charId),
                        charName = account.name,
                        headId = account.headId,
                        level = account.level,
                        curExp = account.curExp,
                        maxExp = account.maxExp,
                        guideStep = 7,
                        curCharacterId = account.selectCharId,
                        curThemeId = (uint)account.selectThemeId,
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
                    themeList = account.themeList,
                    vipInfo = account.vipInfo,
                    arcadeData = account.arcadeData,
                    team = account.team,
                    charList = PlaceholderServerData.characterList,
                    scoreList = account.scoreList,
                    songList = account.songList,
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
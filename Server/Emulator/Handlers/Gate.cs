using Server.Emulator.Database;
using Server.Emulator.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using Serializer = ProtoBuf.Serializer;

namespace Server.Emulator.Handlers;

public class Gate
{
    private Dictionary<uint, Action<byte[], long>> Handlers = new();

    public static cometScene.Ntf_CharacterFullData GetFullCharacterData(types.AccountData account)
    {
        var announcementData = new cometScene.AnnouncementData();
        announcementData.list.Add(
            new cometScene.AnnouncementOneData
            {
                title = "Operation Announcement",
                content =
                    "<b><color=#ffa500ff>《音灵INVAXION》Closing notice</color></b>\n\t\t  \n\n　　It's been a long wait, guardians of the sound.\n\t\t  \n　　Welcome to the<color=#ffa500ff>《音灵INVAXION》</color> world.",
                picId = 0,
                tag = 1,
            }
        );
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
                    titleId = account.titleId
                },
                currencyInfo = account.currencyInfo,
                socialData = new cometScene.SocialData(),
                announcement = announcementData,
                themeList = account.themeList,
                vipInfo = account.vipInfo,
                arcadeData = account.arcadeData,
                team = account.team,
                charList = account.CharacterList,
                scoreList = account.scoreList,
                songList = account.songList
            }
        };
    }

    public Gate()
    {
        Handlers.AddAll(GateHandlers.Shop.Handlers);
        Handlers.AddAll(GateHandlers.Beatmaps.Handlers);
        Handlers.AddAll(GateHandlers.Accounts.Handlers);
        Handlers.AddAll(GateHandlers.CosmicTour.Handlers);

        Handlers.Add((uint)cometGate.ParaCmd.ParaCmd_Ret_UserGameTime + 1000, (_, _) =>
            {
                ServerLogger.LogInfo($"Reported game time.");
            }
        );

        Handlers.Add((uint)cometGate.ParaCmd.ParaCmd_LoginGateVerify + 1000, (msgContent, sessionId) =>
            {
                var data = Serializer.Deserialize<cometGate.LoginGateVerify>(new MemoryStream(msgContent));
                var accId = data.accId;

                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometGate.MainCmd.MainCmd_Time,
                        ParaCmd = (uint)cometGate.ParaCmd.ParaCmd_Ntf_GameTime,
                        Data = Index.ObjectToByteArray(new cometGate.Ntf_GameTime() { gametime = (uint)TimeHelper.getCurUnixTimeOfSec(), }),
                    }
                );

                var account = Server.Database.GetAccount(accId);
                if (account == null)
                    return;

                account.sessionId = (uint)sessionId;
                Server.Database.UpdateAccount(account);

                var userInfoList = new cometGate.SelectUserInfoList();

                if (account.charId != 0)
                {
                    userInfoList.userList.Add(new cometGate.SelectUserInfo { charId = (uint)account.charId, accStates = 0, });
                }

                ServerLogger.LogInfo($"LoginGateVerify: [{((userInfoList.userList.Count > 0) ? ("{ charId: " + account.charId + ", accStates: 0 }") : "")}]");
                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometGate.MainCmd.MainCmd_Select,
                        ParaCmd = (uint)cometGate.ParaCmd.ParaCmd_SelectUserInfoList,
                        Data = Index.ObjectToByteArray(userInfoList),
                    }
                );
            }
        );

        Handlers.Add((uint)cometGate.ParaCmd.ParaCmd_EnterGame + 1000, (_, sessionId) =>
            {
                var account = Server.Database.GetAccount(sessionId);

                ServerLogger.LogInfo("Enter the game: account: " + (account != null ? account.ToString() : "null"));
                if (account == null)
                    return;

                var characterFullData = GetFullCharacterData(account);
                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ntf_CharacterFullData,
                        Data = Index.ObjectToByteArray(characterFullData),
                    }
                );
            }
        );

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_Social_PublishDynamics, (msgContent, _) =>
            {
                var data = Serializer.Deserialize<cometScene.Req_Social_PublishDynamics>(new MemoryStream(msgContent));
                ServerLogger.LogInfo($"Public Activity");

                var activity = new cometScene.Ret_Social_PublishDynamics();
                activity.contentList.AddRange(data.contentList);

                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_Social_PublishDynamics,
                        Data = Index.ObjectToByteArray(activity),
                    }
                );
            }
        );
    }

    public bool Dispatch(uint mainCmd, uint paraCmd, byte[] msgContent, long sessionId)
    {
        if (!Handlers.ContainsKey((uint)(paraCmd + (mainCmd is 1 or 3 ? 1000 : 0))))
            return false;

        Handlers[(uint)(paraCmd + (mainCmd is 1 or 3 ? 1000 : 0))](msgContent, sessionId);
        return true;
    }
}

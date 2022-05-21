using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LitJson;
using ProtoBuf;

namespace Server.Emulator.Handlers;

public class Gate
{
    private Dictionary<uint, Action<uint, uint, byte[]>> Handlers = new();

    public Gate()
    {
        Handlers.Add(1003, (uint mainCmd, uint paraCmd, byte[] msgContent) =>
        {
            ServerLogger.LogInfo($"Reported game time.");
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_ChangeLanguage, (uint mainCmd, uint paraCmd, byte[] msgContent) =>
        {
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ChangeLanguage,
                Data = new byte[0],
            });
        });

        Handlers.Add((uint)cometGate.ParaCmd.ParaCmd_LoginGateVerify + 1000, (uint mainCmd, uint paraCmd, byte[] msgContent) =>
        {
            ServerLogger.LogInfo("LoginGateVerify");
            var data = Serializer.Deserialize<cometGate.LoginGateVerify>(new MemoryStream(msgContent));
            var accId = data.accId;
            var token = data.token;

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

            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometGate.MainCmd.MainCmd_Select,
                ParaCmd = (uint)cometGate.ParaCmd.ParaCmd_SelectUserInfoList,
                Data = Index.ObjectToByteArray(JsonMapper.ToObject<cometGate.SelectUserInfoList>(
                    @"{ ""userList"": [" +
                        (account.charId == "0000000000" ? "" : ("{" + $@"""charId"": ""{account.charId}"", ""accStates"": 0" + "}")
                    + "]}"))
                ),
            });

        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_Event_Info, (uint mainCmd, uint paraCmd, byte[] msgContent) =>
        {
            ServerLogger.LogInfo($"Shop information");
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_Event_Info,
                Data = Index.ObjectToByteArray(PlaceholderServerData.Ret_Event_Info),
            });
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_Social_PublishDynamics, (uint mainCmd, uint paraCmd, byte[] msgContent) =>
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
        if (!Handlers.ContainsKey(paraCmd)) return false;

        Handlers[paraCmd](mainCmd, (uint)(paraCmd + (mainCmd is 1 or 3 ? 1000 : 0)), msgContent);
        return true;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using HarmonyLib;
using ProtoBuf;
using Server.DiscordRichPresence;
using Server.Emulator.Tools;
namespace Server.Emulator.Handlers;

public class Login
{
    private Dictionary<uint, Action<byte[]>> Handlers = new();

    public Login()
    {
        Handlers.Add((uint)cometLogin.ParaCmd.ParaCmd_Req_GameVersion, (byte[] msgContent) =>
        {
            string gameVersion = Aquatrax.GlobalConfig.getInstance().getGameVersion();
            ServerLogger.LogInfo($"GameVersion: {gameVersion}");

            Index.Instance.LoginPackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometLogin.MainCmd.MainCmd_Login,
                ParaCmd = (uint)cometLogin.ParaCmd.ParaCmd_Ret_GameVersion,
                Data = Index.ObjectToByteArray(new cometLogin.Ret_GameVersion()
                {
                    version = gameVersion,
                    announcementContent = "",
                    announcementTitle = "",
                    serverState = 2
                }),
            });
        });

        Handlers.Add((uint)cometLogin.ParaCmd.ParaCmd_Req_ThirdLogin, (byte[] msgContent) =>
        {
            ServerLogger.LogInfo($"ThirdLogin");
            cometLogin.Req_ThirdLogin data = Serializer.Deserialize<cometLogin.Req_ThirdLogin>(new MemoryStream(msgContent));

            if (data == null)
                return;

            string token;
            using (var md5Hash = MD5.Create())
            {
                var hash = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(data.openId + "6031"));
                var sBuilder = new StringBuilder();

                foreach (var t in hash)
                {
                    sBuilder.Append(t.ToString("x2"));
                }

                token = sBuilder.ToString();
            }

            ServerLogger.LogInfo($"Token: {token}");
            ServerLogger.LogInfo($"OpenId: {data.openId}");

            var accountData = Server.Database.GetAccount(data.openId);

            if (accountData == null)
            {
                accountData = Server.Database.CreateAccount();
                accountData.steamId = data.openId;
                accountData.token = token;
                Server.Database.UpdateAccount(accountData);
            }

            var accId = accountData.accId;

            ServerLogger.LogInfo($"AccId: {accId}");

            Data.Update();
            Index.Instance.LoginPackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometLogin.MainCmd.MainCmd_Login,
                ParaCmd = (uint)cometLogin.ParaCmd.ParaCmd_Ret_ThirdLogin,
                Data = Index.ObjectToByteArray(new cometLogin.Ret_ThirdLogin()
                {
                    data = new cometLogin.GatewayServerData()
                    {
                        gateIP = "127.0.0.1",
                        gatePort = 20021,
                        token = token,
                        accId = accId
                    },
                }),
            });
        });

    }

    public bool Dispatch(uint mainCmd, uint paraCmd, byte[] msgContent)
    {
        if (!Handlers.ContainsKey(paraCmd)) return false;

        Handlers[paraCmd](msgContent);
        return true;
    }
}
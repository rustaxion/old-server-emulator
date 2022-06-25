using System;
using System.Collections.Generic;
using System.IO;
using Serializer = ProtoBuf.Serializer;

namespace Server.Emulator.Handlers.GateHandlers;

public static class Accounts
{
    public static Dictionary<uint, Action<byte[], long>> Handlers = new()
    {
        {
            (uint)cometScene.ParaCmd.ParaCmd_Req_ChangeHeadIcon,
            (msgContent, sessionId) =>
            {
                ServerLogger.LogInfo($"Change head Icon.");
                var data = Serializer.Deserialize<cometScene.Req_ChangeHeadIcon>(new MemoryStream(msgContent));

                var account = Server.Database.GetAccount(sessionId);
                if (account == null)
                    return;
                account.headId = data.id;

                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ChangeHeadIcon,
                        Data = Index.ObjectToByteArray(new cometScene.Ret_ChangeHeadIcon { id = data.id }),
                    }
                );

                Server.Database.SaveAll();
            }
        },
        {
            (uint)cometScene.ParaCmd.ParaCmd_Req_SetFavorite,
            (msgContent, sessionId) =>
            {
                var data = Serializer.Deserialize<cometScene.Req_SetFavorite>(new MemoryStream(msgContent));
                ServerLogger.LogInfo($"Set Favourite: {data.songId}");

                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_SetFavorite,
                        Data = Index.ObjectToByteArray(new cometScene.Ret_SetFavorite { songId = data.songId, isFavorite = data.isFavorite }),
                    }
                );

                var account = Server.Database.GetAccount(sessionId);
                if (account == null)
                    return;
                var isFavorite = Convert.ToBoolean(data.isFavorite);

                if (account.songList.favoriteList.Contains(data.songId) && !isFavorite)
                {
                    account.songList.favoriteList.Remove(data.songId);
                }
                else if (!account.songList.favoriteList.Contains(data.songId) && isFavorite)
                {
                    account.songList.favoriteList.Add(data.songId);
                }

                Server.Database.SaveAll();
            }
        },
        {
            (uint)cometScene.ParaCmd.ParaCmd_Req_ChangeCharacter,
            (msgContent, sessionId) =>
            {
                ServerLogger.LogInfo($"Change character.");
                var data = Serializer.Deserialize<cometScene.Req_ChangeCharacter>(new MemoryStream(msgContent));

                var account = Server.Database.GetAccount(sessionId);
                if (account == null)
                    return;
                account.selectCharId = data.id;

                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ChangeCharacter,
                        Data = Index.ObjectToByteArray(new cometScene.Ret_ChangeCharacter { id = data.id }),
                    }
                );

                Server.Database.SaveAll();
            }
        },
        {
            (uint)cometScene.ParaCmd.ParaCmd_Req_ChangeTheme,
            (msgContent, sessionId) =>
            {
                ServerLogger.LogInfo($"Change theme.");
                var data = Serializer.Deserialize<cometScene.Req_ChangeTheme>(new MemoryStream(msgContent));

                var account = Server.Database.GetAccount(sessionId);
                if (account == null)
                    return;
                account.selectThemeId = data.id;

                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ChangeTheme,
                        Data = Index.ObjectToByteArray(new cometScene.Ret_ChangeTheme { id = data.id }),
                    }
                );

                Server.Database.SaveAll();
            }
        },
        {
            (int)cometScene.ParaCmd.ParaCmd_Req_ChangeLanguage,
            (_, _) =>
            {
                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ChangeLanguage,
                        Data = new byte[0],
                    }
                );
            }
        },
        {
            (uint)cometGate.ParaCmd.ParaCmd_CreateCharacter + 1000,
            (msgContent, sessionId) =>
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

                account.CharacterList = new()
                {
                    list =
                    {
                        new cometScene.CharData()
                        {
                            charId = data.selectCharId,
                            level = 1,
                            exp = 0,
                            playCount = 0,
                        }
                    }
                };

                Server.Database.UpdateAccount(account);

                var characterFullData = Gate.GetFullCharacterData(account);
                ServerLogger.LogInfo(characterFullData.ToString());
                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ntf_CharacterFullData,
                        Data = Index.ObjectToByteArray(characterFullData),
                    }
                );
            }
        }
    };
}

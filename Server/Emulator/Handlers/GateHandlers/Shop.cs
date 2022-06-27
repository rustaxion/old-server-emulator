using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serializer = ProtoBuf.Serializer;

namespace Server.Emulator.Handlers.GateHandlers;

public static class Shop
{
    public static Dictionary<uint, Action<byte[], long>> Handlers = new()
    {
        {
            (uint)cometScene.ParaCmd.ParaCmd_Req_Event_Info,
            (_, _) =>
            {
                ServerLogger.LogInfo($"Event information");
                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_Event_Info,
                        Data = Index.ObjectToByteArray(Server.PlaceholderServerData.GetEventInfo()),
                    }
                );
            }
        },
        {
            (uint)cometScene.ParaCmd.ParaCmd_Req_ShopBuy,
            (msgContent, sessionId) =>
            {
                ServerLogger.LogInfo($"Buy item from shop.");
                var data = Serializer.Deserialize<cometScene.Req_ShopBuy>(new MemoryStream(msgContent));
                var account = Server.Database.GetAccount(sessionId);
                var shopInfo = Server.PlaceholderServerData.GetShopInfo();

                switch (data.shopType)
                {
                    case (uint)Aquatrax.eShopType.eShopType_Character:
                        {
                            if (account.CharacterList.list.Any(character => character.charId == data.itemId))
                            {
                                ServerLogger.LogError("You already have a character with that ID!");
                                break;
                            }

                            var price = shopInfo.characterList.First(item => item.id == data.itemId).discountPrice;
                            if (account.currencyInfo.gold < price)
                            {
                                ServerLogger.LogError("You don't have enough money to make this purchase!");
                                break;
                            }

                            account.currencyInfo.gold -= (uint)price;

                            account.CharacterList.list.Add(
                                new cometScene.CharData()
                                {
                                    charId = data.itemId,
                                    level = 1,
                                    exp = 0,
                                    playCount = 0,
                                }
                            );

                            account.curExp += 20;

                            while (account.curExp >= account.maxExp)
                            {
                                account.curExp -= account.maxExp;
                                account.level++;
                                account.maxExp = (uint)Math.Round(account.maxExp * 1.2f, 0);
                            }

                            Server.Database.UpdateAccount(account);
                            Server.Database.SaveAll();
                            Index.Instance.GatePackageQueue.Enqueue(
                                new Index.GamePackage()
                                {
                                    MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                                    ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ShopBuy,
                                    Data = Index.ObjectToByteArray(
                                        new cometScene.Ret_ShopBuy
                                        {
                                            settleData = new cometScene.SettleData()
                                            {
                                                changeList =
                                                {
                                                new()
                                                {
                                                    type = 5,
                                                    count = 1,
                                                    id = data.itemId,
                                                }
                                                },
                                                expData = new cometScene.PlayerExpData()
                                                {
                                                    level = account.level,
                                                    curExp = account.curExp,
                                                    maxExp = account.maxExp,
                                                }
                                            }
                                        }
                                    ),
                                }
                            );
                            break;
                        }
                    case (uint)Aquatrax.eShopType.eShopType_Member:
                        {
                            // might be related to the VIP stuff
                            break;
                        }
                    case (uint)Aquatrax.eShopType.eShopType_Song:
                        {
                            if (account.songList.list.Any(song => song.songId == data.itemId))
                            {
                                ServerLogger.LogError("You already have a song with that ID!");
                                break;
                            }

                            var price = shopInfo.songList.First(item => item.id == data.itemId).discountPrice;
                            if (account.currencyInfo.gold < price)
                            {
                                ServerLogger.LogError("You don't have enough money to make this purchase!");
                                break;
                            }

                            account.currencyInfo.gold -= (uint)price;
                            account.curExp += 20;
                            account.songList.list.Add(new() { songId = data.itemId });

                            while (account.curExp >= account.maxExp)
                            {
                                account.curExp -= account.maxExp;
                                account.level++;
                                account.maxExp = (uint)Math.Round(account.maxExp * 1.2f, 0);
                            }

                            Server.Database.UpdateAccount(account);
                            Server.Database.SaveAll();

                            Index.Instance.GatePackageQueue.Enqueue(
                                new Index.GamePackage()
                                {
                                    MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                                    ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ShopBuy,
                                    Data = Index.ObjectToByteArray(
                                        new cometScene.Ret_ShopBuy
                                        {
                                            settleData = new cometScene.SettleData()
                                            {
                                                changeList = { new() { type = 6, count = 1, id = data.itemId } },
                                                expData = new cometScene.PlayerExpData()
                                                {
                                                    level = account.level,
                                                    curExp = account.curExp,
                                                    maxExp = account.maxExp,
                                                }
                                            }
                                        }
                                    ),
                                }
                            );
                            break;
                        }
                    case (uint)Aquatrax.eShopType.eShopType_Theme:
                        {
                            if (account.themeList.list.Any(theme => theme.themeId == data.itemId))
                            {
                                ServerLogger.LogError("You already have a theme with that ID!");
                                break;
                            }

                            var price = shopInfo.themeList.First(item => item.id == data.itemId).discountPrice;
                            if (account.currencyInfo.gold < price)
                            {
                                ServerLogger.LogError("You don't have enough money to make this purchase!");
                                break;
                            }

                            account.currencyInfo.gold -= (uint)price;
                            account.curExp += 20;
                            account.themeList.list.Add(new() { themeId = data.itemId });

                            while (account.curExp >= account.maxExp)
                            {
                                account.curExp -= account.maxExp;
                                account.level++;
                                account.maxExp = (uint)Math.Round(account.maxExp * 1.2f, 0);
                            }

                            Server.Database.UpdateAccount(account);
                            Server.Database.SaveAll();

                            Index.Instance.GatePackageQueue.Enqueue(
                                new Index.GamePackage()
                                {
                                    MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                                    ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ShopBuy,
                                    Data = Index.ObjectToByteArray(
                                        new cometScene.Ret_ShopBuy
                                        {
                                            settleData = new cometScene.SettleData()
                                            {
                                                changeList = { new() { type = 4, count = 1, id = data.itemId } },
                                                expData = new cometScene.PlayerExpData()
                                                {
                                                    level = account.level,
                                                    curExp = account.curExp,
                                                    maxExp = account.maxExp,
                                                }
                                            }
                                        }
                                    ),
                                }
                            );
                            break;
                        }
                }

                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ntf_CharacterFullData,
                        Data = Index.ObjectToByteArray(Gate.GetFullCharacterData(account)),
                    }
                );
            }
        },
        {
            (uint)cometScene.ParaCmd.ParaCmd_Req_ShopInfo,
            (_, _) =>
            {
                ServerLogger.LogInfo("Sending shop info...");
                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ShopInfo,
                        Data = Index.ObjectToByteArray(Server.PlaceholderServerData.GetShopInfo()),
                    }
                );
            }
        }
    };
}

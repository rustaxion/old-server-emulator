using Server.Emulator.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                charList = account.CharacterList,
                scoreList = account.scoreList,
                songList = account.songList,
            }
        };
    }

    public Gate()
    {
        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_SingleSongRank, (msgContent, _) =>
        {
            var data = Serializer.Deserialize<cometScene.Req_SingleSongRank>(new MemoryStream(msgContent));
            var ranks = new List<cometScene.SingleSongRankData>();

            foreach (var account in Server.Database.Accounts.Values.ToArray())
            {
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

                var singleSongInfo = difficultyList.Find(song => song.songId == data.songId);
                if (singleSongInfo == null) continue;

                ranks.Add(new()
                {
                    rank = 0,
                    charName = account.name,
                    score = singleSongInfo.score,
                    headId = account.headId,
                    charId = (ulong)account.charId,
                    teamName = account.team.teamName,
                    country = account.country,
                    titleId = account.titleId,
                });
            }

            ranks.Sort((x, y) => x.score.CompareTo(y.score));

            for (var i = 1; i <= ranks.Count; i++)
            {
                ranks[i - 1].rank = (uint)i;
            }

            var ranklist = new cometScene.Ret_SingleSongRank();
            ranklist.list.AddRange(ranks);

            if (ranklist.list.Count == 0)
            {
                ranklist.list.AddRange(new List<cometScene.SingleSongRankData>()
                {
                    new cometScene.SingleSongRankData
                    {
                        rank = 1,
                        charName = "No one is playing this song",
                        score = 3,
                        headId = 10010,
                        charId = 000000000,
                        teamName = "Server",
                        country = 1,
                        titleId = 10001
                    },
                    new cometScene.SingleSongRankData
                    {
                        rank = 2,
                        charName = "Come and compete for the top",
                        score = 2,
                        headId = 10010,
                        charId = 000000000,
                        teamName = "Server",
                        country = 1,
                        titleId = 10001
                    },
                    new cometScene.SingleSongRankData
                    {
                        rank = 3,
                        charName = "spot on the leaderboard!",
                        score = 1,
                        headId = 10010,
                        charId = 000000000,
                        teamName = "Server",
                        country = 1,
                        titleId = 10001
                    }
                });
            }

            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_SingleSongRank,
                Data = Index.ObjectToByteArray(ranklist),
            });
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_RankInfo, (msgContent, _) =>
        {
            // Progress: mostly done, must investigate an error from the game's code...

            ServerLogger.LogInfo($"Leaderboard");
            var data = Serializer.Deserialize<cometScene.Req_RankInfo>(new MemoryStream(msgContent));
            var rankInfo = new cometScene.Ret_RankInfo { type = data.type };
            var defaultRanks = new List<cometScene.TotalSongRankData> {
                new()
                {
                    rank = 1,
                    charName = "No one is playing right now.",
                    score = 2,
                    headId = 10010,
                    country = 1,
                    teamName = "Server",
                    titleId = 10001
                },
                new()
                {
                    rank = 2,
                    charName = "Come and be the first!",
                    score = 1,
                    headId = 10010,
                    country = 1,
                    teamName = "Server",
                    titleId = 10001
                }
            };

            var scores = new Dictionary<types.AccountData, uint>();

            switch (data.type)
            {
                case 0: // totalScore
                    {
                        foreach (var account in Server.Database.Accounts.Values.ToArray())
                        {
                            scores.Add(account, account.totalScore);
                        }
                        break;
                    }
                case 4: // preRank
                    {
                        // TODO: Figure out what this shit is.
                        rankInfo.list.AddRange(defaultRanks);
                        Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
                        {
                            MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                            ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_RankInfo,
                            Data = Index.ObjectToByteArray(rankInfo),
                        });
                        break;
                    }
                case 5: // total4KScore
                    {
                        foreach (var account in Server.Database.Accounts.Values.ToArray())
                        {
                            scores.Add(account, account.total4KScore);
                        }
                        break;
                    }
                case 6: // total6KScore
                    {
                        foreach (var account in Server.Database.Accounts.Values.ToArray())
                        {
                            scores.Add(account, account.total6KScore);
                        }
                        break;
                    }
                case 7: // total8KScore
                    {
                        foreach (var account in Server.Database.Accounts.Values.ToArray())
                        {
                            scores.Add(account, account.total8KScore);
                        }
                        break;
                    }
                case 8: // totalArcadeScore
                    {
                        foreach (var account in Server.Database.Accounts.Values.ToArray())
                        {
                            scores.Add(account, account.totalArcadeScore);
                        }
                        break;
                    }
                case 9: // preRank4k && preRank4kParam
                    {
                        // TODO: Figure out what this shit is.
                        rankInfo.list.AddRange(defaultRanks);
                        Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
                        {
                            MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                            ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_RankInfo,
                            Data = Index.ObjectToByteArray(rankInfo),
                        });
                        break;
                    }
                case 10: // preRank6k && preRank6kParam
                    {
                        // TODO: Figure out what this shit is.
                        rankInfo.list.AddRange(defaultRanks);
                        Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
                        {
                            MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                            ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_RankInfo,
                            Data = Index.ObjectToByteArray(rankInfo),
                        });
                        break;
                    }
                default:
                    {
                        rankInfo.list.AddRange(defaultRanks);
                        Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
                        {
                            MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                            ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_RankInfo,
                            Data = Index.ObjectToByteArray(rankInfo),
                        });
                        return;
                    }
            }

            var scoresList = scores.ToList();
            scoresList.Sort((x, y) => x.Value.CompareTo(y.Value));

            for (var i = 0; i < scoresList.Count; i++)
            {
                var score = scoresList[i];
                rankInfo.list.Add(new()
                {
                    rank = (uint)(i + 1),
                    charName = score.Key.name,
                    score = score.Value,
                    headId = score.Key.headId,
                    country = score.Key.country,
                    teamName = score.Key.team.teamName,
                    titleId = score.Key.titleId,
                });
            }

            if (rankInfo.list.Count == 0) rankInfo.list.AddRange(defaultRanks);
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_RankInfo,
                Data = Index.ObjectToByteArray(rankInfo),
            });
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_ChangeHeadIcon, (msgContent, sessionId) =>
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

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_SetFavorite, (msgContent, sessionId) =>
        {
            var data = Serializer.Deserialize<cometScene.Req_SetFavorite>(new MemoryStream(msgContent));
            ServerLogger.LogInfo($"Set Favourite: {data.songId}");
            
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_SetFavorite,
                Data = Index.ObjectToByteArray(new cometScene.Ret_SetFavorite{ songId = data.songId, isFavorite = data.isFavorite }),
            });

            var account = Server.Database.GetAccount(sessionId);
            if (account == null) return;
            var isFavorite = Convert.ToBoolean(data.isFavorite);

            if (account.songList.favoriteList.Contains(data.songId) && !isFavorite)
            {
                account.songList.favoriteList.Remove(data.songId);
            } else if (!account.songList.favoriteList.Contains(data.songId) && isFavorite)
            {
                account.songList.favoriteList.Add(data.songId);
            }

            Server.Database.SaveAll();
        });
        
        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_ChangeCharacter, (msgContent, sessionId) =>
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

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_ChangeTheme, (msgContent, sessionId) =>
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

        Handlers.Add((uint)cometGate.ParaCmd.ParaCmd_Ret_UserGameTime + 1000, (_, _) =>
        {
            ServerLogger.LogInfo($"Reported game time.");
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_BeginSong, (_, _) =>
        {
            ServerLogger.LogInfo($"Start playing song!");
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_FinishSong, (msgContent, sessionId) =>
        {
            ServerLogger.LogInfo($"Finished playing song!");
            var data = Serializer.Deserialize<cometScene.Req_FinishSong>(new MemoryStream(msgContent)).data;
            var account = Server.Database.GetAccount(sessionId);
            if (account == null) return;
            
            var character = account.CharacterList.list.Find(character => character.charId == account.selectCharId);
            if (character == null)
            {
                character = new()
                {
                    level = 1,
                    playCount = 1,
                    charId = account.selectCharId,
                };
                account.CharacterList.list.Add(character);
            }
            
            if (Convert.ToBoolean(data.playData.finishLevel))
            {
                character.playCount++;
                character.exp++;
            }

            account.totalScore = data.totalScore;
            account.total4KScore = data.total4KScore;
            account.total6KScore = data.total6KScore;
            account.total8KScore = data.total8KScore;

            Server.Database.UpdateAccount(account);

            var songInfo = data.playData;
            
            // EXP to reward
            var expToReward = 0u;
            if (songInfo.maxPercent == 100)
            {
                expToReward += (uint)Math.Round(account.curExp * 0.15f, 0);
            }
            
            expToReward += _modeLink[data.mode] switch
            {
                "4k" => 5,
                "6k" => 7,
                _ => 15
            };

            expToReward += _difficultyLink[data.difficulty] switch
            {
                "ez" => 5,
                "nm" => 10,
                _ => 15
            };

            var scoreList = _modeLink[data.mode] switch
            {
                "4k" => account.scoreList.key4List,
                "6k" => account.scoreList.key6List,
                "8k" => account.scoreList.key8List,
                _ => throw new NotSupportedException($"Mode `{_modeLink[data.mode]} ({data.mode})` not supported."),
            };

            var difficultyList = _difficultyLink[data.difficulty] switch
            {
                "ez" => scoreList.easyList,
                "nm" => scoreList.normalList,
                "hd" => scoreList.hardList,
                _ => throw new NotSupportedException($"Difficulty `{_difficultyLink[data.difficulty]} ({data.difficulty})` not supported."),
            };

            var singleSongInfo = difficultyList.Find(song => song.songId == data.songId);
            if (singleSongInfo == null || singleSongInfo.songId != data.songId)
            {
                singleSongInfo = new cometScene.SingleSongInfo() { songId = data.songId, playCount = 0 };
                difficultyList.Add(singleSongInfo);
            }
            
            uint goldGained = 0;
            var charExpGiven = 1;
            if (singleSongInfo.score == 0 && songInfo.accuracy > 50)
            {
                // First clear of the map with accuracy > 50%
                // so we reward the player with 100 currency
                goldGained += 100;
                
                // and we also give some good exp to the pilot/character
                character.exp += 5;
                charExpGiven += 5;
            }
            
            while (character.exp >= 30)
            {
                character.level++;
                character.exp -= 30;
                charExpGiven += 30;
            }
            
            if (singleSongInfo.score != 0 && songInfo.score > singleSongInfo.score)
            {
                expToReward += (uint)Math.Round(expToReward * 0.8f, 0);
            }
            
            if (songInfo.score > singleSongInfo.score) singleSongInfo.score = songInfo.score;

            singleSongInfo.miss = songInfo.miss;
            singleSongInfo.playCount++;
            singleSongInfo.isAllMax = songInfo.isAllMax;
            singleSongInfo.isFullCombo = (songInfo.miss == 0) ? 1u : 0u;
            singleSongInfo.finishLevel = songInfo.finishLevel;
            
            if (Convert.ToBoolean(singleSongInfo.isFullCombo))
            {
                expToReward += (uint)Math.Round(expToReward * 0.5f, 0);
            }

            if (Convert.ToBoolean(singleSongInfo.isAllMax))
            {
                expToReward += (uint)Math.Round(expToReward * 0.3f, 0);
            }
            
            DiscordRichPresence.Data.activity.Details = $"Finished {DiscordRichPresence.GameState.CurrentSong.name} - {DiscordRichPresence.GameState.CurrentSong.composer}";
            DiscordRichPresence.Data.activity.State = $"{DiscordRichPresence.GameState.Difficulty} ({DiscordRichPresence.GameState.DifficultyNumber.ToString()}) - {DiscordRichPresence.GameState.keyCount}";
            DiscordRichPresence.Data.Update();

            if (!Convert.ToBoolean(songInfo.finishLevel)) expToReward /= 2;

            account.curExp += expToReward;
            while (account.curExp >= account.maxExp)
            {
                account.level++;
                if (account.level % 5 == 0)
                {
                    goldGained += (uint)Math.Floor(Math.Pow(account.level, 0.8)) * 60;
                }
                account.curExp -= account.maxExp;
                account.maxExp = (uint)Math.Round(account.maxExp * 1.2f, 0);
            }

            account.currencyInfo.gold += goldGained;
            account.currencyInfo.diamond += goldGained;

            var settleData = new cometScene.SettleData
            {
                changeList = {
                    new() { type = 9, count = (int)expToReward, id = 0 },
                    new() { type = 3, count = charExpGiven, id = character.charId }
                },
                expData = new()
                {
                    level = account.level,
                    curExp = account.curExp,
                    maxExp = account.maxExp
                }
            };
            if (goldGained > 0)
            {
                settleData.changeList.Add(new()
                {
                    type = 1,
                    count = (int)goldGained,
                    id = 420,
                });
            }
            
            Server.Database.SaveAll();
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_FinishSong,
                Data = Index.ObjectToByteArray(new cometScene.Ret_FinishSong()
                {
                    songInfo = singleSongInfo,
                    settleData = settleData,
                }),
            });
        });
        
        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_Arcade_Finish, (msgContent, sessionId) =>
        {
            ServerLogger.LogInfo("Arcade Finish");
            var data = Serializer.Deserialize<cometScene.Req_Arcade_Finish>(new MemoryStream(msgContent));
            var totalScore = data.finishList.Select(x => (int)x.finishData.totalScore).Sum();
            
            var account = Server.Database.GetAccount(sessionId);
            if (account.totalArcadeScore < totalScore) account.totalArcadeScore = (uint)totalScore;

            foreach (var fData in data.finishList)
            {
                var finishData = fData.finishData;
                var playData = finishData.playData;
             
                var arcadeList = _modeLink[finishData.mode] switch
                {
                    "4k" => account.arcadeData.key4List,
                    "6k" => account.arcadeData.key6List,
                    "8k" => account.arcadeData.key8List,
                    _ => throw new NotSupportedException($"Mode `{_modeLink[finishData.mode]} ({finishData.mode})` not supported."),
                };

                var difficultyList = _difficultyLink[finishData.difficulty] switch
                {
                    "ez" => arcadeList.easyList,
                    "nm" => arcadeList.normalList,
                    "hd" => arcadeList.hardList,
                    _ => throw new NotSupportedException($"Difficulty `{_difficultyLink[finishData.difficulty]} ({finishData.difficulty})` not supported."),
                };
                
                var arcadeSongInfo = difficultyList.Find(song => song.songId == finishData.songId);
                if (arcadeSongInfo == null || arcadeSongInfo.songId != finishData.songId)
                {
                    arcadeSongInfo = new cometScene.ArcadeSongInfo { songId = finishData.songId };
                    difficultyList.Add(arcadeSongInfo);
                }

                arcadeSongInfo.miss = playData.miss;
                arcadeSongInfo.score = arcadeSongInfo.score;
                arcadeSongInfo.songId = finishData.songId;
            }

            Server.Database.SaveAll();
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_Arcade_Finish,
                Data = new byte[0],
            });
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_ChangeLanguage, (_, _) =>
        {
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ChangeLanguage,
                Data = new byte[0],
            });
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_ShopInfo, (_, _) =>
        {
            ServerLogger.LogInfo("Sent shop info.");
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ShopInfo,
                Data = Index.ObjectToByteArray(PlaceholderServerData.ShopInfo),
            });
        });

        Handlers.Add((uint)cometGate.ParaCmd.ParaCmd_LoginGateVerify + 1000, (msgContent, sessionId) =>
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

            ServerLogger.LogInfo($"LoginGateVerify: [{((userInfoList.userList.Count > 0) ? ("{ charId: " + account.charId + ", accStates: 0 }") : "")}]");
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometGate.MainCmd.MainCmd_Select,
                ParaCmd = (uint)cometGate.ParaCmd.ParaCmd_SelectUserInfoList,
                Data = Index.ObjectToByteArray(userInfoList),
            });
        });

        Handlers.Add((uint)cometGate.ParaCmd.ParaCmd_EnterGame + 1000, (_, sessionId) =>
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

        Handlers.Add((uint)cometGate.ParaCmd.ParaCmd_CreateCharacter + 1000, (msgContent, sessionId) =>
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
                list = {
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

            var characterFullData = GetFullCharacterData(account);
            ServerLogger.LogInfo(characterFullData.ToString());
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ntf_CharacterFullData,
                Data = Index.ObjectToByteArray(characterFullData),
            });
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_Event_Info, (_, _) =>
        {
            ServerLogger.LogInfo($"Shop information");
            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_Event_Info,
                Data = Index.ObjectToByteArray(PlaceholderServerData.EventInfo),
            });
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_ShopBuy, (msgContent, sessionId) =>
        {
            // TODO: Actually finish this.
            ServerLogger.LogInfo($"Buy item from shop.");
            var data = Serializer.Deserialize<cometScene.Req_ShopBuy>(new MemoryStream(msgContent));
            var account = Server.Database.GetAccount(sessionId);

            switch (data.shopType)
            {
                case (uint)Aquatrax.eShopType.eShopType_Character:
                    {
                        account.CharacterList.list.Add(new cometScene.CharData()
                        {
                            charId = data.itemId,
                            level = 1,
                            exp = 0,
                            playCount = 0,
                        });

                        account.level = 420;
                        account.curExp = 50;
                        account.maxExp = 100;

                        Server.Database.UpdateAccount(account);
                        Server.Database.SaveAll();
                        Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
                        {
                            MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                            ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_ShopBuy,
                            Data = Index.ObjectToByteArray(new cometScene.Ret_ShopBuy
                            {
                                settleData = new cometScene.SettleData()
                                {
                                    expData = new cometScene.PlayerExpData()
                                    {
                                        level = 420,
                                        curExp = 50,
                                        maxExp = 100,
                                    }
                                }
                            }),
                        });
                        break;
                    }
                case (uint)Aquatrax.eShopType.eShopType_Member:
                    {

                        break;
                    }
                case (uint)Aquatrax.eShopType.eShopType_Song:
                    {

                        break;
                    }
                case (uint)Aquatrax.eShopType.eShopType_Theme:
                    {

                        break;
                    }
            }
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_Social_PublishDynamics, (msgContent, _) =>
        {
            var data = Serializer.Deserialize<cometScene.Req_Social_PublishDynamics>(new MemoryStream(msgContent));
            ServerLogger.LogInfo($"Public Activity");

            var activity = new cometScene.Ret_Social_PublishDynamics();
            activity.contentList.AddRange(data.contentList);

            Index.Instance.GatePackageQueue.Enqueue(new Index.GamePackage()
            {
                MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_Social_PublishDynamics,
                Data = Index.ObjectToByteArray(activity),
            });
        });

        Handlers.Add((uint)cometScene.ParaCmd.ParaCmd_Req_Arcade_Info, (_, _) =>
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
                }).OrderBy(_ => rnd.Next()).ToArray();

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
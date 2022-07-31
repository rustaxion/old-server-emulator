using Server.Emulator.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serializer = ProtoBuf.Serializer;

namespace Server.Emulator.Handlers.GateHandlers;

public static class Beatmaps
{
    private static readonly Dictionary<uint, string> _modeLink = new() { { 1, "4k" }, { 2, "6k" }, { 3, "8k" } };
    private static readonly Dictionary<uint, string> _difficultyLink = new() { { 1, "ez" }, { 2, "nm" }, { 3, "hd" } };

    public static Dictionary<uint, Action<byte[], long>> Handlers = new()
    {
        {
            (uint)cometScene.ParaCmd.ParaCmd_Req_SingleSongRank,
            (msgContent, _) =>
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
                    if (singleSongInfo == null)
                        continue;

                    ranks.Add(
                        new()
                        {
                            rank = 0,
                            charName = account.name,
                            score = singleSongInfo.score,
                            headId = account.headId,
                            charId = (ulong)account.charId,
                            teamName = account.team.teamName,
                            country = account.country,
                            titleId = account.titleId,
                        }
                    );
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
                    ranklist.list.AddRange(
                        new List<cometScene.SingleSongRankData>()
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
                        }
                    );
                }

                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_SingleSongRank,
                        Data = Index.ObjectToByteArray(ranklist),
                    }
                );
            }
        },
        {
            (uint)cometScene.ParaCmd.ParaCmd_Req_RankInfo,
            (msgContent, _) =>
            {
                // Progress: mostly done, must investigate an error from the game's code...

                ServerLogger.LogInfo($"Leaderboard");
                var data = Serializer.Deserialize<cometScene.Req_RankInfo>(new MemoryStream(msgContent));
                var rankInfo = new cometScene.Ret_RankInfo { type = data.type };
                var defaultRanks = new List<cometScene.TotalSongRankData>
                {
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
                    default:
                        {
                            rankInfo.list.AddRange(defaultRanks);
                            Index.Instance.GatePackageQueue.Enqueue(
                                new Index.GamePackage()
                                {
                                    MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                                    ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_RankInfo,
                                    Data = Index.ObjectToByteArray(rankInfo),
                                }
                            );
                            return;
                        }
                }

                var scoresList = scores.ToList();
                scoresList.Sort((x, y) => x.Value.CompareTo(y.Value));

                for (var i = 0; i < scoresList.Count; i++)
                {
                    var score = scoresList[i];
                    rankInfo.list.Add(
                        new()
                        {
                            rank = (uint)(i + 1),
                            charName = score.Key.name,
                            score = score.Value,
                            headId = score.Key.headId,
                            country = score.Key.country,
                            teamName = score.Key.team.teamName,
                            titleId = score.Key.titleId,
                        }
                    );
                }

                if (rankInfo.list.Count == 0)
                    rankInfo.list.AddRange(defaultRanks);
                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_RankInfo,
                        Data = Index.ObjectToByteArray(rankInfo),
                    }
                );
            }
        },
        {
            (uint)cometScene.ParaCmd.ParaCmd_Req_BeginSong,
            (_, _) =>
            {
                ServerLogger.LogInfo($"Start playing song!");
            }
        },
        {
            (uint)cometScene.ParaCmd.ParaCmd_Req_BattleFieldInfo,
            (_, _) =>
            {
                ServerLogger.LogInfo($"Start playing song!");
            }
        },
        {
            (uint)cometScene.ParaCmd.ParaCmd_Req_FinishSong,
            (msgContent, sessionId) =>
            {
                ServerLogger.LogInfo($"Finished playing song!");
                var data = Serializer.Deserialize<cometScene.Req_FinishSong>(new MemoryStream(msgContent)).data;
                var account = Server.Database.GetAccount(sessionId);
                if (account == null)
                    return;

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

                if (Convert.ToBoolean(data.playData.life > 0))
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

                if (songInfo.score > singleSongInfo.score)
                    singleSongInfo.score = songInfo.score;

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
                DiscordRichPresence.Data.activity.State =
                    $"{DiscordRichPresence.GameState.Difficulty} ({DiscordRichPresence.GameState.DifficultyNumber}) - {DiscordRichPresence.GameState.keyCount}";
                DiscordRichPresence.Data.Update();

                if (!Convert.ToBoolean(songInfo.finishLevel))
                    expToReward /= 2;

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

                account.currencyInfo.diamond += goldGained;

                var settleData = new cometScene.SettleData
                {
                    changeList =
                    {
                        new()
                        {
                            type = 9,
                            count = (int)expToReward,
                            id = 0
                        },
                        new()
                        {
                            type = 3,
                            count = charExpGiven,
                            id = character.charId
                        }
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
                    settleData.changeList.Add(
                        new()
                        {
                            type = 1,
                            count = (int)goldGained,
                            id = 420,
                        }
                    );
                }

                Server.Database.SaveAll();
                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_FinishSong,
                        Data = Index.ObjectToByteArray(new cometScene.Ret_FinishSong() { songInfo = singleSongInfo, settleData = settleData, }),
                    }
                );
            }
        },
        {
            (uint)cometScene.ParaCmd.ParaCmd_Req_Arcade_Finish,
            (msgContent, sessionId) =>
            {
                ServerLogger.LogInfo("Arcade Finish");
                var data = Serializer.Deserialize<cometScene.Req_Arcade_Finish>(new MemoryStream(msgContent));
                var totalScore = data.finishList.Select(x => (int)x.finishData.totalScore).Sum();

                var account = Server.Database.GetAccount(sessionId);
                if (account.totalArcadeScore < totalScore)
                    account.totalArcadeScore = (uint)totalScore;

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
                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_Arcade_Finish,
                        Data = new byte[0],
                    }
                );
            }
        },
        {
            (uint)cometScene.ParaCmd.ParaCmd_Req_Arcade_Info,
            (_, _) =>
            {
                ServerLogger.LogInfo("Arcade Mode");
                var stageList = new List<cometScene.ArcadeStageData>();

                for (var i = 0; i < 3; i++)
                {
                    var rnd = new Random();

                    var arcadeInfoList = (
                        i switch
                        {
                            0 => Server.PlaceholderServerData.ArcadeInfoList.Item1,
                            1 => Server.PlaceholderServerData.ArcadeInfoList.Item2,
                            _ => Server.PlaceholderServerData.ArcadeInfoList.Item3,
                        }
                    ).OrderBy(_ => rnd.Next()).ToArray();

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

                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_Arcade_Info,
                        Data = Index.ObjectToByteArray(retArcadeInfo),
                    }
                );
            }
        }
    };
}

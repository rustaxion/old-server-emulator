using Aquatrax;
using ProtoBuf;
using Server.Emulator.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server.Emulator.Handlers.GateHandlers;

public static class CosmicTour
{
    private static bool HasCompletedMission(MissionType missionType, uint firstParam, uint secondParam, cometScene.PlayData playData)
    {
        var result = false;

        switch (missionType)
        {
            case MissionType.Accuracy:
                {
                    result = playData.accuracy >= firstParam;
                    break;
                }
            case MissionType.AccuracyLess:
                {
                    result = playData.accuracy < firstParam;
                    break;
                }
            case MissionType.AccuracyBetween:
                {
                    result = playData.accuracy >= firstParam && playData.accuracy <= secondParam;
                    break;
                }
            case MissionType.Score:
                {
                    result = playData.score > firstParam;
                    break;
                }
            case MissionType.MaxPercent:
                {
                    result = playData.maxPercent > firstParam;
                    break;
                }
            case MissionType.Combo:
                {
                    result = playData.combo >= firstParam;
                    break;
                }
            case MissionType.FinishLevel:
                {
                    result = playData.score >= firstParam;
                    break;
                }
            case MissionType.Miss:
                {
                    result = playData.miss <= firstParam;
                    break;
                }
            case MissionType.Just:
                {
                    result = playData.just <= firstParam;
                    break;
                }
            case MissionType.None:
                {
                    result = playData.life > 0;
                    break;
                }
            case MissionType.Life:
                {
                    if (firstParam >= 70u)
                    {
                        result = (playData.life / 100) == 100;
                    }
                    else if (firstParam >= 10u)
                    {
                        result = (playData.life / 100) > 13;
                    }
                    else
                    {
                        result = playData.life > 0;
                    }
                    break;
                }
        }

        return result;
    }

    public static Dictionary<uint, Action<byte[], long>> Handlers = new()
    {
        {
            (uint)cometScene.ParaCmd.ParaCmd_Req_Story_Info,
            (_, sessionId)=> {
                ServerLogger.LogInfo($"Cosmic Tour!");

                var account = Server.Database.GetAccount(sessionId);
                var storyInfo = new cometScene.Ret_Story_Info();
                account.cosmicTourData.ApplyToStoryInfo(ref storyInfo);

                storyInfo.list.AddRange(account.cosmicTourData.storyData);
                storyInfo.specialList.AddRange(account.cosmicTourData.specialStoryData);

                Index.Instance.GatePackageQueue.Enqueue(new()
                {
                    MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                    ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_Story_Info,
                    Data = Index.ObjectToByteArray(storyInfo)
                });
            }
        },
        {
            (uint)cometScene.ParaCmd.ParaCmd_Req_Story_Finish,
            (msgContent, sessionId)=> {
                ServerLogger.LogInfo($"Story: Mission finished!");
                var data = Serializer.Deserialize<cometScene.Req_Story_Finish>(new MemoryStream(msgContent));
                var account = Server.Database.GetAccount(sessionId);
                var planetData = GlobalConfig.getInstance().getStoryPlannetData();
                var chapter = planetData.FirstOrDefault(chapter => chapter.Id == data.data.chapterId);
                if (chapter == null) return;
                var level = chapter.Levels.FirstOrDefault(level => level.Sequence == data.data.levelId);
                if (level == null) return;
                var storyFinish = new cometScene.Ret_Story_Finish
                {
                    data = new()
                    {
                        chapterId = (uint)chapter.Id,
                        finishLevel = data.data.playData.finishLevel,
                        levelId = (uint)level.Sequence,
                        maxCombo = data.data.playData.combo,
                        maxScore = data.data.playData.score,
                        curRank = 1,
                    },
                };

                var settleData = new cometScene.SettleData();

                cometScene.StoryData storyData = null;
                if (chapter.Id >= types.CosmicTourData.DlcStart)
                {
                    storyData = account.cosmicTourData.specialStoryData.FirstOrDefault(story => story.chapterId == chapter.Id && story.curLevelId == level.Sequence)?.list.LastOrDefault();
                }
                else
                {
                    storyData = account.cosmicTourData.storyData.FirstOrDefault(story => story.chapterId == chapter.Id && story.levelId == level.Sequence);
                }

                if (storyData == null)
                {
                    storyData = new(){
                        chapterId = (uint)chapter.Id,
                        levelId = (uint)level.Sequence,
                        finishLevel = data.data.playData.finishLevel,
                        curRank = 0,
                    };
                    if (chapter.Id >= types.CosmicTourData.DlcStart)
                    {
                        account.cosmicTourData.specialStoryData.RemoveAll(story => story.chapterId == chapter.Id && story.curLevelId == level.Sequence);
                        var specialStoryData = new cometScene.SpecialStoryData();
                        specialStoryData.chapterId = (uint)chapter.Id;
                        specialStoryData.curLevelId = (uint)level.Sequence;
                        specialStoryData.list.Add(storyData);
                        account.cosmicTourData.specialStoryData.Add(specialStoryData);
                    }
                    else
                    {
                        account.cosmicTourData.storyData.RemoveAll(story => story.chapterId == chapter.Id && story.levelId == level.Sequence);
                        account.cosmicTourData.storyData.Add(storyData);
                    }
                }

                float missionsCompleted = 0;
                foreach (var mission in level.Missions)
                {
                    ServerLogger.LogInfo($"Mission loop: {level.Missions.IndexOf(mission)}");
                    if (!storyData.missionList.Contains(mission.id) && HasCompletedMission(mission.type, mission.param, mission.paramAdd, data.data.playData))
                    {
                        ServerLogger.LogInfo($"Mission '{mission.id}' completed!");
                        missionsCompleted++;
                        cometScene.eItemType rewardType = default;

                        ServerLogger.LogInfo($"Mission reward: {mission.reward.type}");
                        switch (mission.reward.type)
                        {
                            case "diamond": {
                                rewardType = cometScene.eItemType.eItemType_Diamond;
                                account.currencyInfo.diamond += mission.reward.count;
                                break;
                            }
                            case "song": {
                                rewardType = cometScene.eItemType.eItemType_Song;
                                account.songList.list.Add(new() { songId = mission.reward.id });
                                break;
                            }
                            case "character": {
                                rewardType = cometScene.eItemType.eItemType_Character;
                                account.CharacterList.list.Add(new() { charId = mission.reward.id, level=0, exp=0, playCount=0 });
                                break;
                            }
                            case "theme": {
                                rewardType = cometScene.eItemType.eItemType_Theme;
                                account.themeList.list.Add(new() { themeId = mission.reward.id });
                                break;
                            }
                            default: {
                                    continue;
                            }
                        }
                        ServerLogger.LogInfo(rewardType.ToString());

                        settleData.changeList.Add(new()
                        {
                             count = (int)mission.reward.count,
                             id = mission.reward.id,
                             type = (uint)rewardType,
                        });
                        storyFinish.data.missionList.Add(mission.id);
                        ServerLogger.LogInfo("Mission reward added!");
                    }
                }

                account.currencyInfo.gold += account.IncreaseExp((uint)Math.Round(missionsCompleted * 10f));
                settleData.expData = new()
                {
                    curExp = account.curExp,
                    level = account.level,
                    maxExp = account.maxExp
                };

                storyFinish.settleData = settleData;

                storyFinish.data.missionList.AddRange(storyData.missionList);
                storyData.missionList.RemoveAll(e => true);
                storyData.missionList.AddRange(storyFinish.data.missionList);

                if (storyData.maxCombo < data.data.playData.combo)
                {
                    storyData.maxCombo = data.data.playData.combo;
                }
                if (storyData.maxScore < data.data.playData.score)
                {
                    storyData.maxScore = data.data.playData.score;
                }

                account.cosmicTourData.ApplyToStoryFinish(ref storyFinish);

                Server.Database.UpdateAccount(account);
                Server.Database.SaveAll();

                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ret_Story_Finish,
                        Data = Index.ObjectToByteArray(storyFinish),
                    }
                );

                Index.Instance.GatePackageQueue.Enqueue(
                    new Index.GamePackage()
                    {
                        MainCmd = (uint)cometScene.MainCmd.MainCmd_Game,
                        ParaCmd = (uint)cometScene.ParaCmd.ParaCmd_Ntf_CharacterFullData,
                        Data = Index.ObjectToByteArray(Gate.GetFullCharacterData(account)),
                    }
                );
            }
        }
    };
}

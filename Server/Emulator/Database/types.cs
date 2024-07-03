using System.Collections.Generic;
using System.Linq;

namespace Server.Emulator.Database;

public class types
{
    public class CosmicTourData
    {
        private List<Aquatrax.StoryPlannetData> _planetData;

        public const uint DlcStart = 100;

        private uint latestChapterId = 1;
        private uint latestLevelId = 1;

        private uint latestSpecialChapterId = DlcStart;
        private uint latestSpecialLevelId = 1;

        private void Init()
        {
            if (_planetData == null) _planetData = Aquatrax.GlobalConfig.getInstance().plannetDataList;

            if (storyData.Count != 0)
            {
                try { latestChapterId = storyData.Max(story => story.chapterId); }
                catch (System.ArgumentNullException) { }

                try { latestLevelId = storyData.Max(level => level.levelId); }
                catch (System.ArgumentNullException) { }

                var chapter = _planetData.First(planet => planet.Id == latestChapterId);
                var level = chapter.Levels.First(level => level.Sequence == latestLevelId);

                if (chapter.Levels.IndexOf(level) == chapter.Levels.Count - 1)
                {
                    foreach (var planet in _planetData)
                    {
                        if ((planet.Id - 1) == latestChapterId)
                        {
                            latestChapterId = (uint)planet.Id;
                            latestLevelId = (uint)planet.Levels[0].Sequence;
                            break;
                        }
                    }
                }
                else
                {
                    latestLevelId++;
                }
            }

            if (specialStoryData.Count != 0)
            {
                try { latestSpecialChapterId = specialStoryData.Max(story => story.chapterId); }
                catch (System.ArgumentNullException) { }

                try { latestSpecialLevelId = specialStoryData.Max(level => level.curLevelId); }
                catch (System.ArgumentNullException) { }

                var spec_chapter = _planetData.First(planet => planet.Id == latestSpecialChapterId);
                var spec_level = spec_chapter.Levels.First(level => level.Sequence == latestSpecialLevelId);

                if (spec_chapter.Levels.IndexOf(spec_level) == spec_chapter.Levels.Count - 1)
                {
                    foreach (var planet in _planetData)
                    {
                        if ((planet.Id - 1) == latestSpecialChapterId)
                        {
                            latestSpecialChapterId = (uint)planet.Id;
                            latestSpecialLevelId = (uint)planet.Levels[0].Sequence;
                            break;
                        }
                    }
                }
                else
                {
                    latestSpecialLevelId++;
                }
            }
        }

        public void ApplyToStoryInfo(ref cometScene.Ret_Story_Info info)
        {
            Init();
            info.curNormalChapterId = latestChapterId;
            info.curNormalLevelId = latestLevelId;
            info.curTutorialChapterId = latestChapterId;
            info.curTutorialLevelId = latestLevelId;

            var specList = new cometScene.SpecialStoryData();
            specList.chapterId = latestSpecialChapterId;
            specList.curLevelId = latestSpecialLevelId;
            info.specialList.Add(specList);
        }

        public void ApplyToStoryFinish(ref cometScene.Ret_Story_Finish info)
        {
            Init();
            info.curNormalChapterId = latestChapterId;
            info.curNormalLevelId = latestLevelId;
            info.curTutorialChapterId = latestChapterId;
            info.curTutorialLevelId = latestLevelId;
            info.curSpecialChapterId = latestSpecialChapterId;
            info.curSpecialLevelId = latestSpecialLevelId;
        }

        public List<cometScene.StoryData> storyData = new() { };
        public List<cometScene.SpecialStoryData> specialStoryData = new() { };
    }

    public class AccountData
    {
        public uint accId;
        public string name;
        public uint headId;
        public string token;
        public uint country;
        public string steamId;
        public uint level = 1;
        public uint curExp = 0;
        public uint maxExp
        {
            get
            {
                if (level >= 30)
                    return 0;
                else
                    return (uint)(50 * System.Math.Pow(1.2, level - 1));
            }
        }
        public uint selectCharId;
        public uint language = 2;
        public uint sessionId = 0;
        public uint titleId = 10001;
        public uint totalScore = 0;
        public uint totalArcadeScore = 0;
        public uint onlineTime = 0;
        public uint selectThemeId = 1;
        public uint total4KScore = 0;
        public uint total6KScore = 0;
        public uint total8KScore = 0;
        public long charId = 0000000000;

        public uint IncreaseExp(uint exp)
        {
            if (level >= 30)
            {
                level = 30;
                curExp = 0;
                return 0;
            }

            curExp += exp;
            uint goldGained = 0;
            while (curExp >= maxExp)
            {
                level++;
                if (level % 5 == 0)
                {
                    goldGained += (uint)System.Math.Floor(System.Math.Pow(level, 0.8)) * 60;
                }
                if (level == 30)
                {
                    curExp = 0;
                    break;
                }
                curExp -= maxExp;
            }
            return goldGained;
        }

        public CosmicTourData cosmicTourData = new();

        public cometScene.TeamData team =
            new()
            {
                teamId = 0,
                teamName = "",
                uploadSongCount = 3,
                canUploadSong = 0,
            };

        public cometScene.ArcadeData arcadeData =
            new()
            {
                key4List = new cometScene.ArcadeDiffList(),
                key6List = new cometScene.ArcadeDiffList(),
                key8List = new cometScene.ArcadeDiffList(),
            };

        public cometScene.ScoreList scoreList =
            new()
            {
                key4List = new(),
                key6List = new(),
                key8List = new(),
            };

        public cometScene.PlayerVIPInfo vipInfo =
            new()
            {
                level = 0,
                exp = 0,
                levelUpExp = 100,
                inSubscription = 0,
            };

        public cometScene.ThemeList themeList = new() { list = { new() { themeId = 12 }, new() { themeId = 8 } } };

        public cometScene.PlayerCurrencyInfo currencyInfo =
            new()
            {
                gold = 0,
                diamond = 0,
                curStamina = 0,
                maxStamina = 10,
                honourPoint = 0,
            };

        public cometScene.SongList songList = new();
        public cometScene.CharacterList CharacterList = new()
        { list =
            {
                new() { charId = 50010 },
                new() { charId = 20020}
            }
        };
    }
}

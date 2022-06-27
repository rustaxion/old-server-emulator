using LitJson;
using System.Collections.Generic;
using System.Linq;

namespace Server.Emulator.Database;

public class types
{
    public class CosmicTourData
    {
        private List<uint> _currentData;
        private List<Aquatrax.StoryPlannetData> _planetData;

        private void Init()
        {
            if (_planetData == null) _planetData = Aquatrax.GlobalConfig.getInstance().plannetDataList;

            if (storyData.Count != 0)
            {
                uint latestChapterId = 1;
                uint latestLevelId = 1;


                try { latestChapterId = storyData.Max(story => story.chapterId); }
                catch (System.ArgumentNullException) { }

                try { latestLevelId = storyData.Where(story => story.chapterId == latestChapterId).Max(level => level.levelId); }
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

                _currentData = new() { latestChapterId, latestLevelId, latestChapterId, latestLevelId };
            }
            else _currentData = new() { 1, 1, 1, 1 };
        }

        public uint GetCurNormalChapterId()
        {
            Init();
            return _currentData[0];
        }

        public uint GetCurNormalLevelId()
        {
            Init();
            return _currentData[1];
        }

        public uint GetCurTutorialChapterId()
        {
            Init();
            return _currentData[2];
        }
        public uint GetCurTutorialLevelId()
        {
            Init();
            return _currentData[3];
        }

        public List<cometScene.StoryData> storyData = new() { };
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
        public uint maxExp = 50;
        public uint selectCharId;
        public uint language = 2;
        public uint sessionId = 0;
        public uint titleId = 1001;
        public uint totalScore = 0;
        public uint totalArcadeScore = 0;
        public uint onlineTime = 0;
        public uint selectThemeId = 1;
        public uint total4KScore = 0;
        public uint total6KScore = 0;
        public uint total8KScore = 0;
        public long charId = 0000000000;
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

        public cometScene.ThemeList themeList = JsonMapper.ToObject<cometScene.ThemeList>(
            "{ \"list\": " + JsonMapper.ToJson(Enumerable.Range(1, 14).Select(i => new cometScene.ThemeData { themeId = (uint)i }).ToList()) + "}"
        );

        public cometScene.PlayerCurrencyInfo currencyInfo =
            new()
            {
                gold = 0,
                diamond = 0,
                curStamina = 0,
                maxStamina = 10,
                honourPoint = 0,
            };

        public cometScene.SongList songList = JsonMapper.ToObject<cometScene.SongList>(
            "{ \"list\": "
                + JsonMapper.ToJson(
                    new int[]
                    {
                        80031,
                        80008,
                        80011,
                        80012,
                        80010,
                        80034,
                        80007,
                        80015,
                        80013,
                        80009,
                        80014,
                        80019,
                        80020,
                        80018,
                        63122,
                        63123,
                        63204,
                        62005,
                        62006,
                        63103,
                        69008,
                        68008,
                        68108,
                        80002,
                        64005,
                        69018,
                        68002,
                        68001,
                        82005,
                        82006,
                        82007,
                        82011,
                        65102,
                        68106,
                        64003,
                        62021,
                        65036
                    }
                        .Select(i => new cometScene.SongData { songId = (uint)i })
                        .ToList()
                )
                + @", ""favoriteList"": []"
                + "}"
        );

        public cometScene.CharacterList CharacterList;
    }
}

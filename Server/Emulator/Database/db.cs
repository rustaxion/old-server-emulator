using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable All

namespace Server.Emulator.Database;

public class Database
{
    public Dictionary<uint, types.AccountData> Accounts;

    private void LoadAccounts()
    {
        // validate the file
        Accounts = new();
        try
        {
            var json = UnityEngine.PlayerPrefs.GetString("ServerEmulator/UserDatabase");
            ParseAccounts(json);
        }
        catch (Exception e)
        {
            ServerLogger.LogError($"Error while reading the data base: {e.ToString()}");
            // Can't read the file, so we'll erase it
            UnityEngine.PlayerPrefs.SetString("ServerEmulator/UserDatabase", "[]");
            ServerLogger.LogInfo("UserDatabase: Not found or corrupted, created a new enty.");
        }
        ServerLogger.LogInfo($"Accounts loaded: {Accounts.Count}");
    }

    private void ParseAccounts(string json)
    {
        // Note: ReSharper thinks there are 24 errors here, when in reality there are none.
        // The code works fine, if you can fix the ambiguity warnings that appear as errors I would be thankful.
        var accounts = JsonMapper.ToObject<List<types.AccountData>>(json);
        var accs = JsonMapper.ToObject(json);

        for (var j = 0; j < accounts.Count; j++)
        {
            // I have to do this mumbo jumbo because LitJson doesn't properly construct lists that are within objects,
            // it does actually make the list, and if you use a debugger, you will see the items inside
            // the problem is that it is not properly instantiated, because the Count is 0 and the list itself thinks it's empty.
            // so you literally can't access the items in the list, and you just have an empty list pretty much
            // LitJson might have fixed this in a newer version, but I don't have the choice to upgrade the bundled version of LitJson
            // thank you for listening to my TED talk.
            // *clapping sounds*
            var acc = accs[j];
            var account = accounts[j];

            // ===========ThemeList==============
            account.themeList.list.AddRange(JsonMapper.ToObject<List<cometScene.ThemeData>>(acc["themeList"]["list"].ToJson()));
            // ===========CharacterList==============
            account.CharacterList.list.AddRange(JsonMapper.ToObject<List<cometScene.CharData>>(acc["CharacterList"]["list"].ToJson()));

            // ==========Team BuffList===============
            account.team.buffList.AddRange(JsonMapper.ToObject<List<cometScene.BuffData>>(acc["team"]["buffList"].ToJson()));
            // ==========scoreList===============
            account.scoreList.key4List.easyList.AddRange(JsonMapper.ToObject<List<cometScene.SingleSongInfo>>(acc["scoreList"]["key4List"]["easyList"].ToJson()));
            account.scoreList.key4List.normalList.AddRange(JsonMapper.ToObject<List<cometScene.SingleSongInfo>>(acc["scoreList"]["key4List"]["normalList"].ToJson()));
            account.scoreList.key4List.hardList.AddRange(JsonMapper.ToObject<List<cometScene.SingleSongInfo>>(acc["scoreList"]["key4List"]["hardList"].ToJson()));
            account.scoreList.key6List.easyList.AddRange(JsonMapper.ToObject<List<cometScene.SingleSongInfo>>(acc["scoreList"]["key6List"]["easyList"].ToJson()));
            account.scoreList.key6List.normalList.AddRange(JsonMapper.ToObject<List<cometScene.SingleSongInfo>>(acc["scoreList"]["key6List"]["normalList"].ToJson()));
            account.scoreList.key6List.hardList.AddRange(JsonMapper.ToObject<List<cometScene.SingleSongInfo>>(acc["scoreList"]["key6List"]["hardList"].ToJson()));
            account.scoreList.key8List.easyList.AddRange(JsonMapper.ToObject<List<cometScene.SingleSongInfo>>(acc["scoreList"]["key8List"]["easyList"].ToJson()));
            account.scoreList.key8List.normalList.AddRange(JsonMapper.ToObject<List<cometScene.SingleSongInfo>>(acc["scoreList"]["key8List"]["normalList"].ToJson()));
            account.scoreList.key8List.hardList.AddRange(JsonMapper.ToObject<List<cometScene.SingleSongInfo>>(acc["scoreList"]["key8List"]["hardList"].ToJson()));
            // ==========Arcade list===============
            account.arcadeData.key4List.easyList.AddRange(JsonMapper.ToObject<List<cometScene.ArcadeSongInfo>>(acc["arcadeData"]["key4List"]["easyList"].ToJson()));
            account.arcadeData.key4List.normalList.AddRange(JsonMapper.ToObject<List<cometScene.ArcadeSongInfo>>(acc["arcadeData"]["key4List"]["normalList"].ToJson()));
            account.arcadeData.key4List.hardList.AddRange(JsonMapper.ToObject<List<cometScene.ArcadeSongInfo>>(acc["arcadeData"]["key4List"]["hardList"].ToJson()));
            account.arcadeData.key6List.easyList.AddRange(JsonMapper.ToObject<List<cometScene.ArcadeSongInfo>>(acc["arcadeData"]["key6List"]["easyList"].ToJson()));
            account.arcadeData.key6List.normalList.AddRange(JsonMapper.ToObject<List<cometScene.ArcadeSongInfo>>(acc["arcadeData"]["key6List"]["normalList"].ToJson()));
            account.arcadeData.key6List.hardList.AddRange(JsonMapper.ToObject<List<cometScene.ArcadeSongInfo>>(acc["arcadeData"]["key6List"]["hardList"].ToJson()));
            account.arcadeData.key8List.easyList.AddRange(JsonMapper.ToObject<List<cometScene.ArcadeSongInfo>>(acc["arcadeData"]["key8List"]["easyList"].ToJson()));
            account.arcadeData.key8List.normalList.AddRange(JsonMapper.ToObject<List<cometScene.ArcadeSongInfo>>(acc["arcadeData"]["key8List"]["normalList"].ToJson()));
            account.arcadeData.key8List.hardList.AddRange(JsonMapper.ToObject<List<cometScene.ArcadeSongInfo>>(acc["arcadeData"]["key8List"]["hardList"].ToJson()));
            // ===========Song list==============
            account.songList.list.AddRange(JsonMapper.ToObject<List<cometScene.SongData>>(acc["songList"]["list"].ToJson()));
            account.songList.favoriteList.AddRange(JsonMapper.ToObject<List<uint>>(acc["songList"]["favoriteList"].ToJson()));
            // ===========Cosmic Tour============
            account.cosmicTourData.storyData = JsonMapper.ToObject<List<cometScene.StoryData>>(acc["cosmicTourData"]["storyData"].ToJson());
            for (var i = 0; i < account.cosmicTourData.storyData.Count; i++)
            {
                account.cosmicTourData.storyData[i].missionList.AddRange(JsonMapper.ToObject<List<uint>>(acc["cosmicTourData"]["storyData"][i]["missionList"].ToJson()));
            }
            foreach (var storyData in account.cosmicTourData.storyData)
            {
                var missions = storyData.missionList.ToList().Distinct();
                storyData.missionList.RemoveAll(e => true);
                storyData.missionList.AddRange(missions);
            }

            account.cosmicTourData.specialStoryData = JsonMapper.ToObject<List<cometScene.SpecialStoryData>>(acc["cosmicTourData"]["specialStoryData"].ToJson());
            for (var i = 0; i < account.cosmicTourData.specialStoryData.Count; i++)
            {
                for (var k = 0; k < account.cosmicTourData.specialStoryData[k].list.Count; k++)
                {
                    account.cosmicTourData.specialStoryData[i].list[k].missionList.AddRange(JsonMapper.ToObject<List<uint>>(acc["cosmicTourData"]["specialStoryData"][i]["list"][k]["missionList"].ToJson()));
                }
                foreach (var storyData in account.cosmicTourData.specialStoryData[i].list)
                {
                    var missions = storyData.missionList.ToList().Distinct();
                    storyData.missionList.RemoveAll(e => true);
                    storyData.missionList.AddRange(missions);
                }
            }

            Accounts[account.accId] = account;
        }
    }

    private void LoadAccountsLocal()
    {
        Accounts = new();

        try
        {
            var json = File.ReadAllText("ServerEmu_UserDatabase.json");
            ParseAccounts(json);
        }
        catch (Exception e)
        {
            ServerLogger.LogError($"Error while reading the data base: {e.ToString()}");
            // Can't read the file, so we'll erase it
            File.WriteAllText("ServerEmu_UserDatabase.json", "[]");
            ServerLogger.LogInfo("UserDatabase: Not found or corrupted, created a new enty.");
        }
        ServerLogger.LogInfo($"Accounts loaded: {Accounts.Count}");
    }

    public Database()
    {
        if (Server.Debug)
            LoadAccountsLocal();
        else
            LoadAccounts();
    }

    public types.AccountData CreateAccount()
    {
        var acc = new types.AccountData();
        acc.accId = Accounts.Keys.Count > 0 ? Accounts.Keys.ToArray().Max() + 1 : 1;
        return acc;
    }

    public types.AccountData GetAccount(uint accId)
    {
        if (Accounts.ContainsKey(accId))
        {
            return Accounts[accId];
        }
        return null;
    }

    public types.AccountData GetAccount(long sessionId)
    {
        foreach (var acc in Accounts)
        {
            if (acc.Value.sessionId == sessionId)
            {
                return acc.Value;
            }
        }
        return null;
    }

    public types.AccountData GetAccount(string steamId)
    {
        foreach (var acc in Accounts)
        {
            if (acc.Value.steamId == steamId)
            {
                return acc.Value;
            }
        }
        return null;
    }

    public void UpdateAccount(types.AccountData account)
    {
        Accounts[account.accId] = account;
        SaveAll();
    }

    public void SaveAll()
    {
        if (Server.Debug)
        {
            SaveAllLocal();
            return;
        }

        var writer = new JsonWriter { PrettyPrint = true, IndentValue = 2 };
        JsonMapper.ToJson(Accounts.Values.ToArray(), writer);
        UnityEngine.PlayerPrefs.SetString("ServerEmulator/UserDatabase", writer.ToString().Trim());
        UnityEngine.PlayerPrefs.Save();
    }

    private void SaveAllLocal()
    {
        // Save the accounts
        var writer = new JsonWriter { PrettyPrint = true, IndentValue = 2 };
        JsonMapper.ToJson(Accounts.Values.ToArray(), writer);
        File.WriteAllText("ServerEmu_UserDatabase.json", writer.ToString().Trim());
    }
}

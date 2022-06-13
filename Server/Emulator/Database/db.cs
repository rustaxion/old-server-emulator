using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LitJson;

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
            var accs = JsonMapper.ToObject<List<types.AccountData>>(json);
            foreach (var acc in accs)
            {
                Accounts.Add(acc.accId, acc);
            }
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

    private void LoadAccountsLocal()
    {
        Accounts = new();
        
        try
        {
            var json = File.ReadAllText("ServerEmu_UserDatabase.json");
            var accs = JsonMapper.ToObject<List<types.AccountData>>(json);
            foreach (var acc in accs)
            {
                Accounts.Add(acc.accId, acc);
            }
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
        if (Server.Debug) LoadAccountsLocal();
        else LoadAccounts();
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
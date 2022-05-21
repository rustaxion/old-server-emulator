using LitJson;
using System;
using System.Collections.Generic;
using System.IO;

namespace Server
{
    public class DataBase
    {
        // Due to some mono not wanting to load System.Data, I can't use sqlite
        // so sadly I will have to use json files as a database
        public class Datatypes
        {
            public class AccountData
            {
                public uint accId;
                public long sessionId = 0;
                public string steamId;
                public string token;
                public string charId = "0000000000";
                public string name;
                public int language = 2;
                public int country;
                public uint selectCharId;
                public uint headId;
                public int selectThemeId = 1;
                public uint totalScore = 0;
                public uint total4KScore = 0;
                public uint total6KScore = 0;
                public uint total8KScore = 0;
            }

            class CharacterData
            {
                public string charId;
            }
        }


        private static string dbPath = @$"{Directory.GetCurrentDirectory()}\ServerMod";
        private Dictionary<string, Datatypes.AccountData> Accounts;
        public long Session = (int)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

        public DataBase()
        {
            if (!Directory.Exists(dbPath))
            {
                Directory.CreateDirectory(dbPath);
            }

            // validate the file
            try
            {
                string json = File.ReadAllText(@$"{dbPath}\accounts.json");
                Accounts = JsonMapper.ToObject<Dictionary<string, Datatypes.AccountData>>(json);
            }
            catch
            {
                // Can't read the file, so we'll erase it
                File.WriteAllText(@$"{dbPath}\accounts.json", "{}");
                Accounts = new Dictionary<string, Datatypes.AccountData>();
            }
        }

        public Datatypes.AccountData GetAccount(string steamId)
        {
            if (Accounts.ContainsKey(steamId))
            {
                return Accounts[steamId];
            }
            else
            {
                return null;
            }
        }

        public Datatypes.AccountData GetAccount(uint accId)
        {
            Datatypes.AccountData account = null;
            // iterate over accounts
            foreach (var acc in Accounts)
            {
                if (acc.Value.accId == accId)
                {
                    account = acc.Value;
                }
            }
            return account;
        }

        public void SetAccount(string steamId, Datatypes.AccountData account)
        {
            Accounts[steamId] = account;
        }

        public uint GetAccountId(string steamId)
        {
            if (Accounts.ContainsKey(steamId))
            {
                return Accounts[steamId].accId;
            }
            else
            {
                uint accId = 0;

                foreach (var account in Accounts)
                {
                    if (account.Value.accId > accId)
                    {
                        accId = account.Value.accId;
                    }
                }

                return accId + 1;
            }
        }

        public void Save()
        {
            string json = JsonMapper.ToJson(Accounts);
            File.WriteAllText(@$"{dbPath}\accounts.json", json);
        }
    }
}
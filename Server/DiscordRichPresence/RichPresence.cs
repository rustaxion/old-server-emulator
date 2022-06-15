using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Discord;
namespace Server.DiscordRichPresence;

public class Data
{
    public static Discord.Discord discord;
    public static Discord.ActivityManager activityManager;
    public static Discord.ApplicationManager applicationManager;
    public static Discord.LobbyManager lobbyManager;
    static void UpdateActivity(Discord.Discord discord, string State, string Details)
    {
        var activityManager = discord.GetActivityManager();
        var lobbyManager = discord.GetLobbyManager();
        long epochTicks = new DateTime(1970, 1, 1).Ticks;
        long unixTime = ((DateTime.UtcNow.Ticks - epochTicks) / TimeSpan.TicksPerSecond);
        var activity = new Discord.Activity
        {
            State = State,
            Details = Details,

            Timestamps =
            {
                Start = unixTime
            },
            Assets =
            {
                LargeImage = "useful",
                LargeText = "Invaxion",
                SmallImage = "test",
                SmallText = "testing",
            },
            Party = {
               Id = null,
               Size = {
                    CurrentSize = 1,
                    MaxSize = 1
                },
            },
            Secrets = {
                Join = "",
            },
            Instance = true,
        };

        activityManager.UpdateActivity(activity, result =>
        {
            RichPresenceLogger.LogInfo("Update Activity  " + result);
        });

    }
    public static void Update()
    {
        if (discord == null)
        {
            discord = new Discord.Discord(979470905535250443, (UInt64)Discord.CreateFlags.Default);
            discord.SetLogHook(Discord.LogLevel.Debug, (level, message) =>
            {
                switch (level)
                {
                    case Discord.LogLevel.Error:
                        RichPresenceLogger.LogError(message);
                        break;
                    case Discord.LogLevel.Warn:
                        RichPresenceLogger.LogWarning(message);
                        break;
                    case Discord.LogLevel.Info:
                        RichPresenceLogger.LogInfo(message);
                        break;
                    case Discord.LogLevel.Debug:
                        RichPresenceLogger.LogDebug(message);
                        break;
                    default:
                        RichPresenceLogger.LogInfo(message);
                        break;
                }
            });
        }
        if (applicationManager == null)
        {
            applicationManager = discord.GetApplicationManager();
        }

        // Get the current locale. This can be used to determine what text or audio the user wants.
        RichPresenceLogger.LogInfo("Current Locale: " + applicationManager.GetCurrentLocale());
        // Get the current branch. For example alpha or beta.
        RichPresenceLogger.LogInfo("Current Branch: " + applicationManager.GetCurrentBranch());

        if (activityManager == null)
        {
            activityManager = discord.GetActivityManager();
            // activityManager.RegisterCommand("D:\\Directories\\Downloads\\INVAXION\\INVAXION.exe");
        }
        if (lobbyManager == null)
        {
            lobbyManager = discord.GetLobbyManager();
        }
        // Received when someone accepts a request to join or invite.
        // Use secrets to receive back the information needed to add the user to the group/party/match
        activityManager.OnActivityJoin += secret =>
        {
            RichPresenceLogger.LogInfo("OnJoin " + secret);
            lobbyManager.ConnectLobbyWithActivitySecret(secret, (Discord.Result result, ref Discord.Lobby lobby) =>
            {
                RichPresenceLogger.LogInfo("Connected to lobby: " + lobby.Id);
                lobbyManager.ConnectNetwork(lobby.Id);
                lobbyManager.OpenNetworkChannel(lobby.Id, 0, true);
                foreach (var user in lobbyManager.GetMemberUsers(lobby.Id))
                {
                    lobbyManager.SendNetworkMessage(lobby.Id, user.Id, 0,
                        Encoding.UTF8.GetBytes(String.Format("Hello, " + user.Username + "!")));
                }
                string gamePaused;
                if (false)
                {
                    gamePaused = "Paused";
                }
                else
                {
                    gamePaused = "Playing";
                }
                UpdateActivity(discord, gamePaused, "testing");
            });
        };

        // Pump the event look to ensure all callbacks continue to get fired.
        string gamePaused;
        if (false)
        {
            gamePaused = "Paused";
        }
        else
        {
            gamePaused = "Playing";
        }
        UpdateActivity(discord, gamePaused, "testing");
        try
        {
            discord.RunCallbacks();
        }
        catch (System.Exception e)
        {
            RichPresenceLogger.LogDebug(e.ToString());
            throw;
        }

    }
}
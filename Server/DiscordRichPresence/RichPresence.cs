using System;
using Discord;

namespace Server.DiscordRichPresence;

public class Data
{
    private static Discord.Discord _discord;
    public static ActivityManager _activityManager;
    private static ApplicationManager _applicationManager;
    private static LobbyManager _lobbyManager;
    public static Activity activity;
    private static bool _hasInit = false;
    private static readonly long StartTime = (DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).Ticks) / TimeSpan.TicksPerSecond;

    public static void Init()
    {
        _discord = new Discord.Discord(979470905535250443, (ulong)CreateFlags.Default);
        _discord.SetLogHook(
            LogLevel.Debug,
            (level, message) =>
            {
                switch (level)
                {
                    case LogLevel.Error:
                        RichPresenceLogger.LogError(message);
                        break;
                    case LogLevel.Warn:
                        RichPresenceLogger.LogWarning(message);
                        break;
                    case LogLevel.Debug:
                        RichPresenceLogger.LogDebug(message);
                        break;
                    case LogLevel.Info:
                    default:
                        RichPresenceLogger.LogInfo(message);
                        break;
                }
            }
        );

        _activityManager = _discord.GetActivityManager();
        _lobbyManager = _discord.GetLobbyManager();
        _applicationManager = _discord.GetApplicationManager();
        _lobbyManager = _discord.GetLobbyManager();

        // Get the current locale. This can be used to determine what text or audio the user wants.
        RichPresenceLogger.LogInfo("Current Locale: " + _applicationManager.GetCurrentLocale());
        // Get the current branch. For example alpha or beta.
        RichPresenceLogger.LogInfo("Current Branch: " + _applicationManager.GetCurrentBranch());
        ;
        activity = new Activity()
        {
            State = "Idle",
            Details = "",
            Assets = { LargeImage = "invaxion", LargeText = "音灵 INVAXION" },
            Timestamps = { Start = StartTime }
        };

        Update();
        _hasInit = true;
    }

    public static void Update(bool useInstanceTime = true, bool smallIcon = false)
    {
        if (useInstanceTime)
        {
            activity.Timestamps.End = default;
            activity.Timestamps.Start = StartTime;
        }

        if (!smallIcon)
        {
            activity.Assets.SmallImage = default;
            activity.Assets.SmallText = default;
        }

        _activityManager.UpdateActivity(
            activity,
            (
                result =>
                {
                    RichPresenceLogger.LogInfo("Update activity: " + result);
                }
            )
        );
    }

    public static void UpdateActivity()
    {
        if (!_hasInit)
            return;
        if (GameState.IsPaused && GameState.CurrentScene == "PlayView")
        {
            activity.State = "";
            activity.Details = "Paused";

            activity.Assets.SmallImage = "pause";
            activity.Assets.SmallText = "Paused";

            Update(true, true);
            return;
        }

        switch (GameState.CurrentScene)
        {
            case "MenuScene":
            {
                activity.Details = "In menu";
                activity.State = "";
                Update();
                break;
            }

            case "LoginScene":
            {
                activity.Details = "Logging in";
                activity.State = "";
                Update();
                break;
            }

            case "PC_newQuickPlayView":
            {
                activity.Details = "Selecting song";
                activity.State = $"{DiscordRichPresence.GameState.CurrentSong.name} - {DiscordRichPresence.GameState.CurrentSong.composer}";
                Update();
                break;
            }

            case "ActivityScene":
            {
                activity.Details = "Selecting song";
                activity.State = $"{DiscordRichPresence.GameState.CurrentSong.name} - {DiscordRichPresence.GameState.CurrentSong.composer}";
                Update();
                break;
            }

            case "PlayView":
            {
                var timeRemaining = (Aquatrax.ShortCut.PlayData.MusicLen - Aquatrax.ShortCut.PlayController.currentTime) / 1000;
                var currentTime = (DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).Ticks) / TimeSpan.TicksPerSecond;

                activity.Details = $"{GameState.CurrentSong.name} - {GameState.CurrentSong.composer}";
                activity.State = $"{GameState.Difficulty} ({GameState.DifficultyNumber.ToString()}) - {GameState.keyCount}";
                activity.Timestamps.End = currentTime + timeRemaining;
                activity.Timestamps.Start = default;

                activity.Assets.SmallImage = "play";
                activity.Assets.SmallText = "Playing";

                Update(false, true);
                break;
            }

            case "GameOverView":
            {
                Update();
                break;
            }

            case "ResultScene_Single":
            {
                Update();
                break;
            }

            default:
            {
                activity.Details = $"Listening to {DiscordRichPresence.GameState.CurrentSong.name} - {DiscordRichPresence.GameState.CurrentSong.composer}";
                activity.State = "";
                Update();
                break;
            }
        }
    }

    public static void Poll()
    {
        if (!_hasInit)
            return;
        try
        {
            _discord.RunCallbacks();
        }
        catch (System.Exception e)
        {
            RichPresenceLogger.LogInfo(e.ToString());
            _hasInit = false;
        }
    }
}

using Discord;
namespace Server.DiscordRichPresence;

public class Data
{
    private static Discord.Discord _discord;
    private static ActivityManager _activityManager;
    private static ApplicationManager _applicationManager;
    private static LobbyManager _lobbyManager;
    private static Activity _activity;
    private static bool _hasInit = false;

    public static void Init()
    {
        _discord = new Discord.Discord(979470905535250443, (ulong)CreateFlags.Default);
        _discord.SetLogHook(LogLevel.Debug, (level, message) =>
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
        });


        _activityManager = _discord.GetActivityManager();
        _lobbyManager = _discord.GetLobbyManager();
        _applicationManager = _discord.GetApplicationManager();
        _lobbyManager = _discord.GetLobbyManager();


        // Get the current locale. This can be used to determine what text or audio the user wants.
        RichPresenceLogger.LogInfo("Current Locale: " + _applicationManager.GetCurrentLocale());
        // Get the current branch. For example alpha or beta.
        RichPresenceLogger.LogInfo("Current Branch: " + _applicationManager.GetCurrentBranch());

        _activity = new Activity()
        {
            State = "Idle",
            Details = "",
            Assets =
            {
                LargeImage = "invaxion",
                LargeText = "音灵 INVAXION"
            }
        };

        Update();
        _hasInit = true;
    }

    private static void Update()
    {
        _activityManager.UpdateActivity(_activity, (result =>
        {
            RichPresenceLogger.LogInfo("Update activity: " + result);
        }));
    }

    public static void UpdateActivity()
    {
        if (!_hasInit) return;
        if (DiscordRichPresence.GameState.IsPaused)
        {
            _activity.State = "";
            _activity.Details = "Idle";
            _activityManager.UpdateActivity(_activity, result =>
            {
                RichPresenceLogger.LogInfo("Update activity: " + result);
            });
            Update();
            return;
        }
        else
        {
            _activity.Details = DiscordRichPresence.GameState.Difficulty + " - " + DiscordRichPresence.GameState.keyCount;
            _activity.State = DiscordRichPresence.GameState.CurrentSong.name + " - " + DiscordRichPresence.GameState.CurrentSong.composer;
        }

        Update();
    }

    public static void Poll()
    {
        if (!_hasInit) return;
        try
        {
            _discord.RunCallbacks();
        }
        catch (System.Exception e)
        {
            RichPresenceLogger.LogDebug(e.ToString());
            _hasInit = false;
        }
    }
}
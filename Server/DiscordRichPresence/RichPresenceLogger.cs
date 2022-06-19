namespace Server.DiscordRichPresence;

public static class RichPresenceLogger
{
    public static void LogInfo(string message)
    {
        Server.logger.LogInfo($"[RichPresence]: {message}");
    }

    public static void LogError(string message)
    {
        Server.logger.LogError($"[RichPresence]: {message}");
    }

    public static void LogWarning(string message)
    {
        Server.logger.LogWarning($"[RichPresence]: {message}");
    }

    public static void LogDebug(string message)
    {
        Server.logger.LogDebug($"[RichPresence]: {message}");
    }
}
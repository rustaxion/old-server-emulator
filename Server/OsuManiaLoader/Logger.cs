namespace Server.OsuManiaLoader;

public class Logger
{
    public static void LogInfo(string message)
    {
        Server.logger.LogInfo($"[OsuManiaLoader]: {message}");
    }

    public static void LogError(string message)
    {
        Server.logger.LogError($"[OsuManiaLoader]: {message}");
    }
}
namespace Server.Emulator;

public static class ServerLogger
{
    public static void LogInfo(string message)
    {
        Server.logger.LogInfo($"[ServerEmulator]: {message}");
    }

    public static void LogError(string message)
    {
        Server.logger.LogError($"[ServerEmulator]: {message}");
    }
}

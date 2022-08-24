using System.IO;
using System.Reflection;

namespace Server.NAudioWrapper;

public class NAudio
{
    private static NAudio _instance;
    public static NAudio Instance => _instance ??= new();

    public readonly string DepsFolder = Path.Combine(BepInEx.Paths.BepInExRootPath, "Deps");
    public Assembly NAudioAssembly;
    public bool HasLoaded = false;

    public NAudio()
    {
        if (!Directory.Exists(DepsFolder))
            Directory.CreateDirectory(DepsFolder);
    }

    public void Load()
    {
        return;
        if (File.Exists(Path.Combine(DepsFolder, "NAudio.dll")))
        {
            NAudioAssembly = Assembly.LoadFile(Path.Combine(DepsFolder, "NAudio.dll"));
            HasLoaded = true;
            return;
        }

        var dllLink = "https://github.com/Invaxion-Server-Emulator/invaxion-server-emulator/releases/download/v1.0.7/NAudio.dll";
        var dlReq = new Networking.Request(dllLink);
        Server.Instance.startCoroutine(dlReq.Download((request, success) =>
        {
            if (!success) return;
            File.WriteAllBytes(Path.Combine(DepsFolder, "NAudio.dll"), request._www.bytes);
            Load();
        }));
    }
}
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace Server.Emulator;

public class Index
{
    public static EagleTcp EagleTcpInstance;
    public static Index Instance
    {
        get { return _instance ??= new Index(); }
    }

    private Index()
    {
        EagleTcpPatches.HookManager.Instance.Create();
    }

    private static Index _instance;
    
    public static byte[] ObjectToByteArray(object obj)
    {
        if (obj == null)
            return null;

        using MemoryStream data = new();
        Serializer.Serialize(data, obj);
        
        return data.ToArray();
    }

    public class GamePackage
    {
        public uint MainCmd { get; set; }
        public uint ParaCmd { get; set; }
        public short DataLen { get; set; }
        public byte[] Data { get; set; }
    }

    private Handlers.Login Login = new();
    private Handlers.Gate Gate = new();
    
    public Queue<GamePackage> LoginPackageQueue = new();
    public Queue<GamePackage> GatePackageQueue = new();
    
    public bool Dispatch(uint mainCmd, uint paraCmd, byte[] msgContent, int tag)
    {
        ServerLogger.LogInfo($"Handler request! mainCmd: {mainCmd}, paraCmd: {paraCmd}");
        var sessionId = EagleTcpPatches.EagleTcpClient.Sessions[tag];
        
        bool found = false;
        found = tag == (int)EagleTcp.CSocketType.SOCKET_LOGIN ?
            Login.Dispatch(mainCmd, paraCmd, msgContent) :
            Gate.Dispatch(mainCmd, paraCmd, msgContent, sessionId);
        if (!found) ServerLogger.LogError($"Handler not found!");
        if (!found) Server.MustImplement.Add($"tag: {tag}, mainCmd: {mainCmd}, paraCmd: {paraCmd}");
        
        return found;
    }
}
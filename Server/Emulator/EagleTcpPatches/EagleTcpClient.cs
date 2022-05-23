using System;
using System.Collections.Generic;


namespace Server.Emulator.EagleTcpPatches;

public class EagleTcpClient
{
    private static Dictionary<int, bool> ConnectedClients = new();
    
    private EagleTcpClient()
    {
        ConnectedClients.Add(1, false);
        ConnectedClients.Add(2, false);
    }
    
    public static int SendCmd(int tag, uint mainCmd, uint paraCmd, byte[] msgContent, int size)
    {
        ServerLogger.LogInfo($"SendCmd: tag: {tag}, mainCmd: {mainCmd}, paraCmd: {paraCmd}, size: {size}");
        try
        {
            if (!IsConnected(tag))
                return (int)EagleStatus.ES_Disconnect;
            if (Index.Instance.Dispatch(mainCmd, paraCmd, msgContent, tag))
            {
                return (int)EagleStatus.ES_OK;
            }
            else
            {
                return (int)EagleStatus.ES_Disconnect;
            }
        }
        catch (Exception e)
        {
            Server.logger.LogError(e.Message + "\n" + e.StackTrace);
        }
        return (int)EagleStatus.ES_Disconnect;
    }
    
    public static int ParseCmd(int tag, ref int mainCmd, ref int paraCmd, byte[] msgContent)
    {
        if (tag == (int)EagleTcp.CSocketType.SOCKET_GATEWAY) ServerLogger.LogInfo($"ParseCmd: gate, gateQueueSize {Index.Instance.GatePackageQueue.Count}");
        if (!IsConnected(tag))
        {
            return -1;
        }

        var queue = tag == (int)EagleTcp.CSocketType.SOCKET_LOGIN ? Index.Instance.LoginPackageQueue : Index.Instance.GatePackageQueue;
        var socket = tag == (int)EagleTcp.CSocketType.SOCKET_LOGIN ? EagleTcp.loginSocket : EagleTcp.gateSocket;
        if (socket == null || queue.Count == 0) return 0;

        var gamePackage = queue.Dequeue();

        mainCmd = (int)gamePackage.MainCmd;
        paraCmd = (int)gamePackage.ParaCmd;

        ServerLogger.LogInfo($"EagleTcpInstance: {socket}");
        gamePackage.Data.CopyTo(socket.msgContentOut, 0);

        ServerLogger.LogInfo($"ParseCmd: tag: {tag}, mainCmd: {mainCmd}, paraCmd: {paraCmd}, size: {gamePackage.Data.Length}");
        return gamePackage.Data.Length;
    }
    
    private enum EagleStatus
    {
        ES_OK,
        ES_DescriptorPoolNull,
        ES_MsgNoNotFind,
        ES_DescriptorNull,
        ES_SerializeFailed,
        ES_CompressFailed,
        ES_SendFailed,
        ES_PrototypeMessageNull,
        ES_ParseDescFailed,
        ES_AddDescFailed,
        ES_CreateImporterFailed,
        ES_AlreadyCreateImporter,
        ES_UnSerializeFailed,
        ES_Disconnect
    }

    public static bool IsConnected(int tag)
    {
        return ConnectedClients.ContainsKey(tag) && ConnectedClients[tag];
    }
    
    public static bool ContectServer(string pszServerIP, int nServerPort, int tag)
    {
        ServerLogger.LogInfo("Connected to the '" + (tag == (int)EagleTcp.CSocketType.SOCKET_LOGIN ? "login" : "gate") + "' server");
        ConnectedClients[tag] = true;
        return true;
    }

    public static void DisconnectServer(int tag)
    {
        ServerLogger.LogInfo("Disconnected from the '" + (tag == (int)EagleTcp.CSocketType.SOCKET_LOGIN ? "login" : "gate") + "' server");
        ConnectedClients[tag] = false;
    }
}
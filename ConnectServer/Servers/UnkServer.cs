using Networking;
using System;
using Toolbelt;
using System.Net;


namespace Servers
{
    public static class UnkServer
    {
        public static TCPServer unkServer;
        private static int UnkConnectHandler(SessionTcpClient client)
        {
            Logger.Info("Unk Server Connect Handler");
            var addr = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            var port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;

            Console.WriteLine("STATUS CONNECT CLIENT INFO: {0} {1}", addr, port);
            return 1;
        }
        private static int UnkDataHandler(SessionTcpClient client, byte[] data, int Length)
        {
            Logger.Info("Unk Server Data Handler");
            //Logger.Log(Utility.ByteArrayToString(data));

            return 1;
        }
        private static int UnkDisconnectHandler(SessionTcpClient client)
        {
            Logger.Info("Unk Server Disconnect Handler");
            return 1;
        }
        public static void Initialize(string address, int port)
        {
            unkServer = new TCPServer("Unk Server", address, port, UnkConnectHandler, UnkDataHandler, UnkDisconnectHandler);
        }
    }
}

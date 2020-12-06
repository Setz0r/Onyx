using ConnectServer;
using Networking;
using System;
using System.Net;
using Toolbelt;


namespace Servers
{
    public static class LobbyServer
    {
        public static TCPServer lobbyServer;
        public static int _packetCount;

        private static int LobbyConnectHandler(SessionTcpClient client)
        {
            Logger.Info("Lobby Server Connect Handler");
            var addr = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            var port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;

            LoginSession session = SessionHandler.GetSessionByIP(addr.ToString(), SESSIONSTATUS.SYNCHRONIZING);

            session.Lobby_client = client;

            client.Session = session;

            Console.WriteLine("STATUS CONNECT CLIENT INFO: {0} {1}", addr, port);
            return 1;
        }
        private static int LobbyDataHandler(SessionTcpClient client, Byte[] data, int Length)
        {
            Logger.Info("Lobby Server Data Handler");
            //Logger.Log(Utility.ByteArrayToString(data));

            int result = Length;
            bool bIsNewChar = false;

            ByteRef recvBuf = new ByteRef(data, Length);

            byte temp = recvBuf.GetByte(0x04);

            recvBuf.Fill(0, 0x00, 32);

            switch (_packetCount)
            {
                case 0:
                    recvBuf.Set<byte>(0, 0x81);                    
                    var t = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    recvBuf.Set<uint>(0x14, t);
                    result = 24;
                    break;

                case 1:
                    if (temp != 0x28)
                        bIsNewChar = true;
                    recvBuf.Set<byte>(0x00, 0x28);
                    recvBuf.Set<byte>(0x04, 0x20);
                    recvBuf.Set<byte>(0x08, 0x01);
                    recvBuf.Set<byte>(0x0B, 0x7F);
                    result = bIsNewChar ? 144 : 24;
                    if (bIsNewChar) bIsNewChar = false;
                    break;
            }

            client.Session.LobbySend(recvBuf.Get(), result);

            _packetCount++;

            if (_packetCount == 3)
            {
                client.Client.Disconnect(false);
            }
            /* Echo back the buffer to the server.. */
            //if (send(client, (char*)recvBuffer, result, 0) == SOCKET_ERROR)
            //{
            //    xiloader::console::output(xiloader::color::error, "Client send failed: %d", WSAGetLastError());
            //    break;
            //}

            return 1;
        }
        private static int LobbyDisconnectHandler(SessionTcpClient client)
        {
            Logger.Info("Lobby Server Disconnect Handler");
            return 1;
        }
        public static void Initialize(string address, int port)
        {
            lobbyServer = new TCPServer("Lobby Server", address, port, LobbyConnectHandler, LobbyDataHandler, LobbyDisconnectHandler);
        }
    }
}

using Data.Structs;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Networking
{
    public class TCPServer
    {
        private TcpListener _server;
        private bool _isRunning;
        private string _serverName;

        private Func<SessionTcpClient, byte[], int, int> _datahandler;
        private Func<SessionTcpClient, int> _connecthandler;
        private Func<SessionTcpClient, int> _disconnecthandler;

        public TCPServer(string serverName, string address, int port, Func<SessionTcpClient, int> connecthandler, Func<SessionTcpClient, byte[], int, int> datahandler, Func<SessionTcpClient, int> disconnecthandler)
        {
            _serverName = serverName;
            IPAddress ipAddress = System.Net.IPAddress.Parse(address);
            _server = new TcpListener(ipAddress, port);
            _server.Start();
            _isRunning = true;
            _connecthandler = connecthandler;
            _datahandler = datahandler;
            _disconnecthandler = disconnecthandler;
            Thread t = new Thread(new ParameterizedThreadStart(LoopClients));
            t.Start();
        }

        private static ManualResetEvent sendDone = new ManualResetEvent(false);

        public static void Send(Socket handler, byte[] data)
        {
            // Begin sending the data to the remote device.  
            handler.BeginSend(data, 0, data.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public void CloseSocket(SessionTcpClient client)
        {
            _disconnecthandler(client);
            client.Client.Dispose();
            client.Client.Close();
        }

        public void LoopClients(object obj)
        {
            while (_isRunning)
            {
                // wait for client connection
                SessionTcpClient newClient = new SessionTcpClient(_server.AcceptTcpClient().Client);

                _connecthandler(newClient);
                Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                t.Start(newClient);
            }
        }

        private static bool IsSocketConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        public void HandleClient(object obj)
        {
            SessionTcpClient client = (SessionTcpClient)obj;
            NetworkStream stream = client.GetStream();
            Boolean bClientConnected = true;
            Byte[] bData = new Byte[1024];

            while (bClientConnected)
            {
                Array.Clear(bData, 0, bData.Length);
                if (client.IsDead || !IsSocketConnected(client.Client))
                {
                    CloseSocket(client);
                    bClientConnected = false;
                    break;
                }
                try
                {
                    if (stream.CanRead)
                    {
                        int bytesRead = stream.Read(bData, 0, bData.Length);

                        if (bytesRead > 0 && _datahandler(client, bData, bytesRead) == 0)
                        {
                            CloseSocket(client);
                            bClientConnected = false;
                        }
                    }
                }
                catch (SocketException)
                {
                    CloseSocket(client);
                    bClientConnected = false;
                }
                catch (System.IO.IOException)
                {
                    CloseSocket(client);
                    bClientConnected = false;
                }
            }
        }

    }
}

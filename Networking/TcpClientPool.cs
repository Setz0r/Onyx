using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Toolbelt;

namespace Networking
{
    public class TcpClientPool
    {
        private readonly int MinThreads;
        private readonly int MaxThreads;
        private string Server;
        private int Port;
        private int BufferSize;
        private List<TCPClient> Clients;
        private bool Active = false;
        private Thread CleanupThread;

        public TcpClientPool(string server, int port, int buffersize = 4096, int minThreads = 10, int maxThreads = 20)
        {
            Server = server;
            Port = port;
            BufferSize = buffersize;
            MinThreads = minThreads;
            MaxThreads = maxThreads;
            Clients = new List<TCPClient>();
        }

        public byte[] SendAndReceive(byte[] data)
        {
            byte[] received = null;
            bool completed = false;

            while (!completed)
            {
                foreach (var client in Clients)
                {
                    if (client.Status == CLIENTSTATUS.CONNECTED)
                    {
                        client.SetStatus(CLIENTSTATUS.BUSY);
                        client.Send(data);
                        int r = client.Receive(ref received, BufferSize);
                        completed = true;
                        client.SetStatus(CLIENTSTATUS.CONNECTED);
                        break;
                    }
                }
                if (!completed && Clients.Count < MaxThreads)
                {
                    Logger.Info("Reached capacity of open threads, adding new connection");
                    TCPClient client = new TCPClient();
                    client.Connect(Server, Port);
                    Clients.Add(client);
                }
            }

            return received;
        }

        public void Cleanup()
        {
            while (Active)
            {
                Thread.Sleep(30000);
                if (Clients.Count > MinThreads)
                {
                    int ClientsToRemove = Clients.Count - MinThreads;
                    foreach (var client in Clients)
                    {
                        if (client.Status == CLIENTSTATUS.CONNECTED)
                        {
                            Logger.Info("Terminating TCP Client Thread : {0}", new object[] { client.Client.Client.LocalEndPoint.ToString() });
                            client.SetStatus(CLIENTSTATUS.TERMINATING);
                            client.Disconnect();
                            Clients.Remove(client);
                            ClientsToRemove--;
                        }
                        if (ClientsToRemove == 0)
                            break;
                    }
                }
            }
        }

        public void Connect()
        {
            for (int i = 0; i < MinThreads; i++)
            {
                TCPClient client = new TCPClient();
                Clients.Add(client);
                Logger.Info("Adding connection thread");
            }
            foreach (var client in Clients)
            {
                client.Connect(Server, Port);
            }
            CleanupThread = new Thread(Cleanup);
            CleanupThread.Start();
            Active = true;
        }

        public void Disconnect()
        {
            foreach (var client in Clients)
            {
                client.SetStatus(CLIENTSTATUS.TERMINATING);
                client.Disconnect();
                Clients.Remove(client);
            }
        }

    }
}

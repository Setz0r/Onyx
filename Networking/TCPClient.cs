using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Toolbelt;

namespace Networking
{
    public enum CLIENTSTATUS
    {
        DISCONNECTED,
        CONNECTED,
        BUSY,
        TERMINATING
    }

    public class TCPClient
    {
       
        public TcpClient Client;
        public int Timeout;
        public CLIENTSTATUS Status;

        public bool Connect(string address, int port, int timeout = 10)
        {
            Status = CLIENTSTATUS.DISCONNECTED;

            Client = new TcpClient(address, port);
            Client.Client.SetSocketOption(SocketOptionLevel.Tcp,SocketOptionName.KeepAlive, false);
            Client.Client.SetSocketOption(SocketOptionLevel.Tcp,SocketOptionName.ReuseAddress, false);
            Client.Client.SetSocketOption(SocketOptionLevel.Tcp,SocketOptionName.Linger, false);

            Timeout = timeout;

            if (Client != null && Client.Connected)
            {
                Status = CLIENTSTATUS.CONNECTED;
                return true;
            }

            return false;
        }

        public void SetStatus(CLIENTSTATUS status)
        {
            Status = status;
        }

        public CLIENTSTATUS GetStatus()
        {
            return Status;
        }

        public bool Send(byte[] data)
        {
            if (Client.Connected)
            {
                using NetworkStream stream = Client.GetStream();
                stream.Write(data, 0, data.Length);
                return true;
            }
            return false;
        }

        public bool IsSocketConnected()
        {
            try
            {
                return !(Client.Client.Poll(1, SelectMode.SelectRead) && Client.Client.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        public int Receive(ref byte[] data, int maxLength)
        {
            int bytesReceived = 0;

            if (!Client.Connected)
                return bytesReceived;

            ByteRef outData = new ByteRef(0);

            using (NetworkStream stream = Client.GetStream())
            {
                uint starttime = Utility.Timestamp();
                while (!stream.DataAvailable)
                {
                    if (Utility.Timestamp() - starttime >= Timeout)
                        return bytesReceived;
                }
                int bytesRead;
                byte[] buffer = new byte[4096];
                while (stream.DataAvailable && (bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    outData.Append(buffer.Take(bytesRead).ToArray());
                }
            }
            data = outData.Get();

            return bytesReceived;
        }

        public void Disconnect()
        {
            Status = CLIENTSTATUS.DISCONNECTED;
            Client.Client.Disconnect(true);         
        }

    }
}

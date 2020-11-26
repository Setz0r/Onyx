using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Toolbelt;

namespace Networking
{
    public class UDPClient
    {
        private UDPServer server;
        private EndPoint endpoint;
        private UInt32 lastDataTime = 0;
        public byte[] dataBuffer;
        public int bufferSize;        

        public UDPClient(UDPServer serv, EndPoint ep)
        {
            server = serv;
            endpoint = ep;
            dataBuffer = new byte[4096];
            bufferSize = 0;
        }
        public int TrimData(int size)
        {
            dataBuffer = dataBuffer.Skip(size).Take(4096 - size).ToArray();
            bufferSize = Math.Max(bufferSize - size, 0);
            return bufferSize;
        }

        public int RecvData(byte[] data)
        {
            if (data.Length + bufferSize > 4096)
            {
                Logger.Error("Client data buffer maximum size breeched.");
            }
            else
            {
                data.CopyTo(dataBuffer, bufferSize);
                bufferSize += data.Length;
            }
            return bufferSize;
        }
        
        public int SendPacket(UDPServer server, byte[] data)
        {
            int bytesSent = 0;
            bytesSent = (int)server.Send(endpoint, data);
            return bytesSent;
        }
    }
}

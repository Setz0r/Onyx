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
        private UInt32 lastDataTime;
        public EndPoint endpoint;
        public byte[] dataBuffer;
        public int bufferSize;
        public Blowfish blowfish;

        public UDPClient(UDPServer serv, EndPoint ep)
        {
            server = serv;
            endpoint = ep;
            dataBuffer = new byte[4096];
            bufferSize = 0;
            lastDataTime = 0;
        }

        public void SetBlowfishKey(byte[] key)
        {
            blowfish = new Blowfish();
            Buffer.BlockCopy(key, 0, blowfish.key, 0, 20);
        }

        public void ResetBuffer()
        {
            dataBuffer = new byte[4096];
            bufferSize = 0;
        }

        public int RecvData(byte[] data)
        {
            if (data.Length > 4096)
            {
                Logger.Error("Client data buffer maximum size breeched.");
            }
            else
            {
                data.CopyTo(dataBuffer, 0);
                bufferSize = data.Length;
            }
            lastDataTime = Utility.Timestamp();
            return 0;
        }

        public int SendPacket(UDPServer server, byte[] data)
        {
            int bytesSent = 0;
            bytesSent = (int)server.Send(endpoint, data);
            return bytesSent;
        }
    }
}

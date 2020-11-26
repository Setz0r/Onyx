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
using NetCoreServer;

namespace Networking
{
    public delegate bool ReceivedCallback(UDPServer server, EndPoint endpoint, byte[] buffer, int offset, int size);

    public class UDPServer : UdpServer
    {        
        public UDPServer(IPAddress address, uint port, ReceivedCallback callback) : base(address, (int)port) { Callback = callback; }

        public ReceivedCallback Callback;

        protected override void OnStarted()
        {
            ReceiveAsync();
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            if (size == 0)
                return;
            Callback(this, endpoint, buffer, (int)offset, (int)size);
            ReceiveAsync();
        }

        protected override void OnSent(EndPoint endpoint, long sent)
        {
            // Continue receive datagrams
            ReceiveAsync();
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"UDP server caught an error with code {error}");
        }
    }
}

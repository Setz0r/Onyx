using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Core;
using NetMQ.Sockets;
using Toolbelt;

namespace Networking
{
    public class ZMQServer
    {
        public ResponseSocket socket;
        public NetMQPoller poller;
        readonly string address;

        public ZMQServer(string ip, int port)
        {
            address = string.Format("tcp://{0}:{1}", ip, port);
        }

        public bool Listen(Func<byte[], int, byte[]> callback)
        {
            try
            {
                socket = new ResponseSocket(address);
                
                poller = new NetMQPoller { socket };

                socket.ReceiveReady += (s, a) =>
                {   
                    byte[] data = a.Socket.ReceiveFrameBytes();
                    byte[] response = callback(data, data.Length);
                    if (response != null)
                    {
                        a.Socket.SendFrame(response);
                    }
                    else
                    {
                        a.Socket.SendFrame(new byte[] { 0 });
                    }
                };

                poller.RunAsync();
            }
            catch (Exception e)
            {
                Logger.Error("Error starting ZeroMQ server for address {0} : {1}", new object[] { address, e.Message });
                return false;
            }

            return true;
        }

        public void Close()
        {
            poller.StopAsync();
            socket.Dispose();
        }
    }
}

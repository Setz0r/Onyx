using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using Toolbelt;

namespace Networking
{
    public class ZMQClient
    {
        public RequestSocket socket;
        public NetMQPoller poller;
        string address;
        bool isAsync;

        public ZMQClient(string ip, int port)
        {
            address = string.Format("tcp://{0}:{1}", ip, port);
        }

        public bool Connect(bool async = false, Func<byte[], int, bool> callback = null)
        {
            try
            {
                isAsync = async;
                socket = new RequestSocket(address);
                if (async && callback != null)
                {
                    poller = new NetMQPoller { socket };
                    socket.ReceiveReady += (s, a) =>
                    {
                        byte[] data = a.Socket.ReceiveFrameBytes();
                        callback(data, data.Length);
                    };
                    poller.RunAsync();
                }
            }
            catch (NetMQException e)
            {
                Logger.Error("Error connecting ZeroMQ Socket to address : {0} : {1}", new object[] { address, e.Message });
            }
            return true;
        }

        public byte[] SendData(byte[] data)
        {
            try
            {
                socket.SendFrame(data);
                if (!isAsync)
                {
                    byte[] response = socket.ReceiveFrameBytes();
                    return response;
                }
            }
            catch (NetMQException e)
            {
                Logger.Error("Error sending data to ZeroMQ Socket address : {0} : {1}", new object[] { address, e.Message });
                Console.WriteLine();
            }
            return null;
        }

        public void Close()
        {
            poller.StopAsync();
            socket.Dispose();
        }
    }
}

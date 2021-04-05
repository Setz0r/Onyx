using Data.Game.Entities;
using Data.Game;
using Data.World;
using Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbelt;

namespace Game
{
    public class ZoneCluster
    {        
        public ZoneCluster()
        {
            zones = new ConcurrentDictionary<ZONEID, Zone>();
            clients = new ConcurrentDictionary<EndPoint, ZONEID>();
            zoneTasks = new List<Task>();
        }

        public bool Initialize()
        {

            return true;
        }

        public bool LoadZones()
        {
            return true;
        }

        public void ClusterLoop()
        {
            status = CLUSTERSTATUS.RUNNING;
            while (status != CLUSTERSTATUS.SHUTTINGDOWN)
            {
                Thread.Sleep(500);
            }
            Shutdown();
        }

        public bool Shutdown()
        {
            foreach (var zone in zones)
            {
                // TODO: create player loading/saving mechanic in this class

                //foreach (var player in zone.Value.Players) { }
                // TODO : save all players

                zone.Value.Shutdown();
            }
            return true;
        }

        public void Listen(string hostname = "127.0.0.1", uint portnum = 54240)
        {
            host = hostname;
            ip = Utility.IPToInt(host, true);
            port = portnum;
            ipp = ip | port << 32;
            server = new UDPServer(IPAddress.Parse(host), port, RecieveDataCallback);
            server.Start();
        }

        public bool RecieveDataCallback(UDPServer server, EndPoint endpoint, byte[] buffer, int offset, int size)
        {            
            if (size > 0)
            {
                Console.WriteLine(Utility.ByteArrayToString(buffer.Take(size).ToArray()));
                Player player = null;
                ZONEID playerZone = GetZoneIDByEndpoint(endpoint);
                
                if (playerZone == ZONEID.NONE)
                {
                    if (!clients.TryUpdate(endpoint, playerZone, ZONEID.NONE))
                    {
                        if (clients.TryAdd(endpoint, ZONEID.NONE)) { // temp
                            player = new Player();
                            player.Client = new UDPClient(server, endpoint);
                        }
                    }
                    // possible new connection
                    // TODO: check database for endpoint/zone
                }
                else
                {
                    player = GetPlayerByEndpoint(endpoint);
                    if (player == null)
                    {
                        player = zones[playerZone].InsertPlayer(server, endpoint);

                        if (player != null)
                        {

                        }
                        //else
                        //{
                        //    // player already logged in.
                        //}
                    }
                }

                if (player != null)
                {
                    string keybytes = "000000000000000000000000000000005CE05DAD";
                    byte[] key = Utility.StringToByteArray(keybytes);
                    player.Client.SetBlowfishKey(key);
                    player.Client.RecvData(buffer.Skip(offset).Take(size).ToArray());
                    ushort bytesProcessed = PacketHandler.ProcessPacket(player, player.Client.dataBuffer, size, this);
                    if (bytesProcessed > 0)
                    {
                        player.Client.ResetBuffer();
                    }
                }

                return true;
            }
            return false;
        }

        public ZONEID GetZoneIDByEndpoint(EndPoint ep)
        {
            if (clients.ContainsKey(ep))
                return clients[ep];
            return ZONEID.NONE;
        }

        public Player GetPlayerByEndpoint(EndPoint ep)
        {
            if (clients.ContainsKey(ep))
            {
                ZONEID zoneId = clients[ep];
                if (zones.ContainsKey(zoneId))
                {
                    var player = zones[zoneId].Players.FirstOrDefault(x => x.Value.Client.endpoint == ep);
                    if (!player.Equals(default(KeyValuePair<uint, Player>)))
                        return player.Value;
                }
            }

            return null;
        }

        public UDPServer server;
        public string host;
        public uint ip;
        public uint ipp;
        public uint port;

        public CLUSTERSTATUS status;

        public ConcurrentDictionary<ZONEID, Zone> zones;
        public ConcurrentDictionary<EndPoint, ZONEID> clients;
        public List<Task> zoneTasks;

    }
}

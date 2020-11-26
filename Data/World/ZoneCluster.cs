using Data.Entities;
using Data.Game;
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

namespace Data.World
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
                foreach (var player in zone.Value.players)
                    player.Value.Save();
                zone.Value.Shutdown();
            }
            return true;
        }

        public void Listen(string hostname = "127.0.0.1", uint portnum = 54230)
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
                ZONEID playerZone = GetZoneIDByEndpoint(endpoint);

                if (playerZone == ZONEID.NONE)
                {
                    // possible new connection
                    // @todo check database for endpoint/zone
                    clients.TryAdd(endpoint, ZONEID.BASTOK_MARKETS); // temp

                }
                else
                {
                    Player player = GetPlayerByEndpoint(endpoint);
                    if (player != null)
                    {
                        player.client.RecvData(buffer);
                        //player.client.SendPacket(server, buffer.Skip(offset).Take(size).ToArray());
                        return true;
                    }
                    else
                    {
                        player = zones[playerZone].InsertPlayer(server, endpoint);

                        //player = InsertPlayer(server, endpoint);
                        if (player != null)
                        {
                            ZONEID zoneId = ZONEID.BASTOK_MARKETS; // @todo: pull player zone location from database server
                            clients.TryAdd(endpoint, zoneId);
                            player.client.RecvData(buffer.Skip(offset).Take(size).ToArray());
                        }
                        //else
                        //{
                        //    // player already logged in.
                        //}
                    }
                }
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

                }
                //if (players.ContainsKey(playerID))
                //    return players[playerID];
                //else
                //    clients.TryRemove(ep, out playerID);
            }

            return null;
        }


        public UDPServer server;
        public string host;
        public UInt32 ip;
        public UInt32 ipp;
        public uint port;

        public CLUSTERSTATUS status;

        public ConcurrentDictionary<ZONEID, Zone> zones;
        public ConcurrentDictionary<EndPoint, ZONEID> clients;
        public List<Task> zoneTasks;

    }
}

using Data.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using Data.Game;
using Networking;
using Toolbelt;
using System.Net;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace Data.World
{
    public struct ZoneAnimationInfo
    {
        public byte direction;
        public byte animation;
        public UInt32 startTime;
        public UInt16 duration;
    }

    public class Zone
    {
        public Zone(ZONEID zoneId)
        {
            id = zoneId;
        }

        public bool Initialize()
        {
            // @TODO: load zone details from database server
            players = new ConcurrentDictionary<uint, Player>();
            npcs = new ConcurrentDictionary<uint, Npc>();

            Logger.Info("Zone loaded: {0}", new object[] { (int)id });
            return true;
        }

        public bool Shutdown()
        {
            Logger.Info("Zone {0} shutting down", new object[] { (int) id });
            Thread.Sleep(3000);
            foreach (var player in players)
            {
                player.Value.Save();
            }            
            Logger.Info("Zone {0} successfully shutdown", new object[] { (int)id });
            return true;
        }

        public bool Reset()
        {
            return true;
        }

        public Player GetPlayerByID(UInt32 playerID)
        {
            if (players.ContainsKey(playerID))
            {
                return players[playerID];
            }
            return null;
        }

        public Player InsertPlayer(UDPServer serv, EndPoint ep)
        {
            Player player = new Player
            {
                client = new UDPClient(serv, ep),
                status = ENTITYSTATUS.NORMAL,
                playerStatus = PLAYERSTATUS.REQUESTING_ZONE
            };

            //@todo load player from dbserver
            Random rand = new Random();
            if (players.TryAdd(12345 + (uint)rand.Next(0, 1000000), player))
            {
                Logger.Success("Player connected to Zone ID: {0} ", new object[] { (int)id });
            }
            else
            {
                Logger.Warning("Player attempted to connect to Zone ID {0} when already in zone", new object[] { (int)id });
                return null;
            }
            return player;
        }

        public ZONEID id;
        public string name;
        public ZONETYPE type;

        public REGION region;
        public CONTINENT continent;
        public WeatherInfo weather;

        public ConcurrentDictionary<UInt32, Player> players;
        public ConcurrentDictionary<UInt32, Npc> npcs;
    }
}

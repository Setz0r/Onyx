using Data.Game.Entities;
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
using MongoDB.Bson.Serialization.Attributes;

namespace Data.World
{
    [Serializable]
    public class ZoneAnimationInfo
    {
        public byte direction;
        public byte animation;
        public uint startTime;
        public ushort duration;
    }

    [Serializable]
    [BsonIgnoreExtraElements]
    public class Zone
    {
        public Zone(ZONEID zoneId)
        {
            ZoneId = zoneId;
        }

        public bool Initialize()
        {
            //  TODO: load zone details from database server
            Players = new ConcurrentDictionary<uint, Player>();
            Npcs = new ConcurrentDictionary<uint, Npc>();

            Logger.Info("Zone loaded: {0}", new object[] { (int)ZoneId });
            return true;
        }

        public bool Shutdown()
        {
            Logger.Info("Zone {0} shutting down", new object[] { (int)ZoneId });
            Thread.Sleep(3000);
            
            // TODO: handle zone shutdown tasks

            Logger.Info("Zone {0} successfully shutdown", new object[] { (int)ZoneId });
            return true;
        }

        public bool Reset()
        {
            return true;
        }

        public Player GetPlayerByID(uint playerID)
        {
            if (Players.ContainsKey(playerID))
            {
                return Players[playerID];
            }
            return null;
        }

        public Player InsertPlayer(UDPServer serv, EndPoint ep)
        {
            // TODO: load player from dbserver

            Player player = new Player
            {
                Client = new UDPClient(serv, ep),
                Status = ENTITYSTATUS.NORMAL,
                PlayerStatus = PLAYERSTATUS.REQUESTING_ZONE
            };

            Random rand = new Random();
            if (Players.TryAdd(1002, player))
            {
                Logger.Success("Player connected to Zone ID: {0} ", new object[] { (int)ZoneId });
            }
            else
            {
                Logger.Warning("Player attempted to connect to Zone ID {0} when already in zone", new object[] { (int)ZoneId });
                return null;
            }
            return player;
        }

        public ZONEID ZoneId;
        public string Name;
        public ZONETYPE ZoneType;

        public REGION Region;
        public CONTINENT Continent;
        public WeatherInfo Weather;

        public ZoneAnimationInfo AnimationInfo;

        public ConcurrentDictionary<uint, Player> Players;
        public ConcurrentDictionary<uint, Npc> Npcs;
    }
}

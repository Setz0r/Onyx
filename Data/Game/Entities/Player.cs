using Data.Game;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Networking;
using System.Text;
using Data.World;
using MongoDB.Bson.Serialization.Attributes;

namespace Data.Game.Entities
{
    public struct PlayerRecord
    {
        public ConcurrentDictionary<byte, byte> Jobs;
    }

    public struct PlayerStats
    {
        public UInt32 MaxHP;
        public UInt32 MaxMP;
        public byte Job;
        public byte JobLevel;
        public byte SubJob;
        public byte SubJobLevel;
        public UInt16 Exp;
        public UInt16 ExpTNL;
        public Stats BaseStats;
        public Stats BonusStats;
    }

    [Serializable]
    [BsonIgnoreExtraElements]
    public class Player : Combat, IQueryResult
    {
        public Player()
        {            
            EntityType = ENTITYTYPE.PC;
            SyncId = 1;
        }
        
        public byte Gender
        {
            get { return (byte)((Look.Model.Race) % 2 ^ ((Look.Model.Race > 6) ? 1 : 0)); }
        }

        public int ProcessPacketData()
        {
            int bytesProcessed = 0;
            if (Client != null && Client.bufferSize > 0)
            {
                //@todo process client data
            }
            return bytesProcessed;
        }

        public bool Load(UInt32 id)
        {
            return true;
        }

        public bool Save()
        {
            return true;
        }

        public UInt32 AccountId;

        public PlayerStats Stats;

        public UDPClient Client;
        public Zone CurrentZone;
        public Zone PreviousZone;

        public PlayerRecord Record;
        public UIntFlags NameFlags;

        public Linkshell Linkshell;
        public Linkshell Linkshell2;

        public UInt16 SyncId;
        public PLAYERSTATUS PlayerStatus;

    }
}

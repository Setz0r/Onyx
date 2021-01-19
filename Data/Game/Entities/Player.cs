using Data.Game;
using System;
using System.Collections.Generic;
using Networking;
using System.Text;
using Data.World;
using MongoDB.Bson.Serialization.Attributes;

namespace Data.Game.Entities
{
    [Serializable]
    [BsonIgnoreExtraElements]
    public class PlayerRecord
    {
        public Dictionary<byte, byte> Jobs { get; set; }
    }

    [Serializable]
    [BsonIgnoreExtraElements]
    public class PlayerStats
    {
        public uint MaxHP { get; set; }
        public uint MaxMP { get; set; }
        public byte Job { get; set; }
        public byte JobLevel { get; set; }
        public byte SubJob { get; set; }
        public byte SubJobLevel { get; set; }
        public ushort Exp { get; set; }
        public ushort ExpTNL { get; set; }
        public Stats BaseStats { get; set; }
        public Stats BonusStats { get; set; }
    }

    [Serializable]
    [BsonIgnoreExtraElements]
    public class ProfileInfo
    {
        public byte Nation { get; set; }
        public byte MoghouseFlags { get; set; }
        public ushort Title { get; set; }
        public ushort[] Fame { get; set; }
        public byte[] Rank { get; set; }
        public uint RankPoints { get; set; }
        public byte CampaignAllegience { get; set; }
        public ProfileInfo()
        {
            Fame = new ushort[15];
            Rank = new byte[3];
        }
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
                // TODO: process client data
            }
            return bytesProcessed;
        }

        public bool Load(uint id)
        {
            return true;
        }

        public bool Save()
        {
            return true;
        }

        public uint PlayerId { get; set; }
        public uint AccountId { get; set; }

        public ProfileInfo Profile { get; set; }
        public PlayerStats Stats { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public UDPClient Client { get; set; }
        [BsonIgnore]
        [field: NonSerialized]
        public Zone CurrentZone { get; set; }
        [BsonIgnore]
        [field: NonSerialized]
        public Zone PreviousZone { get; set; }

        public PlayerRecord Record { get; set; }
        public UIntFlags NameFlags { get; set; }

        public Linkshell Linkshell { get; set; }
        public Linkshell Linkshell2 { get; set; }

        public ushort SyncId { get; set; }
        public PLAYERSTATUS PlayerStatus { get; set; }

        public byte GMLevel { get; set; }

        public uint TimeCreate { get; set; }
        public uint TimeLastModify { get; set; }
    }
}

using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Game.Entities
{
    [Serializable]
    public class HealthInfo
    {
        public uint HP;
        public uint MP;
        public uint TP;
    }

    [Serializable]
    public class HealthDisplay
    {
        public byte hpPct;
        public byte mpPct;
    }

    [Serializable]
    public class JobDisplay
    {
        public byte job;
        public byte jobLevel;
        public byte subJob;
        public byte subJobLevel;
    }

    [Serializable]
    public class HealthUpdate
    {
        public HealthInfo healthInfo;

        public ushort targetId;
        
        public HealthDisplay healthDisplay;

        public uint Unknown1;
        public uint Unknown2;

        public JobDisplay jobDisplay;
    }

    [Serializable]
    public class Stats
    {
        public ushort STR;
        public ushort DEX;
        public ushort VIT;
        public ushort AGI;
        public ushort INT;
        public ushort MND;
        public ushort CHR;
    }

    [Serializable]
    [BsonIgnoreExtraElements]
    public class Combat : Entity
    {
        public Combat()
        {
        }

        public Stats baseStats { get; set; }
        public Stats statBonus { get; set; }
    }
}

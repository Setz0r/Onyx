using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Game.Entities
{    
    public struct HealthInfo
    {
        public UInt32 HP;
        public UInt32 MP;
        public UInt32 TP;
    }

    public struct HealthDisplay
    {
        public byte hpPct;
        public byte mpPct;
    }

    public struct JobDisplay
    {
        public byte job;
        public byte jobLevel;
        public byte subJob;
        public byte subJobLevel;
    }

    public struct HealthUpdate
    {
        public HealthInfo healthInfo;

        public UInt16 targetId;
        
        public HealthDisplay healthDisplay;

        public UInt32 Unknown1;
        public UInt32 Unknown2;

        public JobDisplay jobDisplay;
    }

    public struct Stats
    {
        public UInt16 STR;
        public UInt16 DEX;
        public UInt16 VIT;
        public UInt16 AGI;
        public UInt16 INT;
        public UInt16 MND;
        public UInt16 CHR;
    }


    public class Combat : Entity
    {
        public Combat()
        {
        }

        public Stats baseStats;
        public Stats statBonus;
    }
}

using Data.Game;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Data.Entities
{
    public struct PlayerRecord
    {
        public ConcurrentDictionary<byte, byte> Jobs;
    }

    public struct PlayerStats
    {
        public UInt32 maxHP;
        public UInt32 maxMP;
        public byte job;
        public byte jobLevel;
        public byte subJob;
        public byte subJobLevel;
        public UInt16 exp;
        public UInt16 expTNL;
        public Stats baseStats;
        public Stats bonusStats;
    }

    public class Player : Combat
    {
        public Player()
        {            
            type = ENTITYTYPE.PC;
            syncId = 1;
        }
        
        public byte Gender()
        {
            return (byte)((look.model.race) % 2 ^ ((look.model.race > 6) ? 1 : 0));
        }

        public PlayerRecord record;
        public UIntFlags nameFlags;

        public Linkshell linkshell;
        public Linkshell linkshell2;

        public UInt16 syncId;
    }
}

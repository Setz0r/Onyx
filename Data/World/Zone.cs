using Data.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using Data.Game;

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
        public ZONEID id;
        public string name;
        public ZONETYPE type;

        public UInt32 ip;
        public UInt16 port;

        public REGION region;
        public CONTINENT continent;
        public WeatherInfo weather;

        public ConcurrentDictionary<UInt32, Npc> npcs;
        public ConcurrentDictionary<UInt32, Player> players;
    }
}

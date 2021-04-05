using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbelt
{
    public class VanaTime
    {
        public const uint BASE  = 1009810800;
        public const uint YEAR  = 518400;
        public const uint MONTH = 43200;
        public const uint WEEK  = 11520;
        public const uint DAY   = 1440;
        public const uint HOUR  = 60;

        public TYPE TimeType = TYPE.NONE;        
        public uint PrevHour = 24;

        public enum DAYS : byte
        {
            FIRESDAY,
            EARTHSDAY,
            WATERSDAY,
            WINDSDAY,
            ICEDAY,
            LIGHTNINGDAY,
            LIGHTSDAY,
            DARKSDAY
        }

        public enum TYPE : byte
        {
            NONE,
            MIDNIGHT,
            NEWDAY,
            DAWN,
            DAY,
            DUSK,
            EVENING,
            NIGHT
        }

        private VanaTime() { }

        public TYPE Sync()
        {
            uint hour = Hour;
            if (hour != PrevHour)
            {
                PrevHour = hour;
                switch (hour)
                {
                    case 0: { TimeType = TYPE.NIGHT;   return TYPE.MIDNIGHT; }
                    case 4: { TimeType = TYPE.NEWDAY;  return TYPE.NEWDAY; }
                    case 6: { TimeType = TYPE.DAWN;    return TYPE.DAWN; }
                    case 7: { TimeType = TYPE.DAY;     return TYPE.DAY; }
                    case 17:{ TimeType = TYPE.DUSK;    return TYPE.DUSK; }
                    case 18:{ TimeType = TYPE.EVENING; return TYPE.EVENING; }
                    case 20:{ TimeType = TYPE.NIGHT;   return TYPE.NIGHT; }
                }
            }

            return TYPE.NONE;
        }

        public static VanaTime Instance { get; } = new VanaTime();
        public static VanaTime GetInstance()
        {
            return Instance;
        }

        public uint GetTimestamp()
        {
            return Utility.Timestamp() - BASE;
        }

        public uint VanaDate { get { return (uint)(GetTimestamp() / 60.0 * 25) + 886 * YEAR; } }
        public uint Year { get { return VanaDate / YEAR; } }
        public uint Month { get { return ((VanaDate / MONTH) % 12) + 1; } }
        public uint Day { get { return ((VanaDate / DAY) % 30) + 1; } }
        public uint WeekDay { get { return (VanaDate % WEEK) / DAY; } }
        public uint Hour { get { return (VanaDate % DAY) / HOUR; } }
        public uint Minute { get { return VanaDate % HOUR; } }

    }
}

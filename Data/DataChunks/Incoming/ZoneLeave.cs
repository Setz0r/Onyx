using Data.Game.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Toolbelt;

namespace Data.DataChunks.Incoming
{
    //
    // Purpose:  Received when leaving a zone
    //
    // Example Client Sent Data (0x00D) (length:8)
    //     0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F
    //    -----------------------------------------------
    // 0: 0D 04 FC 01 00 00 86 3F                               
    //
    // Notes: This client will send this immediately after receiving a zone out request 0x0B
    //

    [StructLayout(LayoutKind.Explicit)]
    public struct ZoneLeaveData
    {
        [FieldOffset(0)]
        public ChunkHeader header;
        [FieldOffset(4)]
        public ushort empty;   // note, client sets this to 0
        [FieldOffset(6)]
        public ushort unknown; // note, this doesn't appear to be set by the client, 
                               // could be memory garbage although some of it seems to be the same amongst some packets
    }

    public class ZoneLeave : BaseChunk
    {
        public const int MinSize = 0x0;
        public const int MaxSize = 0xFF;

        public bool Validator(ZoneLeaveData data)
        {
            if (data.empty != 0) 
                return false;

            Logger.Success("we got 0x00D");

            return true;
        }

        public bool Handler(Player player, byte[] bytes)
        {
            if (bytes.Length < MinSize || bytes.Length > MaxSize)
                return false;
            
            ZoneLeaveData ZoneLeaveData = Utility.Deserialize<ZoneLeaveData>(bytes);
            if (Validator(ZoneLeaveData))
            {
                // TODO: Handle the player zoning out (save/handle loot drops, party stuff, mob hate/follow, etc)

                return true;
            }
            return false;
        }

        private ZoneLeave() { }

        public static ZoneLeave Instance { get; } = new ZoneLeave();
        public static ZoneLeave GetInstance()
        {
            return Instance;
        }

    }
}

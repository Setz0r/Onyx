using Data.Game.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Toolbelt;

namespace Data.DataChunks.Incoming
{
    //
    // Purpose:  Received when zoning has been successful
    //
    // Example Client Sent Data (0x011) (length:8)
    //     0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F
    //    -----------------------------------------------
    // 0: 11 04 02 00 02 00 00 00                              
    //
    // Notes: This is apparently the go ahead to send equipment info (0x50) chunks
    //

    [StructLayout(LayoutKind.Explicit)]
    public struct ZoneSuccessData
    {
        [FieldOffset(0)]
        public ChunkHeader header;
        [FieldOffset(4)]
        public uint confirmation; // note, have not researched what this does, but it always appears to be 2
    }

    public class ZoneSuccess : BaseChunk
    {
        public const int MinSize = 0x0;
        public const int MaxSize = 0xFF;

        public bool Validator(ZoneSuccessData data)
        {
            if (data.confirmation != 2)
                return false;

            Logger.Success("we got 0x011");

            return true;
        }

        public bool Handler(Player player, byte[] bytes)
        {
            if (bytes.Length < MinSize || bytes.Length > MaxSize)
                return false;

            ZoneSuccessData zoneSuccessData = Utility.Deserialize<ZoneSuccessData>(bytes);
            if (Validator(zoneSuccessData))
            {
                // TODO: Handle the packet by sending equipment info (0x50) chunks                

                return true;
            }
            return false;
        }

        private ZoneSuccess() { }

        public static ZoneSuccess Instance { get; } = new ZoneSuccess();
        public static ZoneSuccess GetInstance()
        {
            return Instance;
        }
    }
}

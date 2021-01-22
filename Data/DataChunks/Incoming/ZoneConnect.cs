using Data.Game.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Toolbelt;

namespace Data.DataChunks.Incoming
{
    //
    // Purpose:  Requesting entry into a zone. This is the very first UDP packet chunk sent to the zone server by the game client.
    //           It is the alpha chunk.
    //
    // Example Client Sent Data (0x0A) (length:92)
    //     0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F
    //    -----------------------------------------------
    // 0: 0A 2E 01 00 30 00 01 00 00 00 00 00 44 55 00 00
    // 1: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
    // 2: 00 00 20 00 00 00 00 00 00 00 00 00 00 00 00 00
    // 3: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
    // 4: 20 09 70 E4 1C BE 86 16 AC EA 95 F9 83 9B 80 D1
    // 5: 00 00 00 00 57 49 4E 00 01 00 01 01
    //
    // Notes:
    //
    // Data[0x04] byte is some sort of checksum value based on adding all the bytes from 0x8 to 0x5C(end)
    // Data[0x06] byte seems to be set to 1 when a character logs out and logs back in without restarting the game client
    // Data[0x0C] uint32 is the character id
    // Data[0x4?] string is possibly a version string, would need captures from other versions to know for sure
    // Data[0x54] string(3 bytes) is "WIN"
    // Data[0x5A] and Data[0x5B] go through some sort of client verifications and are set to a positive value, -1, or -2 depending on currently unknown circumstances 
    //

    [StructLayout(LayoutKind.Explicit)]
    public struct ZoneConnectData
    {
        [FieldOffset(0)]
        public ChunkHeader header;
        [FieldOffset(12)]
        public uint id;           // character id, for now this is the only important information we need.
    }

    public sealed class ZoneConnect : BaseChunk
    {
        public bool Validator(ZoneConnectData data)
        {
            if (data.id > 65535 ) //  TODO: validate player id exists and is currently logged in
                return false;

            Logger.Success("we got 0x00A");

            return true;
        }

        public bool Handler(Player player, byte[] bytes)
        {
            ZoneConnectData zoneConnectData = Utility.Deserialize<ZoneConnectData>(bytes);
            if (Validator(zoneConnectData))
            {
                // TODO: Handle the packet
                return true;
            }
            return false;
        }

        private ZoneConnect() { }

        public static ZoneConnect Instance { get; } = new ZoneConnect();
        public static ZoneConnect GetInstance()
        {
            return Instance;
        }
    }
}

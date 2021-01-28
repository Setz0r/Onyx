using Data.Game.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.DataChunks.Outgoing
{
    // Example Data:
    //      |  0  1  2  3   4  5  6  7   8  9  A  B   C  D  E  F 
    // -----+----------------------------------------------------
    //    0 | 0B 0E B3 04  02 00 00 00  7C 96 98 71  E9 D3 00 00 
    //   10 | 75 93 E8 C0  F6 28 D4 40  DA 00 00 00              
    //
    // Note: 0x10 through 0x1B seem to be useless, but there are a few packets both of type 1 and 2 that will sometimes have this data populated.
    //       Client investigation is proving a bit challenging in discovering what this data is parsed out as.

    public class ZoneOut : BaseChunk
    {
        public ZoneOut(Player player, uint type, uint ip, ushort port)
        {
            id = 0x00D;
            size = 0x0E;

            data.Set<uint>(0x04, type);
            data.Set<uint>(0x08, ip);
            data.Set<ushort>(0x0C, port);

            Complete();
        }
    }
}

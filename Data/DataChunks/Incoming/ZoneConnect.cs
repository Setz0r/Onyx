using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Toolbelt;

namespace Data.DataChunks.Incoming
{
    public class ZoneConnect : BaseChunk
    {

        [StructLayout(LayoutKind.Explicit)]
        public struct ZoneConnectData
        {
            [FieldOffset(0)]
            public BaseChunk header;

        }

        public ZoneConnect(byte[] bytes) : base(bytes)
        {
            ZoneConnectData zoneConnectData = Utility.Deserialize<ZoneConnectData>(bytes);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Data.DataChunks.Outgoing
{
    public class DownloadData : BaseChunk
    {
        public DownloadData(uint value)
        {
            id = 0x04F;
            size = 0x04;

            data.Set<uint>(0x04, value);

            Complete();
        }
    }
}

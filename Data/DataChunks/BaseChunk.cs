using System;
using System.Collections.Generic;
using System.Text;

namespace Data.DataChunks
{
    public class BaseChunk
    {
        public BaseChunk()
        {
            data = new ByteRef(512);
        }

        public void Complete()
        {
            data.Resize(size * 2);

            data.Set<byte>(0, id);
            data.Set<byte>(1, size);            
        }

        public void SetSync(UInt16 sync)
        {
            data.Set<UInt16>(2, sync);
        }

        public byte[] Bytes { get { return data.Get(); } }

        public byte id;
        public byte size;
        public ByteRef data;
    }
}

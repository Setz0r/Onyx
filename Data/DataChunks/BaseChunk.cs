using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Toolbelt;

namespace Data.DataChunks
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ChunkHeader
    {
        [FieldOffset(0)]
        public byte id;
        [FieldOffset(1)]
        public byte size;
        [FieldOffset(2)]
        public ushort sync;
    }

    public class BaseChunk
    {
        public BaseChunk(byte[] bytes = null)
        {
            data = new ByteRef(512);
            if (bytes != null)
            {
                data.Set<byte[]>(0, bytes);
            }
        }

        public void Complete()
        {
            data.Resize(size * 2);

            data.Set<byte>(0, id);
            data.Set<byte>(1, size);            
        }

        public void SetSync(ushort sync)
        {
            data.Set<ushort>(2, sync);
        }

        public byte GetID()
        {
            return data.GetByte(0);
        }

        public byte GetSize()
        {
            return data.GetByte(1);
        }

        public ushort GetSync()
        {
            return data.GetUInt16(2);
        }
        
        public byte[] Bytes { get { return data.Get(); } }

        public byte id;
        public byte size;
        public ByteRef data;
        public bool encrypted;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Toolbelt
{
    public static class Compression
    {
        public unsafe struct ZLib
        {
            public ByteRef enc;
            public ByteRef val;
            public UInt32[] jump;
        }

        public static ZLib zlib;

        public static bool ReadToVector(string filename, ref ByteRef vec)
        {
            byte[] fileData = File.ReadAllBytes(filename);
            vec = new ByteRef(fileData);
            return true;
        }

        public static int CompressedSize(int size)
        {
            return (size + 7) / 8;
        }

        public static int DecomressedSize(int size)
        {
            return size * 8;
        }

        public static int JUMPBIT(byte[] table, int i)
        {
            return ((table[i / 8] >> (i & 7)) & 1);
        }

        static void PopulateJumpTable(ref ByteRef val, ref UInt32[] jump, ref ByteRef dec)
        {
            //jump.Resize(dec.Length);
            jump = new uint[dec.Length / 4];
            UInt32 b = dec.GetUInt32(0) - 4;

            for (int i = 0; i < dec.Length; i += 4)
            {
                UInt32 decval = dec.GetUInt32(i);
                if (decval > 0xFF)
                {
                    jump[i / 4] = (decval - b) / 4; // jump.data() + (dec[i] - base) / sizeof(base);
                }
                else
                {
                    jump[i / 4] = decval;
                }
            }
        }

        public static bool Initialize()
        {
            ByteRef dec = null;
            zlib = new ZLib();

            ReadToVector("decompress.dat", ref dec);
            ReadToVector("compress.dat", ref zlib.enc);
            zlib.val = new ByteRef(0);
            PopulateJumpTable(ref zlib.val, ref zlib.jump, ref dec);
            return false;
        }

        public static int zlib_compress_sub(byte[] b32, uint read, uint elem, ref byte[] outdata, int begin_idx, uint out_sz)
        {

            if (CompressedSize((int)elem) > 4)
            {
                //ShowWarning("zlib_compress_sub: element exceeds 4 bytes (%u)\n", elem);
                return -1;
            }

            if (CompressedSize((int)(read + elem)) > out_sz)
            {
                //ShowWarning("zlib_compress_sub: ran out of space (%u : %u : %u)\n", read, elem, out_sz);
                return -1;
            }

            for (uint i = 0; i < elem; ++i)
            {
                byte shift = (byte)((read + i) & 7);
                uint v = (read + i) / 8;
                uint inv_mask = (uint)((byte)(~(1 << shift)));
                //assert(shift < 8);
                outdata[v + begin_idx] = (byte)((inv_mask & outdata[v + begin_idx]) + (JUMPBIT(b32, (int)i) << shift));
            }

            return 0;
        }

        public static int Compress(byte[] data, UInt32 size, ref byte[] outdata, UInt32 outsize)
        {
            int w = 0;
            outdata = new byte[outsize];
            uint ptrval = 0;
            int skipper = 0;
            int jump_id = 0;
            uint read = 0;
            uint max_sz = (outsize - 1) * 8; // Output buffer may be at least 8 times big than original
            for (uint i = 0; i < size; ++i)
            {
                uint elem = (uint)zlib.enc.GetInt32(((sbyte)data[i] + 0x180) * 4); //[(char)data[i] + 0x180];
                if (elem + read < max_sz)
                {
                    uint index = (uint)((sbyte)data[i] + 0x80);
                    //assert(index < zlib.enc.size());
                    uint v = (uint)zlib.enc.GetInt32((int)index * 4);
                    //swap32_if_be(&v, 1);
                    byte[] b32 = BitConverter.GetBytes(v);
                    zlib_compress_sub(b32, read, elem, ref outdata, 1, outsize - 1);
                    read += elem;
                }
                else if (size + 1 >= outsize)
                {
                    // Ran if input doesn't fit output, outputs garbage(?)
                    //ShowWarning("zlib_compress: ran out of space, outputting garbage(?) (%u : %u : %u : %u)\n", read, elem, max_sz, in[i]);
                    //memset(out, 0, (out_sz / 4) + (in_sz & 3));
                    //memset(out +1, in_sz, in_sz / 4);
                    //memset(out +1 + in_sz / 4, (in_sz + 1) * 8, in_sz & 3);
                    // return (int)size;
                }
                else
                {
                    //ShowWarning("zlib_compress: ran out of space (%u : %u : %u : %u)\n", read, elem, max_sz, in[i]);
                    //return -1;
                }
            }

            outdata[0] = 1;
            return (int)(read + 8);
        }

        public unsafe static Int32 Decompress(byte[] data, UInt32 size, ref byte[] outdata, UInt32 outsize)
        {
            int w = 0;
            outdata = new byte[outsize];
            uint ptrval = 0;
            int skipper = 0;
            int jump_id = 0;
            if (data[0] != 1)
            {
                // ShowWarning("zlib_decompress: invalid compressed data\n");
                return -1;
            }
            byte[] cdata = data.Skip(1).ToArray();
            skipper = (int)zlib.jump[0];
            for (int i = 0; i < size && w < outsize; ++i)
            {
                jump_id = JUMPBIT(cdata, i);
                skipper = (int)zlib.jump[skipper + JUMPBIT(cdata, i)];

                if (zlib.jump[skipper] != 0 || zlib.jump[skipper + 1] != 0)
                    continue;

                outdata[w++] = (byte)zlib.jump[skipper + 3];
                skipper = (int)zlib.jump[0];

            }

            return w;
        }

    }
}
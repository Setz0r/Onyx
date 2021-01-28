using Data.Game;
using Data.Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Toolbelt;

namespace Data.DataChunks.Incoming
{
    //
    // Purpose: Received during zoning process
    //
    // Example Client Sent Data (0x00C) (length:12)
    //     0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F
    //    -----------------------------------------------
    // 0: 0C 06 03 00 00 00 00 00 00 00 00 00                             
    //
    // Notes: This is apparently the go ahead to send player info 
    //
    // reply with:
    // Inventory Size
    // Menu Config
    // Char Jobs
    // 
    // perform:
    // spawn pets/fellow
    //

    [StructLayout(LayoutKind.Explicit)]
    public struct LockItem
    {
        [FieldOffset(1)]
        public byte slotId;
        [FieldOffset(4)]
        public ushort itemId;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct LockStyleInfoDataHeader
    {
        [FieldOffset(0)]
        public ChunkHeader header;
        [FieldOffset(4)]
        public byte count;
        [FieldOffset(5)]
        public byte type;
    }

    public class LockStyleInfo : BaseChunk
    {
        public const int MinSize = 0x88;
        public const int MaxSize = 0x88;

        public bool Validator(LockStyleInfoDataHeader dataHeader, LockItem[] lockItems)
        {
            if (dataHeader.count > EQUIP_SLOTS.BACK + 1 || dataHeader.type > 4 || lockItems.Length != dataHeader.count)
                return false;

            foreach (LockItem item in lockItems)
            {
                if (item.slotId > EQUIP_SLOTS.BACK)
                    return false;
            }

            Logger.Success("we got 0x053");

            return true;
        }

        public bool Handler(Player player, byte[] bytes)
        {
            if (bytes.Length < MinSize || bytes.Length > MaxSize)
                return false;

            LockStyleInfoDataHeader LockStyleInfoDataHeader = Utility.Deserialize<LockStyleInfoDataHeader>(bytes.Take(8).ToArray());
            LockStyleInfoDataHeader.header.size *= 2;

            ByteRef byteRef = new ByteRef(bytes.Skip(8).ToArray());

            if (byteRef.Length != bytes.Length - 8)
                return false;

            LockItem[] lockItems = null;
            if (LockStyleInfoDataHeader.count > 0 && byteRef.Length > 0)
            {
                lockItems = new LockItem[LockStyleInfoDataHeader.count];
                for (int i = 0; i < LockStyleInfoDataHeader.count; i++)
                {
                    lockItems[i] = Utility.Deserialize<LockItem>(byteRef.GetBytes(i * 8, 8));
                }
            }

            if (Validator(LockStyleInfoDataHeader, lockItems))
            {


                // TODO: Handle the packet by sending player info chunks (0x1C, 0xB4, 0x1B)          

                return true;
            }
            return false;
        }

        private LockStyleInfo() { }

        public static LockStyleInfo Instance { get; } = new LockStyleInfo();
        public static LockStyleInfo GetInstance()
        {
            return Instance;
        }
    }
}

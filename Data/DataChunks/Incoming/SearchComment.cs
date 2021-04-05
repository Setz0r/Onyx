using Data.Game.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Toolbelt;

namespace Data.DataChunks.Incoming
{
    //
    // Purpose:  Received when player updates their search comment
    //
    // Example Client Sent Data (0x0E0) (length:152)
    //     0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F
    //    -----------------------------------------------
    // 0: E0 4C 70 07 79 6F 20 20 20 20 20 20 20 20 20 20
    // 1: 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20
    // 2: 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20
    // 3: 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20
    // 4: 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20
    // 5: 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20
    // 6: 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20
    // 7: 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 00
    // 8: 00 00 00 00 F7 3A D1 5B 57 49 4E 08 FF 0F 00 00
    // 9: FF 0F 00 00 13 00 00 00
    //
    // Notes: This is sent as soon as a player exits the search comment edit box
    //
    // 0x04 - 0x83 : (128 bytes) appears to be the actual comment, maybe 0 terminated 
    // 0x84 - 0x87 : (uint) Loaded from first 4 bytes of ffxi/SYS/ffxiusr.sys binary file
    // 0x88 - 0x8A : (string) WIN 
    // 0x94 -      : (byte) appears to be the category of search comment
    //               11 - EXP Party: seek party
    //               12 - EXP Party: find member
    //               13 - EXP Party: other
    //               73 - Others
    //
    // TODO: get the other categories from client captures
    //

    [StructLayout(LayoutKind.Explicit)]
    public struct SearchCommentData
    {
        [FieldOffset(0)]
        public ChunkHeader header;
        [FieldOffset(4)]
        [MarshalAs(UnmanagedType.LPStr, SizeConst = 128)]
        public string comment; 
        [FieldOffset(132)]
        public uint usersysno;  // this number comes from the ffxi /sys/ffxiusr.sys file
        [FieldOffset(148)]
        public byte category;
    }

    public class SearchComment : BaseChunk
    {
        public const int MinSize = 0x98;
        public const int MaxSize = 0x98;

        public bool Validator(SearchCommentData data)
        {
            if (data.empty != 0)
                return false;

            Logger.Success("we got 0x0E0");

            return true;
        }

        public bool Handler(Player player, byte[] bytes)
        {
            if (bytes.Length < MinSize || bytes.Length > MaxSize)
                return false;

            SearchCommentData SearchCommentData = Utility.Deserialize<SearchCommentData>(bytes);
            if (Validator(SearchCommentData))
            {
                // TODO: Handle the packet by updating the players search comment and flags         

                return true;
            }
            return false;
        }

        private SearchComment() { }

        public static SearchComment Instance { get; } = new SearchComment();
        public static SearchComment GetInstance()
        {
            return Instance;
        }
    }
}

using Data.Game.Entities;
using System;
using System.Collections.Generic;
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
    public struct PlayerInfoRequestData
    {
        [FieldOffset(0)]
        public ChunkHeader header;
        [FieldOffset(4)]
        public uint empty_a; // note, have not researched what this does, appears to always be 0 in logs
        [FieldOffset(8)]
        public uint empty_b; // note, have not researched what this does, appears to always be 0 in logs
    }

    public class PlayerInfoRequest : BaseChunk
    {
        public const int MinSize = 0x0;
        public const int MaxSize = 0xFF;

        public bool Validator(PlayerInfoRequestData data)
        {
            if (data.empty_a != 0 && data.empty_b != 0)
                return false;

            Logger.Success("we got 0x00C");

            return true;
        }

        public bool Handler(Player player, byte[] bytes)
        {
            if (bytes.Length < MinSize || bytes.Length > MaxSize)
                return false;

            PlayerInfoRequestData PlayerInfoRequestData = Utility.Deserialize<PlayerInfoRequestData>(bytes);
            if (Validator(PlayerInfoRequestData))
            {
                // TODO: Handle the packet by sending player info chunks (0x1C, 0xB4, 0x1B)          

                return true;
            }
            return false;
        }

        private PlayerInfoRequest() { }

        public static PlayerInfoRequest Instance { get; } = new PlayerInfoRequest();
        public static PlayerInfoRequest GetInstance()
        {
            return Instance;
        }
    }
}

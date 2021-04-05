using Data.Game.Entities;
using Data.OnyxMath;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Toolbelt;

namespace Data.DataChunks.Incoming
{
    //
    // Purpose: Sent by client periodically or when player moves or changes targets
    //
    // Example Client Sent Data (0x015) (length:32)
    //     0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F
    //    -----------------------------------------------
    // 0: 15 10 C5 12 2B 86 97 C3 6C 76 FC BF A6 9B FC 43
    // 1: 00 00 03 00 EC 00 52 01 87 3F 7B 78 00 00 00 00
    //
    // Notes: This is sent to move the player around or show whom they are targetting
    //

    [StructLayout(LayoutKind.Explicit)]
    public struct ClientUpdateData
    {
        [FieldOffset(0)]
        public ChunkHeader header;
        [FieldOffset(4)]
        public OnyxVec3 position;
        [FieldOffset(18)]
        public ushort animationFrame;
        [FieldOffset(20)]
        public byte rotation;
        [FieldOffset(21)]
        public byte flags;   // Bit 0x04 apparently has something to do with "maintenance mode" according to windower notes
        [FieldOffset(22)]
        public ushort targetIndex;
        [FieldOffset(24)]
        public uint timestamp;
        [FieldOffset(28)]
        public uint unknown; //likely a spacer to make packet div 8
    }

    public class ClientUpdate : BaseChunk
    {
        public const int MinSize = 0x20;
        public const int MaxSize = 0x20;

        public bool Validator(ClientUpdateData data)
        {
            // TODO: possibly add some kinda hax prevention for invalid positions or targeting...

            Logger.Success("we got 0x015");

            return true;
        }

        public bool Handler(Player player, byte[] bytes)
        {
            if (bytes.Length < MinSize || bytes.Length > MaxSize)
                return false;

            ClientUpdateData ClientUpdateData = Utility.Deserialize<ClientUpdateData>(bytes);
            if (Validator(ClientUpdateData))
            {
                // TODO: Handle the packet by moving the player and setting their updatemask for other client updates

                return true;
            }
            return false;
        }

        private ClientUpdate() { }

        public static ClientUpdate Instance { get; } = new ClientUpdate();
        public static ClientUpdate GetInstance()
        {
            return Instance;
        }
    }
}

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
    // Purpose: Sent by client when player performs various actions
    //
    // Example Client Sent Data (0x01A) (length:28)
    //     0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F
    //    -----------------------------------------------
    // 0: 1A 0E 37 00 F8 74 04 00 01 04 03 00 05 01 00 00
    // 1: 00 00 00 00 00 00 00 00 00 00 00 00
    //
    // Notes: This is sent to perform in-game actions
    //
    // My client shows 0x10-0x1B are hardcoded to be 0, could be something in newer clients
    //
    // Categories:
    //
    //  0x00 - NPC Interaction
    //  0x02 - Engage monster
    //  0x03 - Magic cast
    //  0x04 - Disengage
    //  0x05 - Call for Help
    //  0x07 - Weaponskill usage
    //  0x09 - Job ability usage
    //  0x0C - Assist
    //  0x0D - Reraise dialogue
    //  0x0E - Cast Fishing Rod
    //  0x0F - Switch target
    //  0x10 - Ranged attack
    //  0x12 - Dismount Chocobo
    //  0x14 - Show/Zone In
    //  0x19 - Monsterskill
    //  0x1A - Mount
    //

    [StructLayout(LayoutKind.Explicit)]
    public struct PlayerActionData
    {
        [FieldOffset(0)]
        public ChunkHeader header;
        [FieldOffset(4)]
        public uint targetId;
        [FieldOffset(8)]
        public ushort targetIndex;
        [FieldOffset(10)]
        public ushort category;
        [FieldOffset(12)]
        public uint param;
        [FieldOffset(16)]
        public uint zero1;
        [FieldOffset(20)]
        public uint zero2;
        [FieldOffset(24)]
        public uint zero3;
    }

    public class PlayerAction : BaseChunk
    {
        public const int MinSize = 0x1C;
        public const int MaxSize = 0x1C;

        public bool Validator(PlayerActionData data)
        {
            // TODO: validate category param is valid
            if (data.category > 0x1A)
                return false;

            Logger.Success("we got 0x01A");

            return true;
        }

        public bool Handler(Player player, byte[] bytes)
        {
            if (bytes.Length < MinSize || bytes.Length > MaxSize)
                return false;

            PlayerActionData PlayerActionData = Utility.Deserialize<PlayerActionData>(bytes);
            if (Validator(PlayerActionData))
            {
                // TODO: Handle the packet by moving the player and setting their updatemask for other client updates

                return true;
            }
            return false;
        }

        private PlayerAction() { }

        public static PlayerAction Instance { get; } = new PlayerAction();
        public static PlayerAction GetInstance()
        {
            return Instance;
        }
    }
}

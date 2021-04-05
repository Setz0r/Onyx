using Data.Game.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Toolbelt;

namespace Data.DataChunks.Incoming
{
    //
    // Purpose:  Received when player selecting a dialog option
    //
    // Example Client Sent Data (0x05B) (length:20)
    //     0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F
    //    -----------------------------------------------
    // 0: 5B 0A EB 12 52 21 00 01 00 00 00 00 52 01 00 00
    // 1: 02 00 14 00
    //
    // Notes: This is sent when the player makes a choice during an in-game dialog option popup
    //

    [StructLayout(LayoutKind.Explicit)]
    public struct DialogChoiceData
    {
        [FieldOffset(0)]
        public ChunkHeader header;
        [FieldOffset(4)]
        public uint targetId;
        [FieldOffset(8)]
        public uint optionIndex;
        [FieldOffset(12)]
        public ushort targetIndex;
        [FieldOffset(14)]
        public ushort automated; // 1 if client sends the packet automatically
        [FieldOffset(16)]
        public ushort zoneId;
        [FieldOffset(18)]
        public ushort menuId;
    }

    public class DialogChoice : BaseChunk
    {
        public const int MinSize = 0x14;
        public const int MaxSize = 0x14;

        public bool Validator(DialogChoiceData data)
        {
            // TODO: validate target, zone, etc

            Logger.Success("we got 0x05B");

            return true;
        }

        public bool Handler(Player player, byte[] bytes)
        {
            if (bytes.Length < MinSize || bytes.Length > MaxSize)
                return false;

            DialogChoiceData DialogChoiceData = Utility.Deserialize<DialogChoiceData>(bytes);
            if (Validator(DialogChoiceData))
            {
                // TODO: Call script for whichever event this dialog is processing for

                return true;
            }
            return false;
        }

        private DialogChoice() { }

        public static DialogChoice Instance { get; } = new DialogChoice();
        public static DialogChoice GetInstance()
        {
            return Instance;
        }
    }
}

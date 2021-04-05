using Data.Game.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Toolbelt;

namespace Data.DataChunks.Incoming
{
    //
    // Purpose:  Received when player equipment screen opened
    //
    // Example Client Sent Data (0x061) (length:8)
    //     0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F
    //    -----------------------------------------------
    // 0: 61 04 05 01 00 00 00 00 -- -- -- -- -- -- -- -- 
    //
    // Notes: This is sent to have the server send all the info shown while viewing the equipment screen
    //

    [StructLayout(LayoutKind.Explicit)]
    public struct EquipmentScreenData
    {
        [FieldOffset(0)]
        public ChunkHeader header;
        [FieldOffset(4)]
        public uint empty; // note, have not researched what this does, if anything
    }

    public class EquipmentScreen : BaseChunk
    {
        public const int MinSize = 0x8;
        public const int MaxSize = 0x8;

        public bool Validator(EquipmentScreenData data)
        {
            if (data.empty != 0)
                return false;

            Logger.Success("we got 0x061");

            return true;
        }

        public bool Handler(Player player, byte[] bytes)
        {
            if (bytes.Length < MinSize || bytes.Length > MaxSize)
                return false;

            EquipmentScreenData EquipmentScreenData = Utility.Deserialize<EquipmentScreenData>(bytes);
            if (Validator(EquipmentScreenData))
            {
                // TODO: Handle the packet by sending various information used in equipment screen         

                return true;
            }
            return false;
        }

        private EquipmentScreen() { }

        public static EquipmentScreen Instance { get; } = new EquipmentScreen();
        public static EquipmentScreen GetInstance()
        {
            return Instance;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data.Entities;
using Data.Game;
using Toolbelt;

namespace Data.DataChunks.Outgoing
{
    public class PlayerUpdate : BaseChunk
    {
        public PlayerUpdate(Player player, UInt16 sync)
        {
            id = 0x0D;
            size = 0x2E;

            // Base Data
            byte[] baseData = Utility.Serialize(player.baseInfo);
            data.Set<byte[]>(0x04, baseData);

            if (player.baseInfo.updateMask.HasFlag(UPDATETYPE.MOVEMENT))
            {
                // Movement Data
                data.Set<byte[]>(0x0B, Utility.Serialize(player.moveInfo));
            }

            if (player.baseInfo.updateMask.HasFlag(UPDATETYPE.DISPLAY))
            {
                // Display Data
                data.Set<byte[]>(0x1E, Utility.Serialize(player.displayInfo));
                data.Set<UInt32>(0x20, player.nameFlags.flags);                
                byte dbyte = data.GetByte(0x21);
                byte val21 = (byte)(dbyte | (byte)(player.Gender * 128 + (1 << player.look.size)));
                data.Set<byte>(0x21, val21);
            }

            //// Linkshell color
            //if (player.linkshell != null)
            //{
            //    byte[] lsData = Utility.Serialize(player.linkshell.color);
            //    data.BlockCopy(lsData, 0x24, lsData.Length);
            //}

            //// Player allegience 
            //data.SetVal(0x29, player.allegience);

            //// Player Look
            //byte[] lookData = Utility.Serialize(player.look);
            //data.BlockCopy(lookData, 0x48, 2, lookData.Length - 2);

            //// Player Name
            //if (player.baseInfo.updateMask.HasFlag(UPDATETYPE.NAME))
            //{
            //    data.SetVal(0x5A, Utility.MaxStr(player.name, 16));
            //    //int sizeAdd = player.name.Length - 2
            //    //@todo: lookup Finalize
            //}

            Complete();
        }
    }
}

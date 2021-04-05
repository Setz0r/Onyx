using Data.Game.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Toolbelt;

namespace Data.DataChunks.Outgoing
{
    public class InventoryData : BaseChunk
    {
        public InventoryData(Player player)
        {
            id = 0x01C;
            size = 0x1A;

            data.Set<byte[]>(0x04, Utility.Serialize(player.InventorySizes.InventoryCap));
            data.Set<byte[]>(0x14, Utility.Serialize(player.InventorySizes.InventoryActive));

            Complete();
        }
    }
}

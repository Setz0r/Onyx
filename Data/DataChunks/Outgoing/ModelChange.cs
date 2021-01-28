using Data.Game.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Toolbelt;

namespace Data.DataChunks.Outgoing
{
    public class ModelChange : BaseChunk
    {
        public ModelChange(Player player)
        {
            id = 0x051;
            size = 0x0C;

            data.Set<byte[]>(0x04, Utility.Serialize(player.Look.Model));
            data.Set<byte[]>(0x06, Utility.Serialize(player.Look.Equipment));

            Complete();
        }
    }
}

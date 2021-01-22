using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data.Game.Entities;
using Data.Game;
using Toolbelt;
using Data.World;

namespace Data.DataChunks.Outgoing
{
    public class ZoneIn : BaseChunk
    {
        public ZoneIn(Player player, Zone zone, ushort sync)
        {
            id = 0x00A;
            size = 0x82;

            // Player Info
            data.Set<uint>(0x04, player.PlayerId);
            data.Set<uint>(0x08, player.BaseInfo.TargetId);
            data.Set<byte[]>(0x0B, Utility.Serialize(player.MoveInfo));
            data.Set<byte[]>(0x1E, Utility.Serialize(player.DisplayInfo));
            data.Set<uint>(0x20, player.NameFlags.flags);
            byte dbyte = data.GetByte(0x21);
            byte val21 = (byte)(dbyte | (byte)(player.Gender * 128 + (1 << player.Look.Size)));
            data.Set<byte>(0x21, val21);
            
            // Zone Animation Info
            data.Set<byte>(0x27, zone.AnimationInfo.direction); // TODO: apparently this is actually a uint written at 0x24, but for now...
            data.Set<byte>(0x28, 0x01);
            data.Set<byte>(0x2A, zone.AnimationInfo.animation);

            // Zone Info
            data.Set<ushort>(0x30, zone.ZoneId);

            

            Complete();
        }
    }
}

using Data.DataChunks.Incoming;
using Data.Game.Entities;
using Data.World;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbelt;

namespace Game
{
    public static class PacketHandler
    {
        public static Dictionary<byte, Func<Player, byte[], bool>> IncomingPacketChunks => new Dictionary<byte, Func<Player, byte[], bool>>();

        public static void Initialize()
        {
            IncomingPacketChunks.Add(0x00A, (Player p, byte[] d) => { return ZoneConnect.Instance.Handler(p, d); });
        }

        public static bool ProcessDataChunk(Player player, byte[] chunkData, ZoneCluster cluster)
        {
            if (chunkData.Length > 0)
            {
                byte id = chunkData[0];
                Func<Player, byte[], bool> method;

                if (!IncomingPacketChunks.TryGetValue(id, out method))
                    return false;
                
                bool result = method(player, chunkData);

                return result;
            }

            return false;
        }

        public static UInt16 ProcessPacket(Player player, byte[] packetData, ZoneCluster cluster)
        {
            bool canProcess = true;
            UInt16 cursor = 0;
            UInt16 size = 0;
            Crypto.DecryptPacket(player.Client.blowfish, ref packetData);
            while (canProcess && packetData.Length - cursor > 4)
            {
                byte id = packetData[0];
                size = (byte)(packetData[1] * 2);
                if (size < packetData.Length)
                    return 0;
                if (!ProcessDataChunk(player, packetData.Skip(cursor).Take(size).ToArray(), cluster))
                {
                    Logger.Warning("Unable to process chunk ID {0} for Player ID: {1}, Possible validation issue", new object[] { id, player.BaseInfo.EntityId });
                }
                cursor += size;
            }

            return cursor;
        }
    }
}

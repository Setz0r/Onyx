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
        public const int PACKET_HEADER_SIZE = 28;

        public static Dictionary<byte, Func<Player, byte[], bool>> IncomingPacketChunks;

        public static void Initialize()
        {
            IncomingPacketChunks = new Dictionary<byte, Func<Player, byte[], bool>>();
            IncomingPacketChunks.Add(0x00A, (Player p, byte[] d) => { return ZoneConnect.Instance.Handler(p, d); });
            IncomingPacketChunks.Add(0x00C, (Player p, byte[] d) => { return PlayerInfoRequest.Instance.Handler(p, d); });
            IncomingPacketChunks.Add(0x00D, (Player p, byte[] d) => { return ZoneLeave.Instance.Handler(p, d); });
            IncomingPacketChunks.Add(0x011, (Player p, byte[] d) => { return ZoneSuccess.Instance.Handler(p, d); });
            IncomingPacketChunks.Add(0x053, (Player p, byte[] d) => { return LockStyleInfo.Instance.Handler(p, d); });
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

        public static ushort ProcessPacket(Player player, byte[] packetData, int packetSize, ZoneCluster cluster)
        {
            bool canProcess = true;
            ByteRef packetRef = new ByteRef(packetData.Take(packetSize).ToArray());
            byte[] decryptedData;
            ushort cursor = 0;

            // TODO: make this more efficient and trim off the header before anything else...

            int checksum = Utility.Checksum(packetRef.GetBytes(PACKET_HEADER_SIZE, packetSize - PACKET_HEADER_SIZE), packetSize - (PACKET_HEADER_SIZE + 16), packetRef.GetBytes(packetSize - 16, 16));
            decryptedData = packetRef.Get();
            if (checksum != 0)
            {                
                Crypto.DecryptPacket(player.Client.blowfish, ref decryptedData);
                packetRef = new ByteRef(decryptedData);
                checksum = Utility.Checksum(packetRef.GetBytes(PACKET_HEADER_SIZE, packetSize - PACKET_HEADER_SIZE), packetSize - (PACKET_HEADER_SIZE + 16), packetRef.GetBytes(packetSize - 16, 16));
                if (checksum != 0)
                {
                    canProcess = false;
                    Logger.Error("Unable to decrypt a packet");
                    packetRef.DebugDump();
                }
            }
            packetRef = new ByteRef(packetRef.Get().Skip(PACKET_HEADER_SIZE).ToArray());
            while (canProcess && packetRef.Length - cursor > 4)
            {                
                byte id = packetRef.GetByte(cursor);
                ushort size = (byte)(packetRef.GetByte(cursor + 1) * 2);
                if (size > packetRef.Length)
                    return 0;
                if (!ProcessDataChunk(player, packetRef.GetBytes(cursor, size), cluster))
                {
                    Logger.Warning("Unable to process chunk ID {0} for Player ID: {1}, Possible validation issue", new object[] { id, player.PlayerId });
                }
                cursor += size;
            }

            return cursor;
        }
    }
}

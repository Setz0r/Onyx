using System;
using Data.Game;
using System.Threading;
using System.IO;
using Toolbelt;
using System.Collections.Generic;
using static Toolbelt.Logger;
using System.Linq;
using Data;
using System.IO.Compression;

namespace ZoneServer
{
    class Program
    {
        public const string ConfigurationFile = "conf/game.conf";
        public static Dictionary<string, string> Configuration;

        static bool LoadConfiguration()
        {
            Logger.SetLoggingLevel(LOGGINGLEVEL.ERROR);
            try
            {
                if (File.Exists(ConfigurationFile))
                {
                    Configuration = Utility.ReadConf(ConfigurationFile);
                }
                else
                {
                    Logger.Warning("Zone server configuration file missing");
                }
                foreach (var config in Configuration)
                {
                    switch (config.Key)
                    {
                        case "Logging":
                            LOGGINGLEVEL logLevel = (LOGGINGLEVEL)Convert.ToUInt16(config.Value);
                            Logger.SetLoggingLevel(logLevel);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("LoadingConfiguration : {0}", new[] { e.Message });
                return false;
            }
            return true;
        }


        private static void Main(string[] args)
        {
            ThreadPool.GetMaxThreads(out int workerThreadsCount, out int ioThreadsCount);
            if (LoadConfiguration())
            {
                GameManager game = new GameManager();
                game.Initialize();
                TestJunk();
                game.GameLoop();
                game.Shutdown();
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }



        static void TestJunk()
        {

            //hashstring: 72BDF6261A0B4EE38EC41C57AB6AAC25
            //keystring: 0000000000000000000000000000000058E05DAD

            //encrypted packet data: 3F003E00DB0200002667BF5F010028010000000000000000280100007 3B0330A3 1924FB87 38FDB7A3 FB7E6B0B A0ADA8BED DF1D113A 80B364929AA24715002ACF3D9F1A381B102F2D18F008B9AD2815
            //[BLOCK: 0] :           3F003E00DB0200002667BF5F010028010000000000000000280100000 10428AF6 498804E7 38FDB7A3 FB7E6B0B A0ADA8BED DF1D113A 80B364929AA24715002ACF3D9F1A381B102F2D18F008B9AD2815
            //[BLOCK: 2] :           3F003E00DB0200002667BF5F010028010000000000000000280100000 10428AF6 498804E4 F4749784 89F9CFDB A0ADA8BED DF1D113A 80B364929AA24715002ACF3D9F1A381B102F2D18F008B9AD2815
            //[BLOCK: 4] :           3F003E00DB0200002667BF5F010028010000000000000000280100000 10428AF6 498804E4 F4749784 89F9CFD8 D2D8FEFD6 5E271F3A 80B364929AA24715002ACF3D9F1A381B102F2D18F008B9AD2815
            //[BLOCK: 6] :           3F003E00DB0200002667BF5F010028010000000000000000280100000 10428AF6 498804E4 F4749784 89F9CFD8 D2D8FEFD6 5E271F12 74BEFC26799FF515002ACF3D9F1A381B102F2D18F008B9AD2815
            //[BLOCK: 8] :           3F003E00DB0200002667BF5F010028010000000000000000280100000 10428AF6 498804E4 F4749784 89F9CFD8 D2D8FEFD6 5E271F12 74BEFC26799FF500000023ED01BD271B102F2D18F008B9AD2815
            //[BLOCK: 10] :          3F003E00DB0200002667BF5F010028010000000000000000280100000 10428AF6 498804E4 F4749784 89F9CFD8 D2D8FEFD6 5E271F12 74BEFC26799FF500000023ED01BD27BA727F92EBB505C6AD2815
//3F003E00DB0200002667BF5F01002801000000000000000028010000 01 04 28 AF 64 98 80 4E 4F 47 49 78 48 9F 9C FD 8D2D8FEFD6 5E271F12 74BEFC26799FF500000023ED01BD27BA727F92EBB505C6AD2815
            //byte[] responseData = Utility.StringToByteArray("0100000098020000672CBF5F00C91900FEFDC17779000000200000000A2E0100300001000000000044550000000000000000000000000000000000000000200000000000000000000000000000000000000000000000000000000000200970E41CBE8616ACEA95F9839B80D10000000057494E0001000101B751DEC81A5FF70D65298AF38FF99335");
            //byte[] responseData = Utility.StringToByteArray("02000100230000008F28BF5F01C9190000000000000000002000000098C50558238A946B04A25B4846873FF50030620B1C1478C23E");
            //int checksum = Utility.Checksum(responseData.Skip(0x1C).ToArray(),responseData.Length - (28 + 16), responseData.Skip(responseData.Length-16).ToArray());
            //List<UInt32> vec = new List<uint>();
            //bool test = Compression.ReadToVector("decompress.dat", ref vec);
            Compression.Initialize();

            int checksum = 0;
        //string packetbytes = "3F003E00DB0200002667BF5F0100280100000000000000002801000073B0330A31924FB8738FDB7A3FB7E6B0BA0ADA8BEDDF1D113A80B364929AA24715002ACF3D9F1A381B102F2D18F008B9AD2815";
        //byte[] packet = Utility.StringToByteArray(packetbytes);

        //string keybytes = "0000000000000000000000000000000058E05DAD";
        //byte[] key = Utility.StringToByteArray(keybytes);

        //Blowfish blowfish = new Blowfish();
        //Buffer.BlockCopy(key,0,blowfish.key,0,20);

        //byte[] hash = Crypto.SetupHashKey(key);

        //Crypto.BlowfishInitialize(hash, 16, ref blowfish.P, ref blowfish.S);

        //Crypto.DecryptPacket(blowfish, ref packet);

        //string decryptedstr = Utility.ByteArrayToString(packet,"").ToUpper();


            //buffsize: 81, refpos: 61, packetdatasize: 259


            //actual packetdatasize: 40

            string decryptedData = "03000100880200001D60C45F013C212C0000000000000000A213870401884C6EF7C0BCCEFF083039BF0EB8B33A34C09DD5A5289DF3FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF8B6C70960E0D37F9F6FF9F009D31831D58D0D351121ED26724A0CBC9E3BBA577E9EB4732C95E2260CEAA72FF1FFAC76481F3FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFBFDCB12F2A08901CEE13C970D1F0E8F7C87FE7FFFFFF7F34FAE6BCFAFFFFFFEEFD07EEFC7E729D9B89040000C0AF10A966C06B51C37220BF9B45A814";

            string compressed = "01C633A96F16ABD2A1E1A65ACF02428E7A4A50923CA44FFAA2F5BCC7DFCF7EC6C7EE5FFD7EF94B518AD882480590469761BF342929024486EFA52835296A7D87817F115002022B8112288112F8FFAA91E4086DE4BE3FFFEF5E8AC8402C2000C4E2FFFF2F3FB4B47D2F23E7BF7AF34E1F888647954450430DB5447E97086A0F4A249144124924979EE5D533B6D74670D19624D19043D19024D1908468F81F5120B7A62578F5FF57471259F50C837F8CE611E69E933B3A982F09D18088864CD2C3AA077CE723E0FE145962504B0DA39E2915C9FEFFCDA1060000";
            byte[] compressedbytes = Utility.StringToByteArray(compressed);
            string compressedstr = Utility.ByteArrayToString(compressedbytes);

            Console.WriteLine(compressedstr);
            string decomopressedtarget = "11040200020000000C0603000000000000000000610403000000000018050300010000001B05030001000000534403000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001A0E030044550000000414000000000000000000000000000000000015100300F2D2864107F068C1FA7EBE4200000200B8000000AE731717000000004B0C030001000102000000000000000000000000EC000000E04C030000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002EE59C5F57494E080E000000FF0F0000000000000F1203000000000000000000000000000000000000000000000000000000000000000000DB14030000000100000000000000000000000000000000000000000000000000000000000200000012050300000000005A020300000A";
            byte[] decompressedtargetbytes = Utility.StringToByteArray(decomopressedtarget);


            ByteRef packetref = new ByteRef(compressedbytes);
            UInt32 PacketDataSize = packetref.GetUInt32(packetref.Length - 4);

            //byte[] decompressed = Utility.Decompress(cb);
            byte[] decompressed = new byte[2520];
            int count = Compression.Decompress(compressedbytes, PacketDataSize, ref decompressed, PacketDataSize);


            string decompressedstr = Utility.ByteArrayToString(decompressed.Take(count).ToArray());
            Console.WriteLine(decompressedstr);

            byte[] recompress = new byte[2520];
            count = Compression.Compress(decompressed, (uint)decompressed.Length,ref recompress, (uint)compressed.Length);
            string recompressedstr = Utility.ByteArrayToString(recompress.Take(count).ToArray());

            Console.WriteLine(recompressedstr);


            //checksum = Utility.Checksum(packet.Skip(0x1C).ToArray(), packet.Length - (28 + 16), packet.Skip(packet.Length - 16).ToArray());
            if (checksum != 0)
            {
                Logger.Error("checksum fucked");
            }
            //UDPServer serv = new UDPServer(IPAddress.Parse("127.0.0.1"), 9999, MyCallback);

            //serv.Start();

            //UdpClient cli = new UdpClient();
            //cli.Connect("localhost", 54231);
            //cli.Send(Encoding.UTF8.GetBytes("test"), 4);

            //UdpClient cli2 = new UdpClient();
            //cli2.Connect("localhost", 54232);
            //cli2.Send(Encoding.UTF8.GetBytes("test"), 4);

            //Logger.Write("Regular Log here");
            //Logger.Info("Cool information here");
            //Logger.Success("Successfully activated such and such");
            //Logger.Warning("Cool warning here");
            //Logger.Error("Oh No! An {0} occurred!", new[] { "error" });

            //ThreadPool.GetMaxThreads(out int workerThreadsCount, out int ioThreadsCount);

            //byte[] responseData = Toolbelt.Utility.StringToByteArray("4c000000495846463c0080830000010000000000000000002303d3cbd3d6fe5c827c8a194844c252ac028c018080fa7070068074c5805000302e0004e9a00588b20bf251238e45283df8bb7445624de79229f156dc035d1f415a7b9333fc73345b64093934ebf99c415a7b9333fc7334df0bcfe5dcbf411926ec31e21e89c8b13a17804842663a95f4fedacc");

            //Toolbelt.Utility.TestBitStream(responseData);

            //Console.WriteLine("Hello World!");
            //ScriptManager scriptManager = new ScriptManager();
            //scriptManager.Test();

            //Vector3 vec = new Vector3(1.0f,2.0f,3.0f);
            //byte[] bytes = new byte[12];

            //Player player = new Player();
            //player.baseStats.STR = 100;
            //player.baseStats.DEX = 100;
            //player.baseStats.INT = 100;
            //player.baseStats.MND = 100;
            //player.baseStats.AGI = 100;
            //player.baseStats.VIT = 100;
            //player.baseStats.CHR = 100;

            ////player.animation.id = 1;
            ////player.animation.subId = 0;

            //player.baseInfo.id = 449586;
            //player.baseInfo.targetId = 1408;
            //player.baseInfo.updateMask = UPDATETYPE.MOVEMENT|UPDATETYPE.DISPLAY;

            //player.moveInfo.rotation = 65;
            //player.moveInfo.position.x = 483.16f;
            //player.moveInfo.position.y = -8.0f;
            //player.moveInfo.position.z = 239.08f;

            //player.moveInfo.moveCount = 2;

            //player.moveInfo.targetIndex = 52;

            //player.moveInfo.movementSpeed = 40;
            //player.moveInfo.animationSpeed = 40;

            //player.displayInfo.hpPercent = 64;

            //player.displayInfo.animation = 1;

            //player.status = Data.Game.ENTITYSTATUS.NORMAL;

            //player.nameFlags.flags = 0x00004000;

            //player.look.size = 0;
            //player.look.model.face = 0x00;
            //player.look.model.race = (byte)RACE.ELVAAN_M;
            //player.look.head = 4208;
            //player.look.body = 8501;
            //player.look.hands = 12359;
            //player.look.legs = 16485;
            //player.look.feet = 20643;
            //player.look.main = 24865;
            //player.look.sub = 28672;
            //player.look.ranged = 32768;

            //player.name = "Tristia";

            //player.linkshell = new Data.Game.Linkshell();
            //player.linkshell.color.Red = 173;
            //player.linkshell.color.Green = 225;
            //player.linkshell.color.Blue = 69;

            //PlayerUpdate playerUpdate = new PlayerUpdate(player, 2958);

            //byte[] data = playerUpdate.data.Get();

            //var builder = new FlatBuffers.FlatBufferBuilder(1024);
            //var name = builder.CreateString("Setzor");
            //FBPlayer.StartFBPlayer(builder);
            //FBPlayer.AddId(builder, 12345);
            //FBPlayer.AddName(builder, name);
            //FBPlayer.AddPos(builder, FBVec3.CreateFBVec3(builder, 1.0f, 2.0f, 3.0f));
            //var player = FBPlayer.EndFBPlayer(builder);
            //builder.Finish(player.Value);

            //byte[] buf = builder.SizedByteArray();

            //var bytes = new FlatBuffers.ByteBuffer(buf);
            //var myPlayer = FBPlayer.GetRootAsFBPlayer(bytes);
            //Console.WriteLine(Toolbelt.Utility.ByteArrayToString(data));
        }

    }
}

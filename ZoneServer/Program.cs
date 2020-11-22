using System;
using Scripting;
using Data.FBCommon;
using Data.FBEntities;
using Data.Entities;
using System.Numerics;
using System.Runtime.InteropServices;
using Data.DataChunks.Outgoing;
using Data.Game;
using System.Threading;

namespace ZoneServer
{
    class Program
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct TestData
        {
            [FieldOffset(0)]
            public float x;
            [FieldOffset(4)]
            public float y;
            [FieldOffset(8)]
            public float z;
        }

        static void Main(string[] args)
        {
            ThreadPool.GetMaxThreads(out int workerThreadsCount, out int ioThreadsCount);

            byte[] responseData = Toolbelt.Utility.StringToByteArray("4c000000495846463c0080830000010000000000000000002303d3cbd3d6fe5c827c8a194844c252ac028c018080fa7070068074c5805000302e0004e9a00588b20bf251238e45283df8bb7445624de79229f156dc035d1f415a7b9333fc73345b64093934ebf99c415a7b9333fc7334df0bcfe5dcbf411926ec31e21e89c8b13a17804842663a95f4fedacc");

            Toolbelt.Utility.TestBitStream(responseData);

            Console.WriteLine("Hello World!");
            ScriptManager scriptManager = new ScriptManager();
            scriptManager.Test();

            Vector3 vec = new Vector3(1.0f,2.0f,3.0f);
            byte[] bytes = new byte[12];
            
            Player player = new Player();
            player.baseStats.STR = 100;
            player.baseStats.DEX = 100;
            player.baseStats.INT = 100;
            player.baseStats.MND = 100;
            player.baseStats.AGI = 100;
            player.baseStats.VIT = 100;
            player.baseStats.CHR = 100;
            
            //player.animation.id = 1;
            //player.animation.subId = 0;

            player.baseInfo.id = 449586;
            player.baseInfo.targetId = 1408;
            player.baseInfo.updateMask = UPDATETYPE.MOVEMENT|UPDATETYPE.DISPLAY;

            player.moveInfo.rotation = 65;
            player.moveInfo.position.x = 483.16f;
            player.moveInfo.position.y = -8.0f;
            player.moveInfo.position.z = 239.08f;

            player.moveInfo.moveCount = 2;

            player.moveInfo.targetIndex = 52;

            player.moveInfo.movementSpeed = 40;
            player.moveInfo.animationSpeed = 40;

            player.displayInfo.hpPercent = 64;

            player.displayInfo.animation = 1;

            player.status = Data.Game.ENTITYSTATUS.NORMAL;
            
            player.nameFlags.flags = 0x00004000;

            player.look.size = 0;
            player.look.model.face = 0x00;
            player.look.model.race = (byte)RACE.ELVAAN_M;
            player.look.head = 4208;
            player.look.body = 8501;
            player.look.hands = 12359;
            player.look.legs = 16485;
            player.look.feet = 20643;
            player.look.main = 24865;
            player.look.sub = 28672;
            player.look.ranged = 32768;

            player.name = "Tristia";

            player.linkshell = new Data.Game.Linkshell();
            player.linkshell.color.Red = 173;
            player.linkshell.color.Green = 225;
            player.linkshell.color.Blue = 69;

            PlayerUpdate playerUpdate = new PlayerUpdate(player, 2958);
            
            byte[] data = playerUpdate.data.Get();

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
            Console.WriteLine(Toolbelt.Utility.ByteArrayToString(data));
            Console.ReadKey();

        }
    }
}

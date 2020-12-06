using System.Runtime.InteropServices;

namespace Networking
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Look
    {
        [FieldOffset(0)]
        public ushort size;
        [FieldOffset(2)]
        public byte face;
        [FieldOffset(3)]
        public byte race;
        [FieldOffset(4)]
        public ushort modelid;
        [FieldOffset(6)]
        public ushort head;
        [FieldOffset(8)]
        public ushort body;
        [FieldOffset(10)]
        public ushort hands;
        [FieldOffset(12)]
        public ushort legs;
        [FieldOffset(14)]
        public ushort feet;
        [FieldOffset(16)]
        public ushort main;
        [FieldOffset(18)]
        public ushort sub;
        [FieldOffset(20)]
        public ushort ranged;
    }

    public class CharMini
    {
        public string Name;
        public byte Job;
        public ushort Zone;
        public byte Nation;
        public Look Look;
    }

    public class LoginCharacter
    {
        private uint _ID;
        private string _Name;
        private ushort _Zone;
        private ushort _PrevZone;
        private byte _Nation;

        private byte _MJob;
        private byte _Race;
        private byte _Face;
        private ushort _Head;
        private ushort _Body;
        private ushort _Hands;
        private ushort _Legs;
        private ushort _Feet;
        private ushort _Main;
        private ushort _Sub;
        private byte[] _JobLevels;
        private byte _GMLevel;

        public uint ID { get => _ID; set => _ID = value; }
        public string Name { get => _Name; set => _Name = value; }
        public ushort Zone { get => _Zone; set => _Zone = value; }
        public ushort PrevZone { get => _PrevZone; set => _PrevZone = value; }
        public byte MJob { get => _MJob; set => _MJob = value; }
        public byte Race { get => _Race; set => _Race = value; }
        public byte Face { get => _Face; set => _Face = value; }
        public ushort Head { get => _Head; set => _Head = value; }
        public ushort Body { get => _Body; set => _Body = value; }
        public ushort Hands { get => _Hands; set => _Hands = value; }
        public ushort Legs { get => _Legs; set => _Legs = value; }
        public ushort Feet { get => _Feet; set => _Feet = value; }
        public ushort Main { get => _Main; set => _Main = value; }
        public ushort Sub { get => _Sub; set => _Sub = value; }
        public byte GMLevel { get => _GMLevel; set => _GMLevel = value; }
        public byte[] JobLevels { get => _JobLevels; set => _JobLevels = value; }
        public byte Nation { get => _Nation; set => _Nation = value; }
    }
}

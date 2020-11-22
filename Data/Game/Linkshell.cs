using Data.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Data.Game
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct LinkshellColor
    {
        [FieldOffset(0)]
        public byte Red;
        [FieldOffset(1)]
        public byte Green;
        [FieldOffset(2)]
        public byte Blue;
    }

    public class Linkshell
    {
        public UInt32 id;
        public string name;
        public LinkshellColor color;
        public string message;        
    }
}

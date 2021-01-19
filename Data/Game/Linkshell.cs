using Data.Game.Entities;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Data.Game
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public class LinkshellColor
    {
        [FieldOffset(0)]
        public byte Red;
        [FieldOffset(1)]
        public byte Green;
        [FieldOffset(2)]
        public byte Blue;
    }

    [Serializable]
    [BsonIgnoreExtraElements]
    public class Linkshell
    {
        public uint id;
        public string name;
        public LinkshellColor color;
        public string message;        
    }
}

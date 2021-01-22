using Data.OnyxMath;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Data.Game
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public class UIntFlags
    {
        [FieldOffset(0)]
        public byte b1;
        [FieldOffset(1)]
        public byte b2;
        [FieldOffset(2)]
        public byte b3;
        [FieldOffset(3)]
        public byte b4;
        [FieldOffset(0)]
        public uint flags;
    }

    [StructLayout(LayoutKind.Explicit)]
    public class WeatherInfo
    {
        [FieldOffset(0)]
        public WEATHER current;
        [FieldOffset(1)]
        public WEATHER previous;
        [FieldOffset(2)]
        public uint currentStartTime;
        [FieldOffset(6)]
        public uint previousStartTime;
        [FieldOffset(10)]
        public byte currentTweenTime;
        [FieldOffset(11)]
        public byte previousTweenTime;
    }

    [Serializable]
    [BsonIgnoreExtraElements]
    public class PositionInfo
    {
        public OnyxVec3 Pos { get; set; }
        public ushort AnimationFrame { get; set; }
        public byte Rotation { get; set; }
    }

    [Serializable]
    [BsonIgnoreExtraElements]
    public class LocationInfo
    {
        public PositionInfo Position { get; set; }
        public ushort PreviousZone { get; set; }
        public ushort CurrentZone { get; set; }
        public ushort DestinationZone { get; set; }
        public ushort Boundary { get; set; }

    }
}

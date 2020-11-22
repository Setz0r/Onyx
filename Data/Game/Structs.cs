using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Data.Game
{
    [StructLayout(LayoutKind.Explicit)]
    public struct UIntFlags
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
        public UInt32 flags;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct WeatherInfo
    {
        [FieldOffset(0)]
        public WEATHER current;
        [FieldOffset(1)]
        public WEATHER previous;
        [FieldOffset(2)]
        public UInt32 currentStartTime;
        [FieldOffset(6)]
        public UInt32 previousStartTime;
        [FieldOffset(10)]
        public byte currentTweenTime;
        [FieldOffset(11)]
        public byte previousTweenTime;
    }
}

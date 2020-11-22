using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Data.OnyxMath
{
    [StructLayout(LayoutKind.Explicit)]
    public struct OnyxVec3
    {
        [FieldOffset(0)]
        public float x;
        [FieldOffset(4)]
        public float y;
        [FieldOffset(8)]
        public float z;
    }

}

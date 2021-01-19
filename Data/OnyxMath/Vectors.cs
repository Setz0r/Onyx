using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Data.OnyxMath
{
    [StructLayout(LayoutKind.Explicit, Size=12, Pack=1)]
    public struct OnyxVec3
    {
        [FieldOffset(0)]
        public float x;
        [FieldOffset(4)]
        public float y;
        [FieldOffset(8)]
        public float z;

        public OnyxVec3(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
    }

}

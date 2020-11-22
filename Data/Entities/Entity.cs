using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Collections.Concurrent;
using Data.OnyxMath;
using Data.Game;
using Toolbelt;

namespace Data.Entities
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ModelInfo
    {
        [FieldOffset(0)]
        public byte face;
        [FieldOffset(1)]
        public byte race;
        [FieldOffset(0)]
        public UInt16 id;
    }

    public struct Look
    {
        public UInt16 size;
        public ModelInfo model;
        public UInt16 head, body, hands, legs, feet, main, sub, ranged;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BaseEntityInfo
    {        
        [FieldOffset(0)] 
        public UInt32 id;
        [FieldOffset(4)]
        public UInt16 targetId;
        [FieldOffset(6)]
        public UPDATETYPE updateMask;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct MovementInfo
    {
        [FieldOffset(0)]
        public byte rotation;
        [FieldOffset(1)]
        public OnyxVec3 position;
        [FieldOffset(13)]
        public UInt16 moveCount;
        [FieldOffset(15)]
        public UInt16 targetIndex;
        [FieldOffset(17)]
        public byte movementSpeed;
        [FieldOffset(18)]
        public byte animationSpeed;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct DisplayInfo
    {
        [FieldOffset(0)]
        public byte hpPercent;
        [FieldOffset(1)]
        public byte animation;
    }


    public class Entity
    {
        public Entity()
        {
            type = ENTITYTYPE.NONE;
            status = ENTITYSTATUS.NORMAL;
            baseInfo.updateMask = UPDATETYPE.NONE;

            target = null;
            localVariables = new ConcurrentDictionary<string, int>();
            name = String.Empty;
        }

        public ENTITYTYPE type;
        public ENTITYSTATUS status;
        public Entity target;
        public ConcurrentDictionary<string, Int32> localVariables;

        // packet data
        public BaseEntityInfo baseInfo;
        public MovementInfo moveInfo;
        public DisplayInfo displayInfo;

        public Look look;
        public byte allegience;
        public string name;
    }
}

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Collections.Concurrent;
using Data.OnyxMath;
using Data.Game;
using Toolbelt;

namespace Data.Game.Entities
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ModelInfo
    {
        [FieldOffset(0)]
        public byte Face;
        [FieldOffset(1)]
        public byte Race;
        [FieldOffset(0)]
        public UInt16 Id;
    }

    public struct Look
    {
        public UInt16 Size;
        public ModelInfo Model;
        public UInt16 Head, Body, Hands, Legs, Feet, Main, Sub, Ranged;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BaseEntityInfo
    {        
        [FieldOffset(0)] 
        public UInt32 EntityId;
        [FieldOffset(4)]
        public UInt16 TargetId;
        [FieldOffset(6)]
        public UPDATETYPE UpdateMask;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct MovementInfo
    {
        [FieldOffset(0)]
        public byte Rotation;
        [FieldOffset(1)]
        public OnyxVec3 Position;
        [FieldOffset(13)]
        public UInt16 MoveCount;
        [FieldOffset(15)]
        public UInt16 TargetIndex;
        [FieldOffset(17)]
        public byte MovementSpeed;
        [FieldOffset(18)]
        public byte animationSpeed;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct DisplayInfo
    {
        [FieldOffset(0)]
        public byte HpPercent;
        [FieldOffset(1)]
        public byte Animation;
    }


    public class Entity
    {
        public Entity()
        {
            EntityType = ENTITYTYPE.NONE;
            Status = ENTITYSTATUS.NORMAL;
            BaseInfo.UpdateMask = UPDATETYPE.NONE;

            Target = null;
            LocalVariables = new ConcurrentDictionary<string, int>();
            Name = String.Empty;
        }

        public ENTITYTYPE EntityType;
        public ENTITYSTATUS Status;
        public Entity Target;
        public ConcurrentDictionary<string, Int32> LocalVariables;

        // packet data
        public BaseEntityInfo BaseInfo;
        public MovementInfo MoveInfo;
        public DisplayInfo DisplayInfo;

        public Look Look;
        public byte Allegience;
        public string Name;
    }
}

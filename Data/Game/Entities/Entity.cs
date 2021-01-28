using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Collections.Concurrent;
using Data.OnyxMath;
using Data.Game;
using Toolbelt;
using MongoDB.Bson.Serialization.Attributes;

namespace Data.Game.Entities
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public class ModelInfo
    {
        [FieldOffset(0)]
        public byte Face;
        [FieldOffset(1)]
        public byte Race;
        [FieldOffset(0)]
        public ushort Id;
    }

    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public class EquipInfo
    {
        [FieldOffset(0)] 
        public ushort Head;
        [FieldOffset(2)]
        public ushort Body;
        [FieldOffset(4)]
        public ushort Hands;
        [FieldOffset(6)]
        public ushort Legs;
        [FieldOffset(8)]
        public ushort Feet;
        [FieldOffset(10)]
        public ushort Main;
        [FieldOffset(12)]
        public ushort Sub;
        [FieldOffset(14)]
        public ushort Ranged;
    }

    [Serializable]
    public class Look
    {
        public ushort Size;
        public ModelInfo Model;
        public EquipInfo Equipment;
        public Look()
        {
            Model = new ModelInfo();
            Equipment = new EquipInfo();
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public class BaseEntityInfo
    {        
        [FieldOffset(0)] 
        public uint EntityId;
        [FieldOffset(4)]
        public ushort TargetId;
        [FieldOffset(6)]
        public UPDATETYPE UpdateMask;
    }

    [Serializable]
    [StructLayout(LayoutKind.Explicit, Size=18, Pack = 1)]
    public class MovementInfo
    {
        [FieldOffset(0)]
        public byte Rotation;
        [FieldOffset(1)]
        public OnyxVec3 Position;
        [FieldOffset(13)]
        public ushort AnimationFrame;
        [FieldOffset(15)]
        public ushort TargetIndex;
        [FieldOffset(17)]
        public byte MovementSpeed;
        [FieldOffset(18)]
        public byte animationSpeed;
    }

    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public class DisplayInfo
    {
        [FieldOffset(0)]
        public byte HpPercent;
        [FieldOffset(1)]
        public byte Animation;
    }

    [Serializable]
    [BsonIgnoreExtraElements]
    public class Entity
    {
        public Entity()
        {
            EntityType = ENTITYTYPE.NONE;
            Status = ENTITYSTATUS.NORMAL;
            BaseInfo = new BaseEntityInfo();            
            
            Target = null;
            Location = new LocationInfo();
            LocalVariables = new Dictionary<string, int>();
            MoveInfo = new MovementInfo();
            DisplayInfo = new DisplayInfo();
            Look = new Look();

            Name = string.Empty;
        }

        public ENTITYTYPE EntityType { get; set; }
        public ENTITYSTATUS Status { get; set; }
        
        [BsonIgnore]
        [field: NonSerialized]
        public Entity Target { get; set; }
        
        [BsonIgnore]
        [field: NonSerialized]
        public Dictionary<string, int> LocalVariables { get; set; }

        public LocationInfo Location { get; set; }

        // packet data
        public BaseEntityInfo BaseInfo { get; set; }
        public MovementInfo MoveInfo { get; set; }
        public DisplayInfo DisplayInfo { get; set; }

        public Look Look { get; set; }
        public byte Allegience { get; set; }
        public string Name { get; set; }
    }
}

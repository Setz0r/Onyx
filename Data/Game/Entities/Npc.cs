using Data.Game;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Game.Entities
{
    [Serializable]
    [BsonIgnoreExtraElements]
    public class Npc : Combat
    {
        public Npc()
        {
            EntityType = ENTITYTYPE.NPC;
            AnimationSub = 0;
        }

        public byte AnimationSub;
    }
}

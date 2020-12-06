using Data.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Game.Entities
{
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

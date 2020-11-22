using Data.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Entities
{
    public class Npc : Combat
    {
        public Npc()
        {
            type = ENTITYTYPE.NPC;
            animationSub = 0;
        }

        public byte animationSub;
    }
}

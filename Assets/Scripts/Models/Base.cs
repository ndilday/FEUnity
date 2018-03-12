using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Models
{
    public enum BaseType
    {
        Battlestation = 2,
        Starbase = 3
    }

    public class Base
    {
        public BaseType BaseType { get; private set; }
        public Factions Faction { get; private set; }
        public IntVector2 Location { get; private set; }

        public Base(BaseType baseType, Factions faction, IntVector2 location)
        {
            BaseType = baseType;
            Faction = faction;
            Location = location;
        }

        public Base(BaseType baseType, Factions faction, int x, int y)
        {
            BaseType = baseType;
            Faction = faction;
            Location = new IntVector2 { x = x, y = y };
        }
    }
}

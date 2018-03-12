using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Models
{
    public class Ship
    {
        public int Id { get; private set; }
        public ShipClass Class { get; private set; }
        public string HullNumber { get; private set; }
        public string Name { get; private set; }
        public bool IsCrippled { get; private set; }
        public Factions Faction { get; private set; }

        public Ship(int id, ShipClass shipClass, Factions faction, string hullNumber, string name, bool isCrippled)
        {
            Id = id;
            Class = shipClass;
            HullNumber = hullNumber;
            Name = name;
            IsCrippled = isCrippled;
        }

        public override string ToString()
        {
            return Class.Designation + " " + HullNumber + " " + Name + (IsCrippled ? " (Crippled)" : "");
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Models
{
    public class MapNode
    {
        public IntVector2 Coordinates { get; set; }
        //public int Coordinates.x { get; set; }
        //public int Coordinates.y { get; set; }
        public Factions OriginalOwner { get; set; }
        public List<Base> Bases { get; set; }
        public Capital Capital { get; set; }
        public List<Planet> Planets { get; set; }
        public List<Ship> Ships { get; set; }

        public MapNode()
        {
            Ships = new List<Ship>();
        }

        public bool HasMajorPlanet()
        {
            return Planets != null && Planets.Any(p => p.PlanetType == PlanetType.MajorPlanet);
        }

        public bool HasMinorPlanet()
        {
            return Planets != null && Planets.Any(p => p.PlanetType == PlanetType.MinorPlanet);
        }

        public bool HasBattlestation()
        {
            return Bases != null && Bases.Any(b => b.BaseType == BaseType.Battlestation);
        }

        public bool HasStarbase()
        {
            return Bases != null && Bases.Any(b => b.BaseType == BaseType.Starbase);
        }

        public bool ContainsOtherFactionShips(Factions faction)
        {
            return false;
        }

        public bool ContainsShipsOfFaction(Factions faction)
        {
            return false;
        }
    }

    public class Capital
    {
        public System[] CapitalSystems { get; set; }
    }

    public class System
    {
        public string Name { get; set; }
        public Planet[] Planets { get; set; }
        public Base[] Bases { get; set; }
    }
}

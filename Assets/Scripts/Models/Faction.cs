using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Assets.Scripts.Econ;

namespace Assets.Scripts.Models
{
    public enum Factions
    {
        Neutral = 0,
        Klingon = 1,
        Kzinti = 2,
        Hydran = 3,
        Federation = 4,
        Romulan = 5,
        Gorn = 6,
        Orion = 7,
        Tholian = 8,
        LDR = 9,
        Wyn = 10,
        Lyran = 11,
    }

    public class Faction
    {
        private List<MapNode> _ownedCells;
        private OffMap _offMapArea;
        public Color Color { get; private set; }
        public string Name { get; private set; }
        public Factions Id { get; private set; }
        public List<Fleet> Fleets { get; private set; }
        public Shipyard Shipyard { get; set; }
        public List<Base> Bases { get; private set; }
        public MapNode CapitalMapNode { get; private set; }
        public List<SupplyGrid> SupplyGrids { get; set; }

        private List<Province> _ownedProvinces;
        private List<SupplyGrid> _supplyGrids;
        private List<Ship> _ships;
        private SupplyGrid _shipyardSupplyGrid;

        public Faction(Factions faction, Color color, string name)
        {
            Id = faction;
            Color = color;
            Name = name;
            _ownedCells = new List<MapNode>();
            _ownedProvinces = new List<Province>();
            _ships = new List<Ship>();
            Fleets = new List<Fleet>();
            Bases = new List<Base>();
        }

        public Faction(Color factionColor, IEnumerable<MapNode> ownedCells, IEnumerable<Province> ownedProvinces)
        {
            Color = factionColor;
            _ownedCells = new List<MapNode>();
            _ownedCells.AddRange(ownedCells);
            _ownedProvinces = new List<Province>();
            _ownedProvinces.AddRange(ownedProvinces);
        }

        public void AddMapNodes(IEnumerable<MapNode> mapNodes, OffMap offMapArea)
        {
            _ownedCells.AddRange(mapNodes);
            CapitalMapNode = _ownedCells.Where(mn => mn.Capital != null && mn.OriginalOwner == Id).FirstOrDefault();
            _offMapArea = offMapArea;
            // TODO: if there are no supply nodes, break the passed in mapNodes into necessary pieces
            if(_supplyGrids == null)
            {
                _supplyGrids = new List<SupplyGrid>(1)
                {
                    new SupplyGrid(_ownedCells, _offMapArea)
                };
            }
            // determine which supply grid the new cells should go into
            //if there's only one, it's easy
            else if(_supplyGrids.Count == 1)
            {
                _supplyGrids[0].AddMapNodes(mapNodes);
            }
            //TODO: handle more sophisticated supply grid cases
        }

        public void RemoveMapNode(int x, int y)
        {
            _ownedCells.RemoveAll(n => n.Coordinates.x == x && n.Coordinates.y == y);
            // TODO: also remove the cell from its supply grid
        }

        public void AddShips(IEnumerable<Ship> ships)
        {
            _ships.AddRange(ships);
        }

        public void AddShip(Ship ship)
        {
            _ships.Add(ship);
        }

        public void AddFleets(IEnumerable<Fleet> fleets)
        {
            Fleets.AddRange(fleets);
        }

        public void AddFleet(Fleet fleet)
        {
            Fleets.Add(fleet);
        }

    }
}
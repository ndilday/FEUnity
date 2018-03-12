using System;
using System.Collections.Generic;
using System.Linq;

using Assets.Scripts.Models;

namespace Assets.Scripts.Econ
{
    public enum SupplyStates
    {
        Unset = -1,
        Clear = 0,
        ContainsFriendlyShips = 1,
        ContainsFriendlyUnits = 2,
        Protected = 3,
        EnemyAdjacent = 4,
        ContainsEnemies = 5,
        OutOfRange = 6
    }

    public class SupplyGrid
    {
        private List<MapNode> _gridCells;
        private int _storedEconomicPoints;
        private OffMap _offMapResources;

        public SupplyGrid(List<MapNode> hexes, OffMap offMapArea)
        {
            _gridCells = hexes;
            _offMapResources = offMapArea;
            _storedEconomicPoints = 0;
        }

        public int GetEconomicPoints()
        {
            // TODO: factor in provice EPs
            // for now, assume that every province represented in owned cells should credit this grid
            //int provinces =_gridCells.GroupBy(gc => gc.ProvinceId).Count();
            int points = 0;
            foreach (MapNode node in _gridCells)
            {
                points += GetMapNodeEconomicPoints(node);
            }
            if (_offMapResources != null)
            {
                points += GetOffMapEconomicPoints(_offMapResources);
            }

            return points + _storedEconomicPoints;
        }

        private int GetMapNodeEconomicPoints(MapNode node)
        {
            if (node.Planets != null)
            {
                return GetPlanetEconomicPoints(node.Planets);
            }
            if (node.Capital != null)
            {
                return GetCapitalEconomicPoints(node.Capital);
            }
            return 0;
        }

        private int GetPlanetEconomicPoints(IEnumerable<Planet> planets)
        {
            int points = 0;
            foreach(Planet planet in planets)
            {
                switch(planet.PlanetType)
                {
                    case PlanetType.MinorPlanet:
                        points += planet.DevestationTurn > -1 || planet.CaptureTurn > -1 ? 1 : 2;
                        break;
                    case PlanetType.MajorPlanet:
                        points += planet.DevestationTurn > -1 || planet.CaptureTurn > -1 ? 2 : 5;
                        break;
                }
            }
            return points;
        }

        private int GetCapitalEconomicPoints(Capital capital)
        {
            int points = 0;
            foreach (Models.System system in capital.CapitalSystems)
            {
                GetPlanetEconomicPoints(system.Planets);
            }
            return points;
        }

        private int GetOffMapEconomicPoints(OffMap offMap)
        {
            return offMap.MajorPlanetCount * 5 + offMap.MinorPlanetCount * 2 + offMap.ProvinceCount * 2;
        }

        public void AddMapNodes(IEnumerable<MapNode> mapNodes)
        {
            _gridCells.AddRange(mapNodes);
        }
    }

}

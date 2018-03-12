using System.Collections.Generic;
using UnityEngine;

using Assets.Scripts.Models;

namespace Assets.Scripts
{
    public enum TopVisual
    {
        None = 0,
        Battlestation = 1,
        Starbase = 2,
        MinorPlanet = 3,
        MajorPlanet = 4,
        Capital = 5
    }

    public class MapData
    {
        private MapNode[,] _mapNodes;
        private int[,] _provinceHexMap;
        private Faction[] _factions;
        private Province[] _provinces;

        public MapData(MapNode[,] mapNodes, int[,] provinceHexMap, Faction[] factions, Province[] provinces)
        {
            _mapNodes = mapNodes;
            _provinceHexMap = provinceHexMap;
            _factions = factions;
            _provinces = provinces;
        }

        public MapNode[,] MapNodes
        {
            get
            {
                return _mapNodes;
            }
        }

        public Faction[] FactionMap
        {
            get
            {
                return _factions;
            }
        }

        public Province[] ProvinceMap
        {
            get
            {
                return _provinces;
            }
        }

        public Faction GetFaction(Factions faction)
        {
            return _factions[(int)faction];
        }

        public int GetMapNodeProvinceId(int x, int y)
        {
            return _provinceHexMap[x, y];
        }

        public Color GetFactionColor(Factions faction)
        {
            return _factions[(int)faction].Color;
        }

        public MapNode GetMapNode(int x, int y)
        {
            if (x >= _mapNodes.GetLength(0) || y >= _mapNodes.GetLength(1) || x < 0 || y < 0) return null;
            return _mapNodes[x, y];
        }

        public List<MapNode> GetAdjacentMapNodes(IntVector2 coordinates)
        {
            int xLen = _mapNodes.GetLength(0);
            int yLen = _mapNodes.GetLength(1);
            List<MapNode> nodes = new List<MapNode>();
            if (coordinates.x >= xLen || coordinates.y >= yLen || coordinates.x < 1 || coordinates.y < 1)
            {
                return nodes;
            }
            bool isTop = coordinates.y == 1;
            bool isBottom = coordinates.y == yLen - 1;
            bool isLeft = coordinates.x == 1;
            bool isRight = coordinates.x == xLen - 1;
            bool isXEven = coordinates.x % 2 == 0;
            if(!isTop)
            {
                nodes.Add(_mapNodes[coordinates.x, coordinates.y - 1]);
            }
            if(!isRight && (!isTop || isXEven))
            {
                nodes.Add(_mapNodes[coordinates.x + 1, coordinates.y - (coordinates.x % 2)]);
            }
            if(!isRight && (!isBottom || !isXEven))
            {
                nodes.Add(_mapNodes[coordinates.x + 1, coordinates.y + 1 - (coordinates.x % 2)]);
            }
            if(!isBottom)
            {
                nodes.Add(_mapNodes[coordinates.x, coordinates.y - 1]);
            }
            if(!isLeft && (!isBottom || !isXEven))
            {
                nodes.Add(_mapNodes[coordinates.x - 1, coordinates.y + 1 - (coordinates.x % 2)]);
            }
            if(!isLeft && (!isTop || isXEven))
            {
                nodes.Add(_mapNodes[coordinates.x - 1, coordinates.y - (coordinates.x % 2)]);
            }

            return nodes;
        }

        public MapNode[] GetProvince(int provinceId)
        {
            return this._provinces[provinceId].MapNodes;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Models
{
    public class Province
    {
        public MapNode[] MapNodes { get; private set; }
        private Factions _originalFaction;

        public Province(IEnumerable<MapNode> mapNodes)
        {
            MapNodes = mapNodes.ToArray();
            SetOriginalFaction();
        }

        public Province(MapNode[] mapNodes)
        {
            MapNodes = mapNodes;
            SetOriginalFaction();
        }

        private void SetOriginalFaction()
        {
            if (MapNodes == null)
            {
                _originalFaction = Factions.Neutral;
            }
            else
            {
                _originalFaction = MapNodes[0].OriginalOwner;
            }
        }

        public Factions GetOwningFaction(out bool isDisrupted)
        {
            isDisrupted = false;
            Factions potentialOwner = _originalFaction;
            bool containsOriginalFactionShips = false;
            // see if there is any unit from another faction in any node; if so, disrupted
            foreach(MapNode node in MapNodes)
            {
                if(node.OriginalOwner != _originalFaction || node.ContainsOtherFactionShips(_originalFaction))
                {
                    isDisrupted = true;

                    if (node.OriginalOwner != _originalFaction)
                    {
                        potentialOwner = node.OriginalOwner;
                    }
                }
                if (node.ContainsShipsOfFaction(_originalFaction))
                {
                    containsOriginalFactionShips = true;
                }
            }

            return containsOriginalFactionShips ? _originalFaction : potentialOwner;
        }
    }
}

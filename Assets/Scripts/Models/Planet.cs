using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Models
{
    public enum PlanetType
    {
        MinorPlanet = 2,
        MajorPlanet = 5
    }
    public class Planet
    {
        public PlanetType PlanetType { get; private set; }
        public int CaptureTurn { get; set; }
        public int DevestationTurn { get; set; }
        public Factions OwningFaction { get; set; }
        public Factions OriginalFaction { get; set; }

        public Planet(PlanetType planetType, Factions owningFaction, Factions originalFaction, int captureTurn, int devestationTurn)
        {
            PlanetType = planetType;
            OwningFaction = owningFaction;
            OriginalFaction = originalFaction;
            CaptureTurn = captureTurn;
            DevestationTurn = devestationTurn;
        }

    }
}

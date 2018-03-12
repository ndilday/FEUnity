using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Models
{
    [Flags]
    public enum ShipRoles : byte
    {
        None = 0,
        Escort = 1,
        Scout = 2,
        PFTender = 4,
        Fast = 8,
        Tug = 16,
        Commando = 32,
        Mauler = 64,
        Survey = 128
    }

    public class ShipClass
    {
        public int Id { get; private set; }
        public Factions Faction { get; private set; }
        public string Designation { get; private set; }
        public int Offense { get; private set; }
        public int Defense { get; private set; }
        public int CrippledOffense { get; private set; }
        public int CrippledDefense { get; private set; }
        public int Command { get; private set; }
        public int DateAvailable { get; private set; }
        public float SalvageValue { get; private set; }
        public float Cost { get; private set; }
        public float FighterFactors { get; private set; }
        public float CrippledFighterFactors { get; private set; }
        public int DroneBombardmentFactor { get; private set; }
        public float HeavyFighterFactors { get; private set; }
        public float CrippledHeavyFighterFactors { get; private set; }
        public ShipRoles ShipRoles { get; private set; }
        public string Factors { get; private set; }
        public string CrippledFactors { get; private set; }

        public ShipClass(int id, Factions faction, string designation, int offense, int defense, int crippledOffense, int crippledDefense,
            int command, int dateAvailable, float salvageValue, float cost, float fighterFactors, float crippledFighterFactors,
            float heavyFighterFactors, float crippledHeavyFighterFactors, int droneFactors, ShipRoles shipRoles)
        {
            Id = id;
            Faction = faction;
            Designation = designation;
            Offense = offense;
            Defense = defense;
            CrippledOffense = crippledOffense;
            CrippledDefense = crippledDefense;
            Command = command;
            DateAvailable = dateAvailable;
            SalvageValue = salvageValue;
            Cost = cost;
            FighterFactors = fighterFactors;
            CrippledFighterFactors = crippledFighterFactors;
            HeavyFighterFactors = heavyFighterFactors;
            CrippledHeavyFighterFactors = crippledHeavyFighterFactors;
            DroneBombardmentFactor = droneFactors;
            ShipRoles = shipRoles;
            BuildFactorsStrings();
        }

        public string GetCostReadout()
        {
            return Designation + " " + Factors + ": " + Cost + "EPs";
        }

        public void BuildFactorsStrings()
        {
            Factors = Offense.ToString() + "-" + Defense.ToString();
            CrippledFactors = CrippledOffense.ToString() + "-" + CrippledDefense.ToString();

            if ((ShipRoles & ShipRoles.PFTender) == ShipRoles.PFTender)
            {
                Factors += 'P';
                CrippledFactors += 'P';
            }
            if (FighterFactors > 0)
            {
                Factors += '(' + FighterFactors + ')';
            }
            if(CrippledFighterFactors > 0)
            {
                CrippledFactors += '(' + CrippledFighterFactors + ')';
            }
            if(HeavyFighterFactors > 0)
            {
                Factors += '(' + HeavyFighterFactors + "H)";
            }
            if(CrippledHeavyFighterFactors > 0)
            {
                CrippledFactors += '(' + CrippledHeavyFighterFactors + "H)";
            }
            if(DroneBombardmentFactor > 0)
            {
                Factors += '<' + DroneBombardmentFactor + '>';
            }
            if ((ShipRoles & ShipRoles.Escort) == ShipRoles.Escort)
            {
                Factors += "\u23f9";
                CrippledFactors += "\u23f9";
            }
            if ((ShipRoles & ShipRoles.Mauler) == ShipRoles.Mauler)
            {
                Factors += "\u271b";
                CrippledFactors += "\u271b";
            }
            if ((ShipRoles & ShipRoles.Survey) == ShipRoles.Survey)
            {
                Factors += "\u25c7";
            }
            else if((ShipRoles & ShipRoles.Scout) == ShipRoles.Scout)
            {
                Factors += "\u25c6";
            }
        }
    }
}

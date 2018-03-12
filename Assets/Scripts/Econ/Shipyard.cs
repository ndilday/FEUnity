using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Models;

namespace Assets.Scripts.Econ
{
    public class Shipyard
    {
        private Dictionary<ShipClass, List<Conversion>> _conversionMap;
        private Dictionary<ShipClass, List<Substitution>> _substitutionMap;

        public ConstructionPlan ConstructionPlan { get; private set; }
        
        public Shipyard(ConstructionPlan constructionPlan, List<Conversion> conversions, List<Substitution> substitutions)
        {
            _conversionMap = conversions.GroupBy(c => c.OriginalClass).ToDictionary(g => g.Key, g => g.ToList());
            _substitutionMap = substitutions.GroupBy(c => c.BaseClass).ToDictionary(g => g.Key, g => g.ToList());
            ConstructionPlan = constructionPlan;
        }

        public IEnumerable<Conversion> GetPossibleConversions(ShipClass baseClass, bool minorOnly, int turn = int.MaxValue)
        {
            return (minorOnly ? _conversionMap[baseClass].Where(c => (c.IsMinor || c.Cost <= 3.0f) && c.NewClass.DateAvailable <= turn) : _conversionMap[baseClass]).Where(c => c.NewClass.DateAvailable <= turn);
        }

        public List<Substitution> GetPossibleSubstitutions(ShipClass baseClass)
        {
            return _substitutionMap[baseClass];
        }
    }
}

using System.Collections.Generic;
using Assets.Scripts.Models;

namespace Assets.Scripts.Econ
{
    public class ConstructionPlan
    {
        private SortedList<int, List<ShipClass>> _springPlans;
        private SortedList<int, List<ShipClass>> _fallPlans;


        public ConstructionPlan(SortedList<int, List<ShipClass>> springPlans, SortedList<int, List<ShipClass>> fallPlans)
        {
            _springPlans = springPlans;
            _fallPlans = fallPlans;
        }

        public List<ShipClass> GetBasePlanForTurn(int turn)
        {
            return turn % 2 == 0 ? GetPlan(turn, _fallPlans) : GetPlan(turn, _springPlans);
        }

        private List<ShipClass> GetPlan(int turn, SortedList<int, List<ShipClass>> planList)
        {
            if(planList == null || planList.Count == 0)
            {
                return null;
            }
            if(planList.Count == 1 && planList.Keys[0] <= turn)
            {
                return planList.Values[0];
            }
            // TODO: finsih
            return null;
        }
    }
}

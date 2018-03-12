using Assets.Scripts.Models;

namespace Assets.Scripts.Econ
{
    public class Conversion
    {
        public ShipClass OriginalClass { get; private set; }
        public ShipClass NewClass { get; private set; }
        public float Cost { get; private set; }
        public bool IsMinor { get; private set; }

        public Conversion(ShipClass originalClass, ShipClass newClass, float cost, bool isMinor)
        {
            OriginalClass = originalClass;
            NewClass = newClass;
            Cost = cost;
            IsMinor = isMinor;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Scripts.Models;

namespace Assets.Scripts.Econ
{
    public class Substitution
    {
        public ShipClass BaseClass { get; private set; }
        public ShipClass NewClass { get; private set; }

        public Substitution(ShipClass baseClass, ShipClass newClass)
        {
            BaseClass = baseClass;
            NewClass = newClass;
        }
    }
}

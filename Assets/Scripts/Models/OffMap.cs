

namespace Assets.Scripts.Models
{
    public class OffMap
    {
        public int ProvinceCount { get; private set; }
        public int MinorPlanetCount { get; private set; }
        public int MajorPlanetCount { get; private set; }
        public int StarbaseCount { get; private set; }

        public OffMap(int provinces, int minors, int majors, int starbases)
        {
            ProvinceCount = provinces;
            MinorPlanetCount = minors;
            MajorPlanetCount = majors;
            StarbaseCount = starbases;
        }
    }
}

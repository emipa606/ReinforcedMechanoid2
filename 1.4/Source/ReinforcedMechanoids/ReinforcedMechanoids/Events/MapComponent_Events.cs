using System.Reflection;
using Verse;

namespace ReinforcedMechanoids
{

    public class MapComponent_Events : MapComponent
    {
        public int lastVultureFlockEventTick;
        public MapComponent_Events(Map map) : base(map)
        {

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastVultureFlockEventTick, "lastVultureFlockEventTick");
        }
    }
}

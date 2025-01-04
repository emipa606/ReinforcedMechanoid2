using Verse;

namespace ReinforcedMechanoids;

public class MapComponent_Events(Map map) : MapComponent(map)
{
    public int lastVultureFlockEventTick;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref lastVultureFlockEventTick, "lastVultureFlockEventTick");
    }
}
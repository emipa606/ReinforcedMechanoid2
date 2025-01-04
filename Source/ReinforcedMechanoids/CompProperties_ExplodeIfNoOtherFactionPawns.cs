using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_ExplodeIfNoOtherFactionPawns : CompProperties
{
    public CompProperties_ExplodeIfNoOtherFactionPawns()
    {
        compClass = typeof(CompExplodeIfNoOtherFactionPawns);
    }
}
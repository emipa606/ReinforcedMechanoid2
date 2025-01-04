using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_InitialHediff : CompProperties
{
    public BodyPartDef bodyPart;
    public HediffDef hediffDef;

    public float severity;

    public CompProperties_InitialHediff()
    {
        compClass = typeof(CompInitialHediff);
    }
}
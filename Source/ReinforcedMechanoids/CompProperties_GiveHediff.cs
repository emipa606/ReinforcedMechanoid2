using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_GiveHediff : CompProperties
{
    public IntRange cooldownTicksRange;

    public bool cooldownWhenInCombat;

    public bool disableWhenInCombat;
    public HediffDef hediff;

    public bool removeWhenDamaged;

    public CompProperties_GiveHediff()
    {
        compClass = typeof(CompGiveHediff);
    }
}
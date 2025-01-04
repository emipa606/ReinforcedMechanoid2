using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_Invisibility : CompProperties
{
    public IntRange cooldownTicksRange;

    public bool cooldownWhenInCombat;

    public bool disableWhenInCombat;

    public bool removeWhenDamaged;

    public CompProperties_Invisibility()
    {
        compClass = typeof(CompInvisibility);
    }
}
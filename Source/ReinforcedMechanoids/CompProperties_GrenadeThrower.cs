using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_GrenadeThrower : CompProperties
{
    public IntRange grenadeDropTickInterval;

    public ThingDef grenadeLauncher;

    public float maxDistance;

    public BodyPartDef requiredBodyPart;

    public CompProperties_GrenadeThrower()
    {
        compClass = typeof(CompGrenadeThrower);
    }
}
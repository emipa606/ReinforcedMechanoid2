using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_PawnJumper : CompProperties
{
    public EffecterDef flightEffecterDef;
    public IntRange jumpTickRateInterval;

    public float maxJumpDistance;

    public float minJumpDistance;

    public float? minLandDistanceToThingTarget;

    public SoundDef soundLanding;

    public EffecterDef warmupEffecter;

    public float warmupTime;

    public CompProperties_PawnJumper()
    {
        compClass = typeof(CompPawnJumper);
    }
}
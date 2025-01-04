using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_MentalStateOnDamage : CompProperties
{
    public readonly float maxHealthPctCondition = 1f;

    public float chanceOfMentalBreak;

    public HediffDef hediff;

    public MentalStateDef mentalState;

    public CompProperties_MentalStateOnDamage()
    {
        compClass = typeof(CompMentalStateOnDamage);
    }
}
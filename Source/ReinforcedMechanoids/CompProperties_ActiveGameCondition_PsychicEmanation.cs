using RimWorld;
using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_ActiveGameCondition_PsychicEmanation : CompProperties
{
    public readonly int droneLevelIncreaseInterval = int.MinValue;
    public PsychicDroneLevel droneLevel = PsychicDroneLevel.BadMedium;

    public CompProperties_ActiveGameCondition_PsychicEmanation()
    {
        compClass = typeof(CompActiveGameCondition_PsychicEmanation);
    }
}
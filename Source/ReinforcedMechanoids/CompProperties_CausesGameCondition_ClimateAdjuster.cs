using RimWorld;

namespace ReinforcedMechanoids;

public class CompProperties_CausesGameCondition_ClimateAdjuster : CompProperties_CausesGameCondition
{
    public CompProperties_CausesGameCondition_ClimateAdjuster()
    {
        compClass = typeof(CompCauseGameCondition_TemperatureOffset);
    }
}
using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_ActiveGameCondition : CompProperties
{
    public GameConditionDef conditionDef;

    public CompProperties_ActiveGameCondition()
    {
        compClass = typeof(CompActiveGameCondition);
    }
}
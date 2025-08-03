using RimWorld;
using Verse;

namespace ReinforcedMechanoids;

public class CompCauseGameConditionPowerDependent : CompCauseGameCondition
{
    public CompPowerTrader compPower;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        compPower = parent.TryGetComp<CompPowerTrader>();
    }

    public override void SetupCondition(GameCondition condition, Map map)
    {
        base.SetupCondition(condition, map);
        condition.conditionCauser = parent;
    }
}
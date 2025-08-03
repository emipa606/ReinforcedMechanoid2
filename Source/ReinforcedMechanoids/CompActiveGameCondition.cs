using RimWorld;
using Verse;

namespace ReinforcedMechanoids;

public class CompActiveGameCondition : ThingComp
{
    protected GameCondition gameCondition;

    private CompProperties_ActiveGameCondition Props => (CompProperties_ActiveGameCondition)props;

    private GameConditionDef ConditionDef => Props.conditionDef;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (gameCondition == null && parent?.Map != null)
        {
            gameCondition = CreateConditionOn(parent.Map);
        }
    }

    public override void CompTick()
    {
        if (gameCondition == null && parent?.Map != null)
        {
            gameCondition = CreateConditionOn(parent.Map);
        }
    }

    protected virtual GameCondition CreateConditionOn(Map map)
    {
        var condition = GameConditionMaker.MakeCondition(ConditionDef);
        condition.Permanent = true;
        condition.conditionCauser = parent;
        map.gameConditionManager.RegisterCondition(condition);
        SetupCondition(condition, map);
        return condition;
    }

    protected virtual void SetupCondition(GameCondition condition, Map map)
    {
        condition.suppressEndMessage = true;
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        Messages.Message("MessageConditionCauserDespawned".Translate(parent.def.LabelCap),
            new TargetInfo(parent.Position, previousMap), MessageTypeDefOf.NeutralEvent);
        gameCondition.End();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_References.Look(ref gameCondition, "gameCondition");
    }
}
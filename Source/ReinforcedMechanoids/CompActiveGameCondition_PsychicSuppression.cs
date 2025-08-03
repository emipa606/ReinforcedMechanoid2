using RimWorld;
using Verse;

namespace ReinforcedMechanoids;

public class CompActiveGameCondition_PsychicSuppression : CompActiveGameCondition
{
    private Gender gender;

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        gender = Rand.Bool ? Gender.Male : Gender.Female;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref gender, "gender");
        Scribe_References.Look(ref gameCondition, "gameCondition");
    }

    public override void CompTick()
    {
        base.CompTick();
        if (gameCondition == null && parent?.Map != null)
        {
            gameCondition = CreateConditionOn(parent.Map);
        }
    }

    public override string CompInspectStringExtra()
    {
        var text = base.CompInspectStringExtra();
        if (!text.NullOrEmpty())
        {
            text += "\n";
        }

        return text + ("AffectedGender".Translate() + ": " + gender.GetLabel().CapitalizeFirst());
    }

    protected override GameCondition CreateConditionOn(Map map)
    {
        var condition = GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicSuppression);
        condition.Permanent = true;
        condition.conditionCauser = parent;
        map.gameConditionManager.RegisterCondition(condition);
        SetupCondition(condition, map);
        return condition;
    }

    protected override void SetupCondition(GameCondition condition, Map map)
    {
        ((GameCondition_PsychicSuppression)condition).gender = gender;
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        Messages.Message("MessageConditionCauserDespawned".Translate(parent.def.LabelCap),
            new TargetInfo(parent.Position, previousMap), MessageTypeDefOf.NeutralEvent);
        gameCondition.End();
    }
}
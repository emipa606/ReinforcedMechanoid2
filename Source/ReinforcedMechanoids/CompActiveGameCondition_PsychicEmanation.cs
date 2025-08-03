using RimWorld;
using Verse;
using Verse.Sound;

namespace ReinforcedMechanoids;

public class CompActiveGameCondition_PsychicEmanation : CompActiveGameCondition
{
    private PsychicDroneLevel droneLevel = PsychicDroneLevel.BadHigh;

    private Gender gender;

    private int ticksToIncreaseDroneLevel;

    private CompProperties_ActiveGameCondition_PsychicEmanation Props =>
        (CompProperties_ActiveGameCondition_PsychicEmanation)props;

    private PsychicDroneLevel Level => droneLevel;

    private bool DroneLevelIncreases => Props.droneLevelIncreaseInterval != int.MinValue;

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        gender = Rand.Bool ? Gender.Male : Gender.Female;
        droneLevel = PsychicDroneLevel.BadMedium;
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (respawningAfterLoad || !DroneLevelIncreases)
        {
            return;
        }

        ticksToIncreaseDroneLevel = Props.droneLevelIncreaseInterval;
        SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(parent.Map);
    }

    public override void CompTick()
    {
        base.CompTick();
        if (gameCondition == null && parent?.Map != null)
        {
            gameCondition = CreateConditionOn(parent.Map);
        }

        if (parent != null && (!parent.Spawned || !DroneLevelIncreases))
        {
            return;
        }

        ticksToIncreaseDroneLevel--;
        if (ticksToIncreaseDroneLevel > 0)
        {
            return;
        }

        IncreaseDroneLevel();
        ticksToIncreaseDroneLevel = Props.droneLevelIncreaseInterval;
    }

    private void IncreaseDroneLevel()
    {
        if (droneLevel == PsychicDroneLevel.BadExtreme)
        {
            return;
        }

        droneLevel++;
        var taggedString = "LetterPsychicDroneLevelIncreased".Translate(gender.GetLabel());
        Find.LetterStack.ReceiveLetter("LetterLabelPsychicDroneLevelIncreased".Translate(), taggedString,
            LetterDefOf.NegativeEvent);
        SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(parent.Map);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref gender, "gender");
        Scribe_Values.Look(ref ticksToIncreaseDroneLevel, "ticksToIncreaseDroneLevel");
        Scribe_Values.Look(ref droneLevel, "droneLevel");
        Scribe_References.Look(ref gameCondition, "gameCondition");
    }

    public override string CompInspectStringExtra()
    {
        var text = base.CompInspectStringExtra();
        if (!text.NullOrEmpty())
        {
            text += "\n";
        }

        return text + ("AffectedGender".Translate() + ": " + gender.GetLabel().CapitalizeFirst() + "\n" +
                       "PsychicDroneLevel".Translate(droneLevel.GetLabelCap()));
    }

    protected override GameCondition CreateConditionOn(Map map)
    {
        var condition = GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicDrone);
        condition.Permanent = true;
        condition.conditionCauser = parent;
        map.gameConditionManager.RegisterCondition(condition);
        SetupCondition(condition, map);
        return condition;
    }

    protected override void SetupCondition(GameCondition condition, Map map)
    {
        var gameCondition_PsychicEmanation = (GameCondition_PsychicEmanation)condition;
        gameCondition_PsychicEmanation.gender = gender;
        gameCondition_PsychicEmanation.level = Level;
        condition.suppressEndMessage = true;
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        Messages.Message("MessageConditionCauserDespawned".Translate(parent.def.LabelCap),
            new TargetInfo(parent.Position, previousMap), MessageTypeDefOf.NeutralEvent);
        gameCondition.End();
    }
}
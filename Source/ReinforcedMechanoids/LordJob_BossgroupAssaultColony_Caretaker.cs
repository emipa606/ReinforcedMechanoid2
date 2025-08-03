using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace ReinforcedMechanoids;

public class LordJob_BossgroupAssaultColony_Caretaker : LordJob
{
    private static readonly IntRange PrepareTicksRange = new(5000, 10000);

    private List<Pawn> bosses = [];

    private Faction faction;

    private IntVec3 stageLoc;

    public LordJob_BossgroupAssaultColony_Caretaker()
    {
    }

    public LordJob_BossgroupAssaultColony_Caretaker(Faction faction, IntVec3 stageLoc, IEnumerable<Pawn> bosses)
    {
        this.faction = faction;
        this.stageLoc = stageLoc;
        this.bosses.AddRange(bosses);
    }

    public override StateGraph CreateGraph()
    {
        var stateGraph = new StateGraph();
        var firstSource = (LordToil_Stage)(stateGraph.StartingToil = new LordToil_Stage(stageLoc));
        var lordToilMoveInBossgroupCaretaker = new LordToil_MoveInBossgroup_Caretaker(bosses);
        stateGraph.AddToil(lordToilMoveInBossgroupCaretaker);
        var lordToil_AssaultColonyBossgroup = new LordToil_AssaultColonyBossgroup();
        stateGraph.AddToil(lordToil_AssaultColonyBossgroup);
        var transition = new Transition(firstSource, lordToilMoveInBossgroupCaretaker);
        transition.AddTrigger(
            new Trigger_TicksPassed(PrepareTicksRange.RandomInRange));
        if (faction != null)
        {
            transition.AddPreAction(new TransitionAction_Message(
                "MessageRaidersBeginningAssault".Translate(faction.def.pawnsPlural.CapitalizeFirst(),
                    faction.Name), MessageTypeDefOf.ThreatBig));
        }

        transition.AddPostAction(new TransitionAction_WakeAll());
        stateGraph.AddTransition(transition);
        var transition2 = new Transition(firstSource, lordToil_AssaultColonyBossgroup);
        transition2.AddTrigger(new Trigger_SpecificPawnLost(1f, false, null, bosses));
        stateGraph.AddTransition(transition2);
        var transition3 =
            new Transition(lordToilMoveInBossgroupCaretaker, lordToil_AssaultColonyBossgroup);
        transition3.AddTrigger(new Trigger_SpecificPawnLost(1f, false, null, bosses));
        stateGraph.AddTransition(transition3);
        return stateGraph;
    }

    public override void ExposeData()
    {
        Scribe_References.Look(ref faction, "faction");
        Scribe_Values.Look(ref stageLoc, "stageLoc");
        Scribe_Collections.Look(ref bosses, "bosses", LookMode.Reference);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            bosses.RemoveAll(x => x == null);
        }
    }
}
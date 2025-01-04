using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids;

public class LordJob_AssaultColony_WraithSiege : LordJob_AssaultColony_ProtectPawn
{
    public float additionalRaidPoints;

    public float additionalRaidPointsSaved;

    public float blueprintPoints;

    public IntVec3 siegeSpot = IntVec3.Invalid;
    public bool siegeStarted;

    public LordJob_AssaultColony_WraithSiege()
    {
    }

    public LordJob_AssaultColony_WraithSiege(Faction assaulterFaction, float blueprintPoints,
        float additionalRaidPoints, bool canKidnap = true, bool canTimeoutOrFlee = true, bool sappers = false,
        bool useAvoidGridSmart = false, bool canSteal = true, bool breachers = false,
        bool canPickUpOpportunisticWeapons = false)
    {
        this.blueprintPoints = blueprintPoints;
        this.additionalRaidPoints = additionalRaidPointsSaved = additionalRaidPoints;
        this.assaulterFaction = assaulterFaction;
        this.canKidnap = canKidnap;
        this.canTimeoutOrFlee = canTimeoutOrFlee;
        this.sappers = sappers;
        this.useAvoidGridSmart = useAvoidGridSmart;
        this.canSteal = canSteal;
        this.breachers = breachers;
        this.canPickUpOpportunisticWeapons = canPickUpOpportunisticWeapons;
    }

    public float SiegeScale => additionalRaidPointsSaved / 666f;

    public float SiegeRadius
    {
        get
        {
            var min = (int)(14f * Mathf.Max(1f, SiegeScale / 2f));
            var max = (int)(25f * Mathf.Max(1f, SiegeScale));
            var intRange = new IntRange(min, max);
            var value = Mathf.InverseLerp(intRange.min, intRange.max, lord.ownedPawns.Count / 50f);
            return Mathf.Clamp(value, intRange.min, intRange.max);
        }
    }

    public override PawnKindDef ProtecteeKind => RM_DefOf.RM_Mech_WraithSiege;

    public override float MaxDistanceFromProtectee => 20f;

    public override StateGraph CreateGraph()
    {
        var stateGraph = new StateGraph();
        LordToil lordToil = new LordToil_WraithSiege();
        stateGraph.AddToil(lordToil);
        var lordToil_ExitMap = new LordToil_ExitMap(LocomotionUrgency.Jog, false, true)
        {
            useAvoidGrid = true
        };
        stateGraph.AddToil(lordToil_ExitMap);
        if (assaulterFaction == null)
        {
            return stateGraph;
        }

        var transition = new Transition(lordToil, lordToil_ExitMap);
        transition.AddTrigger(new Trigger_BecameNonHostileToPlayer());
        transition.AddPreAction(new TransitionAction_Message(
            "MessageRaidersLeaving".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(),
                assaulterFaction.Name)));
        stateGraph.AddTransition(transition);

        return stateGraph;
    }

    public void StartSiege(IntVec3 siegeSpot)
    {
        siegeStarted = true;
        var lordToil_WraithSiege = lord.CurLordToil as LordToil_WraithSiege;
        lordToil_WraithSiege?.StartSiege(siegeSpot, blueprintPoints);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref siegeSpot, "siegeSpot", IntVec3.Invalid);
        Scribe_Values.Look(ref siegeStarted, "siegeStarted");
        Scribe_Values.Look(ref blueprintPoints, "blueprintPoints");
        Scribe_Values.Look(ref additionalRaidPointsSaved, "additionalRaidPointsSaved");
        Scribe_Values.Look(ref additionalRaidPoints, "additionalRaidPoints");
    }
}
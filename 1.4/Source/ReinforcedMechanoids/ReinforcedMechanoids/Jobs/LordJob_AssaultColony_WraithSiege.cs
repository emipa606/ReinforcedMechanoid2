using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids
{
    public class LordJob_AssaultColony_WraithSiege : LordJob_AssaultColony_ProtectPawn
    {
        public bool siegeStarted;
        public IntVec3 siegeSpot = IntVec3.Invalid;
        public float blueprintPoints;
        public float additionalRaidPointsSaved;
        public float additionalRaidPoints;
        public float SiegeScale => additionalRaidPointsSaved / 666f;
        public float SiegeRadius
        {
            get
            {
                var min = (int)(14f * Mathf.Max(1f, SiegeScale / 2f));
                var max = (int)(25f * Mathf.Max(1f, SiegeScale));
                var siegeSize = new IntRange(min, max);
                var baseRadius = Mathf.InverseLerp(siegeSize.min, siegeSize.max, (float)lord.ownedPawns.Count / 50f);
                baseRadius = Mathf.Clamp(baseRadius, siegeSize.min, siegeSize.max);
                return baseRadius;
            }
        }
        public LordJob_AssaultColony_WraithSiege()
        {

        }
        public LordJob_AssaultColony_WraithSiege(Faction assaulterFaction, float blueprintPoints, float additionalRaidPoints, bool canKidnap = true, bool canTimeoutOrFlee = true, 
            bool sappers = false, bool useAvoidGridSmart = false, bool canSteal = true, bool breachers = false, 
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
        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil lordToil3 = new LordToil_WraithSiege();
            stateGraph.AddToil(lordToil3);
            LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap(LocomotionUrgency.Jog, canDig: false, interruptCurrentJob: true);
            lordToil_ExitMap.useAvoidGrid = true;
            stateGraph.AddToil(lordToil_ExitMap);
            if (assaulterFaction != null)
            {
                Transition transition7 = new Transition(lordToil3, lordToil_ExitMap);
                transition7.AddTrigger(new Trigger_BecameNonHostileToPlayer());
                transition7.AddPreAction(new TransitionAction_Message("MessageRaidersLeaving".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
                stateGraph.AddTransition(transition7);
            }
            return stateGraph;
        }
        public void StartSiege(IntVec3 siegeSpot)
        {
            this.siegeStarted = true;
            var toil = this.lord.CurLordToil as LordToil_WraithSiege;
            toil.StartSiege(siegeSpot, blueprintPoints);
        }
        public override PawnKindDef ProtecteeKind => RM_DefOf.RM_Mech_WraithSiege;
        public override float MaxDistanceFromProtectee => 20;
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
}

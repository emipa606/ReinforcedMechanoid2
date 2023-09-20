using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids
{
    public class LordJob_AssaultColony_CaretakerRaid : LordJob_AssaultColony, ILordJobJobOverride
    {
        public LordJob_AssaultColony_CaretakerRaid()
        {

        }
        public LordJob_AssaultColony_CaretakerRaid(Faction assaulterFaction, bool canKidnap = true, bool canTimeoutOrFlee = true, bool sappers = false, bool useAvoidGridSmart = false, bool canSteal = true, bool breachers = false, bool canPickUpOpportunisticWeapons = false)
        {
            this.assaulterFaction = assaulterFaction;
            this.canKidnap = canKidnap;
            this.canTimeoutOrFlee = canTimeoutOrFlee;
            this.sappers = sappers;
            this.useAvoidGridSmart = useAvoidGridSmart;
            this.canSteal = canSteal;
            this.breachers = breachers;
            this.canPickUpOpportunisticWeapons = canPickUpOpportunisticWeapons;
        }

        public bool CanOverrideJobFor(Pawn pawn, Job initialJob)
        {
            return pawn.kindDef != RM_DefOf.RM_Mech_WraithSiege && pawn.kindDef != RM_DefOf.RM_Mech_Locust 
                && pawn.kindDef != RM_DefOf.RM_Mech_Caretaker && (initialJob is null || initialJob.IsCombatJob() is false);
        }

        public Job GetJobFor(Pawn pawn, List<Pawn> otherPawns, Job initialJob = null)
        {
            var otherCaretakers = otherPawns.Where(x => x.kindDef == RM_DefOf.RM_Mech_Caretaker && !x.Downed && !x.Dead).ToList();
            if (otherCaretakers.Count > 0)
            {
                var firstCloseCaretaker = otherCaretakers.FirstOrDefault();
                if (pawn.kindDef == RM_DefOf.RM_Mech_Vulture)
                {
                    var healJob = Utils.HealOtherMechanoidsOrRepairStructures(pawn, otherPawns);
                    if (healJob is null)
                    {
                        var followJob = Utils.TryGiveFollowJob(pawn, firstCloseCaretaker, 6);
                        if (followJob != null)
                        {
                            followJob.locomotionUrgency = LocomotionUrgency.Amble;
                            return followJob;
                        }
                    }
                }
                else
                {
                    var fightEnemiesJobGiver = new JobGiver_AIFightEnemiesNearToMaster();
                    fightEnemiesJobGiver.master = firstCloseCaretaker;
                    fightEnemiesJobGiver.ResolveReferences();
                    fightEnemiesJobGiver.targetAcquireRadius = 12f;
                    fightEnemiesJobGiver.targetKeepRadius = 12f;
                    var fightEnemiesJob = fightEnemiesJobGiver.TryGiveJob(pawn);
                    if (fightEnemiesJob != null)
                    {
                        return fightEnemiesJob;
                    }
                    var centerColony = Utils.FindCenterColony(pawn.Map);
                    var nearestCell = Utils.GetNearestWalkableCellDestination(centerColony, firstCloseCaretaker, out var firstBlockingBuilding, Utils.UnReachabilityCheck);
                    if (firstBlockingBuilding != null && firstBlockingBuilding.Position.DistanceTo(pawn.Position) <= 10)
                    {
                        var attackJob = Utils.MeleeAttackJob(firstBlockingBuilding);
                        if (attackJob != null)
                        {
                            return attackJob;
                        }
                    }

                    if (firstCloseCaretaker.CurJobDef == JobDefOf.Wait)
                    {
                        return null;
                    }
                    else
                    {
                        var followJob = Utils.TryGiveFollowJob(pawn, firstCloseCaretaker, 12);
                        if (followJob != null)
                        {
                            followJob.locomotionUrgency = LocomotionUrgency.Amble;
                            return followJob;
                        }
                    }
                }
            }
            return null;
        }
    }
}

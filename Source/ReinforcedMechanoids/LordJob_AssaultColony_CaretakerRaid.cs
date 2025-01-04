using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

public class LordJob_AssaultColony_CaretakerRaid : LordJob_AssaultColony, ILordJobJobOverride
{
    public LordJob_AssaultColony_CaretakerRaid()
    {
    }

    public LordJob_AssaultColony_CaretakerRaid(Faction assaulterFaction, bool canKidnap = true,
        bool canTimeoutOrFlee = true, bool sappers = false, bool useAvoidGridSmart = false, bool canSteal = true,
        bool breachers = false, bool canPickUpOpportunisticWeapons = false)
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
        return pawn.kindDef != RM_DefOf.RM_Mech_WraithSiege && pawn.kindDef != RM_DefOf.RM_Mech_Locust &&
               pawn.kindDef != RM_DefOf.RM_Mech_Caretaker && (initialJob == null || !initialJob.IsCombatJob());
    }

    public Job GetJobFor(Pawn pawn, List<Pawn> otherPawns, Job initialJob = null)
    {
        var list = otherPawns.Where(x => x.kindDef == RM_DefOf.RM_Mech_Caretaker && !x.Downed && !x.Dead).ToList();
        if (list.Count <= 0)
        {
            return null;
        }

        var pawn2 = list.FirstOrDefault();
        if (pawn.kindDef == RM_DefOf.RM_Mech_Vulture)
        {
            var job = Utils.HealOtherMechanoidsOrRepairStructures(pawn, otherPawns);
            if (job != null)
            {
                return null;
            }

            var job2 = Utils.TryGiveFollowJob(pawn, pawn2, 6f);
            if (job2 == null)
            {
                return null;
            }

            job2.locomotionUrgency = LocomotionUrgency.Amble;
            return job2;
        }

        var jobGiver_AIFightEnemiesNearToMaster = new JobGiver_AIFightEnemiesNearToMaster
        {
            master = pawn2
        };
        jobGiver_AIFightEnemiesNearToMaster.ResolveReferences();
        jobGiver_AIFightEnemiesNearToMaster.targetAcquireRadius = 12f;
        jobGiver_AIFightEnemiesNearToMaster.targetKeepRadius = 12f;
        var job3 = jobGiver_AIFightEnemiesNearToMaster.TryGiveJob(pawn);
        if (job3 != null)
        {
            return job3;
        }

        var destination = Utils.FindCenterColony(pawn.Map);
        Utils.GetNearestWalkableCellDestination(destination, pawn2,
            out var firstBlockingBuilding, Utils.UnReachabilityCheck);
        if (firstBlockingBuilding != null && firstBlockingBuilding.Position.DistanceTo(pawn.Position) <= 10f)
        {
            var job4 = Utils.MeleeAttackJob(firstBlockingBuilding);
            if (job4 != null)
            {
                return job4;
            }
        }

        if (pawn2?.CurJobDef == JobDefOf.Wait)
        {
            return null;
        }

        var job5 = Utils.TryGiveFollowJob(pawn, pawn2, 12f);
        if (job5 == null)
        {
            return null;
        }

        job5.locomotionUrgency = LocomotionUrgency.Amble;
        return job5;
    }
}
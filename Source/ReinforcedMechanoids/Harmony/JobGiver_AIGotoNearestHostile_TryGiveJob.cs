using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(JobGiver_AIGotoNearestHostile), "TryGiveJob")]
public static class JobGiver_AIGotoNearestHostile_TryGiveJob
{
    public static bool Prefix(Pawn pawn, ref Job __result)
    {
        return JobGiver_AIFightEnemy_TryGiveJob_HotSwappable.TryModifyJob(pawn, ref __result);
    }

    public static bool UnReachabilityCheck(Pawn pawn, IntVec3 cell, Building building)
    {
        return cell.Roofed(pawn.Map) || building != null || !pawn.CanReach(cell, PathEndMode.OnCell, Danger.None);
    }

    public static void Postfix(ref Job __result, Pawn pawn)
    {
        if (__result != null && __result.def == JobDefOf.Goto)
        {
            JobGiver_AIFightEnemy_TryGiveJob.gotoCombatJobs.Add(__result);
            return;
        }

        var comp = pawn.GetComp<CompPawnJumper>();
        if (comp is not { JumpAllowed: true } || pawn.Faction == Faction.OfPlayer)
        {
            return;
        }

        var num = float.MaxValue;
        Thing thing = null;
        var potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
        foreach (var attackTarget in potentialTargetsFor)
        {
            if (attackTarget.ThreatDisabled(pawn) || !AttackTargetFinder.IsAutoTargetable(attackTarget))
            {
                continue;
            }

            var thing2 = (Thing)attackTarget;
            if (!comp.CanBeJumpedTo(thing2.Position))
            {
                continue;
            }

            var num2 = thing2.Position.DistanceToSquared(pawn.Position);
            if (!(num2 < num) || !comp.CanJumpTo(thing2) && !Utils.GetNearestWalkableCellDestination(thing2.Position,
                        pawn, out _, Utils.UnReachabilityCheck, comp.Props.maxJumpDistance)
                    .IsValid)
            {
                continue;
            }

            num = num2;
            thing = thing2;
        }

        if (thing == null)
        {
            return;
        }

        if (comp.CanJumpTo(thing))
        {
            var job = JobMaker.MakeJob(JobDefOf.Goto, thing);
            job.checkOverrideOnExpire = true;
            job.expiryInterval = 500;
            job.collideWithPawns = true;
            __result = job;
            JobGiver_AIFightEnemy_TryGiveJob.gotoCombatJobs.Add(job);
            return;
        }

        var nearestWalkableCellDestination = Utils.GetNearestWalkableCellDestination(thing.Position, pawn,
            out _, Utils.UnReachabilityCheck, comp.Props.maxJumpDistance);
        if (nearestWalkableCellDestination == pawn.Position)
        {
            return;
        }

        var job2 = JobMaker.MakeJob(JobDefOf.Goto, nearestWalkableCellDestination);
        job2.checkOverrideOnExpire = true;
        job2.expiryInterval = 500;
        job2.collideWithPawns = true;
        __result = job2;
    }
}
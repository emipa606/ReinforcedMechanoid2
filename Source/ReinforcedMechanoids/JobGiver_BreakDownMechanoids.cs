using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids;

public class JobGiver_BreakDownMechanoids : ThinkNode_JobGiver
{
    public override Job TryGiveJob(Pawn pawn)
    {
        var job = Utils.FleeIfEnemiesAreNearby(pawn);
        if (job != null)
        {
            return job;
        }

        var lord = pawn.GetLord();
        if (lord.LordJob is not LordJob_BreakDownMechanoids lordJob_BreakDownMechanoids)
        {
            return null;
        }

        if (Find.TickManager.TicksGame - lordJob_BreakDownMechanoids.tickStarted >= 60000)
        {
            goto IL_00c3;
        }

        if (lordJob_BreakDownMechanoids.mechanoidCorpses.All(x => x.Destroyed))
        {
            var extractedThings = lordJob_BreakDownMechanoids.extractedThings;
            if (extractedThings != null && extractedThings.All(x => !x.Spawned))
            {
                goto IL_00c3;
            }
        }

        if (!lordJob_BreakDownMechanoids.mechanoidCorpses
                .Where(x => pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly))
                .TryRandomElement(out var result))
        {
            return null;
        }

        job = JobMaker.MakeJob(RM_DefOf.RM_BreakDownMechanoid, result, result.Position);
        job.expiryInterval = 60;
        job.expireRequiresEnemiesNearby = true;
        return job;

        IL_00c3:
        return JobDriver_BreakDownMechanoid.GetLeaveMapJob(pawn, lord, lordJob_BreakDownMechanoids);
    }
}
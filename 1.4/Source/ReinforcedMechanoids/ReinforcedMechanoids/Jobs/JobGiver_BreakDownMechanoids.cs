using RimWorld;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids
{
    public class JobGiver_BreakDownMechanoids : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            var job = Utils.FleeIfEnemiesAreNearby(pawn);
            if (job is null)
            {
                var lord = pawn.GetLord();
                var lordJob = lord.LordJob as LordJob_BreakDownMechanoids;
                if (Find.TickManager.TicksGame - lordJob.tickStarted >= GenDate.TicksPerDay || lordJob.mechanoidCorpses.All(x => x.Destroyed) && (lordJob.extractedThings?.All(x => x.Spawned is false) ?? false))
                {
                    return JobDriver_BreakDownMechanoid.GetLeaveMapJob(pawn, lord, lordJob);
                }
                if (lordJob.mechanoidCorpses.Where(x => pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly)).TryRandomElement(out var corpse))
                {
                    job = JobMaker.MakeJob(RM_DefOf.RM_BreakDownMechanoid, corpse, corpse.Position);
                    job.expiryInterval = 60;
                    job.expireRequiresEnemiesNearby = true;
                    return job;
                }
            }
            return job;
        }
    }
}

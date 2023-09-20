using RimWorld;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace ReinforcedMechanoids
{
    public class JobGiver_WalkToPlayerBase : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Faction != Faction.OfPlayer)
            {
                var otherPawns = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction)
                    .Where(x => x.RaceProps.IsMechanoid && !x.Fogged() && !x.Dead && x.Awake()).Except(pawn)
                    .OrderBy(x => x.Position.DistanceTo(pawn.Position)).ToList();
                if (otherPawns.Count(x => x.IsFighting()) >= otherPawns.Count / 3f)
                {
                    Job job = JobMaker.MakeJob(JobDefOf.Wait_MaintainPosture);
                    job.expiryInterval = 180;
                    return job;
                }
                var centerColony = Utils.FindCenterColony(pawn.Map);
                var nearestCell = Utils.GetNearestWalkableCellDestination(centerColony, pawn, out var firstBlockingBuilding, Utils.UnReachabilityCheck);
                if (nearestCell == pawn.Position)
                {
                    var job = JobMaker.MakeJob(JobDefOf.Wait);
                    job.expiryInterval = 180;
                    return job;
                }
                else if (nearestCell.IsValid)
                {
                    var job = JobMaker.MakeJob(JobDefOf.Goto, nearestCell);
                    job.expiryInterval = 180;
                    return job;
                }
            }
            return null;
        }
    }
}

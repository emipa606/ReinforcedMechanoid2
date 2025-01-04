using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

public class JobGiver_WalkToPlayerBase : ThinkNode_JobGiver
{
    public override Job TryGiveJob(Pawn pawn)
    {
        if (pawn.Faction == Faction.OfPlayer)
        {
            return null;
        }

        var list = (from x in (from x in pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction)
                where x.RaceProps.IsMechanoid && !x.Fogged() && !x.Dead && x.Awake()
                select x).Except(pawn)
            orderby x.Position.DistanceTo(pawn.Position)
            select x).ToList();
        if (list.Count(x => x.IsFighting()) >= list.Count / 3f)
        {
            var job = JobMaker.MakeJob(JobDefOf.Wait_MaintainPosture);
            job.expiryInterval = 180;
            return job;
        }

        var destination = Utils.FindCenterColony(pawn.Map);
        var nearestWalkableCellDestination = Utils.GetNearestWalkableCellDestination(destination, pawn,
            out _, Utils.UnReachabilityCheck);
        if (nearestWalkableCellDestination == pawn.Position)
        {
            var job2 = JobMaker.MakeJob(JobDefOf.Wait);
            job2.expiryInterval = 180;
            return job2;
        }

        if (!nearestWalkableCellDestination.IsValid)
        {
            return null;
        }

        var job3 = JobMaker.MakeJob(JobDefOf.Goto, nearestWalkableCellDestination);
        job3.expiryInterval = 180;
        return job3;
    }
}
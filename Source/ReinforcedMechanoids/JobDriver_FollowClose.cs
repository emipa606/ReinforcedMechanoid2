using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

public class JobDriver_FollowClose : JobDriver
{
    private const TargetIndex FolloweeInd = TargetIndex.A;

    private const int CheckPathIntervalTicks = 30;

    private Pawn Followee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

    private bool CurrentlyWalkingToFollowee
    {
        get
        {
            if (pawn.pather.Moving)
            {
                return pawn.pather.Destination == Followee;
            }

            return false;
        }
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    public override void Notify_Starting()
    {
        base.Notify_Starting();
        if (!(job.followRadius <= 0f))
        {
            return;
        }

        Log.Error($"Follow radius is <= 0. pawn={pawn.ToStringSafe()}");
        job.followRadius = 10f;
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return new Toil
        {
            tickAction = delegate
            {
                var followee = Followee;
                var followRadius = job.followRadius;
                if (pawn.pather.Moving && !pawn.IsHashIntervalTick(30))
                {
                    return;
                }

                var cellToFollow = GetCellToFollow(followee);
                if (pawn.pather.Moving && pawn.pather.Destination.Cell.InHorDistOf(cellToFollow, followRadius))
                {
                    return;
                }

                job.locomotionUrgency = followee.CurJob?.locomotionUrgency ?? LocomotionUrgency.Walk;
                var intVec =
                    CellFinder.RandomClosewalkCellNear(cellToFollow, Map, Mathf.FloorToInt(followRadius));
                if (intVec == pawn.Position)
                {
                    EndJobWith(JobCondition.Succeeded);
                }
                else if (intVec.IsValid && pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly))
                {
                    pawn.pather.StartPath(intVec, PathEndMode.OnCell);
                }
                else
                {
                    EndJobWith(JobCondition.Ongoing | JobCondition.Succeeded);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Never
        };
    }

    private static IntVec3 GetCellToFollow(Pawn target)
    {
        if (!target.pather.Moving || target.pather.curPath == null)
        {
            return target.Position;
        }

        for (var num = 5; num > 0; num--)
        {
            var nodesReversed = target.pather.curPath.NodesReversed;
            if (nodesReversed.Count > num)
            {
                return nodesReversed[num];
            }
        }

        return target.Position;
    }

    public override bool IsContinuation(Job j)
    {
        return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
    }

    public static bool FarEnoughAndPossibleToStartJob(Pawn follower, Pawn followee, float radius)
    {
        if (radius <= 0f)
        {
            var text = $"Checking follow job with radius <= 0. pawn={follower.ToStringSafe()}";
            if (follower.mindState is { duty: not null })
            {
                text = $"{text} duty={follower.mindState.duty.def}";
            }

            Log.ErrorOnce(text, follower.thingIDNumber ^ 0x324308F9);
            return false;
        }

        if (!follower.CanReach(followee, PathEndMode.OnCell, Danger.Deadly))
        {
            return false;
        }

        var radius2 = radius * 1.2f;
        if (!NearFollowee(follower, followee, radius2))
        {
            return true;
        }

        return !NearDestinationOrNotMoving(follower, followee, radius2) &&
               follower.CanReach(followee.pather.LastPassableCellInPath, PathEndMode.OnCell, Danger.Deadly);
    }

    private static bool NearFollowee(Pawn follower, Pawn followee, float radius)
    {
        if (follower.Position.AdjacentTo8WayOrInside(followee.Position))
        {
            return true;
        }

        return follower.Position.InHorDistOf(followee.Position, radius) &&
               GenSight.LineOfSight(follower.Position, followee.Position, follower.Map, false);
    }

    private static bool NearDestinationOrNotMoving(Pawn follower, Pawn followee, float radius)
    {
        if (!followee.pather.Moving)
        {
            return true;
        }

        var lastPassableCellInPath = followee.pather.LastPassableCellInPath;
        if (!lastPassableCellInPath.IsValid)
        {
            return true;
        }

        return follower.Position.AdjacentTo8WayOrInside(lastPassableCellInPath) ||
               follower.Position.InHorDistOf(lastPassableCellInPath, radius);
    }
}
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

public class JobDriver_RepairThing : JobDriver
{
    protected float ticksToNextRepair;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch)
            .FailOnDespawnedNullOrForbidden(TargetIndex.A)
            .FailOn(() => !pawn.CanReach(job.GetTarget(TargetIndex.A), PathEndMode.Touch, Danger.None))
            .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
        var repair = new Toil
        {
            initAction = delegate { ticksToNextRepair = 80f; }
        };
        repair.tickAction = delegate
        {
            var actor = repair.actor;
            actor.rotationTracker.FaceTarget(actor.CurJob.GetTarget(TargetIndex.A));
            var num = 1f;
            ticksToNextRepair -= num;
            if (!(ticksToNextRepair <= 0f))
            {
                return;
            }

            ticksToNextRepair += 35f;
            TargetThingA.HitPoints++;
            TargetThingA.HitPoints = Mathf.Min(TargetThingA.HitPoints, TargetThingA.MaxHitPoints);
            Map.listerBuildingsRepairable.Notify_BuildingRepaired((Building)TargetThingA);
            if (TargetThingA.HitPoints == TargetThingA.MaxHitPoints)
            {
                actor.jobs.EndCurrentJob(JobCondition.Succeeded);
            }
        };
        repair.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
        repair.WithEffect(TargetThingA.def.repairEffect, TargetIndex.A);
        repair.defaultCompleteMode = ToilCompleteMode.Never;
        repair.handlingFacing = true;
        yield return repair;
    }
}
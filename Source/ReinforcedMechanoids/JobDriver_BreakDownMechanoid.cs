using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids;

public class JobDriver_BreakDownMechanoid : JobDriver
{
    public Corpse Corpse => TargetA.Thing as Corpse;

    public IntVec3 InitialCell => TargetB.Cell;

    public int BreakdownDuration =>
        (int)(300f * Corpse.InnerPawn.HealthScale * (Corpse.HitPoints / (float)Corpse.MaxHitPoints));

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(TargetA, job);
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.Touch);
        var breakdownToil = Toils_General.Wait(BreakdownDuration)
            .WithEffect(EffecterDefOf.ConstructMetal, TargetIndex.A);
        breakdownToil.AddPreTickAction(delegate
        {
            var corpse = Corpse;
            var num = (int)(BreakdownDuration / (float)corpse.MaxHitPoints);
            if (num > 0 && corpse.HitPoints > 1 && Find.TickManager.TicksGame % num == 0)
            {
                corpse.HitPoints--;
            }
        });
        yield return breakdownToil;
        yield return new Toil
        {
            initAction = delegate
            {
                job.countQueue = [];
                job.targetQueueA = [];
                foreach (var item in Corpse.ButcherProducts(pawn, 1f))
                {
                    GenPlace.TryPlaceThing(item, Corpse.Position, Map, ThingPlaceMode.Near);
                    if (pawn.GetLord().LordJob is not LordJob_BreakDownMechanoids lordJob_BreakDownMechanoids)
                    {
                        return;
                    }

                    if (lordJob_BreakDownMechanoids.extractedThings == null)
                    {
                        lordJob_BreakDownMechanoids.extractedThings = [];
                    }

                    lordJob_BreakDownMechanoids.extractedThings.Add(item);
                    job.targetQueueA.Add(item);
                    job.countQueue.Add(item.stackCount);
                }

                Corpse.Destroy();
            }
        };
        foreach (var item2 in CollectIngredientsToilsHelper(TargetIndex.A, pawn))
        {
            yield return item2;
        }

        yield return new Toil
        {
            initAction = delegate
            {
                EndJobWith(JobCondition.Succeeded);
                var lord = pawn.GetLord();
                var lordJob = lord.LordJob as LordJob_BreakDownMechanoids;
                var leaveMapJob = GetLeaveMapJob(pawn, lord, lordJob);
                if (leaveMapJob != null)
                {
                    pawn.jobs.TryTakeOrderedJob(leaveMapJob, JobTag.Misc);
                }
            }
        };
    }

    public static Job GetLeaveMapJob(Pawn pawn, Lord lord, LordJob_BreakDownMechanoids lordJob)
    {
        if (!lordJob.cellToExit.IsValid && lordJob.TryFindExitSpot(pawn.Map, lord.ownedPawns, true, out var spot))
        {
            lordJob.cellToExit = spot;
        }

        Job job;
        if (lordJob.cellToExit.IsValid)
        {
            pawn.mindState.duty.focus = lordJob.cellToExit;
            var jobGiver_ExitMapNearDutyTarget = new JobGiver_ExitMapNearDutyTarget();
            jobGiver_ExitMapNearDutyTarget.ResolveReferences();
            job = jobGiver_ExitMapNearDutyTarget.TryGiveJob(pawn);
        }
        else
        {
            var jobGiver_ExitMapBest = new JobGiver_ExitMapBest();
            jobGiver_ExitMapBest.ResolveReferences();
            job = jobGiver_ExitMapBest.TryGiveJob(pawn);
        }

        if (job == null)
        {
            return null;
        }

        job.expiryInterval = 60;
        job.expireRequiresEnemiesNearby = true;

        return job;
    }

    public IEnumerable<Toil> CollectIngredientsToilsHelper(TargetIndex ingredientIndex, Pawn carrier)
    {
        var extract = Toils_JobTransforms.ExtractNextTargetFromQueue(ingredientIndex);
        yield return extract;
        yield return Toils_Goto.GotoThing(ingredientIndex, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(ingredientIndex).FailOnSomeonePhysicallyInteracting(ingredientIndex);
        yield return Toils_Haul.StartCarryThing(ingredientIndex, true, false, true);
        yield return HaulThingInInventory(carrier);
        yield return Toils_Jump.JumpIfHaveTargetInQueue(ingredientIndex, extract);
    }

    private static Toil HaulThingInInventory(Pawn carrier)
    {
        return new Toil
        {
            initAction = delegate
            {
                var carriedThing = carrier.carryTracker.CarriedThing;
                carrier.carryTracker.innerContainer.TryTransferToContainer(carriedThing,
                    carrier.inventory.innerContainer, carriedThing.stackCount, out _);
            }
        };
    }
}
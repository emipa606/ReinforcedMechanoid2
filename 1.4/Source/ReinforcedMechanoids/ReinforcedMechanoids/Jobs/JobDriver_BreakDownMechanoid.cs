using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids
{
    public class JobDriver_BreakDownMechanoid : JobDriver
    {
        public Corpse Corpse => TargetA.Thing as Corpse;
        public IntVec3 InitialCell => TargetB.Cell;
        public int BreakdownDuration => (int)(300f * Corpse.InnerPawn.HealthScale * (Corpse.HitPoints / (float)Corpse.MaxHitPoints));
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
                var tickRate = (int)((float)BreakdownDuration / (float)corpse.MaxHitPoints);
                if (tickRate > 0 && corpse.HitPoints > 1 && Find.TickManager.TicksGame % tickRate == 0)
                {
                    corpse.HitPoints--;
                }
            });
            yield return breakdownToil;
            yield return new Toil
            {
                initAction = delegate
                {
                    job.countQueue = new List<int>();
                    job.targetQueueA = new List<LocalTargetInfo>();
                    foreach (var thing in Corpse.ButcherProducts(pawn, 1f))
                    {
                        GenPlace.TryPlaceThing(thing, Corpse.Position, Map, ThingPlaceMode.Near);
                        var lordJob = pawn.GetLord().LordJob as LordJob_BreakDownMechanoids;
                        if (lordJob.extractedThings is null)
                        {
                            lordJob.extractedThings = new List<Thing>();
                        }
                        lordJob.extractedThings.Add(thing);
                        job.targetQueueA.Add(thing);
                        job.countQueue.Add(thing.stackCount);
                    }
                    Corpse.Destroy(DestroyMode.Vanish);
                }
            };
            foreach (var toil in CollectIngredientsToilsHelper(TargetIndex.A, pawn))
            {
                yield return toil;
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
                        pawn.jobs.TryTakeOrderedJob(leaveMapJob);
                    }
                }
            };
        }

        public static Job GetLeaveMapJob(Pawn pawn, Lord lord, LordJob_BreakDownMechanoids lordJob)
        {
            Job job;
            if (!lordJob.cellToExit.IsValid && lordJob.TryFindExitSpot(pawn.Map, lord.ownedPawns, true, out var spot))
            {
                lordJob.cellToExit = spot;
            }
            if (lordJob.cellToExit.IsValid)
            {
                pawn.mindState.duty.focus = lordJob.cellToExit;
                var jbg = new JobGiver_ExitMapNearDutyTarget();
                jbg.ResolveReferences();
                job = jbg.TryGiveJob(pawn);
            }
            else
            {
                var jbg = new JobGiver_ExitMapBest();
                jbg.ResolveReferences();
                job = jbg.TryGiveJob(pawn);
            }
            if (job != null)
            {
                job.expiryInterval = 60;
                job.expireRequiresEnemiesNearby = true;
            }
            return job;
        }

        public IEnumerable<Toil> CollectIngredientsToilsHelper(TargetIndex ingredientIndex, Pawn carrier)
        {
            Toil extract = Toils_JobTransforms.ExtractNextTargetFromQueue(ingredientIndex);
            yield return extract;
            yield return Toils_Goto.GotoThing(ingredientIndex, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(ingredientIndex)
                .FailOnSomeonePhysicallyInteracting(ingredientIndex);
            yield return Toils_Haul.StartCarryThing(ingredientIndex, putRemainderInQueue: true, subtractNumTakenFromJobCount: false, failIfStackCountLessThanJobCount: true);
            yield return HaulThingInInventory(carrier);
            yield return Toils_Jump.JumpIfHaveTargetInQueue(ingredientIndex, extract);
        }

        private static Toil HaulThingInInventory(Pawn carrier)
        {
            return new Toil
            {
                initAction = delegate
                {
                    Thing carriedThing = carrier.carryTracker.CarriedThing;
                    carrier.carryTracker.innerContainer.TryTransferToContainer(carriedThing, carrier.inventory.innerContainer, carriedThing.stackCount, out var resultingTransferredItem);
                }
            };
        }
    }
}

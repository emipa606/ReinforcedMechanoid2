using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

public class JobDriver_RepairMechanoid : JobDriver
{
    public readonly int TotalWorkTick = 300;

    private int workDone;

    protected Pawn ToRepair => (Pawn)job.GetTarget(TargetIndex.A).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        Map.reservationManager.ReleaseAllForTarget(ToRepair);
        return pawn.Reserve((LocalTargetInfo)ToRepair, job, 1, -1, null, errorOnFailed);
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A)
            .FailOn(() => !pawn.CanReach(job.GetTarget(TargetIndex.A), PathEndMode.Touch, Danger.None))
            .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
        yield return new Toil
        {
            initAction = delegate { PawnUtility.ForceWait(ToRepair, TotalWorkTick, pawn); },
            tickAction = delegate
            {
                if (!pawn.IsHashIntervalTick(4))
                {
                    return;
                }

                workDone++;
                if (workDone >= TotalWorkTick)
                {
                    EndJobWith(JobCondition.Succeeded);
                }
                else if (ToRepair.health.hediffSet.hediffs.OfType<Hediff_Injury>().TryRandomElement(out var result))
                {
                    result.Heal(0.09f);
                }
                else
                {
                    EndJobWith(JobCondition.Succeeded);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Never
        }.WithEffect(() => EffecterDefOf.ConstructMetal, TargetIndex.A);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref workDone, "workDone");
    }
}
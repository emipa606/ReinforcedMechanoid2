using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

internal class JobDriver_AttachTurret : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
        return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOn(() => !TargetA.Thing.TryGetComp<CompPowerTrader>().PowerOn);
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        var getNextIngredient = Toils_General.Label();
        yield return getNextIngredient;
        yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true)
            .FailOnDestroyedNullOrForbidden(TargetIndex.B);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        var findPlaceTarget =
            Toils_JobTransforms.SetTargetToIngredientPlaceCell(TargetIndex.A, TargetIndex.B, TargetIndex.C);
        yield return findPlaceTarget;
        yield return JobDriver_RepairMachine.PlaceHauledThingInCell(TargetIndex.C, findPlaceTarget, false);
        yield return Toils_Jump.JumpIf(getNextIngredient, () => !job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
        var waitForMachineToReturn = Toils_General.Wait(60)
            .FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
        waitForMachineToReturn.AddPreInitAction(delegate
        {
            var compMachineStation = TargetA.Thing.TryGetComp<CompMachineChargingStation>();
            compMachineStation.wantsRest = true;
        });
        yield return waitForMachineToReturn;
        yield return Toils_Jump.JumpIf(waitForMachineToReturn, delegate
        {
            if (TargetA.Thing is IBedMachine bedMachine)
            {
                return bedMachine.occupant == null;
            }

            return false;
        });
        var compMachineStation = TargetA.Thing.TryGetComp<CompMachineChargingStation>();
        var compMachine = compMachineStation.myPawn.TryGetComp<CompMachine>();
        var attachTurret = new Toil
        {
            defaultDuration = (int)compMachine.turretToInstall.GetStatValueAbstract(StatDefOf.WorkToBuild),
            initAction = delegate { GenClamor.DoClamor(pawn, 15f, ClamorDefOf.Construction); }
        };
        attachTurret.WithEffect(() =>
        {
            var def = compMachine.turretToInstall;
            return def.constructEffect ?? EffecterDefOf.ConstructMetal;
        }, TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A);
        attachTurret.defaultCompleteMode = ToilCompleteMode.Delay;
        attachTurret.activeSkill = () => SkillDefOf.Construction;
        yield return attachTurret;
        yield return Finalize(TargetIndex.A);
    }

    private static Toil Finalize(TargetIndex buildingIndex)
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var curJob = toil.actor.CurJob;
            var thing = curJob.GetTarget(buildingIndex).Thing;
            foreach (var toDestroy in toil.actor.CurJob.placedThings)
            {
                toDestroy.thing.Destroy();
            }

            var compMachineStation = thing.TryGetComp<CompMachineChargingStation>();
            var compMachine = compMachineStation.myPawn.TryGetComp<CompMachine>();
            compMachine.AttachTurret();
            compMachineStation.wantsRest = false;
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }
}
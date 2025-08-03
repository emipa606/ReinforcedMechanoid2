using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

[StaticConstructorOnStartup]
public class JobDriver_Recharge : JobDriver
{
    public const TargetIndex BedOrRestSpotIndex = TargetIndex.A;
    private static readonly FleckDef MoteRecharge = DefDatabase<FleckDef>.GetNamed("VFE_Mechanoids_Mote_Recharge");
    private static readonly FleckDef MoteRepair = DefDatabase<FleckDef>.GetNamed("VFE_Mechanoids_Mote_Repair");

    private CompPowerTrader compPowerTrader;

    private CompMachineChargingStation myBuildingCompMachineChargingStation;

    private CompPowerTrader myBuildingCompPowerTrader;

    private Need_Power pawnNeed_Power;

    private CompPowerTrader BedCompPowerTrader
    {
        get
        {
            compPowerTrader ??= job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompPowerTrader>();

            return compPowerTrader;
        }
    }

    private Need_Power PawnNeed_Power
    {
        get
        {
            pawnNeed_Power ??= pawn.needs.TryGetNeed<Need_Power>();

            return pawnNeed_Power;
        }
    }

    private CompPowerTrader MyBuildingCompPowerTrader
    {
        get
        {
            myBuildingCompPowerTrader ??=
                CompMachine.cachedMachinesPawns[pawn].myBuilding.TryGetComp<CompPowerTrader>();

            return myBuildingCompPowerTrader;
        }
    }

    private CompMachineChargingStation MyBuildingCompMachineChargingStation
    {
        get
        {
            myBuildingCompMachineChargingStation ??= CompMachine.cachedMachinesPawns[pawn].myBuilding
                .TryGetComp<CompMachineChargingStation>();

            return myBuildingCompMachineChargingStation;
        }
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return job.GetTarget(TargetIndex.A).HasThing;
    }

    public override bool CanBeginNowWhileLyingDown()
    {
        return InBedOrRestSpotNow(pawn, job.GetTarget(TargetIndex.A));
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        yield return Toils_Goto.GotoCell(TargetA.Cell, PathEndMode.OnCell);
        yield return LayDown();
    }

    private static bool InBedOrRestSpotNow(Pawn pawn, LocalTargetInfo bedOrRestSpot)
    {
        if (!bedOrRestSpot.IsValid || !pawn.Spawned)
        {
            return false;
        }

        if (!bedOrRestSpot.HasThing)
        {
            return false;
        }

        if (bedOrRestSpot.Thing.Map != pawn.Map)
        {
            return false;
        }

        return bedOrRestSpot.Thing.Position == pawn.Position;
    }

    private Toil LayDown() //Largely C&P'ed from vanilla LayDown toil
    {
        var layDown = new Toil();
        layDown.initAction = delegate
        {
            var actor3 = layDown.actor;
            actor3.pather.StopDead();
            MyBuildingCompMachineChargingStation.CompTickRare();
        };
        layDown.tickAction = delegate
        {
            var actor2 = layDown.actor;
            if (BedCompPowerTrader.PowerOn)
            {
                PawnNeed_Power.TickResting(1f);
                if (actor2.IsHashIntervalTick(300) && !actor2.Position.Fogged(actor2.Map))
                {
                    FleckMaker.ThrowMetaIcon(actor2.Position, actor2.Map,
                        actor2.health.hediffSet.GetNaturallyHealingInjuredParts().Any() ? MoteRepair : MoteRecharge);
                }
            }

            actor2.Rotation = Rot4.South;
            if (!MyBuildingCompMachineChargingStation.forceStay && actor2.IsHashIntervalTick(300))
            {
                actor2.jobs.CheckForJobOverride();
            }
        };
        layDown.AddFinishAction(delegate
        {
            if (PawnNeed_Power.CurLevelPercentage > 0.99f &&
                !layDown.actor.health.hediffSet.HasNaturallyHealingInjury() &&
                CompMachine.cachedMachinesPawns[pawn].turretToInstall == null)
            {
                MyBuildingCompMachineChargingStation.wantsRest = false;
            }
        });
        layDown.handlingFacing = true;
        layDown.FailOn(() => !MyBuildingCompMachineChargingStation.forceStay && (!MyBuildingCompPowerTrader.PowerOn ||
            PawnNeed_Power.CurLevelPercentage > 0.99f && !layDown.actor.health.hediffSet.HasNaturallyHealingInjury() &&
            CompMachine.cachedMachinesPawns[pawn].turretToInstall == null));

        layDown.defaultCompleteMode = ToilCompleteMode.Never;
        return layDown;
    }
}
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

internal class WorkGiver_AttachTurret : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Undefined);
    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        var chargingStationComp = t.TryGetComp<CompMachineChargingStation>();
        var mech = chargingStationComp?.myPawn;
        var comp = mech?.TryGetComp<CompMachine>();
        if (mech is null || comp == null || comp.turretToInstall == null)
        {
            return false;
        }

        if (!t.TryGetComp<CompPowerTrader>()?.PowerOn ?? true)
        {
            return false;
        }

        if (mech.Dead || mech.Destroyed)
        {
            JobFailReason.Is("VFEMechNoTurret".Translate());
            return false;
        }

        var products = comp.turretToInstall.costList;
        foreach (var thingNeeded in products)
        {
            if (pawn.Map.itemAvailability.ThingsAvailableAnywhere(thingNeeded.thingDef, thingNeeded.count, pawn))
            {
                continue;
            }

            JobFailReason.Is("VFEMechNoResources".Translate());
            return false;
        }

        return pawn.CanReserveAndReach(t, PathEndMode.OnCell, Danger.Deadly, ignoreOtherReservations: forced);
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        var chargingStationComp = t.TryGetComp<CompMachineChargingStation>();
        var mech = chargingStationComp.myPawn;
        var comp = mech?.TryGetComp<CompMachine>();

        var products = comp?.turretToInstall.costList;
        var toGrab = new List<Thing>();
        var toGrabCount = new List<int>();
        if (products != null)
        {
            foreach (var thingNeeded in products)
            {
                var thingsOfThisType = RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, t.Position,
                    new IntRange(thingNeeded.count, thingNeeded.count), thing => thing.def == thingNeeded.thingDef);
                if (thingsOfThisType == null)
                {
                    return null;
                }

                toGrab.AddRange(thingsOfThisType);
                var totalCountNeeded = thingNeeded.count;
                foreach (var thingGrabbed in thingsOfThisType)
                {
                    if (thingGrabbed.stackCount >= totalCountNeeded)
                    {
                        toGrabCount.Add(totalCountNeeded);
                        totalCountNeeded = 0;
                    }
                    else
                    {
                        toGrabCount.Add(thingGrabbed.stackCount);
                        totalCountNeeded -= thingGrabbed.stackCount;
                    }
                }
            }
        }

        var job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("VFE_Mechanoids_AttachTurret"), t);
        job.targetQueueB = toGrab.Select(f => new LocalTargetInfo(f)).ToList();
        job.countQueue = toGrabCount.ToList();
        return job;
    }

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        var workSet = new List<Thing>();
        foreach (var compMachineChargingStation in CompMachineChargingStation.cachedChargingStations)
        {
            try
            {
                if (compMachineChargingStation.parent.Map == pawn.Map)
                {
                    workSet.Add(compMachineChargingStation.parent);
                }
            }
            catch
            {
                // ignored
            }
        }

        return workSet.AsEnumerable();
    }
}
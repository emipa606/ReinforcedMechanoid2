using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

internal class WorkGiver_RepairMachine : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Undefined);
    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (!CompMachineChargingStation.cachedChargingStationsDict.TryGetValue(t, out var comp))
        {
            return false;
        }

        if (comp is not { wantsRespawn: true })
        {
            return false;
        }

        if (comp.Props.pawnToSpawn == null)
        {
            return false;
        }

        var products = comp.Props.pawnToSpawn.race.butcherProducts.ListFullCopy();
        foreach (var thingNeeded in products)
        {
            var thingsOfThisType = RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, t.Position,
                new IntRange(thingNeeded.count, thingNeeded.count),
                thing => thing.def == thingNeeded.thingDef);
            if (thingsOfThisType != null)
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
        var pawnDef = t.TryGetComp<CompMachineChargingStation>().Props.pawnToSpawn.race;
        var products = pawnDef.butcherProducts.ListFullCopy();
        var toGrab = new List<Thing>();
        var toGrabCount = new List<int>();
        foreach (var thingNeeded in products)
        {
            var thingsOfThisType = RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, t.Position,
                new IntRange(thingNeeded.count, thingNeeded.count),
                thing => thing.def == thingNeeded.thingDef);
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

        var job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("VFE_Mechanoids_RepairMachine"), t);
        job.targetQueueB = toGrab.Select(f => new LocalTargetInfo(f)).ToList();
        job.countQueue = toGrabCount.ToList();
        return job;
    }

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        foreach (var compMachineChargingStation in CompMachineChargingStation.cachedChargingStations)
        {
            if (compMachineChargingStation.parent.Map == pawn.Map)
            {
                yield return compMachineChargingStation.parent;
            }
        }
    }
}
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

public class JobGiver_ReturnToStation : ThinkNode_JobGiver
{
    private static readonly Dictionary<Pawn, int> pawnsWithLastJobScanTick = new();

    public override float GetPriority(Pawn pawn)
    {
        return 8f;
    }

    public override Job TryGiveJob(Pawn pawn)
    {
        var myBuilding = CompMachine.cachedMachinesPawns[pawn].myBuilding;
        if (myBuilding == null)
        {
            return null;
        }

        if (!myBuilding.Spawned || myBuilding.Map != pawn.Map)
        {
            return null;
        }

        var compMachine = CompMachine.cachedMachinesPawns[pawn];
        var compMachineChargingStation = compMachine.myBuilding.TryGetComp<CompMachineChargingStation>();
        if (compMachineChargingStation.wantsRest && compMachine.myBuilding.TryGetComp<CompPowerTrader>().PowerOn
                                                 && pawn.CanReach(myBuilding, PathEndMode.OnCell,
                                                     Danger.Deadly))
        {
            return JobMaker.MakeJob(RM_DefOf.VFE_Mechanoids_Recharge, compMachine.myBuilding);
        }

        if (pawn.mindState.lastJobTag != JobTag.Idle)
        {
            return null;
        }

        if (pawnsWithLastJobScanTick.ContainsKey(pawn))
        {
            if (Find.TickManager.TicksGame - pawnsWithLastJobScanTick[pawn] < 60)
            {
                pawnsWithLastJobScanTick[pawn] = Find.TickManager.TicksGame;
                return JobMaker.MakeJob(JobDefOf.Wait, 300);
            }
        }

        pawnsWithLastJobScanTick[pawn] = Find.TickManager.TicksGame;

        return null;
    }
}
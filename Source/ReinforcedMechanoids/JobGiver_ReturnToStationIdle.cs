using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

public class JobGiver_ReturnToStationIdle : ThinkNode_JobGiver
{
    private readonly float maxLevelPercentage = 0.99f;

    public override Job TryGiveJob(Pawn pawn)
    {
        var myBuilding = CompMachine.cachedMachinesPawns[pawn].myBuilding;
        var buildingPosition = myBuilding.Position;
        var power = pawn.needs.TryGetNeed<Need_Power>();
        if (power == null || power.CurLevelPercentage > maxLevelPercentage)
        {
            return null;
        }

        if (!myBuilding.Spawned || myBuilding.Map != pawn.Map ||
            !pawn.CanReserveAndReach(myBuilding, PathEndMode.OnCell, Danger.Deadly))
        {
            return JobMaker.MakeJob(JobDefOf.Wait, 300);
        }

        if (pawn.Position != buildingPosition)
        {
            return JobMaker.MakeJob(JobDefOf.Goto, buildingPosition);
        }

        pawn.Rotation = Rot4.South;
        return myBuilding.TryGetComp<CompPowerTrader>().PowerOn
            ? JobMaker.MakeJob(RM_DefOf.VFE_Mechanoids_Recharge, myBuilding)
            : JobMaker.MakeJob(JobDefOf.Wait, 300);
    }
}
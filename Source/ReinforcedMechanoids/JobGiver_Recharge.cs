using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

public class JobGiver_Recharge : ThinkNode_JobGiver
{
    private float maxLevelPercentage = 0.99f;

    private RestCategory minCategory = RestCategory.VeryTired;

    public override ThinkNode DeepCopy(bool resolve = true)
    {
        var obj = (JobGiver_Recharge)base.DeepCopy(resolve);
        obj.minCategory = minCategory;
        obj.maxLevelPercentage = maxLevelPercentage;
        return obj;
    }

    public override float GetPriority(Pawn pawn)
    {
        var power = pawn.needs.TryGetNeed<Need_Power>();
        if (power == null)
        {
            return 0f;
        }

        if ((int)power.CurCategory < (int)minCategory)
        {
            return 0f;
        }

        return power.CurLevelPercentage > maxLevelPercentage ? 0f : 8f;
    }

    public override Job TryGiveJob(Pawn pawn)
    {
        var power = pawn.needs.TryGetNeed<Need_Power>();
        if (power == null || power.CurLevelPercentage > maxLevelPercentage)
        {
            return null;
        }

        if (pawn.CurJobDef != RM_DefOf.VFE_Mechanoids_Recharge && power.CurCategory <= minCategory)
        {
            return null;
        }

        var building = CompMachine.cachedMachinesPawns[pawn].myBuilding;
        if (building == null)
        {
            return null;
        }

        if (!building.Spawned || building.Map != pawn.Map ||
            !pawn.CanReserveAndReach(building, PathEndMode.OnCell, Danger.Deadly))
        {
            return null;
        }

        if (building.TryGetComp<CompPowerTrader>().PowerOn)
        {
            return JobMaker.MakeJob(RM_DefOf.VFE_Mechanoids_Recharge, building);
        }

        return building.Position != pawn.Position ? JobMaker.MakeJob(JobDefOf.Goto, building) : null;
    }
}
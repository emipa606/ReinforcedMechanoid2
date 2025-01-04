using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(JobGiver_AITrashBuildingsDistant), nameof(JobGiver_AITrashBuildingsDistant.TryGiveJob))]
public static class JobGiver_AITrashBuildingsDistant_TryGiveJob
{
    public static bool Prefix(Pawn pawn, ref Job __result)
    {
        return JobGiver_AIFightEnemy_TryGiveJob_HotSwappable.TryModifyJob(pawn, ref __result);
    }
}
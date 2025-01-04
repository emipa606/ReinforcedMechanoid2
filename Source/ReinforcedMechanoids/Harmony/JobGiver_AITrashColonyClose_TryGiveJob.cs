using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(JobGiver_AITrashColonyClose), nameof(JobGiver_AITrashColonyClose.TryGiveJob))]
public static class JobGiver_AITrashColonyClose_TryGiveJob
{
    public static bool Prefix(Pawn pawn, ref Job __result)
    {
        return JobGiver_AIFightEnemy_TryGiveJob_HotSwappable.TryModifyJob(pawn, ref __result);
    }
}
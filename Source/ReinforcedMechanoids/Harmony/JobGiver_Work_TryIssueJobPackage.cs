using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(JobGiver_Work), nameof(JobGiver_Work.TryIssueJobPackage))]
public static class JobGiver_Work_TryIssueJobPackage
{
    public static void Postfix(JobGiver_Work __instance, ref ThinkResult __result, Pawn pawn, JobIssueParams jobParams)
    {
        if (pawn.kindDef != RM_DefOf.RM_Mech_Vulture || __result != ThinkResult.NoJob)
        {
            return;
        }

        var otherMechanoids = Utils.GetOtherMechanoids(pawn, pawn.GetLord());
        var job = Utils.HealOtherMechanoidsOrRepairStructures(pawn, otherMechanoids);
        if (job != null)
        {
            __result = new ThinkResult(job, __instance, JobTag.MiscWork);
        }
    }
}
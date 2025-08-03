using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(JobGiver_AIFightEnemy), nameof(JobGiver_AIFightEnemy.FindAttackTarget))]
public static class JobGiver_AIFightEnemy_FindAttackTarget
{
    public static bool Prefix(JobGiver_AIFightEnemy __instance, Pawn pawn, ref Thing __result)
    {
        if (pawn.kindDef != RM_DefOf.RM_Mech_WraithSiege)
        {
            return true;
        }

        __result = findAttackTarget(__instance, pawn);
        return __result == null;
    }

    private static Thing findAttackTarget(JobGiver_AIFightEnemy __instance, Pawn pawn)
    {
        var targetable = TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
        return (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetable,
            x => __instance.ExtraTargetValidator(pawn, x), 0f, __instance.targetAcquireRadius,
            __instance.GetFlagPosition(pawn), __instance.GetFlagRadius(pawn));
    }
}
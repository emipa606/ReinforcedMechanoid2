using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyBefore("legodude17.mvcf")]
[HarmonyPatch(typeof(Pawn), nameof(Pawn.TryGetAttackVerb))]
public static class Pawn_TryGetAttackVerb
{
    [HarmonyPriority(int.MaxValue)]
    public static bool Prefix(Pawn __instance, ref Verb __result, Thing target, bool allowManualCastWeapons = false)
    {
        if (__instance.health.hediffSet.GetFirstHediffOfDef(RM_DefOf.RM_SentinelBerserk) == null)
        {
            return true;
        }

        __result = __instance.equipment is { Primary: not null } &&
                   __instance.equipment.Primary.def.IsMeleeWeapon &&
                   __instance.equipment.PrimaryEq.PrimaryVerb.Available() &&
                   (!__instance.equipment.PrimaryEq.PrimaryVerb.verbProps.onlyManualCast ||
                    __instance.CurJob != null && __instance.CurJob.def != JobDefOf.Wait_Combat ||
                    allowManualCastWeapons)
            ? __instance.equipment.PrimaryEq.PrimaryVerb
            : __instance.meleeVerbs.TryGetMeleeVerb(target);
        return false;
    }

    private static void Postfix(Pawn __instance, ref Verb __result)
    {
        if (__instance.kindDef != RM_DefOf.RM_Mech_Vulture)
        {
            return;
        }

        __result = null;
        var job = Utils.FleeIfEnemiesAreNearby(__instance);
        if (job != null && __instance.CurJobDef != job.def &&
            !__instance.jobs.jobQueue.jobs.Any(x => x.job.def == job.def))
        {
            __instance.jobs.jobQueue.EnqueueFirst(job);
        }
    }
}
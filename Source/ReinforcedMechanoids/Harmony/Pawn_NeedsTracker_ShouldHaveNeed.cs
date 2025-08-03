using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Pawn_NeedsTracker), nameof(Pawn_NeedsTracker.ShouldHaveNeed))]
public static class Pawn_NeedsTracker_ShouldHaveNeed
{
    public static bool Prefix(Pawn ___pawn, NeedDef nd, ref bool __result)
    {
        if (___pawn is not Machine &&
            (!(___pawn.def.GetModExtension<MechanoidExtension>()?.hasPowerNeedWhenHacked ?? false)
             || ___pawn.Faction is not { IsPlayer: true }))
        {
            return nd != RM_DefOf.VFE_Mechanoids_Power;
        }

        if (nd != RM_DefOf.VFE_Mechanoids_Power)
        {
            return true;
        }

        __result = true;
        return false;
    }
}
using HarmonyLib;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(DamageWorker_AddInjury), nameof(DamageWorker_AddInjury.GetExactPartFromDamageInfo))]
public static class DamageWorker_AddInjury_GetExactPartFromDamageInfo
{
    public static bool pickShield;

    private static void Prefix(DamageInfo dinfo, Pawn pawn)
    {
        if (dinfo.Instigator is not Pawn pawn2 || pawn.kindDef != RM_DefOf.RM_Mech_Behemoth)
        {
            return;
        }

        var angle = (pawn2.DrawPos - pawn.DrawPos).AngleFlat();
        var rot = Pawn_RotationTracker.RotFromAngleBiased(angle);
        if (rot == pawn.Rotation && Utils.GetNonMissingBodyPart(pawn, RM_DefOf.RM_BehemothShield) != null)
        {
            pickShield = true;
        }
    }

    private static void Postfix()
    {
        pickShield = false;
    }
}
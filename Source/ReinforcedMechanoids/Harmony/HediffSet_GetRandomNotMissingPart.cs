using HarmonyLib;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(HediffSet), nameof(HediffSet.GetRandomNotMissingPart))]
public static class HediffSet_GetRandomNotMissingPart
{
    private static void Postfix(HediffSet __instance, ref BodyPartRecord __result)
    {
        if (!DamageWorker_AddInjury_GetExactPartFromDamageInfo.pickShield || !Rand.Chance(0.8f))
        {
            return;
        }

        var nonMissingBodyPart = Utils.GetNonMissingBodyPart(__instance.pawn, RM_DefOf.RM_BehemothShield);
        if (nonMissingBodyPart != null)
        {
            __result = nonMissingBodyPart;
        }
    }
}
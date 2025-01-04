using HarmonyLib;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.AdjustedMeleeDamageAmount), typeof(Verb), typeof(Pawn))]
public static class VerbProperties_AdjustedMeleeDamageAmount
{
    public static void Postfix(Pawn attacker, ref float __result)
    {
        if (attacker.health.hediffSet.GetFirstHediffOfDef(RM_DefOf.RM_SentinelBerserk) != null)
        {
            __result *= 1.3f;
        }
    }
}
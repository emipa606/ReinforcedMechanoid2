using HarmonyLib;
using RimWorld;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Need_MechEnergy), nameof(Need_MechEnergy.FallPerDay), MethodType.Getter)]
public static class Need_MechEnergy_FallPerDay
{
    public static void Postfix(Need_MechEnergy __instance, ref float __result)
    {
        if (__instance.pawn.health.hediffSet.HasHediff(RM_DefOf.RM_ZealotInvisibility))
        {
            __result *= 1.5f;
        }
    }
}
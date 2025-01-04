using HarmonyLib;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(HediffSet), nameof(HediffSet.DirtyCache))]
public static class HediffSet_DirtyCache
{
    private static void Postfix(HediffSet __instance)
    {
        __instance.pawn.GetComp<CompChangePawnGraphic>()?.TryChangeGraphic();
    }
}
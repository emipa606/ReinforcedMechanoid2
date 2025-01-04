using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.ButcherProducts))]
public static class Pawn_ButcherProducts
{
    private static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Pawn __instance)
    {
        foreach (var item in __result)
        {
            yield return item;
        }

        var additionalButcherOptions = __instance.def.GetModExtension<AdditionalButcherProducts>();
        if (additionalButcherOptions == null)
        {
            yield break;
        }

        foreach (var option in additionalButcherOptions.butcherOptions)
        {
            if (!Rand.Chance(option.chance))
            {
                continue;
            }

            var thing = ThingMaker.MakeThing(option.thingDef);
            thing.stackCount = option.amount.RandomInRange;
            yield return thing;
        }
    }
}
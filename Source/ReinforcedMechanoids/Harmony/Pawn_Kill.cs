using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
public static class Pawn_Kill
{
    public static void Postfix(Pawn __instance)
    {
        if (__instance is not Machine || __instance.Faction != Faction.OfPlayer ||
            __instance.def.butcherProducts == null)
        {
            return;
        }

        var ingredients = "";
        var comma = false;
        foreach (var ingredient in __instance.def.butcherProducts)
        {
            if (comma)
            {
                ingredients += ", ";
            }

            ingredients += $"{ingredient.thingDef.label} x{ingredient.count}";
            comma = true;
        }

        Find.LetterStack.ReceiveLetter("VFEMechMachineDied".Translate(),
            "VFEMechMachineDiedDesc".Translate(ingredients), LetterDefOf.NegativeEvent, __instance);
    }
}
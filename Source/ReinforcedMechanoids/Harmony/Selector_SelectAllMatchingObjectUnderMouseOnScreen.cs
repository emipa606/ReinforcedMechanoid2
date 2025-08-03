using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Selector), nameof(Selector.SelectAllMatchingObjectUnderMouseOnScreen))]
public static class Selector_SelectAllMatchingObjectUnderMouseOnScreen
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
    {
        var codes = codeInstructions.ToList();
        var clickedThingField = typeof(Selector).GetNestedTypes(AccessTools.all)
            .SelectMany(x => x.GetFields(AccessTools.all))
            .FirstOrDefault(x => x.Name == "clickedThing");
        foreach (var code in codes)
        {
            yield return code;
            if (code.opcode != OpCodes.Stloc_3)
            {
                continue;
            }

            yield return new CodeInstruction(OpCodes.Ldloc_0);
            yield return new CodeInstruction(OpCodes.Ldfld, clickedThingField);
            yield return new CodeInstruction(OpCodes.Ldloc_3);
            yield return new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(Selector_SelectAllMatchingObjectUnderMouseOnScreen),
                    nameof(WrappedPredicator)));
            yield return new CodeInstruction(OpCodes.Stloc_3);
        }
    }

    public static Predicate<Thing> WrappedPredicator(Thing clickedThing, Predicate<Thing> predicate)
    {
        return wrappedPredicate;

        bool wrappedPredicate(Thing t)
        {
            if (predicate(t))
            {
                return true;
            }

            if (t is not Pawn pawn2 || clickedThing is not Pawn pawn1)
            {
                return false;
            }

            return pawn2.Faction == Faction.OfPlayer && pawn1.Faction == Faction.OfPlayer &&
                   (pawn2 is Machine || pawn1 is Machine);
        }
    }
}
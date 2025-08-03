using System;
using HarmonyLib;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.MainDesc))]
public static class Pawn_MainDesc
{
    public static void Postfix(ref string __result)
    {
        var substringToRemove = " ";
        var index = __result.IndexOf(substringToRemove, StringComparison.Ordinal);
        if (index == 0)
        {
            __result = __result.Remove(index, substringToRemove.Length).CapitalizeFirst();
        }
    }
}
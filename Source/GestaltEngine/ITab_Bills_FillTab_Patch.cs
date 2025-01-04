using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GestaltEngine;

[HarmonyPatch(typeof(ITab_Bills), "FillTab")]
public static class ITab_Bills_FillTab_Patch
{
    [HarmonyPriority(int.MaxValue)]
    public static void Prefix(ITab_Bills __instance, out List<RecipeDef> __state)
    {
        var selTable = __instance.SelTable;
        __state = selTable.def.AllRecipes;
        var comp = selTable.GetComp<CompUpgradeable>();
        if (comp != null)
        {
            selTable.def.allRecipesCached = comp.CurrentUpgrade.unlockedRecipes;
        }
    }

    [HarmonyPriority(int.MinValue)]
    public static void Postfix(ITab_Bills __instance, List<RecipeDef> __state)
    {
        var selTable = __instance.SelTable;
        selTable.def.allRecipesCached = __state;
    }
}
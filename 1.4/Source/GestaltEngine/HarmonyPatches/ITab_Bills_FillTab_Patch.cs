using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace GestaltEngine
{
    [HarmonyPatch(typeof(ITab_Bills), "FillTab")]
    public static class ITab_Bills_FillTab_Patch
    {
        [HarmonyPriority(int.MaxValue)]
        public static void Prefix(ITab_Bills __instance, out List<RecipeDef> __state)
        {
            var table = __instance.SelTable;
            __state = table.def.AllRecipes;
            var compUpgradeable = table.GetComp<CompUpgradeable>();
            if (compUpgradeable != null)
            {
                table.def.allRecipesCached = compUpgradeable.CurrentUpgrade.unlockedRecipes;
            }
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(ITab_Bills __instance, List<RecipeDef> __state)
        {
            var table = __instance.SelTable;
            table.def.allRecipesCached = __state;
        }
    }
}

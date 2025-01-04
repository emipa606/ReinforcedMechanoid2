using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GestaltEngine;

[StaticConstructorOnStartup]
internal static class Utils
{
    static Utils()
    {
        var harmony = new Harmony("GestaltEngine.Mod");
        harmony.PatchAll();
        var list = new List<MethodInfo>
        {
            AccessTools.Method(typeof(Game), "InitNewGame"),
            AccessTools.Method(typeof(Game), "LoadGame"),
            AccessTools.Method(typeof(SavedGameLoaderNow), "LoadGameFromSaveFileNow")
        };
        foreach (var item in list)
        {
            harmony.Patch(item, new HarmonyMethod(typeof(Utils), "ResetStaticData"));
        }

        foreach (var ingredient in AC_DefOf.RM_HackBiocodedThings.ingredients)
        {
            ingredient.filter = new ThingFilterBiocodable();
            var list2 = ingredient.filter.thingDefs;
            if (list2 == null)
            {
                list2 = [];
                ingredient.filter.thingDefs = list2;
            }

            foreach (var item2 in DefDatabase<ThingDef>.AllDefs.Where(x =>
                         x.comps != null && x.HasAssignableCompFrom(typeof(CompBiocodable))))
            {
                ingredient.filter.SetAllow(item2, true);
                list2.Add(item2);
            }
        }

        AC_DefOf.RM_HackBiocodedThings.fixedIngredientFilter = new ThingFilterBiocodable();
        var list3 = AC_DefOf.RM_HackBiocodedThings.fixedIngredientFilter.thingDefs;
        if (list3 == null)
        {
            list3 = [];
            AC_DefOf.RM_HackBiocodedThings.fixedIngredientFilter.thingDefs = list3;
        }

        foreach (var item3 in DefDatabase<ThingDef>.AllDefs.Where(x =>
                     x.comps != null && x.HasAssignableCompFrom(typeof(CompBiocodable))))
        {
            list3.Add(item3);
            AC_DefOf.RM_HackBiocodedThings.fixedIngredientFilter.SetAllow(item3, true);
        }

        AC_DefOf.RM_HackBiocodedThings.defaultIngredientFilter = new ThingFilterBiocodable();
        var list4 = AC_DefOf.RM_HackBiocodedThings.defaultIngredientFilter.thingDefs;
        if (list4 == null)
        {
            list4 = new List<ThingDef>();
            AC_DefOf.RM_HackBiocodedThings.defaultIngredientFilter.thingDefs = list4;
        }

        foreach (var item4 in DefDatabase<ThingDef>.AllDefs.Where(x =>
                     x.comps != null && x.HasAssignableCompFrom(typeof(CompBiocodable))))
        {
            list4.Add(item4);
            AC_DefOf.RM_HackBiocodedThings.defaultIngredientFilter.SetAllow(item4, true);
        }
    }

    public static void ResetStaticData()
    {
        CompGestaltEngine.compGestaltEngines.Clear();
    }

    public static bool TryGetGestaltEngineInstead(this Pawn overseer, out CompGestaltEngine result)
    {
        foreach (var compGestaltEngine in CompGestaltEngine.compGestaltEngines)
        {
            if (compGestaltEngine.dummyPawn == overseer)
            {
                result = compGestaltEngine;
                return true;
            }
        }

        result = null;
        return false;
    }
}
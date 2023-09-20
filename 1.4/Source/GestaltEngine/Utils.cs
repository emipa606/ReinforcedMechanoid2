using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace GestaltEngine
{
    [StaticConstructorOnStartup]
    internal static class Utils
    {
        static Utils()
        {
            var harmony = new Harmony("GestaltEngine.Mod");
            harmony.PatchAll();
            var hooks = new List<MethodInfo>
            {
                AccessTools.Method(typeof(Game), "InitNewGame"),
                AccessTools.Method(typeof(Game), "LoadGame"),
                AccessTools.Method(typeof(SavedGameLoaderNow), "LoadGameFromSaveFileNow")
            };

            foreach (var hook in hooks)
            {
                harmony.Patch(hook, new HarmonyMethod(typeof(Utils), nameof(Utils.ResetStaticData)));
            }


            foreach (var li in AC_DefOf.RM_HackBiocodedThings.ingredients)
            {
                li.filter = new ThingFilterBiocodable();
                var list = li.filter.thingDefs;
                if (list is null)
                {
                    list = new List<ThingDef>();
                    li.filter.thingDefs = list;
                }
                foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.comps != null && x.HasAssignableCompFrom(typeof(CompBiocodable))))
                {
                    li.filter.SetAllow(thingDef, true);
                    list.Add(thingDef);
                }
            }
            AC_DefOf.RM_HackBiocodedThings.fixedIngredientFilter = new ThingFilterBiocodable();
            var list2 = AC_DefOf.RM_HackBiocodedThings.fixedIngredientFilter.thingDefs;
            if (list2 is null)
            {
                list2 = new List<ThingDef>();
                AC_DefOf.RM_HackBiocodedThings.fixedIngredientFilter.thingDefs = list2;
            }

            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.comps != null 
                && x.HasAssignableCompFrom(typeof(CompBiocodable))))
            {
                list2.Add(thingDef);
                AC_DefOf.RM_HackBiocodedThings.fixedIngredientFilter.SetAllow(thingDef, true);
            }

            AC_DefOf.RM_HackBiocodedThings.defaultIngredientFilter = new ThingFilterBiocodable();
            var list3 = AC_DefOf.RM_HackBiocodedThings.defaultIngredientFilter.thingDefs;
            if (list3 is null)
            {
                list3 = new List<ThingDef>();
                AC_DefOf.RM_HackBiocodedThings.defaultIngredientFilter.thingDefs = list3;
            }

            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.comps != null 
                && x.HasAssignableCompFrom(typeof(CompBiocodable))))
            {
                list3.Add(thingDef);
                AC_DefOf.RM_HackBiocodedThings.defaultIngredientFilter.SetAllow(thingDef, true);
            }
        }

        public static void ResetStaticData()
        {
            CompGestaltEngine.compGestaltEngines.Clear();
        }
        public static bool TryGetGestaltEngineInstead(this Pawn overseer, out CompGestaltEngine result)
        {
            foreach (var comp in CompGestaltEngine.compGestaltEngines)
            {
                if (comp.dummyPawn == overseer)
                {
                    result = comp;
                    return true;
                }
            }
            result = null;
            return false;
        }
    }
}

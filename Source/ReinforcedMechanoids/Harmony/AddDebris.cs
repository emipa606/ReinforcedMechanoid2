using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch]
public static class AddDebris
{
    [HarmonyTargetMethods]
    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.PropertyGetter(typeof(GenStep_ScatterRoadDebris),
            nameof(GenStep_ScatterRoadDebris.Debris));
        yield return AccessTools.PropertyGetter(typeof(SymbolResolver_AncientComplex_Defences),
            nameof(SymbolResolver_AncientComplex_Defences.AncientVehicles));
    }

    public static IEnumerable<ThingDef> Postfix(IEnumerable<ThingDef> __result)
    {
        foreach (var item in __result)
        {
            yield return item;
        }

        var def = DefDatabase<ThingDef>.GetNamedSilentFail("RM_AncientRustedJeep");
        if (def != null)
        {
            yield return def;
        }
    }
}
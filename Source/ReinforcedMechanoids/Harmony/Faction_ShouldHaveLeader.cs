using HarmonyLib;
using RimWorld;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Faction), nameof(Faction.ShouldHaveLeader), MethodType.Getter)]
public static class Faction_ShouldHaveLeader
{
    public static bool Prefix(Faction __instance)
    {
        return __instance.def != RM_DefOf.RM_Remnants;
    }
}
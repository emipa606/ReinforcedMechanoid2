using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(WorldObject), nameof(WorldObject.GetDescription))]
public static class WorldObject_GetDescription
{
    public static void Postfix(WorldObject __instance, ref string __result)
    {
        if (__instance is Settlement settlement && settlement.Faction?.def == RM_DefOf.RM_Remnants)
        {
            __result = "RM.RemnantsBaseDescription".Translate();
        }
    }
}
using HarmonyLib;
using RimWorld;

namespace GestaltEngine;

[HarmonyPatch(typeof(Pawn_MechanitorTracker), "TotalAvailableControlGroups", MethodType.Getter)]
public static class Pawn_MechanitorTracker_TotalAvailableControlGroups_Patch
{
    public static void Postfix(Pawn_MechanitorTracker __instance, ref int __result)
    {
        if (__instance.pawn.TryGetGestaltEngineInstead(out var result))
        {
            __result = result.CurrentUpgrade.totalControlGroups;
        }
    }
}
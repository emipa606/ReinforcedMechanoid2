using HarmonyLib;
using RimWorld;
using Verse;

namespace GestaltEngine
{
    [HarmonyPatch(typeof(Pawn_MechanitorTracker), "CanControlMechs", MethodType.Getter)]
    public static class Pawn_MechanitorTracker_CanControlMechs_Patch
    {
        public static void Postfix(Pawn_MechanitorTracker __instance, ref AcceptanceReport __result)
        {
            if (__instance.pawn.TryGetGestaltEngineInstead(out _))
            {
                __result = true;
            }
        }
    }
}

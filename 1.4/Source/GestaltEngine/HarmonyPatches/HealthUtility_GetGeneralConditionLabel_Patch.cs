using HarmonyLib;
using Verse;

namespace GestaltEngine
{
    [HarmonyPatch(typeof(HealthUtility), "GetGeneralConditionLabel")]
    public static class HealthUtility_GetGeneralConditionLabel_Patch
    {
        public static void Postfix(ref string __result, Pawn pawn)
        {
            if (pawn.TryGetGestaltEngineInstead(out var comp))
            {
                __result = "";
            }
        }
    }
}

using HarmonyLib;
using RimWorld;

namespace ReinforcedMechanoids;

[HarmonyPatch(typeof(CompCauseGameCondition), nameof(CompCauseGameCondition.Active), MethodType.Getter)]
public static class CompCauseGameCondition_ActivePatch
{
    public static void Postfix(ref bool __result, CompCauseGameCondition __instance)
    {
        if (__result && __instance is CompCauseGameConditionPowerDependent compCauseGameConditionPowerDependent &&
            !compCauseGameConditionPowerDependent.compPower.PowerOn)
        {
            __result = false;
        }
    }
}
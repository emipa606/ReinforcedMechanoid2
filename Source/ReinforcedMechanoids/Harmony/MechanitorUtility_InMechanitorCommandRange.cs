using HarmonyLib;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(MechanitorUtility), nameof(MechanitorUtility.InMechanitorCommandRange))]
public static class MechanitorUtility_InMechanitorCommandRange
{
    public static void Postfix(Pawn mech, ref bool __result)
    {
        if (mech is Machine)
        {
            __result = true;
        }
    }
}
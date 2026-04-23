using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(LordToil_Siege), "CanBeBuilder")]
public static class LordToil_Siege_CanBeBuilder
{
    public static void Postfix(Pawn p, ref bool __result)
    {
        if (!__result)
        {
            return;
        }

        if (p.kindDef == RM_DefOf.RM_Mech_Vulture || p.skills == null || p.workSettings == null)
        {
            __result = false;
        }
    }
}

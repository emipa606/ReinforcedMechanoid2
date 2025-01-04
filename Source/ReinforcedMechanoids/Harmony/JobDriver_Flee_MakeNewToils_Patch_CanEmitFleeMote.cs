using HarmonyLib;
using Verse;
using VFE.Mechanoids.HarmonyPatches;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(JobDriver_Flee_MakeNewToils_Patch), nameof(JobDriver_Flee_MakeNewToils_Patch.CanEmitFleeMote))]
public static class JobDriver_Flee_MakeNewToils_Patch_CanEmitFleeMote
{
    public static void Postfix(ref bool __result, Pawn pawn)
    {
        if (pawn.kindDef == RM_DefOf.RM_Mech_Vulture)
        {
            __result = true;
        }
    }
}
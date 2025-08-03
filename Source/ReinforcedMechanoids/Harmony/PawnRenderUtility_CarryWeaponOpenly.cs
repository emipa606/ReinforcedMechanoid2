using HarmonyLib;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.CarryWeaponOpenly))]
public static class PawnRenderUtility_CarryWeaponOpenly
{
    public static void Postfix(Pawn pawn, ref bool __result)
    {
        if (pawn != null && CompMachine.cachedMachinesPawns.TryGetValue(pawn, out var value) && value != null
            && (value.turretAttached != null || value.Props.violent))
        {
            __result = true;
        }
    }
}
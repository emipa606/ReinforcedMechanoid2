using HarmonyLib;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.SetDead))]
public static class Pawn_HealthTracker_SetDead
{
    private static void Postfix(Pawn ___pawn)
    {
        if (___pawn.kindDef == RM_DefOf.RM_Mech_Caretaker)
        {
            Utils.ShutOffShield(___pawn);
        }
    }
}
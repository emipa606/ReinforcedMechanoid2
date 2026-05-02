using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Bill_ResurrectMech), nameof(Bill_ResurrectMech.PawnAllowedToStartAnew))]
public static class Bill_ResurrectMech_PawnAllowedToStartAnew
{
    public static bool Prefix(ref bool __result, Pawn p)
    {
        if (p.mechanitor != null)
        {
            return true;
        }

        // Matriarchs controlled by the Gestalt Engine have no mechanitor but should
        // be allowed to perform resurrection bills. Skip vanilla logic to avoid null-ref.
        if (p.kindDef == RM_DefOf.RM_Mech_Matriarch && p.Faction == Faction.OfPlayer)
        {
            __result = true;
            return false;
        }

        // Any other pawn with no mechanitor should not be allowed to start this bill.
        __result = false;
        return false;
    }
}

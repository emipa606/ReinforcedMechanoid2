using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Bill_ResurrectMech), nameof(Bill_ResurrectMech.PawnAllowedToStartAnew))]
public static class Bill_ResurrectMech_PawnAllowedToStartAnew
{
    public static bool Prefix(Bill_ResurrectMech __instance, ref bool __result, Pawn p)
    {
        if (p.mechanitor != null)
        {
            return true;
        }

        // Matriarchs controlled by the Gestalt Engine have no mechanitor but should
        // be allowed to perform resurrection bills. Skip vanilla logic to avoid null-ref.
        if (p.kindDef == RM_DefOf.RM_Mech_Matriarch && p.Faction == Faction.OfPlayer)
        {
            // Re-apply the ActiveBill lock from Bill_Autonomous.PawnAllowedToStartAnew,
            // which is skipped along with the rest of the vanilla logic. Without it a
            // Matriarch can start a second bill while another one is mid-gestation,
            // stealing ActiveBill and leaving the first bill permanently stuck in Forming.
            var workTable = (Building_WorkTableAutonomous)__instance.billStack.billGiver;
            __result = workTable.ActiveBill == null || workTable.ActiveBill == __instance
                                                    || workTable.ActiveBill.suspended
                                                    && workTable.ActiveBill.State == FormingState.Gathering
                                                    && workTable.innerContainer.Count == 0;
            return false;
        }

        // Any other pawn with no mechanitor should not be allowed to start this bill.
        __result = false;
        return false;
    }
}
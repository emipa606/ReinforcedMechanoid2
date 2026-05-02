using HarmonyLib;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.DestroyEquipment))]
public static class Pawn_EquipmentTracker_DestroyEquipment
{
    public static bool Prefix(Pawn_EquipmentTracker __instance)
    {
        if (!__instance.pawn.RaceProps.IsMechanoid)
        {
            return true;
        }

        return !__instance.pawn.Downed;
    }
}
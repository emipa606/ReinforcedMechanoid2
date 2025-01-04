using HarmonyLib;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.DropAllEquipment))]
public static class Pawn_EquipmentTracker_DropAllEquipment
{
    public static bool Prefix(Pawn_EquipmentTracker __instance)
    {
        return !__instance.pawn.RaceProps.IsMechanoid || __instance.pawn.Dead;
    }
}
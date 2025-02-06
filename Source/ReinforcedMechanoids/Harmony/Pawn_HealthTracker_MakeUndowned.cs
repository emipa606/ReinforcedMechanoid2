using HarmonyLib;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Pawn_HealthTracker), "MakeUndowned")]
public static class Pawn_HealthTracker_MakeUndowned
{
    public static void Prefix(Pawn ___pawn)
    {
        if (!___pawn.RaceProps.IsMechanoid)
        {
            return;
        }

        var equipmentComponent = Current.Game.GetComponent<GameComponent_MechWeapons>();
        var possibleWeapon = equipmentComponent?.LoadWeapon(___pawn);
        if (possibleWeapon == null)
        {
            return;
        }

        ___pawn.equipment.AddEquipment(possibleWeapon);
    }
}
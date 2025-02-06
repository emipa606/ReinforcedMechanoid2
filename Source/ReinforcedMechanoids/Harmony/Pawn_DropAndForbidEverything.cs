using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.DropAndForbidEverything))]
public static class Pawn_DropAndForbidEverything
{
    public static void Prefix(Pawn __instance)
    {
        if (!__instance.kindDef.destroyGearOnDrop)
        {
            return;
        }

        var pawn = __instance;
        if (pawn.apparel == null)
        {
            pawn.apparel = new Pawn_ApparelTracker(__instance);
        }

        pawn = __instance;
        if (pawn.equipment == null)
        {
            pawn.equipment = new Pawn_EquipmentTracker(__instance);
        }

        if (!pawn.RaceProps.IsMechanoid)
        {
            return;
        }

        if (pawn.equipment.Primary == null)
        {
            return;
        }

        var equipmentComponent = Current.Game.GetComponent<GameComponent_MechWeapons>();

        equipmentComponent?.SaveWeapon(pawn, pawn.equipment.Primary.def);
    }
}
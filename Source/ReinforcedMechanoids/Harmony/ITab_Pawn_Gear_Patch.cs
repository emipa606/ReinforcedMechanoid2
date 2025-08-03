using HarmonyLib;
using RimWorld;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(ITab_Pawn_Gear), nameof(ITab_Pawn_Gear.DrawThingRow))]
public static class ITab_Pawn_Gear_Patch
{
    public static bool drawingThingRow;

    public static void Prefix()
    {
        drawingThingRow = true;
    }

    public static void Postfix()
    {
        drawingThingRow = false;
    }
}
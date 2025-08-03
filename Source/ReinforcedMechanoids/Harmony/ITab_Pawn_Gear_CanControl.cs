using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(ITab_Pawn_Gear), nameof(ITab_Pawn_Gear.CanControl), MethodType.Getter)]
public static class ITab_Pawn_Gear_CanControl
{
    private static readonly PawnToShowInfoAbout pawnToShowInfoAbout = AccessTools.MethodDelegate<PawnToShowInfoAbout>
        (AccessTools.Method(typeof(ITab_Pawn_Gear), "get_SelPawnForGear"));

    public static void Postfix(ITab_Pawn_Gear __instance, ref bool __result)
    {
        if (!ITab_Pawn_Gear_Patch.drawingThingRow)
        {
            return;
        }

        var pawn = pawnToShowInfoAbout(__instance);
        var comp = pawn.GetComp<CompMachine>();
        if (comp != null && !comp.Props.canPickupWeapons)
        {
            __result = false;
        }
    }

    private delegate Pawn PawnToShowInfoAbout(ITab_Pawn_Gear __instance);
}
using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(ITab_Pawn_Character), nameof(ITab_Pawn_Character.IsVisible), MethodType.Getter)]
public static class ITab_Pawn_Character_IsVisible
{
    public delegate Pawn PawnToShowInfoAbout(ITab_Pawn_Character __instance);

    private static readonly PawnToShowInfoAbout pawnToShowInfoAbout = AccessTools.MethodDelegate<PawnToShowInfoAbout>
        (AccessTools.Method(typeof(ITab_Pawn_Character), "get_PawnToShowInfoAbout"));

    public static void Postfix(ITab_Pawn_Character __instance, ref bool __result)
    {
        var pawn = pawnToShowInfoAbout(__instance);
        if (pawn is Machine)
        {
            __result = false;
        }
    }
}
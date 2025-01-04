using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(StatWorker), nameof(StatWorker.IsDisabledFor))]
internal static class StatWorker_IsDisabledFor
{
    private static bool Prefix(Thing thing, ref bool __result)
    {
        if (thing is not Pawn pawn || !pawn.RaceProps.IsMechanoid)
        {
            return true;
        }

        if (pawn.Faction != Faction.OfPlayer)
        {
            return true;
        }

        __result = false;
        return false;
    }
}
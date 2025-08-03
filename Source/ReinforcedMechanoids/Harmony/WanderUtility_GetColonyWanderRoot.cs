using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(WanderUtility), nameof(WanderUtility.GetColonyWanderRoot))]
public static class WanderUtility_GetColonyWanderRoot
{
    public static void Postfix(ref IntVec3 __result, Pawn pawn)
    {
        try
        {
            if (pawn.Map == null || pawn is not Machine || pawn.Faction != Faction.OfPlayer ||
                !__result.IsForbidden(pawn) ||
                !(pawn.playerSettings?.EffectiveAreaRestrictionInPawnCurrentMap.ActiveCells).Any())
            {
                return;
            }

            if (pawn.playerSettings != null)
            {
                __result = pawn.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap.ActiveCells
                    .OrderBy(x => x.DistanceTo(pawn.Position))
                    .Where(x => x.Walkable(pawn.Map) &&
                                pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly))
                    .Take(10).RandomElement();
            }
        }
        catch
        {
            // ignored
        }
    }
}
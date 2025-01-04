using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.StartPath))]
public class Pawn_PathFollower_StartPath
{
    private static void Postfix(Pawn_PathFollower __instance, Pawn ___pawn)
    {
        var comp = ___pawn.GetComp<CompPawnJumper>();
        var destination = __instance.Destination;
        if (comp != null && comp.CanJumpTo(destination) && ___pawn.InCombat() && ___pawn.Faction != Faction.OfPlayer)
        {
            comp.DoJump(destination);
        }
    }
}
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
public class Pawn_JobTracker_StartJob
{
    private static bool Prefix(Pawn ___pawn, Job newJob)
    {
        if (!JobGiver_AIFightEnemy_TryGiveJob.gotoCombatJobs.Contains(newJob))
        {
            return true;
        }

        var comp = ___pawn.GetComp<CompPawnJumper>();
        if (comp == null || ___pawn.Faction == Faction.OfPlayer)
        {
            return true;
        }

        var targetA = newJob.targetA;
        if (!comp.CanJumpTo(targetA))
        {
            return true;
        }

        comp.DoJump(targetA);
        return false;
    }
}
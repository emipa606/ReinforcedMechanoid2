using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse.AI;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(JobGiver_AIFightEnemy), nameof(JobGiver_AIFightEnemy.TryGiveJob))]
public static class JobGiver_AIFightEnemy_TryGiveJob
{
    public static readonly HashSet<Job> gotoCombatJobs = [];

    public static void Postfix(ref Job __result)
    {
        if (__result != null && __result.def == JobDefOf.Goto)
        {
            gotoCombatJobs.Add(__result);
        }
    }
}
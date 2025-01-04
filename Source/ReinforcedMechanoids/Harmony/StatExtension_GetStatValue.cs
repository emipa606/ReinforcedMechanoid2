using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]
public static class StatExtension_GetStatValue
{
    private static void Postfix(Thing thing, StatDef stat, ref float __result)
    {
        if (thing is not Pawn pawn)
        {
            return;
        }

        if (pawn.CurJob == null || pawn.CurJobDef != RM_DefOf.RM_FollowClose)
        {
            return;
        }

        if (stat != StatDefOf.MoveSpeed)
        {
            return;
        }

        if (pawn.CurJob.targetA.Thing is not Pawn pawn2)
        {
            return;
        }

        var statValue = pawn2.GetStatValue(StatDefOf.MoveSpeed);
        var num = pawn2.Position.DistanceTo(pawn.Position);
        var num2 = Mathf.Lerp(1.2f, 2f, Mathf.Min(1f, num / 10f));
        __result = Mathf.Min(__result, statValue * num2);
    }
}
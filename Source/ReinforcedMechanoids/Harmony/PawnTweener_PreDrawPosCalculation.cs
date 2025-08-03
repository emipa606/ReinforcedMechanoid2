using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(PawnTweener), nameof(PawnTweener.PreDrawPosCalculation))]
public static class PawnTweener_PreDrawPosCalculation
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        var pawnField = AccessTools.Field(typeof(PawnTweener), "pawn");
        for (var i = 0; i < codes.Count; i++)
        {
            yield return codes[i];
            if (i <= 1 || codes[i - 1].opcode != OpCodes.Ldloc_1 || codes[i].opcode != OpCodes.Ldloc_2)
            {
                continue;
            }

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
            yield return new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(PawnTweener_PreDrawPosCalculation), nameof(ReturnNum)));
        }
    }

    public static float ReturnNum(float num, Pawn pawn)
    {
        if (pawn.pather is { moving: false } &&
            pawn.health?.hediffSet?.GetFirstHediffOfDef(RM_DefOf.RM_BehemothAttack) != null)
        {
            return num * 0.5f;
        }

        return num;
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch]
public static class JobDriver_Flee_MakeNewToils
{
    public static MethodBase TargetMethod()
    {
        return typeof(JobDriver_Flee).GetMethods(AccessTools.all).First(x => x.Name.Contains("<MakeNewToils>"));
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        var isColonistCheck = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.IsColonist));
        foreach (var code in codes)
        {
            if (code.Calls(isColonistCheck))
            {
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(JobDriver_Flee_MakeNewToils), nameof(CanEmitFleeMote)));
            }
            else
            {
                yield return code;
            }
        }
    }

    // this is for harmony patching via other mods
    public static bool CanEmitFleeMote(Pawn pawn)
    {
        return pawn.IsColonist || pawn.kindDef == RM_DefOf.RM_Mech_Vulture;
    }
}
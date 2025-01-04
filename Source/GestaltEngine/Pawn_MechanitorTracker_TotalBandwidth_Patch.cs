using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GestaltEngine;

[HarmonyPatch(typeof(Pawn_MechanitorTracker), "TotalBandwidth", MethodType.Getter)]
public static class Pawn_MechanitorTracker_TotalBandwidth_Patch
{
    public static void Postfix(Pawn_MechanitorTracker __instance, ref int __result)
    {
        if (!__instance.pawn.TryGetGestaltEngineInstead(out var result))
        {
            return;
        }

        if (result.compPower.PowerOn)
        {
            __result = result.CurrentUpgrade.totalMechBandwidth;
            {
                foreach (var item in result.dummyPawn.health.hediffSet.hediffs.OfType<Hediff_BandNode>())
                {
                    var statModifier = item.CurStage.statOffsets.FirstOrDefault(x => x.stat == StatDefOf.MechBandwidth);
                    if (statModifier != null)
                    {
                        __result += (int)statModifier.value;
                    }
                }

                return;
            }
        }

        __result = 0;
    }
}
using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace GestaltEngine
{
    [HarmonyPatch(typeof(Pawn_MechanitorTracker), "TotalBandwidth", MethodType.Getter)]
    public static class Pawn_MechanitorTracker_TotalBandwidth_Patch
    {
        public static void Postfix(Pawn_MechanitorTracker __instance, ref int __result)
        {
            if (__instance.pawn.TryGetGestaltEngineInstead(out var comp))
            {
                if (comp.compPower.PowerOn)
                {
                    __result = comp.CurrentUpgrade.totalMechBandwidth;
                    foreach (var hediff in comp.dummyPawn.health.hediffSet.hediffs.OfType<Hediff_BandNode>())
                    {
                        var statOffset = hediff.CurStage.statOffsets.FirstOrDefault(x => x.stat == StatDefOf.MechBandwidth);
                        if (statOffset != null)
                        {
                            __result += (int)statOffset.value;
                        }
                    }
                }
                else
                {
                    __result = 0;
                }
            }
        }
    }
}

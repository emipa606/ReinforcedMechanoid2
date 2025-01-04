using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(MechClusterGenerator), nameof(MechClusterGenerator.MechKindSuitableForCluster))]
public class MechClusterGenerator_MechKindSuitableForCluster
{
    public static void Postfix(PawnKindDef __0, ref bool __result)
    {
        if (__result && ReinforcedMechanoidsSettings.disabledMechanoids.Contains(__0.defName))
        {
            __result = false;
        }
    }
}
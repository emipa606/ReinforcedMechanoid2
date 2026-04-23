using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(JobGiver_Work), nameof(JobGiver_Work.PawnCanUseWorkGiver))]
public static class JobGiver_Work_PawnCanUseWorkGiver
{
    public static void Postfix(ref bool __result, Pawn pawn, WorkGiver giver)
    {
        if (pawn.kindDef == RM_DefOf.RM_Mech_Vulture
            && !__result
            && giver.def.workType == WorkTypeDefOf.Construction
            && pawn.mindState?.duty?.def == RM_DefOf.RM_Build)
        {
            __result = true;
        }

        if (pawn is not Machine || !CompMachine.cachedMachinesPawns.TryGetValue(pawn, out var comp))
        {
            return;
        }

        var chargingComp = comp?.myBuilding?.TryGetComp<CompMachineChargingStation>();
        if (chargingComp == null)
        {
            return;
        }

        switch (__result)
        {
            case true when chargingComp.Props.disallowedWorkGivers != null &&
                           chargingComp.Props.disallowedWorkGivers.Contains(giver.def):
                __result = false;
                break;
            case false when pawn.def.race.mechEnabledWorkTypes != null
                            && pawn.def.race.mechEnabledWorkTypes.Contains(giver.def.workType):
                __result = true;
                break;
        }
    }
}
using HarmonyLib;
using RimWorld;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(PawnGroupMaker), nameof(PawnGroupMaker.CanGenerateFrom))]
public static class PawnGroupMaker_CanGenerateFrom
{
    public static void Postfix(ref bool __result, PawnGroupMaker __instance, PawnGroupMakerParms parms)
    {
        if (!__result)
        {
            return;
        }

        switch (__instance)
        {
            case PawnGroupMaker_CaretakerRaid:
                __result = parms.raidStrategy?.Worker is RaidStrategyWorker_ImmediateAttackMechanoidCaretaker;
                break;
            case PawnGroupMaker_CaretakerRaidWithMechPresence
                pawnGroupMaker_CaretakerRaidWithMechPresence:
                __result = pawnGroupMaker_CaretakerRaidWithMechPresence.CanGenerate(parms) &&
                           parms.raidStrategy?.Worker is RaidStrategyWorker_ImmediateAttackMechanoidCaretaker;
                break;
            case PawnGroupMaker_WraithSiege:
                __result = parms.raidStrategy?.Worker is RaidStrategyWorker_ImmediateAttackMechanoidWraith;
                break;
            case PawnGroupMaker_LocustRaid:
                __result = parms.raidStrategy?.Worker is RaidStrategyWorker_ImmediateAttackMechanoidLocust;
                break;
            default:
                __result = parms.raidStrategy?.Worker is not RaidStrategyWorker_ImmediateAttackWithCertainPawnKind;
                break;
        }
    }
}
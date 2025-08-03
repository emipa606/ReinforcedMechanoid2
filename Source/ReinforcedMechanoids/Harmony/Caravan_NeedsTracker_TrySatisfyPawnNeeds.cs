using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Caravan_NeedsTracker), nameof(Caravan_NeedsTracker.TrySatisfyPawnNeeds))]
public static class Caravan_NeedsTracker_TrySatisfyPawnNeeds
{
    public static void Postfix(Caravan_NeedsTracker __instance, Pawn pawn)
    {
        var need = pawn.needs.TryGetNeed<Need_Power>();
        if (need == null)
        {
            return;
        }

        if (!(need.CurLevel <= 0))
        {
            return;
        }

        PawnBanishUtility.Banish(pawn);
        pawn.Kill(null);
        Messages.Message("VFEM.CaravanMachineRanOutPower".Translate(pawn.Named("MACHINE")), __instance.caravan,
            MessageTypeDefOf.CautionInput);
    }
}
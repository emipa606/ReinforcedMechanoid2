using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(Pawn_MeleeVerbs), nameof(Pawn_MeleeVerbs.ChooseMeleeVerb))]
public static class Pawn_MeleeVerbs_ChooseMeleeVerb
{
    public static bool Prefix(Pawn_MeleeVerbs __instance, Thing target)
    {
        if (__instance.pawn.kindDef != RM_DefOf.RM_Mech_Behemoth)
        {
            return true;
        }

        var nonMissingBodyPart = Utils.GetNonMissingBodyPart(__instance.pawn, RM_DefOf.RM_BehemothShield);
        if (nonMissingBodyPart == null)
        {
            return true;
        }

        var verbEntry = (from x in __instance.GetUpdatedAvailableVerbsList(false)
            where x.verb is Verb_MeleeAttackDamageBehemoth
            select x).FirstOrDefault();
        if (verbEntry.verb == null)
        {
            return true;
        }

        __instance.SetCurMeleeVerb(verbEntry.verb, target);
        return false;
    }
}
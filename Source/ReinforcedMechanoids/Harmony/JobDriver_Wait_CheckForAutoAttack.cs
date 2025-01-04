using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(JobDriver_Wait), nameof(JobDriver_Wait.CheckForAutoAttack))]
public static class JobDriver_Wait_CheckForAutoAttack
{
    public static bool Prefix(JobDriver_Wait __instance)
    {
        if (__instance.pawn.kindDef != RM_DefOf.RM_Mech_WraithSiege)
        {
            return true;
        }

        CheckForAutoAttack(__instance);
        return false;
    }

    private static void CheckForAutoAttack(JobDriver_Wait __instance)
    {
        if (__instance.pawn.Downed || __instance.pawn.stances.FullBodyBusy || __instance.pawn.IsCarryingPawn())
        {
            return;
        }

        __instance.collideWithPawns = false;
        if (!(!__instance.pawn.WorkTagIsDisabled(WorkTags.Violent) || __instance.pawn.RaceProps.ToolUser &&
                __instance.pawn.Faction == Faction.OfPlayer &&
                !__instance.pawn.WorkTagIsDisabled(WorkTags.Firefighting)))
        {
            return;
        }

        Fire fire = null;
        for (var i = 0; i < 9; i++)
        {
            var c = __instance.pawn.Position + GenAdj.AdjacentCellsAndInside[i];
            if (!c.InBounds(__instance.pawn.Map))
            {
                continue;
            }

            var thingList = c.GetThingList(__instance.Map);
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var j = 0; j < thingList.Count; j++)
            {
                if (!__instance.pawn.WorkTagIsDisabled(WorkTags.Violent) &&
                    thingList[j] is Pawn { Downed: false } pawn && __instance.pawn.HostileTo(pawn) &&
                    !__instance.pawn.ThreatDisabledBecauseNonAggressiveRoamer(pawn) &&
                    GenHostility.IsActiveThreatTo(pawn, __instance.pawn.Faction))
                {
                    __instance.pawn.meleeVerbs.TryMeleeAttack(pawn);
                    __instance.collideWithPawns = true;
                    return;
                }

                if (__instance.pawn.RaceProps.ToolUser && __instance.pawn.Faction == Faction.OfPlayer &&
                    !__instance.pawn.WorkTagIsDisabled(WorkTags.Firefighting) && thingList[j] is Fire fire2 &&
                    (fire == null || fire2.fireSize < fire.fireSize || i == 8) &&
                    (fire2.parent == null || fire2.parent != __instance.pawn))
                {
                    fire = fire2;
                }
            }
        }

        if (fire != null && (!__instance.pawn.InMentalState || __instance.pawn.MentalState.def.allowBeatfire))
        {
            __instance.pawn.natives.TryBeatFire(fire);
        }
        else
        {
            if (__instance.pawn.WorkTagIsDisabled(WorkTags.Violent) || !__instance.job.canUseRangedWeapon ||
                __instance.pawn.Faction == null ||
                __instance.job.def != JobDefOf.Wait_Combat ||
                __instance.pawn.drafter is { FireAtWill: false })
            {
                return;
            }

            var currentEffectiveVerb = __instance.pawn.CurrentEffectiveVerb;
            if (currentEffectiveVerb == null || currentEffectiveVerb.verbProps.IsMeleeAttack)
            {
                return;
            }

            var targetable = TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
            if (currentEffectiveVerb.IsIncendiary_Ranged())
            {
                targetable |= TargetScanFlags.NeedNonBurning;
            }

            var thing = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(__instance.pawn,
                targetable);
            if (thing == null)
            {
                return;
            }

            __instance.pawn.TryStartAttack(thing);
            __instance.collideWithPawns = true;
        }
    }
}
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids;

public class CompInvisibility : ThingComp
{
    private bool invisibilityOn;
    private int nextAssignTicks;

    private CompProperties_Invisibility Props => props as CompProperties_Invisibility;

    private bool CanGiveHediff
    {
        get
        {
            if (!invisibilityOn)
            {
                return false;
            }

            if (parent is not Pawn pawn)
            {
                return false;
            }

            if (Props.disableWhenInCombat && pawn.InCombat())
            {
                return false;
            }

            if (nextAssignTicks > Find.TickManager.TicksGame &&
                (Props.cooldownWhenInCombat && pawn.InCombat() || !Props.cooldownWhenInCombat))
            {
                return false;
            }

            return pawn.health.hediffSet.GetFirstHediffOfDef(RM_DefOf.RM_ZealotInvisibility) == null;
        }
    }

    public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.PostPostApplyDamage(dinfo, totalDamageDealt);
        if (parent is not Pawn pawn || !Props.removeWhenDamaged ||
            !pawn.health.hediffSet.HasHediff(RM_DefOf.RM_ZealotInvisibility))
        {
            return;
        }

        var firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(RM_DefOf.RM_ZealotInvisibility);
        pawn.health.RemoveHediff(firstHediffOfDef);
    }

    public override void CompTick()
    {
        base.CompTick();
        if (parent is not Pawn pawn)
        {
            return;
        }

        if (CanGiveHediff)
        {
            MakeInvisible(pawn);
        }
        else if (Props.disableWhenInCombat && pawn.InCombat() &&
                 pawn.health.hediffSet.HasHediff(RM_DefOf.RM_ZealotInvisibility))
        {
            TurnOffInvisibility(pawn);
        }
    }

    private void MakeInvisible(Pawn pawn)
    {
        var hediff = HediffMaker.MakeHediff(RM_DefOf.RM_ZealotInvisibility, pawn);
        pawn.health.AddHediff(hediff);
        nextAssignTicks = Find.TickManager.TicksGame + Props.cooldownTicksRange.RandomInRange;
    }

    private static void TurnOffInvisibility(Pawn pawn)
    {
        var firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(RM_DefOf.RM_ZealotInvisibility);
        pawn.health.RemoveHediff(firstHediffOfDef);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (parent.Faction != Faction.OfPlayer)
        {
            yield break;
        }

        yield return new Command_Toggle
        {
            defaultLabel = "RM.ToggleInvisibility".Translate(),
            defaultDesc = "RM.ToggleInvisibilityDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Buttons/ZealotInvisibility"),
            isActive = () => invisibilityOn,
            toggleAction = delegate
            {
                if (parent is not Pawn pawn)
                {
                    return;
                }

                invisibilityOn = !invisibilityOn;
                switch (invisibilityOn)
                {
                    case true when CanGiveHediff:
                        MakeInvisible(pawn);
                        break;
                    case false when
                        pawn.health.hediffSet.HasHediff(RM_DefOf.RM_ZealotInvisibility):
                        TurnOffInvisibility(pawn);
                        break;
                }
            }
        };
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref invisibilityOn, "invisibilityOn");
    }
}
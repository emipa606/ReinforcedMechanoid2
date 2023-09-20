using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids
{
    public class CompProperties_Invisibility : CompProperties
    {
        public IntRange cooldownTicksRange;
        public bool disableWhenInCombat;
        public bool cooldownWhenInCombat;
        public bool removeWhenDamaged;
        public CompProperties_Invisibility()
        {
            this.compClass = typeof(CompInvisibility);
        }
    }

    public class CompInvisibility : ThingComp
    {
        public int nextAssignTicks;

        public bool invisibilityOn;
        public CompProperties_Invisibility Props => base.props as CompProperties_Invisibility;
        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            var pawn = this.parent as Pawn;
            if (Props.removeWhenDamaged && pawn.health.hediffSet.HasHediff(RM_DefOf.RM_ZealotInvisibility))
            {
                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(RM_DefOf.RM_ZealotInvisibility);
                pawn.health.RemoveHediff(hediff);
            }
        }
        public bool CanGiveHediff
        {
            get
            {
                if (this.invisibilityOn is false)
                {
                    return false;
                }

                var pawn = this.parent as Pawn;
                if (Props.disableWhenInCombat && pawn.InCombat())
                {
                    return false;
                }
                else if (nextAssignTicks > Find.TickManager.TicksGame)
                {
                    if (Props.cooldownWhenInCombat && pawn.InCombat() || Props.cooldownWhenInCombat is false)
                    {
                        return false;
                    }
                }
                if (pawn.health.hediffSet.GetFirstHediffOfDef(RM_DefOf.RM_ZealotInvisibility) != null)
                {
                    return false;
                }
                return true;
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            var pawn = this.parent as Pawn;
            if (CanGiveHediff)
            {
                MakeInvisible(pawn);
            }
            else if (Props.disableWhenInCombat && pawn.InCombat() && pawn.health.hediffSet.HasHediff(RM_DefOf.RM_ZealotInvisibility))
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
        private void TurnOffInvisibility(Pawn pawn)
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(RM_DefOf.RM_ZealotInvisibility);
            pawn.health.RemoveHediff(hediff);
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (this.parent.Faction == Faction.OfPlayer)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "RM.ToggleInvisibility".Translate(),
                    defaultDesc = "RM.ToggleInvisibilityDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/ZealotInvisibility"),
                    isActive = () => invisibilityOn,
                    toggleAction = delegate
                    {
                        var pawn = this.parent as Pawn;
                        invisibilityOn = !invisibilityOn;
                        if (invisibilityOn && CanGiveHediff)
                        {
                            MakeInvisible(pawn);
                        }
                        else if (invisibilityOn is false && pawn.health.hediffSet.HasHediff(RM_DefOf.RM_ZealotInvisibility))
                        {
                            TurnOffInvisibility(pawn);
                        }
                    }
                };
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref invisibilityOn, "invisibilityOn");
        }
    }
}

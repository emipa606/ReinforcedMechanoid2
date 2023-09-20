using Verse;

namespace ReinforcedMechanoids
{
    public class CompProperties_GiveHediff : CompProperties
    {
        public HediffDef hediff;
        public IntRange cooldownTicksRange;
        public bool disableWhenInCombat;
        public bool cooldownWhenInCombat;
        public bool removeWhenDamaged;
        public CompProperties_GiveHediff()
        {
            this.compClass = typeof(CompGiveHediff);
        }
    }
    public class CompGiveHediff : ThingComp
    {
        public Hediff assignedHediff;

        public int nextAssignTicks;
        public CompProperties_GiveHediff Props => base.props as CompProperties_GiveHediff;

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            var pawn = this.parent as Pawn;
            if (Props.removeWhenDamaged && assignedHediff != null && pawn.health.hediffSet.hediffs.Contains(assignedHediff))
            {
                pawn.health.RemoveHediff(assignedHediff);
                assignedHediff = null;
            }
        }
        public bool CanGiveHediff
        {
            get
            {
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
                if (pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff) != null)
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
                var hediff = HediffMaker.MakeHediff(Props.hediff, pawn);
                pawn.health.AddHediff(hediff);
                assignedHediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                nextAssignTicks = Find.TickManager.TicksGame + Props.cooldownTicksRange.RandomInRange;
            }
            else if (Props.disableWhenInCombat && pawn.InCombat() && assignedHediff != null && pawn.health.hediffSet.hediffs.Contains(assignedHediff))
            {
                pawn.health.RemoveHediff(assignedHediff);
                assignedHediff = null;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref assignedHediff, "assignedHediff");
        }
    }
}

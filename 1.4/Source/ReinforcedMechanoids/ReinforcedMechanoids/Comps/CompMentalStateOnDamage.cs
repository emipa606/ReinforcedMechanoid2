using Verse;
using Verse.AI;

namespace ReinforcedMechanoids
{
    public class CompProperties_MentalStateOnDamage : CompProperties
    {
        public float maxHealthPctCondition = 1f;
        public float chanceOfMentalBreak;
        public MentalStateDef mentalState;
        public HediffDef hediff;
        public CompProperties_MentalStateOnDamage()
        {
            this.compClass = typeof(CompMentalStateOnDamage);
        }
    }
    public class CompMentalStateOnDamage : ThingComp
    {
        public bool mentalStateIsCaused;
        public CompProperties_MentalStateOnDamage Props => base.props as CompProperties_MentalStateOnDamage;
        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            var pawn = this.parent as Pawn;
            var mentalState = pawn.MentalState;
            var healthPct = pawn.health.summaryHealth.SummaryHealthPercent;
            if (mentalState is null)
            {
                if (Props.maxHealthPctCondition >= healthPct)
                {
                    if (Rand.Chance(Props.chanceOfMentalBreak))
                    {
                        if (pawn.mindState is null)
                        {
                            pawn.mindState = new Pawn_MindState(pawn);
                        }
                        if (pawn.mindState.mentalStateHandler.TryStartMentalState(Props.mentalState))
                        {
                            if (Props.hediff != null)
                            {
                                var hediff = HediffMaker.MakeHediff(Props.hediff, pawn);
                                pawn.health.AddHediff(hediff);
                            }
                            mentalStateIsCaused = true;
                        }
                    }
                }
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (mentalStateIsCaused)
            {
                var pawn = this.parent as Pawn;
                var healthPct = pawn.health.summaryHealth.SummaryHealthPercent;
                if (healthPct > Props.maxHealthPctCondition)
                {
                    var mentalState = pawn.MentalState;
                    mentalState.RecoverFromState();
                    if (Props.hediff != null)
                    {
                        var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                        if (hediff != null)
                        {
                            pawn.health.RemoveHediff(hediff);
                        }
                    }
                    mentalStateIsCaused = false;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref mentalStateIsCaused, "mentalStateIsCaused");
        }
    }
}

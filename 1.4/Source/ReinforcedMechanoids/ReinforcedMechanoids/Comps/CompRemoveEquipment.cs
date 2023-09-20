using RimWorld;
using Verse;

namespace ReinforcedMechanoids
{

    public class CompProperties_RemoveEquipment : CompProperties
    {
        public float healthPctThreshold;
        public bool removePrimaryEquipment;
        public HediffDef removeHediff;
        public CompProperties_RemoveEquipment()
        {
            this.compClass = typeof(CompRemoveEquipment);
        }
    }
    public class CompRemoveEquipment : ThingComp
    {
        public CompProperties_RemoveEquipment Props => base.props as CompProperties_RemoveEquipment;

        public bool hadHealthHigherThanThreshold;
        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(dinfo, out absorbed);
            var pawn = this.parent as Pawn;
            if (Props.healthPctThreshold > 0)
            {
                hadHealthHigherThanThreshold = pawn.health.summaryHealth.SummaryHealthPercent > Props.healthPctThreshold;
            }
        }
        
        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            var pawn = this.parent as Pawn;
            if (Props.healthPctThreshold > 0 && hadHealthHigherThanThreshold
                && pawn.health.summaryHealth.SummaryHealthPercent <= Props.healthPctThreshold)
            {
                if (Props.removePrimaryEquipment)
                {
                    if (pawn.equipment?.Primary != null)
                    {
                        pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
                    }
                }
                if (Props.removeHediff != null)
                {
                    var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.removeHediff);
                    if (hediff != null)
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                }
            }
        }
    }
}

using Verse;

namespace ReinforcedMechanoids;

public class CompRemoveEquipment : ThingComp
{
    private bool hadHealthHigherThanThreshold;

    private CompProperties_RemoveEquipment Props => props as CompProperties_RemoveEquipment;

    public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
        base.PostPreApplyDamage(ref dinfo, out absorbed);
        if (parent is not Pawn pawn)
        {
            return;
        }

        if (Props.healthPctThreshold > 0f)
        {
            hadHealthHigherThanThreshold = pawn.health.summaryHealth.SummaryHealthPercent > Props.healthPctThreshold;
        }
    }

    public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.PostPostApplyDamage(dinfo, totalDamageDealt);
        if (parent is not Pawn pawn)
        {
            return;
        }

        if (!(Props.healthPctThreshold > 0f) || !hadHealthHigherThanThreshold ||
            !(pawn.health.summaryHealth.SummaryHealthPercent <= Props.healthPctThreshold))
        {
            return;
        }

        if (Props.removePrimaryEquipment && pawn.equipment?.Primary != null)
        {
            pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
        }

        if (Props.removeHediff == null)
        {
            return;
        }

        var firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(Props.removeHediff);
        if (firstHediffOfDef != null)
        {
            pawn.health.RemoveHediff(firstHediffOfDef);
        }
    }
}
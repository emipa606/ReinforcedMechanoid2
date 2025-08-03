using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

public class CompMentalStateOnDamage : ThingComp
{
    private bool mentalStateIsCaused;

    private CompProperties_MentalStateOnDamage Props => props as CompProperties_MentalStateOnDamage;

    public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.PostPostApplyDamage(dinfo, totalDamageDealt);
        if (parent is not Pawn pawn)
        {
            return;
        }

        var mentalState = pawn.MentalState;
        var summaryHealthPercent = pawn.health.summaryHealth.SummaryHealthPercent;
        if (mentalState != null || !(Props.maxHealthPctCondition >= summaryHealthPercent) ||
            !Rand.Chance(Props.chanceOfMentalBreak))
        {
            return;
        }

        pawn.mindState ??= new Pawn_MindState(pawn);

        if (!pawn.mindState.mentalStateHandler.TryStartMentalState(Props.mentalState))
        {
            return;
        }

        if (Props.hediff != null)
        {
            var hediff = HediffMaker.MakeHediff(Props.hediff, pawn);
            pawn.health.AddHediff(hediff);
        }

        mentalStateIsCaused = true;
    }

    public override void CompTick()
    {
        base.CompTick();
        if (!mentalStateIsCaused)
        {
            return;
        }

        if (parent is not Pawn pawn)
        {
            return;
        }

        var summaryHealthPercent = pawn.health.summaryHealth.SummaryHealthPercent;
        if (!(summaryHealthPercent > Props.maxHealthPctCondition))
        {
            return;
        }

        var mentalState = pawn.MentalState;
        mentalState?.RecoverFromState();
        if (Props.hediff != null)
        {
            var firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
            if (firstHediffOfDef != null)
            {
                pawn.health.RemoveHediff(firstHediffOfDef);
            }
        }

        mentalStateIsCaused = false;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref mentalStateIsCaused, "mentalStateIsCaused");
    }
}
using Verse;

namespace ReinforcedMechanoids;

public class CompGiveHediff : ThingComp
{
    public Hediff assignedHediff;

    public int nextAssignTicks;

    public CompProperties_GiveHediff Props => props as CompProperties_GiveHediff;

    public bool CanGiveHediff
    {
        get
        {
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

            return pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff) == null;
        }
    }

    public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.PostPostApplyDamage(dinfo, totalDamageDealt);
        if (parent is not Pawn pawn)
        {
            return;
        }

        if (!Props.removeWhenDamaged || assignedHediff == null ||
            !pawn.health.hediffSet.hediffs.Contains(assignedHediff))
        {
            return;
        }

        pawn.health.RemoveHediff(assignedHediff);
        assignedHediff = null;
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
            var hediff = HediffMaker.MakeHediff(Props.hediff, pawn);
            pawn.health.AddHediff(hediff);
            assignedHediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
            nextAssignTicks = Find.TickManager.TicksGame + Props.cooldownTicksRange.RandomInRange;
            return;
        }

        if (!Props.disableWhenInCombat || !pawn.InCombat() || assignedHediff == null ||
            !pawn.health.hediffSet.hediffs.Contains(assignedHediff))
        {
            return;
        }

        pawn.health.RemoveHediff(assignedHediff);
        assignedHediff = null;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_References.Look(ref assignedHediff, "assignedHediff");
    }
}
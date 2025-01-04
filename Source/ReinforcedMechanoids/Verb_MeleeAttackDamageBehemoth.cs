using RimWorld;
using Verse;

namespace ReinforcedMechanoids;

public class Verb_MeleeAttackDamageBehemoth : Verb_MeleeAttackDamage
{
    public override bool Available()
    {
        return Utils.GetNonMissingBodyPart(CasterPawn, RM_DefOf.RM_BehemothShield) != null && base.Available();
    }

    public override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
    {
        if (target.Thing is Pawn pawn && pawn.health.hediffSet.GetFirstHediffOfDef(RM_DefOf.RM_BehemothAttack) == null)
        {
            pawn.health.AddHediff(HediffMaker.MakeHediff(RM_DefOf.RM_BehemothAttack, pawn));
        }

        return base.ApplyMeleeDamageToTarget(target);
    }
}
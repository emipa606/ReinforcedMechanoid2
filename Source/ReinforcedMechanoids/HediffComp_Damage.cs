using Verse;

namespace ReinforcedMechanoids;

public class HediffComp_Damage : HediffComp
{
    public bool damaged;

    public HediffCompProperties_Damage Props => props as HediffCompProperties_Damage;

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (damaged || !(parent.Severity >= Props.severityThreshold))
        {
            return;
        }

        damaged = true;
        if (Props.explosionRadius > 0f && Pawn.MapHeld != null)
        {
            GenExplosion.DoExplosion(Pawn.PositionHeld, Pawn.MapHeld, Props.explosionRadius, Props.damageDef, Pawn,
                Props.damageAmount);
        }
        else
        {
            Pawn.TakeDamage(new DamageInfo(Props.damageDef, Props.damageAmount));
        }
    }

    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_Values.Look(ref damaged, "exploded");
    }
}
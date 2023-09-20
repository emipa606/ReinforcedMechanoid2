using Verse;


namespace ReinforcedMechanoids
{
    public class HediffCompProperties_Damage : HediffCompProperties
    {
        public float severityThreshold;
        public DamageDef damageDef;
        public float explosionRadius;
        public int damageAmount;
        public HediffCompProperties_Damage()
        {
            this.compClass = typeof(HediffComp_Damage);
        }
    }
    public class HediffComp_Damage : HediffComp
    {
        public HediffCompProperties_Damage Props => base.props as HediffCompProperties_Damage;

        public bool damaged;
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (!damaged && this.parent.Severity >= Props.severityThreshold)
            {
                damaged = true;
                if (Props.explosionRadius > 0 && this.Pawn.MapHeld != null)
                {
                    GenExplosion.DoExplosion(this.Pawn.PositionHeld, this.Pawn.MapHeld, Props.explosionRadius, Props.damageDef, this.Pawn, Props.damageAmount);
                }
                else
                {
                    this.Pawn.TakeDamage(new DamageInfo(Props.damageDef, Props.damageAmount));
                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref damaged, "exploded");
        }
    }
}


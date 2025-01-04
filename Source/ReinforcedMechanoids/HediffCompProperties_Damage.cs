using Verse;

namespace ReinforcedMechanoids;

public class HediffCompProperties_Damage : HediffCompProperties
{
    public int damageAmount;

    public DamageDef damageDef;

    public float explosionRadius;
    public float severityThreshold;

    public HediffCompProperties_Damage()
    {
        compClass = typeof(HediffComp_Damage);
    }
}
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids;

public class HediffCompProperties_EffectOnPawn : HediffCompProperties
{
    public readonly float moteCastScale = 1f;

    public readonly float moteSolidTimeOverride = -1f;
    public FleckDef fleckDef;

    public Vector3 moteCastOffset = Vector3.zero;

    public ThingDef moteDef;

    public int tickRefreshRate;

    public HediffCompProperties_EffectOnPawn()
    {
        compClass = typeof(HediffCompEffectOnPawn);
    }
}
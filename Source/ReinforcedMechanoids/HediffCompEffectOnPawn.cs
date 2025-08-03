using RimWorld;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids;

public class HediffCompEffectOnPawn : HediffComp
{
    private int lastRefreshTick;
    private Mote moteCast;

    private HediffCompProperties_EffectOnPawn Props => props as HediffCompProperties_EffectOnPawn;

    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        base.CompPostPostAdd(dinfo);
        if (!Pawn.Spawned)
        {
            return;
        }

        if (Props.fleckDef != null)
        {
            FleckMaker.AttachedOverlay(Pawn, Props.fleckDef, Vector3.zero);
        }

        if (Props.moteDef != null)
        {
            MakeMote();
        }
    }

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (Props.moteDef == null || !Pawn.Spawned)
        {
            return;
        }

        if (Pawn.IsHashIntervalTick(Props.tickRefreshRate))
        {
            moteCast?.Destroy();
            MakeMote();
        }

        moteCast?.Maintain();
    }

    private void MakeMote()
    {
        moteCast = MoteMaker.MakeAttachedOverlay(Pawn, Props.moteDef, Props.moteCastOffset, Props.moteCastScale,
            Props.moteSolidTimeOverride);
        lastRefreshTick = Find.TickManager.TicksGame;
    }
}
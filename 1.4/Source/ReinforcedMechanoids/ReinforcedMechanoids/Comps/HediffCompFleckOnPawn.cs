using RimWorld;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids
{
    public class HediffCompProperties_EffectOnPawn : HediffCompProperties
    {
        public FleckDef fleckDef;
        public ThingDef moteDef;
        public float moteSolidTimeOverride = -1;
        public float moteCastScale = 1f;
        public Vector3 moteCastOffset = Vector3.zero;
        public int tickRefreshRate;
        public HediffCompProperties_EffectOnPawn()
        {
            this.compClass = typeof(HediffCompEffectOnPawn);
        }
    }
    public class HediffCompEffectOnPawn : HediffComp
    {
        private Mote moteCast;

        private int lastRefreshTick;
        public HediffCompProperties_EffectOnPawn Props => base.props as HediffCompProperties_EffectOnPawn;
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (this.Pawn.Spawned)
            {
                if (Props.fleckDef != null)
                {
                    FleckMaker.AttachedOverlay(Pawn, Props.fleckDef, Vector3.zero, 1);
                }
                if (Props.moteDef != null)
                {
                    MakeMote();
                }
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Props.moteDef != null && this.Pawn.Spawned)
            {
                if (this.Pawn.IsHashIntervalTick(Props.tickRefreshRate))
                {
                    moteCast?.Destroy();
                    MakeMote();
                }
                moteCast?.Maintain();
            }
        }

        private void MakeMote()
        {
            moteCast = MoteMaker.MakeAttachedOverlay(Pawn, Props.moteDef, Props.moteCastOffset, Props.moteCastScale, Props.moteSolidTimeOverride);
            lastRefreshTick = Find.TickManager.TicksGame;
        }
    }
}

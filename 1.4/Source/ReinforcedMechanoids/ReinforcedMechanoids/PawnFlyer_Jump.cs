using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace ReinforcedMechanoids
{
    public class PawnFlyer_Jump : AbilityPawnFlyer
    {
        private Effecter flightEffecter;
        public override Vector3 DrawPos => position;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                var a = Mathf.Max(this.flightDistance, 1f) / 12f;
                a = Mathf.Max(a, def.pawnFlyer.flightDurationMin);
                ticksFlightTime = a.SecondsToTicks();
                ticksFlying = 0;
            }
        }
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            var vector = position;
            vector.y += 5;
            FlyingPawn.Drawer.renderer.RenderPawnAt(vector, direction);
        }

        public override void Tick()
        {
            if (flightEffecter == null && this.flightEffecterDef != null)
            {
                flightEffecter = this.flightEffecterDef.Spawn();
                flightEffecter.Trigger(this, TargetInfo.Invalid);
            }
            else
                flightEffecter?.EffectTick(this, TargetInfo.Invalid);

            base.Tick();
        }

        protected override void LandingEffects()
        {
            base.LandingEffects();
            FleckMaker.ThrowDustPuff(base.Position, base.Map, 2f);
        }
    }
}


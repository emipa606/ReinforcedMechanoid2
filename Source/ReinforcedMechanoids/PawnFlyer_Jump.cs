using RimWorld;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace ReinforcedMechanoids;

public class PawnFlyer_Jump : AbilityPawnFlyer
{
    private new Effecter flightEffecter;

    public override Vector3 DrawPos => effectivePos;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (respawningAfterLoad)
        {
            return;
        }

        var a = Mathf.Max(flightDistance, 1f) / 12f;
        a = Mathf.Max(a, def.pawnFlyer.flightDurationMin);
        ticksFlightTime = a.SecondsToTicks();
        ticksFlying = 0;
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        var position = effectivePos;
        position.y += 5f;
        FlyingPawn.Drawer.renderer.RenderPawnAt(position, Rotation);
    }

    public override void Tick()
    {
        if (flightEffecter == null && flightEffecterDef != null)
        {
            flightEffecter = flightEffecterDef.Spawn();
            flightEffecter.Trigger((TargetInfo)(Thing)this, TargetInfo.Invalid);
        }
        else
        {
            flightEffecter?.EffectTick((Thing)this, TargetInfo.Invalid);
        }

        base.Tick();
    }

    protected new void LandingEffects()
    {
        ((AbilityPawnFlyer)this).LandingEffects();
        FleckMaker.ThrowDustPuff(Position, Map, 2f);
    }
}
using RimWorld;
using VanillaWeaponsExpandedLaser;
using Verse;

namespace ReinforcedMechanoids;

public class LaserLocust : LaserBeam
{
    private new LaserBeamDef def
    {
        get
        {
            var thingDef = ((Thing)this).def;
            return (LaserBeamDef)(thingDef is LaserBeamDef ? thingDef : null);
        }
    }

    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        var normalized = (destination - origin).normalized;
        normalized.y = 0f;
        var vect = hitThing.IsShielded() && def.IsWeakToShields || blockedByShield
            ? hitThing.TrueCenter() - (normalized.RotatedBy(Rand.Range(-22.5f, 22.5f)) * 0.8f)
            : destination;
        var intVec = vect.ToIntVec3();
        var locustBeam = (LocustBeam)GenSpawn.Spawn(RM_DefOf.RM_LocustBeam, intVec, Map);
        locustBeam.duration = locustBeam.Props.duration;
        locustBeam.instigator = launcher;
        locustBeam.weaponDef = launcher.def;
        locustBeam.StartStrike();
        GenExplosion.DoExplosion(intVec, Map, 1f, DamageDefOf.Bomb, launcher);
        if (launcher is Pawn pawn)
        {
            var hd = pawn.health.AddHediff(RM_DefOf.RM_LocustBeamEffect);
            hd.TryGetComp<HediffComp_Disappears>().ticksToDisappear = def.lifetime;
        }

        base.Impact(hitThing, blockedByShield);
    }
}
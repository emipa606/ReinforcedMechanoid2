using RimWorld;
using UnityEngine;
using Verse;

namespace VFEMech;

public class Building_MechTurretGun : Building_TurretGun
{
    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        if (!TargetCurrentlyAimingAt.IsValid ||
            TargetCurrentlyAimingAt.HasThing && !TargetCurrentlyAimingAt.Thing.Spawned)
        {
            return;
        }

        var b = !TargetCurrentlyAimingAt.HasThing
            ? TargetCurrentlyAimingAt.Cell.ToVector3Shifted()
            : TargetCurrentlyAimingAt.Thing.TrueCenter();
        var a = this.TrueCenter();
        b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
        a.y = b.y;
        GenDraw.DrawLineBetween(a, b, ForcedTargetLineMat);
    }
}
using System.Linq;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids;

public class HullRepairModule : MechShipPart
{
    public override void Tick()
    {
        base.Tick();
        if (Map == null || Find.TickManager.TicksGame % 600 != 0)
        {
            return;
        }

        FleckMaker.Static(Position, Map, FleckDefOf.PsycastAreaEffect, 10f);
        var list = Map.listerThings.AllThings
            .Where(x => (x.def.building?.buildingTags?.Contains("RM_AncientShip")).GetValueOrDefault()).ToList();
        foreach (var item in list)
        {
            var num = item.HitPoints + 10;
            if (num > item.MaxHitPoints)
            {
                num = item.MaxHitPoints;
            }

            item.HitPoints = num;
        }
    }
}
using RimWorld;
using System.Linq;
using Verse;

namespace ReinforcedMechanoids
{
    public class HullRepairModule : MechShipPart
    {
        public override void Tick()
        {
            base.Tick();
            if (this.Map != null && Find.TickManager.TicksGame % 600 == 0)
            {
                FleckMaker.Static(this.Position, this.Map, FleckDefOf.PsycastAreaEffect, 10f);
                var buildings = this.Map.listerThings.AllThings.Where(x => x.def.building?.buildingTags?.Contains("RM_AncientShip") ?? false && x.Position.DistanceTo(this.Position) <= 20f).ToList();
                foreach (var building in buildings)
                {
                    var hp = building.HitPoints + 10;
                    if (hp > building.MaxHitPoints)
                        hp = building.MaxHitPoints;
                    building.HitPoints = hp;
                }
            }
        }
    }
}


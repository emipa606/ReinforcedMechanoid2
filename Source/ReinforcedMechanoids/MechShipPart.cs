using System.Linq;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids;

public class MechShipPart : Building
{
    public override AcceptanceReport ClaimableBy(Faction by)
    {
        return false;
    }

    public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
        base.PreApplyDamage(ref dinfo, out absorbed);
        if (def == RM_DefOf.RM_AncientTrooperStorage || Map == null)
        {
            return;
        }

        var source = from TrooperStorage x in Map.listerThings.ThingsOfDef(RM_DefOf.RM_AncientTrooperStorage)
            where x.CanSpawnTroopers
            select x;
        if (!source.Any())
        {
            return;
        }

        var trooperStorage = source.OrderBy(y => y.Position.DistanceTo(Position)).First();
        trooperStorage.ReleaseTroopers();
    }
}
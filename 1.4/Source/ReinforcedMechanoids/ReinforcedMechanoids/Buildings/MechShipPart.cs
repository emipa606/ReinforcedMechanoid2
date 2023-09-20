using RimWorld;
using System.Linq;
using System.Text;
using Verse;
using VFECore;

namespace ReinforcedMechanoids
{

    public class MechShipPart : Building
    {
        public override bool ClaimableBy(Faction by, StringBuilder reason = null)
        {
            return false;
        }
        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(ref dinfo, out absorbed);
            if (this.def != RM_DefOf.RM_AncientTrooperStorage && this.Map != null)
            {
                var trooperStorages = this.Map.listerThings.ThingsOfDef(RM_DefOf.RM_AncientTrooperStorage).Cast<TrooperStorage>().Where(x => x.CanSpawnTroopers);
                if (trooperStorages.Count() > 0)
                {
                    var trooperStorage = trooperStorages.OrderBy(y => y.Position.DistanceTo(this.Position)).First();
                    trooperStorage.ReleaseTroopers();
                }
            }
        }
    }
}


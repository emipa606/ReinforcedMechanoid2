using RimWorld;
using Verse;

namespace ReinforcedMechanoids
{
    public class TrooperStorage : MechShipPart
    {
        private bool troopersAreReleased;
        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(ref dinfo, out absorbed);
            if (CanSpawnTroopers)
            {
                ReleaseTroopers();
            }
        }
        public bool CanSpawnTroopers => !troopersAreReleased && this.Map != null;
        public void ReleaseTroopers()
        {
            var faction = Find.FactionManager.FirstFactionOfDef(RM_DefOf.RM_Remnants);
            PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
            pawnGroupMakerParms.tile = this.Map.Tile;
            pawnGroupMakerParms.faction = faction;
            pawnGroupMakerParms.points = 800f;
            pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Combat;
            var pawns = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms);
            foreach (var pawn in pawns)
            {
                GenPlace.TryPlaceThing(pawn, this.Position, this.Map, ThingPlaceMode.Near);
                CompPawnProducer.CreateOrAddToAssaultLord(pawn);
            }
            troopersAreReleased = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref troopersAreReleased, "troopersAreReleased");
        }
    }
}


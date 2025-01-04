using RimWorld;
using Verse;

namespace ReinforcedMechanoids;

public class TrooperStorage : MechShipPart
{
    private bool troopersAreReleased;

    public bool CanSpawnTroopers => !troopersAreReleased && Map != null;

    public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
        base.PreApplyDamage(ref dinfo, out absorbed);
        if (CanSpawnTroopers)
        {
            ReleaseTroopers();
        }
    }

    public void ReleaseTroopers()
    {
        var faction = Find.FactionManager.FirstFactionOfDef(RM_DefOf.RM_Remnants);
        var pawnGroupMakerParms = new PawnGroupMakerParms
        {
            tile = Map.Tile,
            faction = faction,
            points = 800f,
            groupKind = PawnGroupKindDefOf.Combat
        };
        var enumerable = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms);
        foreach (var item in enumerable)
        {
            GenPlace.TryPlaceThing(item, Position, Map, ThingPlaceMode.Near);
            CompPawnProducer.CreateOrAddToAssaultLord(item);
        }

        troopersAreReleased = true;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref troopersAreReleased, "troopersAreReleased");
    }
}
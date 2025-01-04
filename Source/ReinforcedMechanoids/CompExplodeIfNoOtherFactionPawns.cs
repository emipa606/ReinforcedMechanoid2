using System.Linq;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids;

[StaticConstructorOnStartup]
public class CompExplodeIfNoOtherFactionPawns : ThingComp
{
    public CompProperties_ExplodeIfNoOtherFactionPawns Props => props as CompProperties_ExplodeIfNoOtherFactionPawns;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (parent is not Pawn pawn)
        {
            return;
        }

        if (pawn.equipment == null)
        {
            pawn.equipment = new Pawn_EquipmentTracker(pawn);
        }

        if (pawn.apparel == null)
        {
            pawn.apparel = new Pawn_ApparelTracker(pawn);
        }
    }

    public override void CompTick()
    {
        base.CompTick();
        if (Find.TickManager.TicksGame % 60 == 0 && (parent is not Pawn pawn || !pawn.Downed && !pawn.Dead))
        {
            TryExplode();
        }
    }

    public void TryExplode()
    {
        if (parent is Pawn { Map: not null, Faction: not null } pawn &&
            !(from x in pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction)
                where x.kindDef != pawn.kindDef
                select x).Any())
        {
            pawn.GetComp<CompExplosive>().StartWick(pawn);
        }
    }
}
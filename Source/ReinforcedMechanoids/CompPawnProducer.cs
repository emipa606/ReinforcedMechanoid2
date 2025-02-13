using RimWorld;
using Verse;
using Verse.AI.Group;

namespace ReinforcedMechanoids;

public class CompPawnProducer : ThingComp
{
    public PawnKindDef curPawnKindDef;

    public int nextTick = -1;

    public CompProperties_PawnProducer Props => (CompProperties_PawnProducer)props;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (respawningAfterLoad)
        {
            return;
        }

        curPawnKindDef = Props.pawnKindToProduceOneOf.RandomElement();
        nextTick = Find.TickManager.TicksAbs + Props.tickSpawnIntervalRange.RandomInRange;
    }

    public override string TransformLabel(string label)
    {
        return $"{base.TransformLabel(label)} (" + curPawnKindDef.LabelCap + ")";
    }

    public override void CompTick()
    {
        if (Find.TickManager.TicksAbs < nextTick)
        {
            return;
        }

        var randomInRange = Props.spawnCountRange.RandomInRange;
        nextTick = Find.TickManager.TicksAbs + Props.tickSpawnIntervalRange.RandomInRange;
        var faction = Props.faction != null ? Find.FactionManager.FirstFactionOfDef(Props.faction) : parent.Faction;
        Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
        for (var i = 0; i < randomInRange; i++)
        {
            var pawn = PawnGenerator.GeneratePawn(curPawnKindDef, faction);
            var center = FindNearCloseWalk(parent.Position, parent.Map);
            GenPlace.TryPlaceThing(pawn, center, parent.Map, ThingPlaceMode.Near);
            CreateOrAddToAssaultLord(pawn);
        }
    }

    public static void CreateOrAddToAssaultLord(Pawn pawn, Lord lord = null, bool canKidnap = false,
        bool canTimeoutOrFlee = false, bool sappers = false, bool useAvoidGridSmart = false, bool canSteal = false)
    {
        if (lord == null && pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction).Any(p => p != pawn))
        {
            lord = ((Pawn)GenClosest.ClosestThing_Global(pawn.Position,
                pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction), 99999f,
                p => p != pawn && ((Pawn)p).GetLord() != null)).GetLord();
        }

        if (lord == null)
        {
            var lordJob = new LordJob_AssaultColony(pawn.Faction, canKidnap, canTimeoutOrFlee, sappers,
                useAvoidGridSmart, canSteal);
            lord = LordMaker.MakeNewLord(pawn.Faction, lordJob, pawn.Map);
        }

        lord.AddPawn(pawn);
    }

    public IntVec3 FindNearCloseWalk(IntVec3 root, Map map)
    {
        foreach (var item in GenRadial.RadialCellsAround(root, 10f, true))
        {
            if (item.Walkable(map) && !item.Fogged(map))
            {
                return item;
            }
        }

        return root;
    }

    public override string CompInspectStringExtra()
    {
        return base.CompInspectStringExtra() +
               "SpawningNextPawnIn".Translate((nextTick - Find.TickManager.TicksAbs).ToStringTicksToDays());
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Defs.Look(ref curPawnKindDef, "curMechKindDef");
        Scribe_Values.Look(ref nextTick, "nextTick");
    }
}
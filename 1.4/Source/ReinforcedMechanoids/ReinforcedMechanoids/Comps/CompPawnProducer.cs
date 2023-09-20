using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace ReinforcedMechanoids
{
    public class CompProperties_PawnProducer : CompProperties
    {
        public FactionDef faction;
        public List<PawnKindDef> pawnKindToProduceOneOf;
        public IntRange tickSpawnIntervalRange;
        public IntRange spawnCountRange;
        public CompProperties_PawnProducer()
        {
            compClass = typeof(CompPawnProducer);
        }
    }
    public class CompPawnProducer : ThingComp
    {
        public PawnKindDef curPawnKindDef;

        public int nextTick = -1;
        public CompProperties_PawnProducer Props => (CompProperties_PawnProducer)props;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                curPawnKindDef = Props.pawnKindToProduceOneOf.RandomElement();
                nextTick = Find.TickManager.TicksAbs + Props.tickSpawnIntervalRange.RandomInRange;
            }
        }

        public override string TransformLabel(string label)
        {
            return base.TransformLabel(label) + " (" + this.curPawnKindDef.LabelCap + ")";
        }
        public override void CompTick()
        {
            if (Find.TickManager.TicksAbs >= nextTick)
            {
                var spawnCount = Props.spawnCountRange.RandomInRange;
                nextTick = Find.TickManager.TicksAbs + Props.tickSpawnIntervalRange.RandomInRange;
                var faction = Props.faction != null ? Find.FactionManager.FirstFactionOfDef(Props.faction) : this.parent.Faction;
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                for (int i = 0; i < spawnCount; i++)
                {
                    var mech = PawnGenerator.GeneratePawn(curPawnKindDef, faction);
                    var pos = FindNearCloseWalk(this.parent.Position, this.parent.Map);
                    GenPlace.TryPlaceThing(mech, pos, this.parent.Map, ThingPlaceMode.Near);
                    CreateOrAddToAssaultLord(mech);
                }
            }
        }

        public static void CreateOrAddToAssaultLord(Pawn pawn, Lord lord = null, bool canKidnap = false, bool canTimeoutOrFlee = false, bool sappers = false,
                                            bool useAvoidGridSmart = false, bool canSteal = false)
        {
            if (lord == null && pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction).Any((Pawn p) => p != pawn))
            {
                lord = ((Pawn)GenClosest.ClosestThing_Global(pawn.Position, pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction), 99999f,
                    (Thing p) => p != pawn && ((Pawn)p).GetLord() != null, null)).GetLord();
            }
            if (lord == null)
            {
                var lordJob = new LordJob_AssaultColony(pawn.Faction, canKidnap, canTimeoutOrFlee, sappers, useAvoidGridSmart, canSteal);
                lord = LordMaker.MakeNewLord(pawn.Faction, lordJob, pawn.Map, null);
            }
            lord.AddPawn(pawn);
        }

        public IntVec3 FindNearCloseWalk(IntVec3 root, Map map)
        {
            foreach (var cell in GenRadial.RadialCellsAround(root, 10, true))
            {
                if (cell.Walkable(map) && !cell.Fogged(map))
                {
                    return cell;
                }
            }
            return root;
        }
        public override string CompInspectStringExtra()
        {
            return base.CompInspectStringExtra() + "SpawningNextPawnIn".Translate((nextTick - Find.TickManager.TicksAbs).ToStringTicksToDays());
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref curPawnKindDef, "curMechKindDef");
            Scribe_Values.Look(ref nextTick, "nextTick");
        }
    }
}


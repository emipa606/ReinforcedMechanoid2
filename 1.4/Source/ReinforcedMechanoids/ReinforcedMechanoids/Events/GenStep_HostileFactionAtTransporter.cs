using RimWorld;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids
{
    public class GenStep_HostileFactionAtTransporter : GenStep
    {
        public override int SeedPart => 1466666193;
        public override void Generate(Map map, GenStepParams parms)
        {
            if (Rand.ChanceSeeded(0.15f, SeedPart))
            {
                var hostileFaction = Find.FactionManager.RandomEnemyFaction(allowNonHumanlike: false);
                var thingsToAssault = map.listerThings.AllThings.Where(x => x is MechShipPart).ToList();
                var lord = LordMaker.MakeNewLord(hostileFaction, new LordJob_AssaultThings(hostileFaction, thingsToAssault), map);
                var pawns = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
                {
                    groupKind = PawnGroupKindDefOf.Combat,
                    tile = map.Tile,
                    faction = hostileFaction,
                    points = Rand.Range(500, 1000)
                }, true);
                MapGenerator.PlayerStartSpot = map.Center;
                if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 spawnCenter, map, CellFinder.EdgeRoadChance_Hostile))
                {
                    if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Standable(map) && !x.Fogged(map), map, 
                        CellFinder.EdgeRoadChance_Ignore, out spawnCenter))
                    {
                        return;
                    }
                }

                foreach (var pawn in pawns)
                {
                    var loc = CellFinder.RandomClosewalkCellNear(spawnCenter, map, 8, (IntVec3 x) => x.Standable(map) && !x.Fogged(map));
                    if (loc.IsValid)
                    {
                        GenSpawn.Spawn(pawn, loc, map);
                        lord.AddPawn(pawn);
                    }
                }
            }
        }
    }
}

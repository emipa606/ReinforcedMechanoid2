using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace ReinforcedMechanoids
{
    public class IncidentWorker_WanderingMechanoids : IncidentWorker_NeutralGroup
	{
		public override PawnGroupKindDef PawnGroupKindDef => PawnGroupKindDefOf.Combat;
        public override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!TryResolveParms(parms))
			{
				return false;
			}
			if (!RCellFinder.TryFindTravelDestFrom(parms.spawnCenter, map, out var travelDest))
			{
				Log.Warning(string.Concat("Failed to do traveler incident from ", parms.spawnCenter, ": Couldn't find anywhere for the traveler to go."));
				return false;
			}
			List<Pawn> list = SpawnPawns(parms);
			foreach (var pawn in list)
            {
				var targetHealth = Rand.Range(0.15f, 0.7f);
				var num = 0;
				while (num < 100)
                {
					num++;
					var healthPct = pawn.health.summaryHealth.SummaryHealthPercent;
					if (targetHealth >= healthPct)
                    {
						break;
                    }
					IEnumerable<BodyPartRecord> source = from x in pawn.health.hediffSet.GetNotMissingParts()
														 where x.depth == BodyPartDepth.Outside
														 && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(x)
														 select x;
					if (!source.Any())
					{
						continue;
					}
					BodyPartRecord bodyPartRecord = source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
					var combatDamages = new List<DamageDef>
					{
						DamageDefOf.Bullet, DamageDefOf.Blunt, DamageDefOf.Bomb, DamageDefOf.Stab, DamageDefOf.Cut
					};
					HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(combatDamages.RandomElement(), pawn, bodyPartRecord);
					Hediff_Injury hediff_Injury = (Hediff_Injury)HediffMaker.MakeHediff(hediffDefFromDamage, pawn);
					hediff_Injury.Severity = Rand.RangeInclusive(2, 6);
					hediff_Injury.Part = bodyPartRecord;
					if (!pawn.health.WouldDieAfterAddingHediff(hediff_Injury) && !pawn.health.WouldBeDownedAfterAddingHediff(hediff_Injury))
					{
						pawn.health.AddHediff(hediff_Injury, bodyPartRecord);
					}
				}
			}
			if (list.Count == 0)
			{
				return false;
			}
			SendStandardLetter(parms, list);
			LordJob_TravelAndExit lordJob = new LordJob_TravelAndExit(travelDest);
			LordMaker.MakeNewLord(parms.faction, lordJob, map, list);
			return true;
		}

        public override bool TryResolveParmsGeneral(IncidentParms parms)
        {
			Map map = (Map)parms.target;
			if (!parms.spawnCenter.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, map, CellFinder.EdgeRoadChance_Neutral))
			{
				return false;
			}
			parms.faction = Faction.OfMechanoids;
			return true;
		}
        public override void ResolveParmsPoints(IncidentParms parms)
		{
			parms.points = Rand.Range(400, 1000);
		}
	}
}

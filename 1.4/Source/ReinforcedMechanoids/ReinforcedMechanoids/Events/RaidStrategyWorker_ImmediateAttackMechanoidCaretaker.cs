using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace ReinforcedMechanoids
{
    public class RaidStrategyWorker_ImmediateAttackMechanoidCaretaker : RaidStrategyWorker_ImmediateAttackWithCertainPawnKind
    {
        public override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
        {
            return new LordJob_AssaultColony_CaretakerRaid(parms.faction, canKidnap: true, canTimeoutOrFlee: false, canSteal: false);
        }
		protected override bool MatchesRequiredPawnKind(PawnKindDef kind)
		{
            return kind == RM_DefOf.RM_Mech_Caretaker;
        }

        protected override int MaxRequiredPawnsForPoints(float pointsTotal, Faction faction = null)
        {
            return 2;
        }
    }
}

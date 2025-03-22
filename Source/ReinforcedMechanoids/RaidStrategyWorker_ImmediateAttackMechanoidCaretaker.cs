using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace ReinforcedMechanoids;

public class
    RaidStrategyWorker_ImmediateAttackMechanoidCaretaker : RaidStrategyWorker_ImmediateAttackWithCertainPawnKind
{
    public override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
    {
        return new LordJob_BossgroupAssaultColony_Caretaker(parms.faction,
            RCellFinder.FindSiegePositionFrom(parms.spawnCenter.IsValid ? parms.spawnCenter : pawns[0].PositionHeld,
                map), from x in pawns
            where x.kindDef == RM_DefOf.RM_Mech_Caretaker
            select x);
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
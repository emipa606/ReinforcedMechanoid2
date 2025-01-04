using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace ReinforcedMechanoids;

public class RaidStrategyWorker_ImmediateAttackMechanoidWraith : RaidStrategyWorker_ImmediateAttackWithCertainPawnKind
{
    public override void TryGenerateThreats(IncidentParms parms)
    {
        parms.points *= 0.8f;
        base.TryGenerateThreats(parms);
    }

    public override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
    {
        var blueprintPoints = parms.points * Rand.Range(0.2f, 0.3f);
        var additionalRaidPoints = (parms.points / 0.8f) - parms.points;
        return new LordJob_AssaultColony_WraithSiege(parms.faction, blueprintPoints, additionalRaidPoints, false, false,
            false, false, false);
    }

    protected override bool MatchesRequiredPawnKind(PawnKindDef kind)
    {
        return kind == RM_DefOf.RM_Mech_WraithSiege;
    }

    protected override int MaxRequiredPawnsForPoints(float pointsTotal, Faction faction = null)
    {
        return 2;
    }
}
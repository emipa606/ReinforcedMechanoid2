using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace ReinforcedMechanoids;

public class RaidStrategyWorker_ImmediateAttackMechanoidLocust : RaidStrategyWorker_ImmediateAttackWithCertainPawnKind
{
    public override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
    {
        return new LordJob_AssaultColony_LocustRaid(parms.faction, true, false, false, false, false);
    }

    protected override bool MatchesRequiredPawnKind(PawnKindDef kind)
    {
        return kind == RM_DefOf.RM_Mech_Locust;
    }

    protected override int MaxRequiredPawnsForPoints(float pointsTotal, Faction faction = null)
    {
        return 2;
    }
}
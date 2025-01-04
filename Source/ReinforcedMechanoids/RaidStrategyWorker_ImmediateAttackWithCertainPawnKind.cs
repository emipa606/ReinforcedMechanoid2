using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids;

public abstract class RaidStrategyWorker_ImmediateAttackWithCertainPawnKind : RaidStrategyWorker_ImmediateAttack
{
    public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
    {
        return PawnGenOptionsWithRequiredPawns(parms.faction, groupKind).Any() && base.CanUseWith(parms, groupKind);
    }

    protected abstract bool MatchesRequiredPawnKind(PawnKindDef kind);

    protected virtual int MinRequiredPawnsForPoints(float pointsTotal, Faction faction = null)
    {
        return 1;
    }

    protected virtual int MaxRequiredPawnsForPoints(float pointsTotal, Faction faction = null)
    {
        return 99999;
    }

    public override float MinimumPoints(Faction faction, PawnGroupKindDef groupKind)
    {
        return Mathf.Max(base.MinimumPoints(faction, groupKind), CheapestRequiredPawnCost(faction, groupKind));
    }

    public override float MinMaxAllowedPawnGenOptionCost(Faction faction, PawnGroupKindDef groupKind)
    {
        return CheapestRequiredPawnCost(faction, groupKind);
    }

    private float CheapestRequiredPawnCost(Faction faction, PawnGroupKindDef groupKind)
    {
        var enumerable = PawnGenOptionsWithRequiredPawns(faction, groupKind);
        if (!enumerable.Any())
        {
            Log.Error("Tried to get MinimumPoints for " + GetType() + " for faction " + faction +
                      " but the faction has no groups with the required pawn kind. groupKind=" + groupKind);
            return 99999f;
        }

        var num = 9999999f;
        foreach (var item in enumerable)
        {
            foreach (var item2 in item.options.Where(op => MatchesRequiredPawnKind(op.kind)))
            {
                if (item2.Cost < num)
                {
                    num = item2.Cost;
                }
            }
        }

        return num;
    }

    public override bool CanUsePawnGenOption(float pointsTotal, PawnGenOption g,
        List<PawnGenOptionWithXenotype> chosenGroups, Faction faction = null)
    {
        if (chosenGroups == null)
        {
            return true;
        }

        if (chosenGroups.Count < MinRequiredPawnsForPoints(pointsTotal, faction) &&
            !MatchesRequiredPawnKind(g.kind))
        {
            return false;
        }

        return !MatchesRequiredPawnKind(g.kind) || MaxRequiredPawnsForPoints(pointsTotal, faction) >
            chosenGroups.Count(x => x.option.kind == g.kind);
    }

    private IEnumerable<PawnGroupMaker> PawnGenOptionsWithRequiredPawns(Faction faction, PawnGroupKindDef groupKind)
    {
        if (faction.def.pawnGroupMakers == null)
        {
            return [];
        }

        return faction.def.pawnGroupMakers.Where(gm =>
            gm.kindDef == groupKind && gm.options != null && gm.options.Any(op => MatchesRequiredPawnKind(op.kind)));
    }
}
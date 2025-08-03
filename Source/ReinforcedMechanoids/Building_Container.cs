using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

public class Building_Container : Building_Casket
{
    private static readonly List<Pawn> tmpAllowedPawns = [];

    private CompProperties_LootContainer Props => def.GetCompProperties<CompProperties_LootContainer>();

    public override int OpenTicks => Props.openingTicks;

    public override void EjectContents()
    {
        innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near, null, c => c.GetEdifice(Map) == null);
        contentsKnown = true;
        if (def.building.openingEffect == null)
        {
            return;
        }

        var effecter = def.building.openingEffect.Spawn();
        effecter.Trigger(new TargetInfo(Position, Map), null);
        effecter.Cleanup();
    }

    public override void Open()
    {
        if (CanOpen)
        {
            base.Open();
        }
    }

    public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(IEnumerable<Pawn> enumerable)
    {
        foreach (var multiSelectFloatMenuOption in base.GetMultiSelectFloatMenuOptions(enumerable))
        {
            yield return multiSelectFloatMenuOption;
        }

        if (!CanOpen)
        {
            yield break;
        }

        tmpAllowedPawns.Clear();
        foreach (var pawn in enumerable)
        {
            if (pawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
            {
                tmpAllowedPawns.Add(pawn);
            }
        }

        if (tmpAllowedPawns.Count <= 0)
        {
            yield return new FloatMenuOption(
                "CannotOpen".Translate(this) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
            yield break;
        }

        tmpAllowedPawns.Clear();
        yield return new FloatMenuOption("Open".Translate(this), delegate
        {
            tmpAllowedPawns[0].jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Open, this), JobTag.Misc);
            for (var j = 1; j < tmpAllowedPawns.Count; j++)
            {
                FloatMenuOptionProvider_DraftedMove.PawnGotoAction(Position, tmpAllowedPawns[j],
                    RCellFinder.BestOrderedGotoDestNear(Position, tmpAllowedPawns[j]));
            }
        });
    }
}
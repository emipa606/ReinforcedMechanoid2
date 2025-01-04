using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids;

public class LordJob_BreakDownMechanoids : LordJob
{
    public IntVec3 cellToExit;
    public List<Thing> extractedThings;

    public List<Corpse> mechanoidCorpses;

    public int tickStarted;

    public LordJob_BreakDownMechanoids()
    {
    }

    public LordJob_BreakDownMechanoids(List<Corpse> mechanoidCorpses)
    {
        this.mechanoidCorpses = mechanoidCorpses;
        tickStarted = Find.TickManager.TicksGame;
    }

    public override StateGraph CreateGraph()
    {
        return new StateGraph
        {
            StartingToil = new LordToil_BreakDownMechanoids()
        };
    }

    public bool TryFindExitSpot(Map map, List<Pawn> pawns, bool reachableForEveryColonist, out IntVec3 spot)
    {
        if (reachableForEveryColonist)
        {
            return CellFinder.TryFindRandomEdgeCellWith(delegate(IntVec3 x)
            {
                if (!Validator(x))
                {
                    return false;
                }

                foreach (var pawn in pawns)
                {
                    if (!pawn.Downed && !pawn.CanReach(x, PathEndMode.Touch, Danger.Deadly))
                    {
                        return false;
                    }
                }

                return true;
            }, map, CellFinder.EdgeRoadChance_Always, out spot);
        }

        var intVec = IntVec3.Invalid;
        var num = -1;
        foreach (var item in RotationsToUse().InRandomOrder())
        {
            foreach (var item2 in CellRect.WholeMap(map).GetEdgeCells(item).InRandomOrder())
            {
                if (!Validator(item2))
                {
                    continue;
                }

                var num2 = 0;
                foreach (var pawn in pawns)
                {
                    if (!pawn.Downed && pawn.CanReach(item2, PathEndMode.Touch, Danger.Deadly))
                    {
                        num2++;
                    }
                }

                if (num2 <= num)
                {
                    continue;
                }

                num = num2;
                intVec = item2;
            }

            if (!intVec.IsValid)
            {
                continue;
            }

            spot = intVec;
            return true;
        }

        spot = intVec;
        return intVec.IsValid;

        bool Validator(IntVec3 x)
        {
            return !x.Fogged(map) && x.Standable(map);
        }

        static IEnumerable<Rot4> RotationsToUse()
        {
            yield return new Rot4(0);
            yield return new Rot4(1);
            yield return new Rot4(2);
            yield return new Rot4(3);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref mechanoidCorpses, "mechanoidCorpses", LookMode.Reference);
        Scribe_Collections.Look(ref extractedThings, "extractedThings", LookMode.Reference);
        Scribe_Values.Look(ref cellToExit, "cellToExit");
        Scribe_Values.Look(ref tickStarted, "tickStarted");
    }
}
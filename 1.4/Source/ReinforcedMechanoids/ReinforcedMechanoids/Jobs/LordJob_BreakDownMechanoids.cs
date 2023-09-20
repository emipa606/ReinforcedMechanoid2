using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids
{
    public class LordJob_BreakDownMechanoids : LordJob
    {
        public List<Thing> extractedThings;
        public List<Corpse> mechanoidCorpses;
        public IntVec3 cellToExit;
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
            Predicate<IntVec3> validator = (IntVec3 x) => !x.Fogged(map) && x.Standable(map);
            if (reachableForEveryColonist)
            {
                return CellFinder.TryFindRandomEdgeCellWith(delegate (IntVec3 x)
                {
                    if (!validator(x))
                    {
                        return false;
                    }
                    for (int j = 0; j < pawns.Count; j++)
                    {
                        if (!pawns[j].Downed && !pawns[j].CanReach(x, PathEndMode.Touch, Danger.Deadly))
                        {
                            return false;
                        }
                    }
                    return true;
                }, map, CellFinder.EdgeRoadChance_Always, out spot);
            }
            IntVec3 intVec = IntVec3.Invalid;
            int num = -1;
            IEnumerable<Rot4> RotationsToUse()
            {
                yield return new Rot4(0);
                yield return new Rot4(1);
                yield return new Rot4(2);
                yield return new Rot4(3);
            }
            foreach (var rot4 in RotationsToUse().InRandomOrder())
            {
                foreach (IntVec3 item in CellRect.WholeMap(map).GetEdgeCells(rot4).InRandomOrder())
                {
                    if (!validator(item))
                    {
                        continue;
                    }
                    int num2 = 0;
                    for (int i = 0; i < pawns.Count; i++)
                    {
                        if (!pawns[i].Downed && pawns[i].CanReach(item, PathEndMode.Touch, Danger.Deadly))
                        {
                            num2++;
                        }
                    }
                    if (num2 > num)
                    {
                        num = num2;
                        intVec = item;
                    }
                }
                if (intVec.IsValid)
                {
                    spot = intVec;
                    return true;
                }
            }

            spot = intVec;
            return intVec.IsValid;
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
}

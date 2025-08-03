using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids;

public class LordToil_MoveInBossgroup_Caretaker : LordToil
{
    private static readonly FloatRange EscortRadiusRanged = new(5f, 10f);

    private readonly List<Pawn> bosses = [];

    public LordToil_MoveInBossgroup_Caretaker(IEnumerable<Pawn> bosses)
    {
        this.bosses.AddRange(bosses);
    }

    public override bool AllowSatisfyLongNeeds => false;

    public override bool ForceHighStoryDanger => true;

    public override void UpdateAllDuties()
    {
        foreach (var pawn in lord.ownedPawns)
        {
            if (bosses.Contains(pawn))
            {
                var index = bosses.IndexOf(pawn);
                bosses[index].mindState.duty =
                    new PawnDuty(bosses[index].RaceProps.dutyBoss ?? DutyDefOf.AssaultColony);
                continue;
            }

            pawn.mindState.duty = new PawnDuty(DutyDefOf.Escort, bosses.RandomElement(),
                EscortRadiusRanged.RandomInRange);
        }
    }
}
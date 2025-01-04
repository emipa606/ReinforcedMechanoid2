using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids;

public class LordToil_BreakDownMechanoids : LordToil
{
    public override void UpdateAllDuties()
    {
        foreach (var pawn in lord.ownedPawns)
        {
            pawn.mindState.duty = new PawnDuty(RM_DefOf.RM_BreakDownMechanoids);
        }
    }
}
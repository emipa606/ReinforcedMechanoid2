using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids
{
    public class LordToil_BreakDownMechanoids : LordToil
    {
        public override void UpdateAllDuties()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                lord.ownedPawns[i].mindState.duty = new PawnDuty(RM_DefOf.RM_BreakDownMechanoids);
            }
        }
    }
}

using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

public class ThinkNode_ConditionalHasPower : ThinkNode_Conditional
{
    public override bool Satisfied(Pawn pawn)
    {
        return pawn.needs?.TryGetNeed<Need_Power>()?.CurLevel > 0;
    }
}
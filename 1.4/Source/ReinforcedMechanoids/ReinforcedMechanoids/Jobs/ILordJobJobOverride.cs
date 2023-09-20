using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids
{
    public interface ILordJobJobOverride
    {
        Job GetJobFor(Pawn pawn, List<Pawn> otherPawns, Job initialJob = null);
        bool CanOverrideJobFor(Pawn pawn, Job initialJob);
    }
}

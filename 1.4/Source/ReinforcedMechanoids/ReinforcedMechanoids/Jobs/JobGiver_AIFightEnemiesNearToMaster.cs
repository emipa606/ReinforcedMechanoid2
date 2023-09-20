using RimWorld;
using Verse;

namespace ReinforcedMechanoids
{
    public class JobGiver_AIFightEnemiesNearToMaster : JobGiver_AIFightEnemies
    {
        public Pawn master;
        public override bool ExtraTargetValidator(Pawn pawn, Thing target)
        {
            return master.Position.DistanceTo(target.Position) <= 10;
        }
    }
}


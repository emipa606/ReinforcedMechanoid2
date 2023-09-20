using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids
{
    public abstract class LordJob_AssaultColony_ProtectPawn : LordJob_AssaultColony, ILordJobJobOverride
    {
        public abstract PawnKindDef ProtecteeKind { get; }
        public abstract float MaxDistanceFromProtectee { get; }
        public bool CanOverrideJobFor(Pawn pawn, Job initialJob)
        {
            return pawn.kindDef != ProtecteeKind && pawn.kindDef != RM_DefOf.RM_Mech_Vulture && (initialJob is null || initialJob.IsCombatJob() is false);
        }
        public Job GetJobFor(Pawn pawn, List<Pawn> otherPawns, Job initialJob = null)
        {
            var pawnsToProtect = otherPawns.Where(x => x.kindDef == ProtecteeKind && !x.Downed && !x.Dead).ToList();
            if (pawnsToProtect.TryRandomElementByWeight(x => Mathf.Max(9999 - x.Position.DistanceTo(pawn.Position), 0), out var pawnToProtect))
            {
                var fightEnemiesJobGiver = new JobGiver_AIFightEnemiesNearToMaster();
                fightEnemiesJobGiver.master = pawnToProtect;
                fightEnemiesJobGiver.ResolveReferences();
                fightEnemiesJobGiver.targetAcquireRadius = 12f;
                fightEnemiesJobGiver.targetKeepRadius = 12f;
                var fightEnemiesJob = fightEnemiesJobGiver.TryGiveJob(pawn);
                if (fightEnemiesJob != null)
                {
                    return fightEnemiesJob;
                }
                else
                {
                    if (pawnToProtect.CurJobDef == JobDefOf.AttackMelee)
                    {
                        return null;
                    }
                    var followJob = Utils.TryGiveFollowJob(pawn, pawnToProtect, MaxDistanceFromProtectee);
                    if (followJob != null)
                    {
                        followJob.locomotionUrgency = LocomotionUrgency.Amble;
                        return followJob;
                    }
                }
            }
            return null;
        }
    }
}

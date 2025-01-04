using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

public abstract class LordJob_AssaultColony_ProtectPawn : LordJob_AssaultColony, ILordJobJobOverride
{
    public abstract PawnKindDef ProtecteeKind { get; }

    public abstract float MaxDistanceFromProtectee { get; }

    public bool CanOverrideJobFor(Pawn pawn, Job initialJob)
    {
        return pawn.kindDef != ProtecteeKind && pawn.kindDef != RM_DefOf.RM_Mech_Vulture &&
               (initialJob == null || !initialJob.IsCombatJob());
    }

    public Job GetJobFor(Pawn pawn, List<Pawn> otherPawns, Job initialJob = null)
    {
        var source = otherPawns.Where(x => x.kindDef == ProtecteeKind && !x.Downed && !x.Dead).ToList();
        if (!source.TryRandomElementByWeight(x => Mathf.Max(9999f - x.Position.DistanceTo(pawn.Position), 0f),
                out var result))
        {
            return null;
        }

        var jobGiver_AIFightEnemiesNearToMaster = new JobGiver_AIFightEnemiesNearToMaster
        {
            master = result
        };
        jobGiver_AIFightEnemiesNearToMaster.ResolveReferences();
        jobGiver_AIFightEnemiesNearToMaster.targetAcquireRadius = 12f;
        jobGiver_AIFightEnemiesNearToMaster.targetKeepRadius = 12f;
        var job = jobGiver_AIFightEnemiesNearToMaster.TryGiveJob(pawn);
        if (job != null)
        {
            return job;
        }

        if (result.CurJobDef == JobDefOf.AttackMelee)
        {
            return null;
        }

        var job2 = Utils.TryGiveFollowJob(pawn, result, MaxDistanceFromProtectee);
        if (job2 == null)
        {
            return null;
        }

        job2.locomotionUrgency = LocomotionUrgency.Amble;
        return job2;
    }
}
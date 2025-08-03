using System;
using System.Collections.Generic;
using System.Linq;
using ReinforcedMechanoids.Harmony;
using RimWorld;
using VEF.Apparels;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids;

public static class Utils
{
    private static readonly HashSet<JobDef> combatJobs =
    [
        JobDefOf.AttackMelee,
        JobDefOf.AttackStatic,
        JobDefOf.FleeAndCower,
        JobDefOf.ManTurret,
        JobDefOf.Flee
    ];

    private static readonly Dictionary<Pawn, CachedValue> cachedValuesPawn = new();

    private static readonly Dictionary<Map, CachedValue> cachedValuesCenterColony = new();

    public static readonly IntRange ExpiryInterval_ShooterSucceeded = new(450, 550);

    private static readonly IntRange ExpiryInterval_Melee = new(360, 480);

    public static bool InCombat(this Pawn pawn)
    {
        if (JobGiver_AIFightEnemy_TryGiveJob.gotoCombatJobs.Contains(pawn.CurJob))
        {
            return true;
        }

        if (combatJobs.Contains(pawn.CurJobDef))
        {
            return true;
        }

        return pawn.CurJobDef == JobDefOf.Wait_Combat &&
               pawn.stances.curStance is Stance_Busy { focusTarg.IsValid: true };
    }

    public static bool IsCombatJob(this Job job)
    {
        return job != null && (combatJobs.Contains(job.def) || job.def.alwaysShowWeapon);
    }

    public static BodyPartRecord GetNonMissingBodyPart(Pawn pawn, BodyPartDef def)
    {
        foreach (var notMissingPart in pawn.health.hediffSet.GetNotMissingParts())
        {
            if (notMissingPart.def == def)
            {
                return notMissingPart;
            }
        }

        return null;
    }

    public static IntVec3 GetNearestWalkableCellDestination(IntVec3 destination, Pawn pawn,
        out Building firstBlockingBuilding, Func<Pawn, IntVec3, Building, bool> unReachabilityCheck,
        float maxDistance = 0f)
    {
        if (cachedValuesPawn.TryGetValue(pawn, out var value) && Find.TickManager.TicksGame < value.lastCheckTick + 180)
        {
            firstBlockingBuilding = value.firstBlockingBuilding;
            return value.value;
        }

        firstBlockingBuilding = null;
        var pawnPath =
            pawn.Map.pathFinder.FindPathNow(pawn.Position, destination, TraverseMode.NoPassClosedDoorsOrWater);
        var position = pawn.Position;
        var intVec = position;
        var list = pawnPath.NodesReversed.ListFullCopy();
        list.Reverse();
        pawn.Map.pawnPathPool.paths.Clear();
        foreach (var item in list)
        {
            firstBlockingBuilding = item.GetEdifice(pawn.Map);
            var num = item.DistanceTo(destination);
            if (maxDistance > 0f && maxDistance > num || unReachabilityCheck(pawn, item, firstBlockingBuilding))
            {
                if (intVec != pawn.Position)
                {
                    cachedValuesPawn[pawn] = new CachedValue
                    {
                        value = intVec,
                        firstBlockingBuilding = firstBlockingBuilding,
                        lastCheckTick = Find.TickManager.TicksGame
                    };
                    return intVec;
                }

                cachedValuesPawn[pawn] = new CachedValue
                {
                    value = IntVec3.Invalid,
                    firstBlockingBuilding = firstBlockingBuilding,
                    lastCheckTick = Find.TickManager.TicksGame
                };
                return IntVec3.Invalid;
            }

            intVec = item;
        }

        cachedValuesPawn[pawn] = new CachedValue
        {
            value = destination,
            firstBlockingBuilding = firstBlockingBuilding,
            lastCheckTick = Find.TickManager.TicksGame
        };
        return destination;
    }

    public static bool UnReachabilityCheck(Pawn pawn, IntVec3 cell, Building building)
    {
        return cell.Roofed(pawn.Map) || building != null || !pawn.CanReach(cell, PathEndMode.OnCell, Danger.None);
    }

    public static IntVec3 FindCenterColony(Map map)
    {
        if (cachedValuesCenterColony.TryGetValue(map, out var value) &&
            Find.TickManager.TicksGame < value.lastCheckTick + 2500)
        {
            return value.value;
        }

        var source = from x in map.listerThings.AllThings
            where x.Faction == Faction.OfPlayer
            select x.Position;
        if (source.Any())
        {
            var source2 = source.OrderBy(x => x.x);
            var x2 = source2.ElementAt(source2.Count() / 2).x;
            var source3 = source.OrderBy(x => x.z);
            var z = source3.ElementAt(source3.Count() / 2).z;
            var intVec = new IntVec3(x2, 0, z);
            cachedValuesCenterColony[map] = new CachedValue
            {
                value = intVec,
                lastCheckTick = Find.TickManager.TicksGame
            };
            return intVec;
        }

        cachedValuesCenterColony[map] = new CachedValue
        {
            value = map.Center,
            lastCheckTick = Find.TickManager.TicksGame
        };
        return map.Center;
    }

    public static Job FleeIfEnemiesAreNearby(Pawn pawn)
    {
        var enumerable = pawn.Map.attackTargetsCache?.GetPotentialTargetsFor(pawn)?.Where(x =>
                (x is Pawn { Dead: false, Downed: false } || x.Thing is not Pawn && x.Thing.DestroyedOrNull()) &&
                x.Thing.Position.DistanceTo(pawn.Position) < 15f &&
                GenSight.LineOfSight(x.Thing.Position, pawn.Position, pawn.Map, false))
            .Select(y => y.Thing);
        if (enumerable != null && enumerable.Any())
        {
            return MakeFlee(pawn, enumerable.OrderBy(x => x.Position.DistanceTo(pawn.Position)).First(), 15,
                enumerable.ToList());
        }

        return null;
    }

    private static Job MakeFlee(Pawn pawn, Thing danger, int radius, List<Thing> dangers)
    {
        Job result = null;
        var intVec = pawn.CurJob == null || pawn.CurJob.def != JobDefOf.Flee
            ? CellFinderLoose.GetFleeDest(pawn, dangers, 24f)
            : pawn.CurJob.targetA.Cell;
        if (intVec == pawn.Position)
        {
            intVec = GenRadial.RadialCellsAround(pawn.Position, radius, radius * 2).RandomElement();
        }

        if (intVec != pawn.Position)
        {
            result = JobMaker.MakeJob(JobDefOf.Flee, intVec, danger);
        }

        return result;
    }

    public static Job MeleeAttackJob(Thing enemyTarget)
    {
        var job = JobMaker.MakeJob(JobDefOf.AttackMelee, enemyTarget);
        job.expiryInterval = ExpiryInterval_Melee.RandomInRange;
        job.checkOverrideOnExpire = true;
        job.expireRequiresEnemiesNearby = true;
        return job;
    }


    public static void ShutOffShield(Pawn pawn)
    {
        var shieldFieldComp = pawn.GetComp<CompShieldField>();
        if (shieldFieldComp == null)
        {
            return;
        }

        if (!shieldFieldComp.active)
        {
            return;
        }

        shieldFieldComp.Energy = 0;
        shieldFieldComp.active = false;
    }

    public static Job HealOtherMechanoidsOrRepairStructures(Pawn pawn, List<Pawn> otherPawns)
    {
        foreach (var otherPawn in otherPawns)
        {
            if (!otherPawn.CanBeHealed())
            {
                continue;
            }

            if (!otherPawn.PositionHeld.InAllowedArea(pawn))
            {
                continue;
            }

            if (!pawn.CanReserveAndReach(otherPawn, PathEndMode.Touch, Danger.None))
            {
                continue;
            }

            var job = JobMaker.MakeJob(RM_DefOf.RM_RepairMechanoid, otherPawn);
            job.locomotionUrgency = LocomotionUrgency.Sprint;
            return job;
        }

        var list = new List<Thing>();
        var lord = pawn.GetLord();
        if (lord is { ownedBuildings: not null })
        {
            list.AddRange(lord.ownedBuildings);
        }

        if (pawn.Faction == Faction.OfPlayer)
        {
            var list2 = pawn.Map.listerBuildingsRepairable.RepairableBuildings(pawn.Faction);
            var workGiver_Repair = new WorkGiver_Repair();
            foreach (var item in list2)
            {
                if (workGiver_Repair.HasJobOnThing(pawn, item))
                {
                    list.Add(item);
                }
            }
        }

        list = list.Distinct().ToList();
        foreach (var targetA in list.OrderBy(x => x.Position.DistanceTo(pawn.Position)).ToList())
        {
            if (!targetA.Spawned)
            {
                continue;
            }

            if (!RepairUtility.PawnCanRepairNow(pawn, targetA))
            {
                continue;
            }

            if (!targetA.PositionHeld.InAllowedArea(pawn))
            {
                continue;
            }

            if (!pawn.CanReserve(targetA))
            {
                continue;
            }

            if (!pawn.CanReach(targetA, PathEndMode.Touch, Danger.None))
            {
                continue;
            }

            var job2 = JobMaker.MakeJob(RM_DefOf.RM_RepairThing, targetA);
            job2.locomotionUrgency = LocomotionUrgency.Sprint;
            return job2;
        }

        return null;
    }

    public static List<Pawn> GetOtherMechanoids(Pawn seeker, Lord lord)
    {
        if (lord == null)
        {
            return (from x in (from x in seeker.Map.mapPawns.SpawnedPawnsInFaction(seeker.Faction)
                    where x.RaceProps.IsMechanoid && !x.Fogged() && !x.Dead && x.Awake()
                    select x).Except(seeker)
                orderby x.Position.DistanceTo(seeker.Position)
                select x).ToList();
        }

        var list = (from x in lord.ownedPawns.Where(x =>
                x.Spawned && x.Faction == seeker.Faction && x.RaceProps.IsMechanoid && !x.Fogged() && !x.Dead &&
                x.Awake()).Except(seeker)
            orderby x.Position.DistanceTo(seeker.Position)
            select x).ToList();
        if (list.Any())
        {
            return list;
        }

        return (from x in (from x in seeker.Map.mapPawns.SpawnedPawnsInFaction(seeker.Faction)
                where x.RaceProps.IsMechanoid && !x.Fogged() && !x.Dead && x.Awake()
                select x).Except(seeker)
            orderby x.Position.DistanceTo(seeker.Position)
            select x).ToList();
    }

    public static Job FollowOtherMechanoids(Pawn pawn, List<Pawn> otherPawns)
    {
        foreach (var item in otherPawns.InRandomOrder())
        {
            if (RM_DefOf.RM_FollowClose == item.CurJobDef || item.kindDef == RM_DefOf.RM_Mech_Vulture)
            {
                continue;
            }

            var job = TryGiveFollowJob(pawn, item, 6f);
            if (job != null)
            {
                return job;
            }
        }

        return null;
    }

    private static bool CanBeHealed(this Pawn pawn)
    {
        return pawn.kindDef != RM_DefOf.RM_Mech_Vulture && pawn.health.hediffSet.hediffs.Any(x => x is Hediff_Injury);
    }

    public static Job TryGiveFollowJob(Pawn pawn, Pawn followee, float radius)
    {
        if (!followee.Spawned || !pawn.CanReach(followee, PathEndMode.Touch, Danger.None))
        {
            return null;
        }

        var job = JobMaker.MakeJob(RM_DefOf.RM_FollowClose, followee);
        job.expiryInterval = 60;
        job.checkOverrideOnExpire = true;
        job.followRadius = radius;
        return job;
    }

    private class CachedValue
    {
        public Building firstBlockingBuilding;

        public int lastCheckTick;
        public IntVec3 value;
    }
}
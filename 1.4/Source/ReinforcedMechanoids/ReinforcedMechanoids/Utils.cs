using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids
{
    public static class Utils
    {
        private static HashSet<JobDef> combatJobs = new HashSet<JobDef>
                                                    {
                                                        JobDefOf.AttackMelee,
                                                        JobDefOf.AttackStatic,
                                                        JobDefOf.FleeAndCower,
                                                        JobDefOf.ManTurret,
                                                        JobDefOf.Flee
                                                    };

        public static bool InCombat(this Pawn pawn)
        {
            if (JobGiver_AIFightEnemy_TryGiveJob_Patch.gotoCombatJobs.Contains(pawn.CurJob))
            {
                return true;
            }
            else if (combatJobs.Contains(pawn.CurJobDef))
            {
                return true;
            }
            Stance_Busy stance_Busy;
            if (pawn.CurJobDef == JobDefOf.Wait_Combat && (stance_Busy = pawn.stances.curStance as Stance_Busy) != null && stance_Busy.focusTarg.IsValid)
            {
                return true;
            }
            return false;
        }
        public static bool IsCombatJob(this Job job)
        {
            return job != null && (combatJobs.Contains(job.def) || job.def.alwaysShowWeapon);
        }

        public static BodyPartRecord GetNonMissingBodyPart(Pawn pawn, BodyPartDef def)
        {
            foreach (BodyPartRecord notMissingPart in pawn.health.hediffSet.GetNotMissingParts())
            {
                if (notMissingPart.def == def)
                {
                    return notMissingPart;
                }
            }
            return null;
        }
        public static Dictionary<Pawn, CachedValue> cachedValuesPawn = new Dictionary<Pawn, CachedValue>();
        public static IntVec3 GetNearestWalkableCellDestination(IntVec3 destination, Pawn pawn, out Building firstBlockingBuilding, 
            Func<Pawn, IntVec3, Building, bool> unReachabilityCheck, float maxDistance = 0)
        {
            if (cachedValuesPawn.TryGetValue(pawn, out var cachedValue))
            {
                if (Find.TickManager.TicksGame < cachedValue.lastCheckTick + 180)
                {
                    firstBlockingBuilding = cachedValue.firstBlockingBuilding;
                    return cachedValue.value;
                }
            }
            firstBlockingBuilding = null;
            var path = pawn.Map.pathFinder.FindPath(pawn.Position, destination, TraverseMode.PassAllDestroyableThingsNotWater);
            var initialCell = pawn.Position;
            IntVec3 prevCell = initialCell;
            var pathNodes = path.NodesReversed.ListFullCopy();
            pathNodes.Reverse();
            pawn.Map.pawnPathPool.paths.Clear();
            foreach (var cell in pathNodes)
            {
                firstBlockingBuilding = cell.GetEdifice(pawn.Map);
                var distance = cell.DistanceTo(destination);
                if (maxDistance > 0 && maxDistance > distance || unReachabilityCheck(pawn, cell, firstBlockingBuilding))
                {
                    if (prevCell != pawn.Position)
                    {
                        cachedValuesPawn[pawn] = new CachedValue
                        {
                            value = prevCell,
                            firstBlockingBuilding = firstBlockingBuilding,
                            lastCheckTick = Find.TickManager.TicksGame
                        };
                        return prevCell;
                    }
                    else
                    {
                        cachedValuesPawn[pawn] = new CachedValue
                        {
                            value = IntVec3.Invalid,
                            firstBlockingBuilding = firstBlockingBuilding,
                            lastCheckTick = Find.TickManager.TicksGame
                        };
                        return IntVec3.Invalid;
                    }
                }
                prevCell = cell;
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
            return (cell.Roofed(pawn.Map) || building != null || !pawn.CanReach(cell, PathEndMode.OnCell, Danger.None));
        }

        public class CachedValue
        {
            public IntVec3 value;
            public int lastCheckTick;
            public Building firstBlockingBuilding;
        }
        public static Dictionary<Map, CachedValue> cachedValuesCenterColony = new Dictionary<Map, CachedValue>();
        public static IntVec3 FindCenterColony(Map map)
        {
            if (cachedValuesCenterColony.TryGetValue(map, out var cachedValue))
            {
                if (Find.TickManager.TicksGame < cachedValue.lastCheckTick + GenDate.TicksPerHour)
                {
                    return cachedValue.value;
                }
            }
            var colonyThings = map.listerThings.AllThings.Where(x => x.Faction == Faction.OfPlayer).Select(x => x.Position);
            if (colonyThings.Any())
            {
                var x_Averages = colonyThings.OrderBy(x => x.x);
                var x_average = x_Averages.ElementAt(x_Averages.Count() / 2).x;
                var z_Averages = colonyThings.OrderBy(x => x.z);
                var z_average = z_Averages.ElementAt(z_Averages.Count() / 2).z;
                var middleCell = new IntVec3(x_average, 0, z_average);
                cachedValuesCenterColony[map] = new CachedValue
                {
                    value = middleCell,
                    lastCheckTick = Find.TickManager.TicksGame
                };
                return middleCell;
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
            var enemies = pawn.Map.attackTargetsCache?.GetPotentialTargetsFor(pawn)?.Where(x =>
                        (x is Pawn pawnEnemy && !pawnEnemy.Dead && !pawnEnemy.Downed || !(x.Thing is Pawn) && x.Thing.DestroyedOrNull())
                        && x.Thing.Position.DistanceTo(pawn.Position) < 15f
                        && GenSight.LineOfSight(x.Thing.Position, pawn.Position, pawn.Map))?.Select(y => y.Thing);
            if (enemies?.Count() > 0)
            {
                return MakeFlee(pawn, enemies.OrderBy(x => x.Position.DistanceTo(pawn.Position)).First(), 15, enemies.ToList());
            }
            return null;
        }

        public static Job MakeFlee(Pawn pawn, Thing danger, int radius, List<Thing> dangers)
        {
            Job job = null;
            IntVec3 intVec;
            if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Flee)
            {
                intVec = pawn.CurJob.targetA.Cell;
            }
            else
            {
                intVec = CellFinderLoose.GetFleeDest(pawn, dangers, 24f);
            }

            if (intVec == pawn.Position)
            {
                intVec = GenRadial.RadialCellsAround(pawn.Position, radius, radius * 2).RandomElement();
            }
            if (intVec != pawn.Position)
            {
                job = JobMaker.MakeJob(JobDefOf.Flee, intVec, danger);
            }
            return job;
        }
        public static readonly IntRange ExpiryInterval_ShooterSucceeded = new IntRange(450, 550);

        public static readonly IntRange ExpiryInterval_Melee = new IntRange(360, 480);
        public static Job MeleeAttackJob(Thing enemyTarget)
        {
            Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, enemyTarget);
            job.expiryInterval = ExpiryInterval_Melee.RandomInRange;
            job.checkOverrideOnExpire = true;
            job.expireRequiresEnemiesNearby = true;
            return job;
        }

        public static Job HealOtherMechanoidsOrRepairStructures(Pawn pawn, List<Pawn> otherPawns)
        {
            foreach (var otherPawn in otherPawns)
            {
                if (otherPawn.CanBeHealed() && pawn.CanReserveAndReach(otherPawn, PathEndMode.Touch, Danger.None))
                {
                    var job = JobMaker.MakeJob(RM_DefOf.RM_RepairMechanoid, otherPawn);
                    job.locomotionUrgency = LocomotionUrgency.Sprint;
                    return job;
                }
            }

            var buildingsToRepair = new List<Thing>();
            var lord = pawn.GetLord();
            if (lord != null && lord.ownedBuildings != null)
            {
                buildingsToRepair.AddRange(lord.ownedBuildings);
            }
            if (pawn.Faction == Faction.OfPlayer)
            {
                var buildings = pawn.Map.listerBuildingsRepairable.RepairableBuildings(pawn.Faction);
                var workgiver = new WorkGiver_Repair();
                foreach (var building in buildings)
                {
                    if (workgiver.HasJobOnThing(pawn, building))
                    {
                        buildingsToRepair.Add(building);
                    }
                }
            }
            buildingsToRepair = buildingsToRepair.Distinct().ToList();
            foreach (var building in buildingsToRepair.OrderBy(x => x.Position.DistanceTo(pawn.Position)).ToList())
            {
                if (building.Spawned && RepairUtility.PawnCanRepairNow(pawn, building) && pawn.CanReserve(building)
                    && pawn.CanReach(building, PathEndMode.Touch, Danger.None))
                {
                    var job = JobMaker.MakeJob(RM_DefOf.RM_RepairThing, building);
                    job.locomotionUrgency = LocomotionUrgency.Sprint;
                    return job;
                }
            }
            return null;
        }

        public static List<Pawn> GetOtherMechanoids(Pawn seeker, Lord lord)
        {
            if (lord != null)
            {
                var pawns = lord.ownedPawns.Where(x => x.Spawned && x.Faction == seeker.Faction && x.RaceProps.IsMechanoid
                    && !x.Fogged() && !x.Dead && x.Awake()).Except(seeker).OrderBy(x => x.Position.DistanceTo(seeker.Position)).ToList();
                if (pawns.Any())
                {
                    return pawns;
                }
            }
            return seeker.Map.mapPawns.SpawnedPawnsInFaction(seeker.Faction)
                    .Where(x => x.RaceProps.IsMechanoid && !x.Fogged() && !x.Dead && x.Awake()).Except(seeker)
                    .OrderBy(x => x.Position.DistanceTo(seeker.Position)).ToList();
        }
        public static Job FollowOtherMechanoids(Pawn pawn, List<Pawn> otherPawns)
        {
            foreach (var otherPawn in otherPawns.InRandomOrder())
            {
                if (RM_DefOf.RM_FollowClose != otherPawn.CurJobDef && otherPawn.kindDef != RM_DefOf.RM_Mech_Vulture)
                {
                    var job = TryGiveFollowJob(pawn, otherPawn, 6);
                    if (job != null)
                    {
                        return job;
                    }
                }
            }
            return null;
        }

        public static bool CanBeHealed(this Pawn pawn)
        {
            return pawn.kindDef != RM_DefOf.RM_Mech_Vulture && pawn.health.hediffSet.hediffs.Any(x => x is Hediff_Injury);
        }

        public static Job TryGiveFollowJob(Pawn pawn, Pawn followee, float radius)
        {
            if (!followee.Spawned || !pawn.CanReach(followee, PathEndMode.Touch, Danger.None))
            {
                return null;
            }
            Job job = JobMaker.MakeJob(RM_DefOf.RM_FollowClose, followee);
            job.expiryInterval = 60;
            job.checkOverrideOnExpire = true;
            job.followRadius = radius;
            return job;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids.Harmony;

[HotSwappable]
[HarmonyPatch(typeof(JobGiver_AIFightEnemy), nameof(JobGiver_AIFightEnemy.TryGiveJob))]
public static class JobGiver_AIFightEnemy_TryGiveJob_HotSwappable
{
    public static void Prefix(JobGiver_AIFightEnemy __instance, Pawn pawn, ref Job __result)
    {
        if (pawn.kindDef != RM_DefOf.RM_Mech_WraithSiege)
        {
            return;
        }

        var primaryEq = pawn.equipment.PrimaryEq;
        if (primaryEq == null)
        {
            return;
        }

        __instance.targetKeepRadius = primaryEq.VerbTracker.PrimaryVerb.verbProps.range;
        __instance.targetAcquireRadius = __instance.targetKeepRadius - 9f;
        __instance.needLOSToAcquireNonPawnTargets = false;
    }

    public static void Postfix(JobGiver_AIFightEnemy __instance, Pawn pawn, ref Job __result)
    {
        if (__instance is not JobGiver_AIFightEnemiesNearToMaster)
        {
            TryModifyJob(pawn, ref __result);
        }
    }

    public static bool TryModifyJob(Pawn pawn, ref Job __result)
    {
        if (!pawn.RaceProps.IsMechanoid || pawn.Faction == Faction.OfPlayer ||
            pawn.kindDef == RM_DefOf.RM_Mech_Caretaker)
        {
            return true;
        }

        List<Pawn> list = null;
        var lord = pawn.GetLord();
        if (lord is { LordJob: ILordJobJobOverride lordJobJobOverride })
        {
            if (lordJobJobOverride.CanOverrideJobFor(pawn, __result))
            {
                list = Utils.GetOtherMechanoids(pawn, lord);

                var jobFor = lordJobJobOverride.GetJobFor(pawn, list, __result);
                if (jobFor != null)
                {
                    __result = jobFor;
                    return false;
                }
            }

            if (pawn.kindDef == RM_DefOf.RM_Mech_WraithSiege &&
                lord.LordJob is LordJob_AssaultColony_WraithSiege lordJob_AssaultColony_WraithSiege)
            {
                if (lordJob_AssaultColony_WraithSiege.siegeSpot.IsValid)
                {
                    if (!lordJob_AssaultColony_WraithSiege.siegeStarted)
                    {
                        if (!(pawn.Position == lordJob_AssaultColony_WraithSiege.siegeSpot))
                        {
                            __result = JobMaker.MakeJob(JobDefOf.Goto, lordJob_AssaultColony_WraithSiege.siegeSpot);
                            return false;
                        }

                        lordJob_AssaultColony_WraithSiege.StartSiege(pawn.Position);
                    }
                }
                else
                {
                    var intVec = findSiegePositionFrom(pawn.Position, pawn.Map, lordJob_AssaultColony_WraithSiege);
                    if (intVec.IsValid)
                    {
                        lordJob_AssaultColony_WraithSiege.siegeSpot = intVec;
                        __result = JobMaker.MakeJob(JobDefOf.Goto, intVec);
                        return false;
                    }
                }
            }
        }

        if (pawn.kindDef != RM_DefOf.RM_Mech_Vulture || pawn.mindState.duty?.def == RM_DefOf.RM_Build)
        {
            return true;
        }

        __result = Utils.FleeIfEnemiesAreNearby(pawn);
        if (__result != null)
        {
            return false;
        }

        list ??= Utils.GetOtherMechanoids(pawn, lord);

        __result = Utils.HealOtherMechanoidsOrRepairStructures(pawn, list) ?? Utils.FollowOtherMechanoids(pawn, list);

        return false;
    }

    private static IntVec3 findSiegePositionFrom(IntVec3 entrySpot, Map map,
        LordJob_AssaultColony_WraithSiege lordSiege,
        bool allowRoofed = false)
    {
        var list = new List<IntVec3>();
        foreach (var item in map.mapPawns.FreeColonistsSpawned)
        {
            list.Add(item.Position);
        }

        foreach (var allBuildingsColonistCombatTarget in map.listerBuildings.allBuildingsColonistCombatTargets)
        {
            list.Add(allBuildingsColonistCombatTarget.Position);
        }

        IntVec3 result;
        for (var num = 70; num >= 20; num -= 10)
        {
            if (tryFindSiegePosition(entrySpot, num, map, allowRoofed, lordSiege, list, out result))
            {
                return result;
            }
        }

        if (tryFindSiegePosition(entrySpot, 100f, map, allowRoofed, lordSiege, list, out result))
        {
            return result;
        }

        for (var num2 = 70; num2 >= 20; num2 -= 10)
        {
            if (tryFindSiegePositionVanilla(entrySpot, num2, map, allowRoofed, list, out result))
            {
                return result;
            }
        }

        return tryFindSiegePositionVanilla(entrySpot, 100f, map, allowRoofed, list, out result)
            ? result
            : IntVec3.Invalid;
    }

    private static bool tryFindSiegePosition(IntVec3 entrySpot, float minDistToColony, Map map, bool allowRoofed,
        LordJob_AssaultColony_WraithSiege lordSiege, List<IntVec3> list, out IntVec3 result)
    {
        var cellRect = CellRect.CenteredOn(entrySpot, 150);
        cellRect.ClipInsideMap(map);
        var num = minDistToColony * minDistToColony;
        return tryFindSiegeSpot(entrySpot, map, allowRoofed, (int)lordSiege.SiegeRadius, out result,
            cellRect, list, num);
    }

    private static bool tryFindSiegePositionVanilla(IntVec3 entrySpot, float minDistToColony, Map map, bool allowRoofed,
        List<IntVec3> list, out IntVec3 result)
    {
        var cellRect = CellRect.CenteredOn(entrySpot, 60);
        cellRect.ClipInsideMap(map);
        cellRect = cellRect.ContractedBy(14);
        var num = minDistToColony * minDistToColony;
        return tryFindSiegeSpotVanilla(entrySpot, map, allowRoofed, out result, cellRect, list, num);
    }

    private static bool isGoodCell(Map map, IntVec3 cell)
    {
        var terrain = cell.GetTerrain(map);
        return terrain.affordances.Contains(TerrainAffordanceDefOf.Heavy) &&
               terrain.affordances.Contains(TerrainAffordanceDefOf.Light) && terrain != RM_DefOf.MarshyTerrain &&
               cell.GetEdifice(map) == null;
    }

    private static bool tryFindSiegeSpot(IntVec3 entrySpot, Map map, bool allowRoofed, int siegeRadius,
        out IntVec3 result, CellRect cellRect,
        List<IntVec3> list, float num)
    {
        var list2 = cellRect.Cells.OrderBy(x => entrySpot.DistanceTo(x)).Where((_, i) => i % 10 == 0).ToList();
        while (list2.Any())
        {
            var intVec = list2.First();
            list2.Remove(intVec);
            if (intVec.CloseToEdge(map, 20) || !isGoodCell(map, intVec) ||
                !map.reachability.CanReach(intVec, entrySpot, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors,
                    Danger.Some) || !map.reachability.CanReachColony(intVec))
            {
                continue;
            }

            var inRange = false;
            foreach (var intVec3 in list)
            {
                if (!((intVec3 - intVec).LengthHorizontalSquared < num))
                {
                    continue;
                }

                inRange = true;
                break;
            }

            if (inRange || !allowRoofed && intVec.Roofed(map))
            {
                continue;
            }

            var list3 = CellRect.CenteredOn(intVec, siegeRadius).ClipInsideMap(map).Cells.ToList();
            var num2 = list3.Count(x => isGoodCell(map, x));
            if (!(num2 >= list3.Count / 1.2f))
            {
                continue;
            }

            result = intVec;
            return true;
        }

        result = IntVec3.Invalid;
        return false;
    }

    private static bool tryFindSiegeSpotVanilla(IntVec3 entrySpot, Map map, bool allowRoofed, out IntVec3 result,
        CellRect cellRect, List<IntVec3> list, float num)
    {
        var num2 = 0;
        while (true)
        {
            num2++;
            if (num2 > 200)
            {
                break;
            }

            var randomCell = cellRect.RandomCell;
            if (!randomCell.Standable(map) || !randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy) ||
                !randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Light) ||
                !map.reachability.CanReach(randomCell, entrySpot, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors,
                    Danger.Some) || !map.reachability.CanReachColony(randomCell))
            {
                continue;
            }

            var inRange = false;
            foreach (var intVec3 in list)
            {
                if (!((intVec3 - randomCell).LengthHorizontalSquared < num))
                {
                    continue;
                }

                inRange = true;
                break;
            }

            if (inRange || !allowRoofed && randomCell.Roofed(map))
            {
                continue;
            }

            var num3 = 0;
            foreach (var unused in CellRect.CenteredOn(randomCell, 10).ClipInsideMap(map))
            {
                if (randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy) &&
                    randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Light))
                {
                    num3++;
                }
            }

            if (num3 < 35)
            {
                continue;
            }

            result = randomCell;
            return true;
        }

        result = IntVec3.Invalid;
        return false;
    }
}
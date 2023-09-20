using HarmonyLib;
using MVCF.VerbComps;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using VFE.Mechanoids.HarmonyPatches;

namespace ReinforcedMechanoids
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        public static Harmony harmony;
        static HarmonyPatches()
        {
            harmony = new Harmony("ReinforcedMechanoids.Mod");
            harmony.PatchAll();
            ReinforcedMechanoidsMod.ApplySettings();
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.race != null && def.race.IsMechanoid)
                {
                    def.comps.Add(new CompProperties_AllianceOverlayToggle());
                }
            }
        }
    }

    [HarmonyBefore("legodude17.mvcf")]
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.TryGetAttackVerb))]
    public static class TryGetAttackVerb_Patch
    {
        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Pawn __instance, ref Verb __result, Thing target, bool allowManualCastWeapons = false)
        {
            if (__instance.health.hediffSet.GetFirstHediffOfDef(RM_DefOf.RM_SentinelBerserk) != null)
            {
                __result = __instance.equipment != null && __instance.equipment.Primary != null && __instance.equipment.Primary.def.IsMeleeWeapon
                    && __instance.equipment.PrimaryEq.PrimaryVerb.Available()
                    && (!__instance.equipment.PrimaryEq.PrimaryVerb.verbProps.onlyManualCast
                    || (__instance.CurJob != null && __instance.CurJob.def != JobDefOf.Wait_Combat) || allowManualCastWeapons)
                    ? __instance.equipment.PrimaryEq.PrimaryVerb
                    : __instance.meleeVerbs.TryGetMeleeVerb(target);
                return false;
            }
            return true;
        }
        private static void Postfix(Pawn __instance, ref Verb __result, Thing target)
        {
            if (__instance.kindDef == RM_DefOf.RM_Mech_Vulture)
            {
                __result = null;
                var job = Utils.FleeIfEnemiesAreNearby(__instance);
                if (job != null && __instance.CurJobDef != job.def && __instance.jobs.jobQueue.jobs.Any(x => x.job.def == job.def) is false)
                {
                    __instance.jobs.jobQueue.EnqueueFirst(job);
                }
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_AITrashColonyClose), "TryGiveJob")]
    public static class JobGiver_AITrashColonyClose_TryGiveJob
    {
        public static bool Prefix(Pawn pawn, ref Job __result)
        {
            return JobGiver_AIFightEnemy_TryGiveJob.TryModifyJob(pawn, ref __result);
        }
    }

    [HarmonyPatch(typeof(JobGiver_AISapper), "TryGiveJob")]
    public static class JobGiver_AISapper_TryGiveJob
    {
        public static bool Prefix(Pawn pawn, ref Job __result)
        {
            return JobGiver_AIFightEnemy_TryGiveJob.TryModifyJob(pawn, ref __result);
        }
    }

    [HarmonyPatch(typeof(JobGiver_AIGotoNearestHostile), "TryGiveJob")]
    public static class JobGiver_AIGotoNearestHostile_TryGiveJob
    {
        public static bool Prefix(Pawn pawn, ref Job __result)
        {
            return JobGiver_AIFightEnemy_TryGiveJob.TryModifyJob(pawn, ref __result);
        }

        public static bool UnReachabilityCheck(Pawn pawn, IntVec3 cell, Building building)
        {
            return cell.Roofed(pawn.Map) || building != null || !pawn.CanReach(cell, PathEndMode.OnCell, Danger.None);
        }
        public static void Postfix(ref Job __result, Pawn pawn)
        {
            if (__result != null && __result.def == JobDefOf.Goto)
            {
                bool unused1 = JobGiver_AIFightEnemy_TryGiveJob_Patch.gotoCombatJobs.Add(__result);
            }
            else
            {
                var compJump = pawn.GetComp<CompPawnJumper>();
                if (compJump != null && compJump.JumpAllowed && pawn.Faction != Faction.OfPlayer)
                {
                    float num = float.MaxValue;
                    Thing thing = null;
                    var potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
                    for (int i = 0; i < potentialTargetsFor.Count; i++)
                    {
                        var attackTarget = potentialTargetsFor[i];
                        if (!attackTarget.ThreatDisabled(pawn) && AttackTargetFinder.IsAutoTargetable(attackTarget))
                        {
                            var thing2 = (Thing)attackTarget;
                            if (compJump.CanBeJumpedTo(thing2.Position))
                            {
                                int num2 = thing2.Position.DistanceToSquared(pawn.Position);
                                if (num2 < num)
                                {
                                    if (compJump.CanJumpTo(thing2)
                                        || Utils.GetNearestWalkableCellDestination(thing2.Position, pawn, out _,
                                                Utils.UnReachabilityCheck, compJump.Props.maxJumpDistance).IsValid)
                                    {
                                        num = num2;
                                        thing = thing2;
                                    }
                                }
                            }
                        }
                    }
                    if (thing != null)
                    {
                        if (compJump.CanJumpTo(thing))
                        {
                            var job = JobMaker.MakeJob(JobDefOf.Goto, thing);
                            job.checkOverrideOnExpire = true;
                            job.expiryInterval = 500;
                            job.collideWithPawns = true;
                            __result = job;
                            bool unused = JobGiver_AIFightEnemy_TryGiveJob_Patch.gotoCombatJobs.Add(job);
                        }
                        else
                        {
                            var nearestCell = Utils.GetNearestWalkableCellDestination(thing.Position, pawn, out _, Utils.UnReachabilityCheck, compJump.Props.maxJumpDistance);
                            if (nearestCell != pawn.Position)
                            {
                                var job = JobMaker.MakeJob(JobDefOf.Goto, nearestCell);
                                job.checkOverrideOnExpire = true;
                                job.expiryInterval = 500;
                                job.collideWithPawns = true;
                                __result = job;
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_AITrashBuildingsDistant), "TryGiveJob")]
    public static class JobGiver_AITrashBuildingsDistant_TryGiveJob
    {
        public static bool Prefix(Pawn pawn, ref Job __result)
        {
            return JobGiver_AIFightEnemy_TryGiveJob.TryModifyJob(pawn, ref __result);
        }
    }
    [HarmonyPatch(typeof(JobDriver_Wait), "CheckForAutoAttack")]
    public static class JobDriver_Wait_CheckForAutoAttack
    {
        public static bool Prefix(JobDriver_Wait __instance)
        {
            if (__instance.pawn.kindDef == RM_DefOf.RM_Mech_WraithSiege)
            {
                CheckForAutoAttack(__instance);
                return false;
            }
            return true;
        }

        private static void CheckForAutoAttack(JobDriver_Wait __instance)
        {
            if (__instance.pawn.Downed || __instance.pawn.stances.FullBodyBusy || __instance.pawn.IsCarryingPawn())
            {
                return;
            }
            __instance.collideWithPawns = false;
            bool flag = !__instance.pawn.WorkTagIsDisabled(WorkTags.Violent);
            bool flag2 = __instance.pawn.RaceProps.ToolUser && __instance.pawn.Faction == Faction.OfPlayer && !__instance.pawn.WorkTagIsDisabled(WorkTags.Firefighting);
            if (!(flag || flag2))
            {
                return;
            }
            Fire fire = null;
            for (int i = 0; i < 9; i++)
            {
                var c = __instance.pawn.Position + GenAdj.AdjacentCellsAndInside[i];
                if (!c.InBounds(__instance.pawn.Map))
                {
                    continue;
                }
                var thingList = c.GetThingList(__instance.Map);
                for (int j = 0; j < thingList.Count; j++)
                {
                    if (flag)
                    {
                        if (thingList[j] is Pawn pawn && !pawn.Downed && __instance.pawn.HostileTo(pawn) && !__instance.pawn.ThreatDisabledBecauseNonAggressiveRoamer(pawn) && GenHostility.IsActiveThreatTo(pawn, __instance.pawn.Faction))
                        {
                            _ = __instance.pawn.meleeVerbs.TryMeleeAttack(pawn);
                            __instance.collideWithPawns = true;
                            return;
                        }
                    }
                    if (flag2)
                    {
                        if (thingList[j] is Fire fire2 && (fire == null || fire2.fireSize < fire.fireSize || i == 8) && (fire2.parent == null || fire2.parent != __instance.pawn))
                        {
                            fire = fire2;
                        }
                    }
                }
            }
            if (fire != null && (!__instance.pawn.InMentalState || __instance.pawn.MentalState.def.allowBeatfire))
            {
                _ = __instance.pawn.natives.TryBeatFire(fire);
            }
            else
            {
                if (!flag || !__instance.job.canUseRangedWeapon || __instance.pawn.Faction == null
                    || __instance.job.def != JobDefOf.Wait_Combat || (__instance.pawn.drafter != null
                    && !__instance.pawn.drafter.FireAtWill))
                {
                    return;
                }
                var currentEffectiveVerb = __instance.pawn.CurrentEffectiveVerb;
                if (currentEffectiveVerb != null && !currentEffectiveVerb.verbProps.IsMeleeAttack)
                {
                    var targetScanFlags = TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
                    if (currentEffectiveVerb.IsIncendiary_Ranged())
                    {
                        targetScanFlags |= TargetScanFlags.NeedNonBurning;
                    }
                    var thing = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(__instance.pawn, targetScanFlags);
                    if (thing != null)
                    {
                        _ = __instance.pawn.TryStartAttack(thing);
                        __instance.collideWithPawns = true;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "FindAttackTarget")]
    public static class JobGiver_AIFightEnemy_FindAttackTarget
    {
        public static bool Prefix(JobGiver_AIFightEnemy __instance, Pawn pawn, ref Thing __result)
        {
            if (pawn.kindDef == RM_DefOf.RM_Mech_WraithSiege)
            {
                __result = FindAttackTarget(__instance, pawn);
                return __result is null;
            }
            return true;
        }

        public static Thing FindAttackTarget(JobGiver_AIFightEnemy __instance, Pawn pawn)
        {
            var targetScanFlags = TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
            return (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags,
                (Thing x) => __instance.ExtraTargetValidator(pawn, x), 0f, __instance.targetAcquireRadius,
                __instance.GetFlagPosition(pawn), __instance.GetFlagRadius(pawn));
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob")]
    public static class JobGiver_AIFightEnemy_TryGiveJob
    {
        public static void Prefix(JobGiver_AIFightEnemy __instance, Pawn pawn, ref Job __result)
        {
            if (pawn.kindDef == RM_DefOf.RM_Mech_WraithSiege)
            {
                var equipment = pawn.equipment.PrimaryEq;
                if (equipment != null)
                {
                    __instance.targetKeepRadius = equipment.VerbTracker.PrimaryVerb.verbProps.range;
                    __instance.targetAcquireRadius = __instance.targetKeepRadius - 9;
                    __instance.needLOSToAcquireNonPawnTargets = false;
                }
            }
        }

        public static void Postfix(JobGiver_AIFightEnemy __instance, Pawn pawn, ref Job __result)
        {
            if (__instance is not JobGiver_AIFightEnemiesNearToMaster)
            {
                _ = TryModifyJob(pawn, ref __result);
            }
        }

        public static bool TryModifyJob(Pawn pawn, ref Job __result)
        {
            if (pawn.RaceProps.IsMechanoid && pawn.Faction != Faction.OfPlayer)
            {
                if (pawn.kindDef != RM_DefOf.RM_Mech_Caretaker)
                {
                    List<Pawn> otherPawns = null;
                    var lord = pawn.GetLord();
                    if (lord != null && lord.LordJob is ILordJobJobOverride lordJobOverride)
                    {
                        if (lordJobOverride.CanOverrideJobFor(pawn, __result))
                        {
                            otherPawns ??= Utils.GetOtherMechanoids(pawn, lord);
                            var job = lordJobOverride.GetJobFor(pawn, otherPawns, __result);
                            if (job != null)
                            {
                                __result = job;
                                return false;
                            }
                        }

                        if (pawn.kindDef == RM_DefOf.RM_Mech_WraithSiege
                            && lord.LordJob is LordJob_AssaultColony_WraithSiege lordSiege)
                        {
                            if (lordSiege.siegeSpot.IsValid)
                            {
                                if (lordSiege.siegeStarted is false)
                                {
                                    if (pawn.Position == lordSiege.siegeSpot)
                                    {
                                        lordSiege.StartSiege(pawn.Position);
                                    }
                                    else
                                    {
                                        __result = JobMaker.MakeJob(JobDefOf.Goto, lordSiege.siegeSpot);
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                var siegeSpot = FindSiegePositionFrom(pawn.Position, pawn.Map, lordSiege);
                                if (siegeSpot.IsValid)
                                {
                                    lordSiege.siegeSpot = siegeSpot;
                                    __result = JobMaker.MakeJob(JobDefOf.Goto, siegeSpot);
                                    return false;
                                }
                            }
                        }
                    }
                    if (pawn.kindDef == RM_DefOf.RM_Mech_Vulture && pawn.mindState.duty?.def != RM_DefOf.RM_Build)
                    {
                        __result = Utils.FleeIfEnemiesAreNearby(pawn);
                        if (__result is null)
                        {
                            otherPawns ??= Utils.GetOtherMechanoids(pawn, lord);
                            __result = Utils.HealOtherMechanoidsOrRepairStructures(pawn, otherPawns);
                            __result ??= Utils.FollowOtherMechanoids(pawn, otherPawns);
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        public static IntVec3 FindSiegePositionFrom(IntVec3 entrySpot, Map map, LordJob_AssaultColony_WraithSiege lordSiege, bool allowRoofed = false)
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
            for (int num = 70; num >= 20; num -= 10)
            {
                if (TryFindSiegePosition(entrySpot, num, map, allowRoofed, lordSiege, list, out result))
                {
                    return result;
                }
            }
            if (TryFindSiegePosition(entrySpot, 100f, map, allowRoofed, lordSiege, list, out result))
            {
                return result;
            }

            for (int num = 70; num >= 20; num -= 10)
            {
                if (TryFindSiegePositionVanilla(entrySpot, num, map, allowRoofed, lordSiege, list, out result))
                {
                    return result;
                }
            }
            if (TryFindSiegePositionVanilla(entrySpot, 100f, map, allowRoofed, lordSiege, list, out result))
            {
                return result;
            }
            return IntVec3.Invalid;
        }

        private static bool TryFindSiegePosition(IntVec3 entrySpot, float minDistToColony, Map map, bool allowRoofed,
            LordJob_AssaultColony_WraithSiege lordSiege, List<IntVec3> list, out IntVec3 result)
        {
            var cellRect = CellRect.CenteredOn(entrySpot, 150);
            _ = cellRect.ClipInsideMap(map);
            float num = minDistToColony * minDistToColony;
            return TryFindSiegeSpot(entrySpot, map, allowRoofed, lordSiege, (int)lordSiege.SiegeRadius, out result, cellRect, list, num);
        }
        private static bool TryFindSiegePositionVanilla(IntVec3 entrySpot, float minDistToColony, Map map, bool allowRoofed,
                LordJob_AssaultColony_WraithSiege lordSiege, List<IntVec3> list, out IntVec3 result)
        {
            var cellRect = CellRect.CenteredOn(entrySpot, 60);
            _ = cellRect.ClipInsideMap(map);
            cellRect = cellRect.ContractedBy(14);
            float num = minDistToColony * minDistToColony;
            return TryFindSiegeSpotVanilla(entrySpot, map, allowRoofed, out result, cellRect, list, num);
        }

        private static bool IsGoodCell(Map map, IntVec3 cell)
        {
            var terrain = cell.GetTerrain(map);
            return terrain.affordances.Contains(TerrainAffordanceDefOf.Heavy)
                && terrain.affordances.Contains(TerrainAffordanceDefOf.Light) && terrain != RM_DefOf.MarshyTerrain
                                    && cell.GetEdifice(map) is null;
        }

        private static bool TryFindSiegeSpot(IntVec3 entrySpot, Map map, bool allowRoofed, LordJob_AssaultColony_WraithSiege lordSiege, int siegeRadius, out IntVec3 result, CellRect cellRect, List<IntVec3> list, float num)
        {
            var allCellCandidates = cellRect.Cells.OrderBy(x => entrySpot.DistanceTo(x)).Where((x, i) => i % 10 == 0).ToList();
            while (allCellCandidates.Any())
            {
                var cell = allCellCandidates.First();
                bool unused = allCellCandidates.Remove(cell);
                if (cell.CloseToEdge(map, 20) || IsGoodCell(map, cell) is false
                    || !map.reachability.CanReach(cell, entrySpot, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Some)
                    || !map.reachability.CanReachColony(cell))
                {
                    continue;
                }
                bool flag = false;
                for (int i = 0; i < list.Count; i++)
                {
                    if ((list[i] - cell).LengthHorizontalSquared < num)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag || (!allowRoofed && cell.Roofed(map)))
                {
                    continue;
                }
                var cells = CellRect.CenteredOn(cell, siegeRadius).ClipInsideMap(map).Cells.ToList();
                int goodCellCount = cells.Where(x => IsGoodCell(map, x)).Count();
                if (goodCellCount >= (cells.Count / 1.2f))
                {
                    result = cell;
                    return true;
                }
            }
            result = IntVec3.Invalid;
            return false;
        }
        private static bool TryFindSiegeSpotVanilla(IntVec3 entrySpot, Map map, bool allowRoofed, out IntVec3 result, CellRect cellRect, List<IntVec3> list, float num)
        {
            int num2 = 0;
            while (true)
            {
                num2++;
                if (num2 > 200)
                {
                    break;
                }
                var randomCell = cellRect.RandomCell;
                if (!randomCell.Standable(map) || !randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy) || !randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Light) || !map.reachability.CanReach(randomCell, entrySpot, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Some) || !map.reachability.CanReachColony(randomCell))
                {
                    continue;
                }
                bool flag = false;
                for (int i = 0; i < list.Count; i++)
                {
                    if ((list[i] - randomCell).LengthHorizontalSquared < num)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag || (!allowRoofed && randomCell.Roofed(map)))
                {
                    continue;
                }
                int num3 = 0;
                foreach (var item2 in CellRect.CenteredOn(randomCell, 10).ClipInsideMap(map))
                {
                    _ = item2;
                    if (randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy) && randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Light))
                    {
                        num3++;
                    }
                }
                if (num3 >= 35)
                {
                    result = randomCell;
                    return true;
                }
            }
            result = IntVec3.Invalid;
            return false;
        }

    }

    [HarmonyPatch(typeof(LordMaker), nameof(LordMaker.MakeNewLord))]
    public static class LordMaker_MakeNewLord_Patch
    {
        public static void Prefix(Faction faction, LordJob lordJob, Map map, IEnumerable<Pawn> startingPawns = null)
        {
            if (faction?.def == RM_DefOf.RM_Remnants && lordJob is LordJob_AssaultColony lordJob2)
            {
                lordJob2.canTimeoutOrFlee = false;
            }
        }
    }

    [HarmonyPatch(typeof(HediffSet), nameof(HediffSet.DirtyCache))]
    public static class DirtyCache_Patch
    {
        private static void Postfix(HediffSet __instance)
        {
            var comp = __instance.pawn.GetComp<CompChangePawnGraphic>();
            if (comp != null)
            {
                comp.TryChangeGraphic();
            }
        }
    }

    [HarmonyPatch(typeof(DamageWorker_AddInjury), "GetExactPartFromDamageInfo")]
    public static class GetExactPartFromDamageInfo_Patch
    {
        public static bool pickShield;
        private static void Prefix(DamageInfo dinfo, Pawn pawn)
        {
            if (dinfo.Instigator is Pawn attacker && pawn.kindDef == RM_DefOf.RM_Mech_Behemoth)
            {
                float angle = (attacker.DrawPos - pawn.DrawPos).AngleFlat();
                var rot = Pawn_RotationTracker.RotFromAngleBiased(angle);
                if (rot == pawn.Rotation && Utils.GetNonMissingBodyPart(pawn, RM_DefOf.RM_BehemothShield) != null)
                {
                    pickShield = true;
                }
            }
        }

        private static void Postfix(DamageInfo dinfo, Pawn pawn)
        {
            pickShield = false;
        }
    }

    [HarmonyPatch(typeof(HediffSet), nameof(HediffSet.GetRandomNotMissingPart))]
    public static class GetRandomNotMissingPart_Patch
    {
        private static void Postfix(HediffSet __instance, ref BodyPartRecord __result)
        {
            if (GetExactPartFromDamageInfo_Patch.pickShield && Rand.Chance(0.8f))
            {
                var part = Utils.GetNonMissingBodyPart(__instance.pawn, RM_DefOf.RM_BehemothShield);
                if (part != null)
                {
                    __result = part;
                }
            }
        }
    }

    [HarmonyPatch(typeof(MechClusterGenerator), nameof(MechClusterGenerator.MechKindSuitableForCluster))]
    public class MechClusterGenerator_MechKindSuitableForCluster_Patch
    {
        public static void Postfix(PawnKindDef __0, ref bool __result)
        {
            if (__result && ReinforcedMechanoidsSettings.disabledMechanoids.Contains(__0.defName))
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_MeleeVerbs), "ChooseMeleeVerb")]
    public static class Patch_Pawn_MeleeVerbs_ChooseMeleeVerb
    {
        public static bool Prefix(Pawn_MeleeVerbs __instance, Thing target)
        {
            if (__instance.pawn.kindDef == RM_DefOf.RM_Mech_Behemoth)
            {
                var part = Utils.GetNonMissingBodyPart(__instance.pawn, RM_DefOf.RM_BehemothShield);
                if (part != null)
                {
                    var verb = __instance.GetUpdatedAvailableVerbsList(false).Where(x => x.verb is Verb_MeleeAttackDamageBehemoth).FirstOrDefault();
                    if (verb.verb != null)
                    {
                        __instance.SetCurMeleeVerb(verb.verb, target);
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnTweener), "PreDrawPosCalculation")]
    public static class PawnTweener_PreDrawPosCalculation_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var pawnField = AccessTools.Field(typeof(PawnTweener), "pawn");
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (i > 1 && codes[i - 1].opcode == OpCodes.Ldloc_1 && codes[i].opcode == OpCodes.Ldloc_2)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnTweener_PreDrawPosCalculation_Patch), "ReturnNum"));
                }
            }
        }

        public static float ReturnNum(float num, Pawn pawn)
        {
            if (pawn.pather != null && !pawn.pather.moving && pawn.health?.hediffSet?.GetFirstHediffOfDef(RM_DefOf.RM_BehemothAttack) != null)
            {
                return num * 0.5f;
            }
            return num;
        }
    }

    [HarmonyPatch(typeof(JobDriver_Flee_MakeNewToils_Patch), nameof(JobDriver_Flee_MakeNewToils_Patch.CanEmitFleeMote))]
    public static class CanEmitFleeMotePatch
    {
        public static void Postfix(ref bool __result, Pawn pawn)
        {
            if (pawn.kindDef == RM_DefOf.RM_Mech_Vulture)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.ButcherProducts))]
    public static class Pawn_ButcherProducts_Patch
    {
        private static IEnumerable<Thing> MakeButcherProducts_Postfix(IEnumerable<Thing> __result, Pawn __instance, Pawn butcher, float efficiency)
        {
            foreach (var r in __result)
            {
                yield return r;
            }

            var additionalButcherOptions = __instance.def.GetModExtension<AdditionalButcherProducts>();
            if (additionalButcherOptions != null)
            {
                foreach (var option in additionalButcherOptions.butcherOptions)
                {
                    if (Rand.Chance(option.chance))
                    {
                        var thing = ThingMaker.MakeThing(option.thingDef, null);
                        thing.stackCount = option.amount.RandomInRange;
                        yield return thing;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob")]
    public static class JobGiver_AIFightEnemy_TryGiveJob_Patch
    {
        public static HashSet<Job> gotoCombatJobs = new();
        public static void Postfix(ref Job __result, Pawn pawn)
        {
            if (__result != null && __result.def == JobDefOf.Goto)
            {
                _ = gotoCombatJobs.Add(__result);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
    public class Pawn_JobTracker_StartJob_Patch
    {
        private static bool Prefix(Pawn_JobTracker __instance, Pawn ___pawn, Job newJob, JobTag? tag)
        {
            if (JobGiver_AIFightEnemy_TryGiveJob_Patch.gotoCombatJobs.Contains(newJob))
            {
                var compJumper = ___pawn.GetComp<CompPawnJumper>();
                if (compJumper != null && ___pawn.Faction != Faction.OfPlayer)
                {
                    var target = newJob.targetA;
                    if (compJumper.CanJumpTo(target))
                    {
                        compJumper.DoJump(target);
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_PathFollower), "StartPath")]
    public class Pawn_PathFollower_StartPath_Patch
    {
        private static void Postfix(Pawn_PathFollower __instance, Pawn ___pawn, LocalTargetInfo dest, PathEndMode peMode)
        {
            var compJumper = ___pawn.GetComp<CompPawnJumper>();
            var target = __instance.Destination;
            if (compJumper != null && compJumper.CanJumpTo(target) && ___pawn.InCombat() && ___pawn.Faction != Faction.OfPlayer)
            {
                compJumper.DoJump(target);
            }
        }
    }

    [HarmonyPatch(typeof(VerbProperties), "AdjustedMeleeDamageAmount", new Type[] { typeof(Verb), typeof(Pawn) })]
    public static class AdjustedMeleeDamageAmount_Patch
    {
        public static void Postfix(Verb ownerVerb, Pawn attacker, ref float __result)
        {
            if (attacker.health.hediffSet.GetFirstHediffOfDef(RM_DefOf.RM_SentinelBerserk) != null)
            {
                __result *= 1.3f;
            }
        }
    }

    [HarmonyPatch]
    public static class AddDebrisPatch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.PropertyGetter(typeof(GenStep_ScatterRoadDebris), nameof(GenStep_ScatterRoadDebris.Debris));
            yield return AccessTools.PropertyGetter(typeof(SymbolResolver_AncientComplex_Defences), "AncientVehicles");
        }

        public static IEnumerable<ThingDef> Postfix(IEnumerable<ThingDef> __result)
        {
            foreach (var r in __result)
            {
                yield return r;
            }
            var def = DefDatabase<ThingDef>.GetNamedSilentFail("RM_AncientRustedJeep");
            if (def != null)
            {
                yield return def;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.DropAndForbidEverything))]
    public static class Pawn_DropAndForbidEverything_Patch
    {
        public static void Prefix(Pawn __instance)
        {
            if (__instance.kindDef.destroyGearOnDrop)
            {
                __instance.apparel ??= new Pawn_ApparelTracker(__instance);
                __instance.equipment ??= new Pawn_EquipmentTracker(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipment")]
    public static class PawnRenderer_DrawEquipment_Patch
    {
        public static void Prefix(PawnRenderer __instance, ref Vector3 rootLoc, Rot4 pawnRotation, PawnRenderFlags flags)
        {
            var extension = __instance.pawn.def.GetModExtension<EquipmentDrawPositionOffsetExtension>();
            if (extension != null)
            {
                switch (__instance.pawn.Rotation.AsInt)
                {
                    case 0: if (extension.northDrawOffset.HasValue) { rootLoc += extension.northDrawOffset.Value; } break;
                    case 1: if (extension.eastDrawOffset.HasValue) { rootLoc += extension.eastDrawOffset.Value; } break;
                    case 2: if (extension.southDrawOffset.HasValue) { rootLoc += extension.southDrawOffset.Value; } break;
                    case 3: if (extension.westDrawOffset.HasValue) { rootLoc += extension.westDrawOffset.Value; } break;
                    default: break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
    public static class PawnRenderer_DrawEquipmentAiming_Patch
    {
        public static void Prefix(PawnRenderer __instance, Thing eq, out float __state)
        {
            __state = eq.def.equippedAngleOffset;
            var extension = __instance.pawn.def.GetModExtension<EquipmentDrawPositionOffsetExtension>();
            if (extension != null)
            {
                switch (__instance.pawn.Rotation.AsInt)
                {
                    case 0: if (extension.northEquippedAngleOffset.HasValue) { eq.def.equippedAngleOffset = extension.northEquippedAngleOffset.Value; } break;
                    case 1: if (extension.eastEquippedAngleOffset.HasValue) { eq.def.equippedAngleOffset = extension.eastEquippedAngleOffset.Value; } break;
                    case 2: if (extension.southEquippedAngleOffset.HasValue) { eq.def.equippedAngleOffset = extension.southEquippedAngleOffset.Value; } break;
                    case 3: if (extension.westEquippedAngleOffset.HasValue) { eq.def.equippedAngleOffset = extension.westEquippedAngleOffset.Value; } break;
                    default: break;
                }
            }
        }

        public static void Postfix(PawnRenderer __instance, Thing eq, float __state)
        {
            eq.def.equippedAngleOffset = __state;
        }
    }

    [HarmonyPatch(typeof(WorldObject), "GetDescription")]
    public static class GetDescription_Patch
    {
        public static void Postfix(WorldObject __instance, ref string __result)
        {
            if (__instance is Settlement settlement && settlement.Faction?.def == RM_DefOf.RM_Remnants)
            {
                __result = "RM.RemnantsBaseDescription".Translate();
            }
        }
    }

    public class DrawPositionVector3 : DrawPosition
    {
        public Vector3 down;
        public Vector3 left;
        public Vector3 right;
        public Vector3 up;
    }

    [HarmonyPatch(typeof(DrawPosition), "ForRot")]
    public static class DrawPosition_ForRot_Patch
    {
        public static bool Prefix(DrawPosition __instance, ref Vector3 __result, Rot4 rot)
        {
            if (__instance is DrawPositionVector3 drawPositionOverride)
            {
                __result = ForRot(drawPositionOverride, rot);
                return false;
            }
            return true;
        }
        public static Vector3 ForRot(DrawPositionVector3 drawPosition, Rot4 rot)
        {
            var vec = Vector3.positiveInfinity;
            switch (rot.AsInt)
            {
                case 0:
                    vec = drawPosition.up;
                    break;
                case 1:
                    vec = drawPosition.right;
                    break;
                case 2:
                    vec = drawPosition.down;
                    break;
                case 3:
                    vec = drawPosition.left;
                    break;
                default:
                    break;
            }

            if (double.IsPositiveInfinity(vec.x))
            {
                vec = Vector3.positiveInfinity;
            }

            if (double.IsPositiveInfinity(vec.x))
            {
                vec = Vector2.zero;
            }

            return vec;
        }
    }

    [HarmonyPatch(typeof(PawnGroupMaker), "CanGenerateFrom")]
    public static class PawnGroupMaker_CanGenerateFrom_Patch
    {
        public static void Postfix(ref bool __result, PawnGroupMaker __instance, PawnGroupMakerParms parms)
        {
            if (__result)
            {
                if (__instance is PawnGroupMaker_CaretakerRaid)
                {
                    __result = parms.raidStrategy?.Worker is RaidStrategyWorker_ImmediateAttackMechanoidCaretaker;
                }
                else if (__instance is PawnGroupMaker_CaretakerRaidWithMechPresence pgmm)
                {
                    __result = pgmm.CanGenerate(parms) && parms.raidStrategy?.Worker is RaidStrategyWorker_ImmediateAttackMechanoidCaretaker;
                }
                else if (__instance is PawnGroupMaker_WraithSiege)
                {
                    __result = parms.raidStrategy?.Worker is RaidStrategyWorker_ImmediateAttackMechanoidWraith;
                }
                else if (__instance is PawnGroupMaker_LocustRaid)
                {
                    __result = parms.raidStrategy?.Worker is RaidStrategyWorker_ImmediateAttackMechanoidLocust;
                }
                else
                {
                    __result = parms.raidStrategy?.Worker is not RaidStrategyWorker_ImmediateAttackWithCertainPawnKind;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRelationUtility), "Notify_PawnsSeenByPlayer_Letter_Send")]
    public static class PawnRelationUtility_Notify_PawnsSeenByPlayer_Letter_Send_Patch
    {
        public static Exception Finalizer(Exception __exception)
        {
            if (__exception != null)
            {
                Log.Error(__exception.ToString());
            }
            return null;
        }
    }

    //[HarmonyPatch(typeof(MapGenerator), "GenerateMap")]
    //public static class MapGenerator_GenerateMap_Patch
    //{
    //    public static void Prefix(ref IntVec3 mapSize, MapParent parent, MapGeneratorDef mapGenerator, IEnumerable<GenStepWithParams> extraGenStepDefs = null, Action<Map> extraInitBeforeContentGen = null)
    //    {
    //        if (parent is Site site && site.parts.Exists(x => x.def == RM_DefOf.RM_MechanoidPresense))
    //        {
    //            mapSize = Find.World.info.initialMapSize;
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "DestroyEquipment")]
    public static class Pawn_EquipmentTracker_DestroyEquipment
    {
        public static bool Prefix(Pawn_EquipmentTracker __instance)
        {
            if (__instance.pawn.RaceProps.IsMechanoid && !__instance.pawn.Dead)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_PawnSpawned")]
    public static class Pawn_EquipmentTracker_DropAllEquipment
    {
        public static bool Prefix(Pawn_EquipmentTracker __instance)
        {
            if (__instance.pawn.RaceProps.IsMechanoid && !__instance.pawn.Dead)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnComponentsUtility), "AddAndRemoveDynamicComponents")]
    public static class PawnComponentsUtility_AddAndRemoveDynamicComponents
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn.kindDef == RM_DefOf.RM_Mech_Vulture)
            {
                AssignPawnComponents(pawn);
            }
        }

        public static void AssignPawnComponents(Pawn pawn)
        {
            pawn.story ??= new Pawn_StoryTracker(pawn);
            pawn.skills ??= new Pawn_SkillTracker(pawn);
            if (pawn.workSettings == null)
            {
                pawn.workSettings = new Pawn_WorkSettings(pawn);
                var priorities = new DefMap<WorkTypeDef, int>();
                priorities.SetAll(0);
                pawn.workSettings.priorities = priorities;
            }
        }
    }

    [HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]
    public static class StatExtension_GetStatValue_Patch
    {
        private static void Postfix(Thing thing, StatDef stat, bool applyPostProcess, ref float __result)
        {
            if (stat == StatDefOf.MoveSpeed && thing is Pawn pawn)
            {
                var curJob = pawn.CurJob;
                if (curJob != null && curJob.def == RM_DefOf.RM_FollowClose)
                {
                    var followee = curJob.targetA.Thing as Pawn;
                    float followeeMoveSpeed = followee.GetStatValue(StatDefOf.MoveSpeed);
                    float distance = followee.Position.DistanceTo(pawn.Position);
                    float multiplier = Mathf.Lerp(1.2f, 2f, Mathf.Min(1f, distance / 10f));
                    __result = Mathf.Min(__result, followeeMoveSpeed * multiplier);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Need_MechEnergy), "FallPerDay", MethodType.Getter)]
    public static class Need_MechEnergy_FallPerDay_Patch
    {
        public static void Postfix(Need_MechEnergy __instance, ref float __result)
        {
            if (__instance.pawn.health.hediffSet.HasHediff(RM_DefOf.RM_ZealotInvisibility))
            {
                __result *= 1.5f;
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_Work), "TryIssueJobPackage")]
    public static class JobGiver_Work_TryIssueJobPackage_Patch
    {
        public static void Postfix(JobGiver_Work __instance, ref ThinkResult __result, Pawn pawn, JobIssueParams jobParams)
        {
            if (pawn.kindDef == RM_DefOf.RM_Mech_Vulture && __result == ThinkResult.NoJob)
            {
                var otherPawns = Utils.GetOtherMechanoids(pawn, pawn.GetLord());
                var job = Utils.HealOtherMechanoidsOrRepairStructures(pawn, otherPawns);
                if (job != null)
                {
                    __result = new ThinkResult(job, __instance, JobTag.MiscWork);
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.DrawPawnBody))]
    public static class PawnRenderer_DrawPawnBody_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var shouldSkip = AccessTools.Method(typeof(PawnRenderer_DrawPawnBody_Patch), nameof(AllowOverlay));
            var get_IsMechanoidInfo = AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.IsMechanoid));
            var pawnInfo = AccessTools.Field(typeof(PawnRenderer), nameof(PawnRenderer.pawn));
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (i > 0 && codes[i].opcode == OpCodes.Brfalse_S && codes[i - 1].Calls(get_IsMechanoidInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnInfo);
                    yield return new CodeInstruction(OpCodes.Call, shouldSkip);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, codes[i].operand);
                }
            }
        }

        public static bool AllowOverlay(Pawn pawn)
        {
            var comp = pawn.GetComp<CompAllianceOverlayToggle>();
            if (comp != null && comp.isActive is false)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Faction), nameof(Faction.ShouldHaveLeader), MethodType.Getter)]
    public static class Faction_ShouldHaveLeader_Patch
    {
        public static bool Prefix(Faction __instance)
        {
            if (__instance.def == RM_DefOf.RM_Remnants)
            {
                return false;
            }
            return true;
        }
    }
}


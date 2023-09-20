using RimWorld;
using RimWorld.SketchGen;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
namespace ReinforcedMechanoids
{
    [HotSwappable]
    public class LordToil_WraithSiege : LordToil_AssaultColony
    {
        public Dictionary<Pawn, DutyDef> rememberedDuties = new Dictionary<Pawn, DutyDef>();

        private static readonly FloatRange BuilderCountFraction = new FloatRange(0.25f, 0.4f);
        public override IntVec3 FlagLoc => Data?.siegeCenter ?? IntVec3.Invalid;
        private LordToilData_Siege Data => (LordToilData_Siege)data;
        private LordJob_AssaultColony_WraithSiege LordJob => this.lord.LordJob as LordJob_AssaultColony_WraithSiege;
        private IEnumerable<Frame> Frames
        {
            get
            {
                LordToilData_Siege data = Data;
                float radSquared = (data.baseRadius + 10f) * (data.baseRadius + 10f);
                List<Thing> framesList = base.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame);
                if (framesList.Count == 0)
                {
                    yield break;
                }
                for (int i = 0; i < framesList.Count; i++)
                {
                    Frame frame = (Frame)framesList[i];
                    if (frame.Faction == lord.faction && (float)(frame.Position - data.siegeCenter).LengthHorizontalSquared < radSquared)
                    {
                        yield return frame;
                    }
                }
            }
        }
        public override bool ForceHighStoryDanger => true;
        public LordToil_WraithSiege()
        {

        }

        public void StartSiege(IntVec3 siegeCenter, float blueprintPoints)
        {
            SiegeBlueprintPlacer.center = siegeCenter;
            SiegeBlueprintPlacer.faction = lord.faction;
            data = new LordToilData_Siege();
            Data.siegeCenter = siegeCenter;
            Data.blueprintPoints = blueprintPoints;
            LordToilData_Siege lordToilData_Siege = Data;
            lordToilData_Siege.baseRadius = LordJob.SiegeRadius;
            List<Thing> list = new List<Thing>();
            foreach (Blueprint_Build item2 in PlaceBlueprints(lordToilData_Siege.siegeCenter, base.Map, lord.faction, lordToilData_Siege.blueprintPoints))
            {
                lordToilData_Siege.blueprints.Add(item2);
                foreach (ThingDefCountClass cost in item2.MaterialsNeeded())
                {
                    Thing thing = list.FirstOrDefault((Thing t) => t.def == cost.thingDef);
                    if (thing != null)
                    {
                        thing.stackCount += cost.count;
                        continue;
                    }
                    Thing thing2 = ThingMaker.MakeThing(cost.thingDef);
                    thing2.stackCount = cost.count;
                    list.Add(thing2);
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                list[i].stackCount = Mathf.CeilToInt((float)list[i].stackCount * Rand.Range(1f, 1.2f));
            }
            List<List<Thing>> list2 = new List<List<Thing>>();
            for (int j = 0; j < list.Count; j++)
            {
                while (list[j].stackCount > list[j].def.stackLimit)
                {
                    int num = Mathf.CeilToInt((float)list[j].def.stackLimit * Rand.Range(0.9f, 0.999f));
                    Thing thing4 = ThingMaker.MakeThing(list[j].def);
                    thing4.stackCount = num;
                    list[j].stackCount -= num;
                    list.Add(thing4);
                }
            }
            List<Thing> list3 = new List<Thing>();
            for (int k = 0; k < list.Count; k++)
            {
                list3.Add(list[k]);
                if (k % 2 == 1 || k == list.Count - 1)
                {
                    list2.Add(list3);
                    list3 = new List<Thing>();
                }
            }

            var vultureCount = Rand.RangeInclusive(2, 3) * LordJob.SiegeScale;
            for (var i = 0; i < vultureCount; i++)
            {
                if (this.LordJob.additionalRaidPoints > 0)
                {
                    var vulture = PawnGenerator.GeneratePawn(RM_DefOf.RM_Mech_Vulture, SiegeBlueprintPlacer.faction);
                    vulture.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
                    this.LordJob.additionalRaidPoints -= vulture.kindDef.combatPower;
                    lord.ownedPawns.Add(vulture);
                    var newList = new List<Thing> { vulture };
                    list2.Add(newList);
                }
            }

            if (ModsConfig.RoyaltyActive)
            {
                SpawnMechCluster(siegeCenter);
            }

            DropPodUtility.DropThingGroupsNear(lordToilData_Siege.siegeCenter, base.Map, list2);
            lordToilData_Siege.desiredBuilderFraction = BuilderCountFraction.RandomInRange;
        }

        private void SpawnMechCluster(IntVec3 siegeCenter)
        {
            ResolveParams parms = new ResolveParams();
            parms.mechClusterForMap = lord.Map;
            parms.points = this.LordJob.additionalRaidPoints;
            parms.totalPoints = null;
            parms.mechClusterDormant = false;
            parms.mechClusterSize = new IntVec2((int)Data.baseRadius - 10, (int)Data.baseRadius - 10);
            parms.sketch = new Sketch();
            parms.forceNoConditionCauser = true;
            bool canBeDormant = false;
            float num = parms.points.Value;
            float value = (parms.totalPoints.HasValue ? parms.totalPoints.Value : num);
            IntVec2 intVec = parms.mechClusterSize.Value;
            List<ThingDef> buildingDefsForCluster = GetBuildingDefsForCluster(num, intVec, canBeDormant, value, true);
            AddBuildingsToSketch(parms.sketch, intVec, buildingDefsForCluster);
            parms.sketch.Spawn(lord.Map, siegeCenter, lord.faction, Sketch.SpawnPosType.OccupiedCenter, Sketch.SpawnMode.TransportPod);
        }
        private static void AddBuildingsToSketch(Sketch sketch, IntVec2 size, List<ThingDef> buildings)
        {
            List<CellRect> edgeWallRects = new List<CellRect>
            {
                new CellRect(0, 0, size.x, 1),
                new CellRect(0, 0, 1, size.z),
                new CellRect(size.x - 1, 0, 1, size.z),
                new CellRect(0, size.z - 1, size.x, 1)
            };

            foreach (ThingDef item in buildings.OrderBy((ThingDef x) => x.building.IsTurret && !x.building.IsMortar))
            {
                bool flag = item.building.IsTurret && !item.building.IsMortar;
                if (!MechClusterGenerator.TryFindRandomPlaceFor(item, sketch, size, out var pos, lowerLeftQuarterOnly: false, flag, flag, !flag, edgeWallRects) && !MechClusterGenerator.TryFindRandomPlaceFor(item, sketch, size + new IntVec2(6, 6), out pos, lowerLeftQuarterOnly: false, flag, flag, !flag, edgeWallRects))
                {
                    continue;
                }
                sketch.AddThing(item, pos, Rot4.North, GenStuff.RandomStuffByCommonalityFor(item));
            }
        }

        private static readonly SimpleCurve LampBuildingMinCountCurve = new SimpleCurve
        {
            new CurvePoint(400f, 1f),
            new CurvePoint(1000f, 2f)
        };

        private static readonly SimpleCurve LampBuildingMaxCountCurve = new SimpleCurve
        {
            new CurvePoint(400f, 1f),
            new CurvePoint(1000f, 4f),
            new CurvePoint(2000f, 6f)
        };

        private static readonly SimpleCurve BulletShieldChanceCurve = new SimpleCurve
        {
            new CurvePoint(400f, 0.1f),
            new CurvePoint(1000f, 0.4f),
            new CurvePoint(2200f, 0.5f)
        };

        private static readonly SimpleCurve BulletShieldMaxCountCurve = new SimpleCurve
        {
            new CurvePoint(400f, 1f),
            new CurvePoint(3000f, 1.5f)
        };


        private static readonly SimpleCurve MortarShieldChanceCurve = new SimpleCurve
        {
            new CurvePoint(400f, 0.1f),
            new CurvePoint(1000f, 0.4f),
            new CurvePoint(2200f, 0.5f)
        };

        private static List<ThingDef> GetBuildingDefsForCluster(float points, IntVec2 size, bool canBeDormant, float? totalPoints, bool forceNoConditionCauser)
        {
            List<ThingDef> list = new List<ThingDef>();
            List<ThingDef> source = DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef def) 
                => def != ThingDef.Named("MechDropBeacon") && def.building != null && def.building.buildingTags != null 
                && def.building.buildingTags.Contains("MechClusterMember") 
                && (!totalPoints.HasValue || (float)def.building.minMechClusterPoints <= totalPoints)).ToList();

            int num4 = Rand.RangeInclusive(Mathf.FloorToInt(LampBuildingMinCountCurve.Evaluate(points)), Mathf.CeilToInt(LampBuildingMaxCountCurve.Evaluate(points)));
            for (int l = 0; l < num4; l++)
            {
                if (!source.Where((ThingDef x) => x.building.buildingTags.Contains("MechClusterMemberLamp")).TryRandomElement(out var result3))
                {
                    break;
                }
                list.Add(result3);
            }
            if (Rand.Chance(BulletShieldChanceCurve.Evaluate(points)))
            {
                points *= 0.85f;
                int num5 = Rand.RangeInclusive(0, GenMath.RoundRandom(BulletShieldMaxCountCurve.Evaluate(points)));
                for (int m = 0; m < num5; m++)
                {
                    list.Add(ThingDefOf.ShieldGeneratorBullets);
                }
            }
            if (Rand.Chance(MortarShieldChanceCurve.Evaluate(points)))
            {
                points *= 0.9f;
                list.Add(ThingDefOf.ShieldGeneratorMortar);
            }
            float pointsLeft = points;
            ThingDef thingDef = source.Where((ThingDef x) => x.building.buildingTags.Contains("MechClusterCombatThreat")).MinBy((ThingDef x) => x.building.combatPower);
            ThingDef result4;
            for (pointsLeft = Mathf.Max(pointsLeft, thingDef.building.combatPower); pointsLeft > 0f && source.Where((ThingDef x) => x.building.combatPower <= pointsLeft && x.building.buildingTags.Contains("MechClusterCombatThreat")).TryRandomElement(out result4); pointsLeft -= result4.building.combatPower)
            {
                list.Add(result4);
            }
            return list;
        }

        public IEnumerable<Blueprint_Build> PlaceBlueprints(IntVec3 placeCenter, Map map, Faction placeFaction, float points)
        {
            foreach (Blueprint_Build item in PlaceCoverBlueprints(map))
            {
                yield return item;
            }
        }

        private IntVec3 FindCoverRoot(Map map, ThingDef coverThing, ThingDef coverStuff)
        {
            CellRect cellRect = CellRect.CenteredOn(SiegeBlueprintPlacer.center, (int)(Data.baseRadius));
            cellRect.ClipInsideMap(map);
            CellRect cellRect2 = CellRect.CenteredOn(SiegeBlueprintPlacer.center, (int)(Data.baseRadius) - 5);
            int num = 0;
            IntVec3 randomCell;
            while (true)
            {
                num++;
                if (num > 200)
                {
                    return IntVec3.Invalid;
                }
                randomCell = cellRect.RandomCell;
                if (cellRect2.Contains(randomCell) || !map.reachability.CanReach(randomCell, SiegeBlueprintPlacer.center, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly) 
                    || !SiegeBlueprintPlacer.CanPlaceBlueprintAt(randomCell, Rot4.North, coverThing, map, coverStuff))
                {
                    continue;
                }
                bool flag = false;
                for (int i = 0; i < SiegeBlueprintPlacer.placedCoverLocs.Count; i++)
                {
                    if ((float)(SiegeBlueprintPlacer.placedCoverLocs[i] - randomCell).LengthHorizontalSquared < Mathf.Sqrt(Data.baseRadius))
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    break;
                }
            }
            return randomCell;
        }
        private IEnumerable<Blueprint_Build> PlaceCoverBlueprints(Map map)
        {
            SiegeBlueprintPlacer.placedCoverLocs.Clear();
            ThingDef coverThing;
            ThingDef coverStuff;
            coverThing = ThingDefOf.Barricade;
            coverStuff = ThingDefOf.Plasteel;
            int numCover = SiegeBlueprintPlacer.NumCoverRange.RandomInRange * Mathf.Max(1, (int)LordJob.SiegeScale);
            for (int i = 0; i < numCover; i++)
            {
                IntVec3 bagRoot = FindCoverRoot(map, coverThing, coverStuff);
                if (!bagRoot.IsValid)
                {
                    break;
                }
                Rot4 growDir = ((bagRoot.x <= SiegeBlueprintPlacer.center.x) ? Rot4.East : Rot4.West);
                Rot4 growDirB = ((bagRoot.z <= SiegeBlueprintPlacer.center.z) ? Rot4.North : Rot4.South);
                foreach (Blueprint_Build item in SiegeBlueprintPlacer.MakeCoverLine(bagRoot, map, growDir, SiegeBlueprintPlacer.CoverLengthRange.RandomInRange, coverThing, coverStuff))
                {
                    yield return item;
                }
                bagRoot += growDirB.FacingCell;
                foreach (Blueprint_Build item2 in SiegeBlueprintPlacer.MakeCoverLine(bagRoot, map, growDirB, SiegeBlueprintPlacer.CoverLengthRange.RandomInRange, coverThing, coverStuff))
                {
                    yield return item2;
                }
            }
        }

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
                lord.ownedPawns[i].mindState.duty.attackDownedIfStarving = attackDownedIfStarving;
                lord.ownedPawns[i].mindState.duty.pickupOpportunisticWeapon = canPickUpOpportunisticWeapons;
            }

            LordToilData_Siege data = Data;
            if (data != null)
            {
                rememberedDuties.Clear();
                int num2 = Frames.Count() + data.blueprints.Where(x => x.Destroyed is false).Count();
                int num3 = 0;
                for (int j = 0; j < lord.ownedPawns.Count; j++)
                {
                    Pawn pawn = lord.ownedPawns[j];
                    if (pawn.mindState.duty.def == RM_DefOf.RM_Build)
                    {
                        rememberedDuties.Add(pawn, RM_DefOf.RM_Build);
                        SetAsBuilder(pawn);
                        num3++;
                    }
                }
                int num4 = num2 - num3;
                for (int k = 0; k < num4; k++)
                {
                    if (lord.ownedPawns.Where((Pawn pa) => !rememberedDuties.ContainsKey(pa)
                    && CanBeBuilder(pa)).TryRandomElement(out var result))
                    {
                        rememberedDuties.Add(result, RM_DefOf.RM_Build);
                        SetAsBuilder(result);
                        num3++;
                    }
                }
            }
        }

        public override void Notify_PawnLost(Pawn victim, PawnLostCondition cond)
        {
            UpdateAllDuties();
            base.Notify_PawnLost(victim, cond);
        }

        public override void Notify_ConstructionFailed(Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
        {
            base.Notify_ConstructionFailed(pawn, frame, newBlueprint);
            if (frame.Faction == lord.faction && newBlueprint != null)
            {
                Data.blueprints.Add(newBlueprint);
            }
        }

        private bool CanBeBuilder(Pawn p)
        {
            return p.kindDef == RM_DefOf.RM_Mech_Vulture;
        }

        private void SetAsBuilder(Pawn p)
        {
            LordToilData_Siege lordToilData_Siege = Data;
            p.mindState.duty = new PawnDuty(RM_DefOf.RM_Build, lordToilData_Siege.siegeCenter);
            p.mindState.duty.radius = lordToilData_Siege.baseRadius;
            int minLevel = 20;
            p.skills.GetSkill(SkillDefOf.Construction).EnsureMinLevelWithMargin(minLevel);
            p.workSettings.EnableAndInitialize();
            List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                WorkTypeDef workTypeDef = allDefsListForReading[i];
                if (workTypeDef == WorkTypeDefOf.Construction)
                {
                    p.workSettings.SetPriority(workTypeDef, 1);
                }
                else
                {
                    p.workSettings.Disable(workTypeDef);
                }
            }
        }

        public override void LordToilTick()
        {
            base.LordToilTick();
            LordToilData_Siege lordToilData_Siege = Data;
            if (lordToilData_Siege != null)
            {
                if (lord.ticksInToil == 450)
                {
                    lord.CurLordToil.UpdateAllDuties();
                }
                if (lord.ticksInToil > 450 && lord.ticksInToil % 500 == 0)
                {
                    UpdateAllDuties();
                }
            }
        }

        public override void Cleanup()
        {
            LordToilData_Siege lordToilData_Siege = Data;
            lordToilData_Siege.blueprints.RemoveAll((Blueprint blue) => blue.Destroyed);
            for (int i = 0; i < lordToilData_Siege.blueprints.Count; i++)
            {
                lordToilData_Siege.blueprints[i].Destroy(DestroyMode.Cancel);
            }
            foreach (Frame item in Frames.ToList())
            {
                item.Destroy(DestroyMode.Cancel);
            }
            foreach (Building ownedBuilding in lord.ownedBuildings)
            {
                ownedBuilding.SetFaction(null);
            }
        }
    }
}

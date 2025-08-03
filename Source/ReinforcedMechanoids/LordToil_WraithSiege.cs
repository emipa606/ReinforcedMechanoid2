using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.SketchGen;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids;

[HotSwappable]
public class LordToil_WraithSiege : LordToil_AssaultColony
{
    private static readonly FloatRange BuilderCountFraction = new(0.25f, 0.4f);

    private static readonly SimpleCurve LampBuildingMinCountCurve =
    [
        new CurvePoint(400f, 1f),
        new CurvePoint(1000f, 2f)
    ];

    private static readonly SimpleCurve LampBuildingMaxCountCurve =
    [
        new CurvePoint(400f, 1f),
        new CurvePoint(1000f, 4f),
        new CurvePoint(2000f, 6f)
    ];

    private static readonly SimpleCurve BulletShieldChanceCurve =
    [
        new CurvePoint(400f, 0.1f),
        new CurvePoint(1000f, 0.4f),
        new CurvePoint(2200f, 0.5f)
    ];

    private static readonly SimpleCurve BulletShieldMaxCountCurve =
    [
        new CurvePoint(400f, 1f),
        new CurvePoint(3000f, 1.5f)
    ];

    private static readonly SimpleCurve MortarShieldChanceCurve =
    [
        new CurvePoint(400f, 0.1f),
        new CurvePoint(1000f, 0.4f),
        new CurvePoint(2200f, 0.5f)
    ];

    private readonly Dictionary<Pawn, DutyDef> rememberedDuties = new();

    public override IntVec3 FlagLoc => Data?.siegeCenter ?? IntVec3.Invalid;

    private LordToilData_Siege Data => (LordToilData_Siege)data;

    private LordJob_AssaultColony_WraithSiege LordJob => lord.LordJob as LordJob_AssaultColony_WraithSiege;

    private IEnumerable<Frame> Frames
    {
        get
        {
            var dataSiege = Data;
            var radSquared = (dataSiege.baseRadius + 10f) * (dataSiege.baseRadius + 10f);
            var framesList = Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame);
            if (framesList.Count == 0)
            {
                yield break;
            }

            foreach (var thing in framesList)
            {
                var frame = (Frame)thing;
                if (frame.Faction == lord.faction &&
                    (frame.Position - dataSiege.siegeCenter).LengthHorizontalSquared < radSquared)
                {
                    yield return frame;
                }
            }
        }
    }

    public override bool ForceHighStoryDanger => true;

    public void StartSiege(IntVec3 siegeCenter, float blueprintPoints)
    {
        SiegeBlueprintPlacer.center = siegeCenter;
        SiegeBlueprintPlacer.faction = lord.faction;
        data = new LordToilData_Siege();
        Data.siegeCenter = siegeCenter;
        Data.blueprintPoints = blueprintPoints;
        var lordToilData_Siege = Data;
        lordToilData_Siege.baseRadius = LordJob.SiegeRadius;
        var list = new List<Thing>();
        foreach (var item2 in PlaceBlueprints(Map))
        {
            lordToilData_Siege.blueprints.Add(item2);
            foreach (var cost in item2.TotalMaterialCost())
            {
                var thing = list.FirstOrDefault(t => t.def == cost.thingDef);
                if (thing != null)
                {
                    thing.stackCount += cost.count;
                    continue;
                }

                var thing2 = ThingMaker.MakeThing(cost.thingDef);
                thing2.stackCount = cost.count;
                list.Add(thing2);
            }
        }

        foreach (var thing in list)
        {
            thing.stackCount = Mathf.CeilToInt(thing.stackCount * Rand.Range(1f, 1.2f));
        }

        var list2 = new List<List<Thing>>();
        for (var j = 0; j < list.Count; j++)
        {
            while (list[j].stackCount > list[j].def.stackLimit)
            {
                var num = Mathf.CeilToInt(list[j].def.stackLimit * Rand.Range(0.9f, 0.999f));
                var thing3 = ThingMaker.MakeThing(list[j].def);
                thing3.stackCount = num;
                list[j].stackCount -= num;
                list.Add(thing3);
            }
        }

        var list3 = new List<Thing>();
        for (var k = 0; k < list.Count; k++)
        {
            list3.Add(list[k]);
            if (k % 2 != 1 && k != list.Count - 1)
            {
                continue;
            }

            list2.Add(list3);
            list3 = [];
        }

        var num2 = Rand.RangeInclusive(2, 3) * LordJob.SiegeScale;
        for (var l = 0; l < num2; l++)
        {
            if (!(LordJob.additionalRaidPoints > 0f))
            {
                continue;
            }

            var pawn = PawnGenerator.GeneratePawn(RM_DefOf.RM_Mech_Vulture, SiegeBlueprintPlacer.faction);
            pawn.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
            LordJob.additionalRaidPoints -= pawn.kindDef.combatPower;
            lord.ownedPawns.Add(pawn);
            var item = new List<Thing> { pawn };
            list2.Add(item);
        }

        if (ModsConfig.RoyaltyActive)
        {
            SpawnMechCluster(siegeCenter);
        }

        DropPodUtility.DropThingGroupsNear(lordToilData_Siege.siegeCenter, Map, list2);
        lordToilData_Siege.desiredBuilderFraction = BuilderCountFraction.RandomInRange;
    }

    private void SpawnMechCluster(IntVec3 siegeCenter)
    {
        var resolveParams = default(SketchResolveParams);
        resolveParams.mechClusterForMap = lord.Map;
        resolveParams.points = LordJob.additionalRaidPoints;
        resolveParams.totalPoints = null;
        resolveParams.mechClusterDormant = false;
        resolveParams.mechClusterSize = new IntVec2((int)Data.baseRadius - 10, (int)Data.baseRadius - 10);
        resolveParams.sketch = new Sketch();
        resolveParams.forceNoConditionCauser = true;
        var value = resolveParams.points.Value;
        var value3 = resolveParams.mechClusterSize.Value;
        var buildingDefsForCluster = GetBuildingDefsForCluster(value, value);
        AddBuildingsToSketch(resolveParams.sketch, value3, buildingDefsForCluster);
        resolveParams.sketch.Spawn(lord.Map, siegeCenter, lord.faction, Sketch.SpawnPosType.OccupiedCenter,
            Sketch.SpawnMode.TransportPod);
    }

    private static void AddBuildingsToSketch(Sketch sketch, IntVec2 size, List<ThingDef> buildings)
    {
        var edgeWallRects = new List<CellRect>
        {
            new(0, 0, size.x, 1),
            new(0, 0, 1, size.z),
            new(size.x - 1, 0, 1, size.z),
            new(0, size.z - 1, size.x, 1)
        };
        foreach (var item in buildings.OrderBy(x => x.building.IsTurret && !x.building.IsMortar))
        {
            var isTurret = item.building.IsTurret && !item.building.IsMortar;
            if (MechClusterGenerator.TryFindRandomPlaceFor(item, sketch, size, out var pos, false, isTurret, isTurret,
                    !isTurret,
                    edgeWallRects) || MechClusterGenerator.TryFindRandomPlaceFor(item, sketch, size + new IntVec2(6, 6),
                    out pos, false, isTurret, isTurret, !isTurret, edgeWallRects))
            {
                sketch.AddThing(item, pos, Rot4.North, GenStuff.RandomStuffByCommonalityFor(item));
            }
        }
    }

    private static List<ThingDef> GetBuildingDefsForCluster(float points, float? totalPoints)
    {
        var list = new List<ThingDef>();
        var source = DefDatabase<ThingDef>.AllDefsListForReading.Where(def =>
            def != ThingDef.Named("MechDropBeacon") && def.building is { buildingTags: not null } &&
            def.building.buildingTags.Contains("MechClusterMember") &&
            (!totalPoints.HasValue || (float)def.building.minMechClusterPoints <= totalPoints)).ToList();
        var num = Rand.RangeInclusive(Mathf.FloorToInt(LampBuildingMinCountCurve.Evaluate(points)),
            Mathf.CeilToInt(LampBuildingMaxCountCurve.Evaluate(points)));
        for (var i = 0; i < num; i++)
        {
            if (!source.Where(x => x.building.buildingTags.Contains("MechClusterMemberLamp"))
                    .TryRandomElement(out var result))
            {
                break;
            }

            list.Add(result);
        }

        if (Rand.Chance(BulletShieldChanceCurve.Evaluate(points)))
        {
            points *= 0.85f;
            var num2 = Rand.RangeInclusive(0, GenMath.RoundRandom(BulletShieldMaxCountCurve.Evaluate(points)));
            for (var j = 0; j < num2; j++)
            {
                list.Add(ThingDefOf.ShieldGeneratorBullets);
            }
        }

        if (Rand.Chance(MortarShieldChanceCurve.Evaluate(points)))
        {
            points *= 0.9f;
            list.Add(ThingDefOf.ShieldGeneratorMortar);
        }

        var pointsLeft = points;
        var thingDef = source.Where(x => x.building.buildingTags.Contains("MechClusterCombatThreat"))
            .MinBy(x => x.building.combatPower);
        for (pointsLeft = Mathf.Max(pointsLeft, thingDef.building.combatPower);
             pointsLeft > 0f && source
                 .Where(x => x.building.combatPower <= pointsLeft &&
                             x.building.buildingTags.Contains("MechClusterCombatThreat"))
                 .TryRandomElement(out var result2);
             pointsLeft -= result2.building.combatPower)
        {
            list.Add(result2);
        }

        return list;
    }

    private IEnumerable<Blueprint_Build> PlaceBlueprints(Map map)
    {
        foreach (var item in PlaceCoverBlueprints(map))
        {
            yield return item;
        }
    }

    private IntVec3 FindCoverRoot(Map map, ThingDef coverThing, ThingDef coverStuff)
    {
        var cellRect = CellRect.CenteredOn(SiegeBlueprintPlacer.center, (int)Data.baseRadius);
        cellRect.ClipInsideMap(map);
        var cellRect2 = CellRect.CenteredOn(SiegeBlueprintPlacer.center, (int)Data.baseRadius - 5);
        var num = 0;
        IntVec3 randomCell;
        while (true)
        {
            num++;
            if (num > 200)
            {
                return IntVec3.Invalid;
            }

            randomCell = cellRect.RandomCell;
            if (cellRect2.Contains(randomCell) ||
                !map.reachability.CanReach(randomCell, SiegeBlueprintPlacer.center, PathEndMode.OnCell,
                    TraverseMode.NoPassClosedDoors, Danger.Deadly) ||
                !SiegeBlueprintPlacer.CanPlaceBlueprintAt(randomCell, Rot4.North, coverThing, map, coverStuff))
            {
                continue;
            }

            var foundSpot = false;
            foreach (var intVec3 in SiegeBlueprintPlacer.placedCoverLocs)
            {
                if ((intVec3 - randomCell).LengthHorizontalSquared <
                    Mathf.Sqrt(Data.baseRadius))
                {
                    foundSpot = true;
                }
            }

            if (foundSpot)
            {
                continue;
            }

            break;
        }

        return randomCell;
    }

    private IEnumerable<Blueprint_Build> PlaceCoverBlueprints(Map map)
    {
        SiegeBlueprintPlacer.placedCoverLocs.Clear();
        var coverThing = ThingDefOf.Barricade;
        var coverStuff = ThingDefOf.Plasteel;
        var numCover = SiegeBlueprintPlacer.NumCoverRange.RandomInRange * Mathf.Max(1, (int)LordJob.SiegeScale);
        for (var i = 0; i < numCover; i++)
        {
            var bagRoot = FindCoverRoot(map, coverThing, coverStuff);
            if (!bagRoot.IsValid)
            {
                break;
            }

            var growDir = bagRoot.x <= SiegeBlueprintPlacer.center.x ? Rot4.East : Rot4.West;
            var growDirB = bagRoot.z <= SiegeBlueprintPlacer.center.z ? Rot4.North : Rot4.South;
            foreach (var item in SiegeBlueprintPlacer.MakeCoverLine(bagRoot, map, growDir,
                         SiegeBlueprintPlacer.CoverLengthRange.RandomInRange, coverThing, coverStuff))
            {
                yield return item;
            }

            bagRoot += growDirB.FacingCell;
            foreach (var item2 in SiegeBlueprintPlacer.MakeCoverLine(bagRoot, map, growDirB,
                         SiegeBlueprintPlacer.CoverLengthRange.RandomInRange, coverThing, coverStuff))
            {
                yield return item2;
            }
        }
    }

    public override void UpdateAllDuties()
    {
        foreach (var pawn in lord.ownedPawns)
        {
            pawn.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony)
            {
                attackDownedIfStarving = attackDownedIfStarving,
                pickupOpportunisticWeapon = canPickUpOpportunisticWeapons
            };
        }

        var lordToilData_Siege = Data;
        if (lordToilData_Siege == null)
        {
            return;
        }

        rememberedDuties.Clear();
        var num = Frames.Count() + lordToilData_Siege.blueprints.Count(x => !x.Destroyed);
        var num2 = 0;
        foreach (var pawn in lord.ownedPawns)
        {
            if (pawn.mindState.duty.def != RM_DefOf.RM_Build)
            {
                continue;
            }

            rememberedDuties.Add(pawn, RM_DefOf.RM_Build);
            SetAsBuilder(pawn);
            num2++;
        }

        var num3 = num - num2;
        for (var k = 0; k < num3; k++)
        {
            if (!lord.ownedPawns.Where(pa => !rememberedDuties.ContainsKey(pa) && CanBeBuilder(pa))
                    .TryRandomElement(out var result))
            {
                continue;
            }

            rememberedDuties.Add(result, RM_DefOf.RM_Build);
            SetAsBuilder(result);
            num2++;
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

    private static bool CanBeBuilder(Pawn p)
    {
        return p.kindDef == RM_DefOf.RM_Mech_Vulture;
    }

    private void SetAsBuilder(Pawn p)
    {
        var lordToilData_Siege = Data;
        p.mindState.duty = new PawnDuty(RM_DefOf.RM_Build, lordToilData_Siege.siegeCenter)
        {
            radius = lordToilData_Siege.baseRadius
        };
        var minLevel = 20;
        p.skills.GetSkill(SkillDefOf.Construction).EnsureMinLevelWithMargin(minLevel);
        p.workSettings.EnableAndInitialize();
        var allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
        foreach (var workTypeDef in allDefsListForReading)
        {
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
        var lordToilData_Siege = Data;
        if (lordToilData_Siege == null)
        {
            return;
        }

        if (lord.ticksInToil == 450)
        {
            lord.CurLordToil.UpdateAllDuties();
        }

        if (lord.ticksInToil > 450 && lord.ticksInToil % 500 == 0)
        {
            UpdateAllDuties();
        }
    }

    public override void Cleanup()
    {
        var lordToilData_Siege = Data;
        lordToilData_Siege.blueprints.RemoveAll(blue => blue.Destroyed);
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < lordToilData_Siege.blueprints.Count; i++)
        {
            lordToilData_Siege.blueprints[i].Destroy(DestroyMode.Cancel);
        }

        foreach (var item in Frames.ToList())
        {
            item.Destroy(DestroyMode.Cancel);
        }

        foreach (var ownedBuilding in lord.ownedBuildings)
        {
            ownedBuilding.SetFaction(null);
        }
    }
}
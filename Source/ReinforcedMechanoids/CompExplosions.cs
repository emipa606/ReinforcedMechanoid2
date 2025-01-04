using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids;

public class CompExplosions : ThingComp
{
    public readonly Dictionary<int, bool> bodyPartWasPresents = new Dictionary<int, bool>();

    public readonly Dictionary<int, bool> hadHealthHigherThanThresholds = new Dictionary<int, bool>();

    public int currentExplosionCount;

    public int curTicks;

    public int lastExplosionTicks;

    public Dictionary<int, int> toDetonate = new Dictionary<int, int>();

    public CompProperties_Explosions Props => props as CompProperties_Explosions;

    public virtual void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
    {
        base.PostPreApplyDamage(ref dinfo, out absorbed);
        if (parent is not Pawn pawn)
        {
            return;
        }

        for (var i = 0; i < Props.explosions.Count; i++)
        {
            var curProps = Props.explosions[i];
            if (curProps.missingBodyPartTrigger != null)
            {
                bodyPartWasPresents[i] = pawn.health.hediffSet.GetNotMissingParts()
                    .Any(x => x.def == curProps.missingBodyPartTrigger);
                continue;
            }

            if (curProps.healthPctThreshold > 0f)
            {
                hadHealthHigherThanThresholds[i] =
                    pawn.health.summaryHealth.SummaryHealthPercent > curProps.healthPctThreshold;
            }
        }
    }

    public void TryDetonateOnKilled(Map previousMap)
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < Props.explosions.Count; i++)
        {
            var compProperties_Explosions = Props.explosions[i];
            if (compProperties_Explosions.explodeOnKilled)
            {
                Detonate(previousMap, compProperties_Explosions, true);
            }
        }
    }

    public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.PostPostApplyDamage(dinfo, totalDamageDealt);
        if (parent is not Pawn pawn)
        {
            return;
        }

        curTicks = Find.TickManager.TicksGame;
        for (var i = 0; i < Props.explosions.Count; i++)
        {
            var curProps = Props.explosions[i];
            if (curProps.anyDamageCausesExplosion && CanExplode())
            {
                TryDetonateOrSchedule(parent.Map, curProps);
                continue;
            }

            if (curProps.missingBodyPartTrigger != null && bodyPartWasPresents.TryGetValue(i, out var wasPresent) &&
                wasPresent && pawn.health.hediffSet.GetNotMissingParts()
                    .All(x => x.def != curProps.missingBodyPartTrigger) &&
                CanExplode())
            {
                TryDetonateOrSchedule(parent.Map, curProps);
                continue;
            }

            if (curProps.healthPctThreshold > 0f &&
                hadHealthHigherThanThresholds.TryGetValue(i, out var healthHigher) &&
                healthHigher && pawn.health.summaryHealth.SummaryHealthPercent <= curProps.healthPctThreshold &&
                CanExplode())
            {
                TryDetonateOrSchedule(parent.Map, curProps);
            }
        }
    }

    public override void CompTick()
    {
        base.CompTick();
        var mapHeld = parent.MapHeld;
        foreach (var item in toDetonate.Keys.ToList())
        {
            if (Find.TickManager.TicksGame == toDetonate[item])
            {
                toDetonate.Remove(item);
                if (mapHeld != null)
                {
                    Detonate(mapHeld, Props.explosions[item], true);
                }
            }
            else if (Find.TickManager.TicksGame > toDetonate[item])
            {
                toDetonate.Remove(item);
            }
        }
    }

    private bool CanExplode()
    {
        return parent.Spawned &&
               (lastExplosionTicks == 0 || lastExplosionTicks + Props.cooldownTicks <= Find.TickManager.TicksGame) &&
               (Props.maxExplosionCount == 0 || Props.maxExplosionCount > currentExplosionCount);
    }

    public void TryDetonateOrSchedule(Map map, CompProperties_Explosions compProperties_Explosive)
    {
        if (compProperties_Explosive.ticksDelay > 0)
        {
            curTicks += compProperties_Explosive.ticksDelay;
        }

        if (curTicks > Find.TickManager.TicksGame && Props.explosions.Contains(compProperties_Explosive))
        {
            toDetonate[Props.explosions.IndexOf(compProperties_Explosive)] = curTicks;
        }
        else
        {
            Detonate(map, compProperties_Explosive);
        }
    }

    public void Detonate(Map map, CompProperties_Explosions compProperties_Explosive, bool ignoreUnspawned = false)
    {
        if (!ignoreUnspawned && !parent.SpawnedOrAnyParentSpawned)
        {
            return;
        }

        if (compProperties_Explosive.explosiveExpandPerFuel > 0f && parent.GetComp<CompRefuelable>() != null)
        {
            parent.GetComp<CompRefuelable>().ConsumeFuel(parent.GetComp<CompRefuelable>().Fuel);
        }

        var explosiveRadius = compProperties_Explosive.explosiveRadius;
        if (map == null)
        {
            Log.Warning("Tried to detonate CompExplosive in a null map.");
            return;
        }

        if (compProperties_Explosive.explosionEffect != null)
        {
            var effecter = compProperties_Explosive.explosionEffect.Spawn();
            effecter.Trigger(new TargetInfo(parent.PositionHeld, map), new TargetInfo(parent.PositionHeld, map));
            effecter.Cleanup();
        }

        Thing thing = parent;
        var positionHeld = parent.PositionHeld;
        var explosiveDamageType = compProperties_Explosive.explosiveDamageType;
        var damageAmountBase = compProperties_Explosive.damageAmountBase;
        var armorPenetrationBase = compProperties_Explosive.armorPenetrationBase;
        var explosionSound = compProperties_Explosive.explosionSound;
        var postExplosionSpawnThingDef = compProperties_Explosive.postExplosionSpawnThingDef;
        var postExplosionSpawnChance = compProperties_Explosive.postExplosionSpawnChance;
        var num = compProperties_Explosive.postExplosionSpawnThingCountRange?.RandomInRange ??
                  compProperties_Explosive.postExplosionSpawnThingCount;
        var applyDamageToExplosionCellsNeighbors = compProperties_Explosive.applyDamageToExplosionCellsNeighbors;
        var preExplosionSpawnThingDef = compProperties_Explosive.preExplosionSpawnThingDef;
        var preExplosionSpawnChance = compProperties_Explosive.preExplosionSpawnChance;
        var preExplosionSpawnThingCount = compProperties_Explosive.preExplosionSpawnThingCount;
        var chanceToStartFire = compProperties_Explosive.chanceToStartFire;
        var damageFalloff = compProperties_Explosive.damageFalloff;
        GenExplosion.DoExplosion(positionHeld, map, explosiveRadius, explosiveDamageType, thing, damageAmountBase,
            armorPenetrationBase, explosionSound, null, null, null, postExplosionSpawnThingDef,
            postExplosionSpawnChance, num, null, applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef,
            preExplosionSpawnChance, preExplosionSpawnThingCount, chanceToStartFire, damageFalloff);
        currentExplosionCount++;
        lastExplosionTicks = Find.TickManager.TicksGame;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref lastExplosionTicks, "lastExplosionTicks");
        Scribe_Values.Look(ref currentExplosionCount, "currentExplosionCount");
        Scribe_Collections.Look(ref toDetonate, "toDetonate", LookMode.Value, LookMode.Value);
        if (Scribe.mode == LoadSaveMode.PostLoadInit && toDetonate == null)
        {
            toDetonate = new Dictionary<int, int>();
        }
    }

    [HarmonyPatch(typeof(Thing), "Kill")]
    public static class Thing_Kill_Patch
    {
        public static void Prefix(Thing __instance, out Map __state)
        {
            __state = __instance.MapHeld;
        }

        public static void Postfix(Thing __instance, Map __state)
        {
            if (__instance.Destroyed && __state != null)
            {
                __instance.TryGetComp<CompExplosions>()?.TryDetonateOnKilled(__state);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Pawn_Kill_Patch
    {
        public static void Prefix(Pawn __instance, out Map __state)
        {
            __state = __instance.MapHeld;
        }

        public static void Postfix(Pawn __instance, Map __state)
        {
            if (__instance.Dead && __state != null)
            {
                __instance.GetComp<CompExplosions>()?.TryDetonateOnKilled(__state);
            }
        }
    }
}
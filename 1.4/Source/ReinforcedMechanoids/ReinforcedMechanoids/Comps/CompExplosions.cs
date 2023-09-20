using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ReinforcedMechanoids
{
    public class CompProperties_Explosions : CompProperties_Explosive
    {
        public List<CompProperties_Explosions> explosions;

        public int cooldownTicks = -1;

        public BodyPartDef missingBodyPartTrigger;

        public float healthPctThreshold;

        public bool anyDamageCausesExplosion;

        public int maxExplosionCount;

        public IntRange? postExplosionSpawnThingCountRange;

        public int ticksDelay;
        public CompProperties_Explosions()
        {
            this.compClass = typeof(CompExplosions);
        }
    }
    public class CompExplosions : ThingComp
    {
        public int lastExplosionTicks;
        public CompProperties_Explosions Props => base.props as CompProperties_Explosions;
        public int currentExplosionCount;
        public int curTicks;

        public Dictionary<int, int> toDetonate = new Dictionary<int, int>();
        public Dictionary<int, bool> bodyPartWasPresents = new Dictionary<int, bool>();
        public Dictionary<int, bool> hadHealthHigherThanThresholds = new Dictionary<int, bool>();
        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(dinfo, out absorbed);
            var pawn = this.parent as Pawn;
            for (var i = 0; i < Props.explosions.Count; i++)
            {
                var curProps = Props.explosions[i];
                if (curProps.missingBodyPartTrigger != null)
                {
                    bodyPartWasPresents[i] = pawn.health.hediffSet.GetNotMissingParts().Any(x => x.def == curProps.missingBodyPartTrigger);
                }
                else if (curProps.healthPctThreshold > 0)
                {
                    hadHealthHigherThanThresholds[i] = pawn.health.summaryHealth.SummaryHealthPercent > curProps.healthPctThreshold;
                }
            }
        }

        [HarmonyPatch(typeof(Thing), nameof(Thing.Kill))]
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
                    var comp = __instance.TryGetComp<CompExplosions>();
                    if (comp != null)
                    {
                        comp.TryDetonateOnKilled(__state);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
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
                    var comp = __instance.GetComp<CompExplosions>();
                    if (comp != null)
                    {
                        comp.TryDetonateOnKilled(__state);
                    }
                }
            }
        }
        public void TryDetonateOnKilled(Map previousMap)
        {
            for (var i = 0; i < Props.explosions.Count; i++)
            {
                var curProps = Props.explosions[i];
                if (curProps.explodeOnKilled)
                {
                    Detonate(previousMap, curProps, ignoreUnspawned: true);
                }
            }
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            var pawn = this.parent as Pawn;
            curTicks = Find.TickManager.TicksGame;
            for (var i = 0; i < Props.explosions.Count; i++)
            {
                var curProps = Props.explosions[i];
                if (curProps.anyDamageCausesExplosion && CanExplode())
                {
                    TryDetonateOrSchedule(parent.Map, curProps);
                }
                else if (curProps.missingBodyPartTrigger != null && bodyPartWasPresents[i] 
                    && pawn.health.hediffSet.GetNotMissingParts()
                    .Any(x => x.def == curProps.missingBodyPartTrigger) is false && CanExplode())
                {
                    TryDetonateOrSchedule(parent.Map, curProps);
                }
                else if (curProps.healthPctThreshold > 0 && hadHealthHigherThanThresholds[i]
                    && pawn.health.summaryHealth.SummaryHealthPercent <= curProps.healthPctThreshold && CanExplode())
                {
                    TryDetonateOrSchedule(parent.Map, curProps);
                }
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            var map = this.parent.MapHeld;
            foreach (var key in toDetonate.Keys.ToList())
            {
                if (Find.TickManager.TicksGame == toDetonate[key])
                {
                    toDetonate.Remove(key);
                    if (map != null)
                    {
                        Detonate(map, Props.explosions[key], ignoreUnspawned: true);
                    }
                }
                else if (Find.TickManager.TicksGame > toDetonate[key])
                {
                    toDetonate.Remove(key);
                }
            }
        }
        private bool CanExplode()
        {
            return this.parent.Spawned && (lastExplosionTicks == 0 || lastExplosionTicks + Props.cooldownTicks <= Find.TickManager.TicksGame)
                && (Props.maxExplosionCount == 0 || Props.maxExplosionCount > currentExplosionCount);
        }
        public void TryDetonateOrSchedule(Map map, CompProperties_Explosions compProperties_Explosive)
        {
            if (compProperties_Explosive.ticksDelay > 0)
            {
                curTicks += compProperties_Explosive.ticksDelay;
            }
            if (curTicks > Find.TickManager.TicksGame)
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
            var radius = compProperties_Explosive.explosiveRadius;
            if (map == null)
            {
                Log.Warning("Tried to detonate CompExplosive in a null map.");
                return;
            }
            if (compProperties_Explosive.explosionEffect != null)
            {
                Effecter effecter = compProperties_Explosive.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(parent.PositionHeld, map), new TargetInfo(parent.PositionHeld, map));
                effecter.Cleanup();
            }
            GenExplosion.DoExplosion(instigator: this.parent, center: parent.PositionHeld, map: map, radius: radius, damType: compProperties_Explosive.explosiveDamageType, damAmount: compProperties_Explosive.damageAmountBase, armorPenetration: compProperties_Explosive.armorPenetrationBase, explosionSound: compProperties_Explosive.explosionSound, weapon: null, projectile: null, intendedTarget: null, postExplosionSpawnThingDef: compProperties_Explosive.postExplosionSpawnThingDef, postExplosionSpawnChance: compProperties_Explosive.postExplosionSpawnChance, postExplosionSpawnThingCount: compProperties_Explosive.postExplosionSpawnThingCountRange?.RandomInRange ?? compProperties_Explosive.postExplosionSpawnThingCount, applyDamageToExplosionCellsNeighbors: compProperties_Explosive.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: compProperties_Explosive.preExplosionSpawnThingDef, preExplosionSpawnChance: compProperties_Explosive.preExplosionSpawnChance, preExplosionSpawnThingCount: compProperties_Explosive.preExplosionSpawnThingCount, chanceToStartFire: compProperties_Explosive.chanceToStartFire, damageFalloff: compProperties_Explosive.damageFalloff, direction: null);
            currentExplosionCount++;
            lastExplosionTicks = Find.TickManager.TicksGame;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastExplosionTicks, "lastExplosionTicks");
            Scribe_Values.Look(ref currentExplosionCount, "currentExplosionCount");
            Scribe_Collections.Look(ref toDetonate, "toDetonate", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                toDetonate ??= new Dictionary<int, int>();
            }
        }
    }
}


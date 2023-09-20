using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VanillaWeaponsExpandedLaser;
using Verse;
using Verse.Sound;

namespace ReinforcedMechanoids
{
    public class BeamExtension : DefModExtension
    {
        public int duration = 300;
        public float radius = 2;
        public int firesStartedPerTick = 4;
        public IntRange flameDamageAmountRange = new IntRange(65, 100);
        public IntRange corpseFlameDamageAmountRange = new IntRange(5, 10);
        public SoundDef sustainerSound;
    }
    public class LocustBeam : OrbitalStrike
    {
        public BeamExtension Props => def.GetModExtension<BeamExtension>();
        private static List<Thing> tmpThings = new List<Thing>();
        private Sustainer sustainer;
        public override void StartStrike()
        {
            angle = AngleRange.RandomInRange;
            startTick = Find.TickManager.TicksGame;
            MakeLocustBeamMote(base.Position, base.Map);
            if (Props.sustainerSound != null)
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    sustainer = Props.sustainerSound.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
                });
            }
        }

        public void MakeLocustBeamMote(IntVec3 cell, Map map)
        {
            Mote obj = (Mote)ThingMaker.MakeThing(RM_DefOf.RM_Mote_LocustBeam);
            obj.exactPosition = cell.ToVector3Shifted();
            obj.Scale = Props.radius * 6f;
            obj.rotationRate = 1.2f;
            GenSpawn.Spawn(obj, cell, map);
        }
        public override void Tick()
        {
            base.Tick();
            if (this.instigator is Pawn pawn && (pawn.stances.curStance is Stance_Mobile || pawn.Downed || pawn.Dead || pawn.Destroyed))
            {
                this.Destroy();
            }
            if (!base.Destroyed)
            {
                for (int i = 0; i < Props.firesStartedPerTick; i++)
                {
                    StartRandomFireAndDoFlameDamage();
                }
                sustainer?.Maintain();
            }
            else
            {
                sustainer?.End();
                sustainer = null;
            }
        }

        private void StartRandomFireAndDoFlameDamage()
        {
            IntVec3 c = (from x in GenRadial.RadialCellsAround(base.Position, Props.radius, useCenter: true)
                         where x.InBounds(base.Map)
                         select x).RandomElementByWeight((IntVec3 x) => 1f - Mathf.Min(x.DistanceTo(base.Position) / Props.radius, 1f) + 0.05f);
            FireUtility.TryStartFireIn(c, base.Map, Rand.Range(0.1f, 0.925f));
            tmpThings.Clear();
            tmpThings.AddRange(c.GetThingList(base.Map));
            for (int i = 0; i < tmpThings.Count; i++)
            {

                int num = ((tmpThings[i] is Corpse) ? Props.corpseFlameDamageAmountRange.RandomInRange : Props.flameDamageAmountRange.RandomInRange);
                Pawn pawn = tmpThings[i] as Pawn;
                BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = null;
                if (pawn != null)
                {
                    if (pawn.RaceProps.IsMechanoid)
                    {
                        continue;
                    }
                    else
                    {
                        battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_PowerBeam, instigator as Pawn);
                        Find.BattleLog.Add(battleLogEntry_DamageTaken);
                    }
                }
                tmpThings[i].TakeDamage(new DamageInfo(DamageDefOf.Flame, num, 0f, -1f, instigator, null, weaponDef)).AssociateWithLog(battleLogEntry_DamageTaken);
            }
            tmpThings.Clear();
        }
    }
    public class LaserLocust : LaserBeam
    {
        new LaserBeamDef def => base.def as LaserBeamDef;

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            bool shielded = hitThing.IsShielded() && def.IsWeakToShields || blockedByShield;
            Vector3 dir = (destination - origin).normalized;
            dir.y = 0;
            Vector3 b = shielded ? hitThing.TrueCenter() - dir.RotatedBy(Rand.Range(-22.5f, 22.5f)) * 0.8f : destination;
            var cell = b.ToIntVec3();
            LocustBeam obj = (LocustBeam)GenSpawn.Spawn(RM_DefOf.RM_LocustBeam, cell, this.Map);
            obj.duration = obj.Props.duration;
            obj.instigator = launcher;
            obj.weaponDef = launcher.def;
            obj.StartStrike();
            GenExplosion.DoExplosion(cell, Map, 1f, DamageDefOf.Bomb, this.launcher);
            if (this.launcher is Pawn pawn)
            {
                var hediff = pawn.health.AddHediff(RM_DefOf.RM_LocustBeamEffect);
                hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = this.def.lifetime;
            }
            base.Impact(hitThing, blockedByShield);
        }
    }
}


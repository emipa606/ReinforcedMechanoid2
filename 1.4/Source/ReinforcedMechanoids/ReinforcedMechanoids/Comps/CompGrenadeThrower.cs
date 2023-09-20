using AnimalBehaviours;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids
{
    public class CompProperties_GrenadeThrower : CompProperties
    {
        public IntRange grenadeDropTickInterval;
        public BodyPartDef requiredBodyPart;
        public ThingDef grenadeLauncher;
        public float maxDistance;
        public CompProperties_GrenadeThrower()
        {
            this.compClass = typeof(CompGrenadeThrower);
        }
    }
    public class CompGrenadeThrower : ThingComp, PawnGizmoProvider
    {
        public int nextAllowedThrowTick;
        public CompProperties_GrenadeThrower Props => base.props as CompProperties_GrenadeThrower;

        private ThingWithComps grenadeLauncher;
        public ThingWithComps GrenadeLauncher
        {
            get
            {
                if (grenadeLauncher is null)
                {
                    grenadeLauncher = ThingMaker.MakeThing(Props.grenadeLauncher) as ThingWithComps;
                }
                return grenadeLauncher;
            }
        }
        public override string CompInspectStringExtra()
        {
            var sb = new StringBuilder();
            if (CanThrow)
            {
                var ticks = nextAllowedThrowTick - Find.TickManager.TicksGame;
                if (ticks > 0)
                {
                    sb.AppendLine("RM.CanThrowGrenadesIn".Translate(ticks.ToStringTicksToPeriod()));
                }
                else
                {
                    sb.AppendLine("RM.CanThrowGrenadesNow".Translate());
                }
            }

            return sb.ToString().TrimEndNewlines();
        }
        public bool CanThrow
        {
            get
            {
                if (Props.requiredBodyPart != null && this.parent is Pawn pawn 
                    && Utils.GetNonMissingBodyPart(pawn, Props.requiredBodyPart) is null)
                {
                    return false;
                }
                return this.parent.Spawned;
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            if (CanThrow && Find.TickManager.TicksGame >= nextAllowedThrowTick && Find.TickManager.TicksGame % 60 == 0)
            {
                var enemies = this.parent.Map.attackTargetsCache?.GetPotentialTargetsFor(this.parent as IAttackTargetSearcher)?
                    .Where(x => x is Pawn pawnEnemy && !pawnEnemy.Dead && !pawnEnemy.Downed
                    && x.Thing.Position.DistanceTo(this.parent.Position) <= Props.maxDistance
                    && GenSight.LineOfSight(x.Thing.Position, this.parent.Position, this.parent.Map))?.Select(y => y.Thing);
                if (enemies.TryRandomElement(out var enemy))
                {
                    var projectile2 = (Projectile)GenSpawn.Spawn(GrenadeLauncher.GetComp<CompEquippable>().PrimaryVerb.GetProjectile(), this.parent.Position, this.parent.Map);
                    projectile2.Launch(this.parent, enemy, enemy, ProjectileHitFlags.IntendedTarget, false, null);
                    nextAllowedThrowTick = Find.TickManager.TicksGame + Props.grenadeDropTickInterval.RandomInRange;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref nextAllowedThrowTick, "nextAllowedThrowTick");
        }

        public IEnumerable<Gizmo> GetGizmos()
        {
            var comp = GrenadeLauncher.GetComp<CompEquippable>();
            foreach (var verb in comp.AllVerbs)
            {
                verb.caster = this.parent;
            }
            foreach (Command verbsCommand in GetVerbsCommands())
            {
                if (this.parent.Faction == Faction.OfPlayer)
                {
                    verbsCommand.Disable("RM.AutomaticMode".Translate());
                }
                yield return verbsCommand;
            }
        }

        public IEnumerable<Command> GetVerbsCommands(KeyCode hotKey = KeyCode.None)
        {
            var comp = GrenadeLauncher.GetComp<CompEquippable>();
            CompEquippable ce = comp.VerbTracker.directOwner as CompEquippable;
            if (ce == null)
            {
                yield break;
            }
            Thing ownerThing = ce.parent;
            List<Verb> verbs = comp.VerbTracker.AllVerbs;
            for (int i = 0; i < verbs.Count; i++)
            {
                Verb verb = verbs[i];
                if (verb.verbProps.hasStandardCommand)
                {
                    yield return comp.VerbTracker.CreateVerbTargetCommand(ownerThing, verb);
                }
            }
            if (!comp.VerbTracker.directOwner.Tools.NullOrEmpty() && ce != null && ce.parent.def.IsMeleeWeapon)
            {
                yield return comp.VerbTracker.CreateVerbTargetCommand(ownerThing, verbs.Where((Verb v) => v.verbProps.IsMeleeAttack).FirstOrDefault());
            }
        }
    }
}

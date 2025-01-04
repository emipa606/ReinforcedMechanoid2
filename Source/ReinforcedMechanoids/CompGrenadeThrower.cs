using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnimalBehaviours;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

public class CompGrenadeThrower : ThingComp, PawnGizmoProvider
{
    private ThingWithComps grenadeLauncher;
    public int nextAllowedThrowTick;

    public CompProperties_GrenadeThrower Props => props as CompProperties_GrenadeThrower;

    public ThingWithComps GrenadeLauncher
    {
        get
        {
            if (grenadeLauncher == null)
            {
                grenadeLauncher = ThingMaker.MakeThing(Props.grenadeLauncher) as ThingWithComps;
            }

            return grenadeLauncher;
        }
    }

    public bool CanThrow
    {
        get
        {
            if (Props.requiredBodyPart != null && parent is Pawn pawn &&
                Utils.GetNonMissingBodyPart(pawn, Props.requiredBodyPart) == null)
            {
                return false;
            }

            return parent.Spawned;
        }
    }

    public IEnumerable<Gizmo> GetGizmos()
    {
        var comp = GrenadeLauncher.GetComp<CompEquippable>();
        foreach (var verb in comp.AllVerbs)
        {
            verb.caster = parent;
        }

        foreach (var verbsCommand in GetVerbsCommands())
        {
            if (parent.Faction == Faction.OfPlayer)
            {
                verbsCommand.Disable("RM.AutomaticMode".Translate());
            }

            yield return verbsCommand;
        }
    }

    public override string CompInspectStringExtra()
    {
        var stringBuilder = new StringBuilder();
        if (!CanThrow)
        {
            return stringBuilder.ToString().TrimEndNewlines();
        }

        var num = nextAllowedThrowTick - Find.TickManager.TicksGame;
        stringBuilder.AppendLine(num > 0
            ? "RM.CanThrowGrenadesIn".Translate(num.ToStringTicksToPeriod())
            : "RM.CanThrowGrenadesNow".Translate());

        return stringBuilder.ToString().TrimEndNewlines();
    }

    public override void CompTick()
    {
        base.CompTick();
        if (!CanThrow || Find.TickManager.TicksGame < nextAllowedThrowTick || Find.TickManager.TicksGame % 60 != 0)
        {
            return;
        }

        var source = parent.Map.attackTargetsCache?.GetPotentialTargetsFor(parent as IAttackTargetSearcher)?.Where(
                x => x is Pawn { Dead: false, Downed: false } &&
                     x.Thing.Position.DistanceTo(parent.Position) <= Props.maxDistance &&
                     GenSight.LineOfSight(x.Thing.Position, parent.Position, parent.Map, false))
            .Select(y => y.Thing);
        if (!source.TryRandomElement(out var result))
        {
            return;
        }

        var projectile = (Projectile)GenSpawn.Spawn(
            GrenadeLauncher.GetComp<CompEquippable>().PrimaryVerb.GetProjectile(), parent.Position, parent.Map);
        projectile.Launch(parent, result, result, ProjectileHitFlags.IntendedTarget);
        nextAllowedThrowTick = Find.TickManager.TicksGame + Props.grenadeDropTickInterval.RandomInRange;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref nextAllowedThrowTick, "nextAllowedThrowTick");
    }

    public IEnumerable<Command> GetVerbsCommands(KeyCode hotKey = KeyCode.None)
    {
        var comp = GrenadeLauncher.GetComp<CompEquippable>();
        if (comp.VerbTracker.directOwner is not CompEquippable ce)
        {
            yield break;
        }

        Thing ownerThing = ce.parent;
        var verbs = comp.VerbTracker.AllVerbs;
        foreach (var verb in verbs)
        {
            if (verb.verbProps.hasStandardCommand)
            {
                yield return comp.VerbTracker.CreateVerbTargetCommand(ownerThing, verb);
            }
        }

        if (!comp.VerbTracker.directOwner.Tools.NullOrEmpty() && ce.parent.def.IsMeleeWeapon)
        {
            yield return comp.VerbTracker.CreateVerbTargetCommand(ownerThing,
                verbs.FirstOrDefault(v => v.verbProps.IsMeleeAttack));
        }
    }
}
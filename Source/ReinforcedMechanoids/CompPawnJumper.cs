using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace ReinforcedMechanoids;

public class CompPawnJumper : ThingComp
{
    public LocalTargetInfo curTarget = LocalTargetInfo.Invalid;
    protected Effecter effecter;

    public int jumpTick = -1;

    public int nextAllowedJumpTick;

    public CompProperties_PawnJumper Props => props as CompProperties_PawnJumper;

    public bool JumpAllowed => Find.TickManager.TicksGame >= nextAllowedJumpTick;

    public override string CompInspectStringExtra()
    {
        var stringBuilder = new StringBuilder();
        var num = nextAllowedJumpTick - Find.TickManager.TicksGame;
        stringBuilder.AppendLine(num <= 0
            ? "RM.CanJumpNow".Translate()
            : "RM.CanJumpIn".Translate(num.ToStringTicksToPeriod()));

        return stringBuilder.ToString().TrimEndNewlines();
    }

    public bool CanJumpTo(LocalTargetInfo target)
    {
        if (parent is Pawn pawn && (pawn.Downed || pawn.Dead))
        {
            return false;
        }

        if (!JumpAllowed)
        {
            return false;
        }

        if (!JumpDistanceCheck(target.Cell))
        {
            return false;
        }

        return parent.Spawned && CanBeJumpedTo(target.Cell);
    }

    public TargetingParameters JumpTargetParameters()
    {
        return new TargetingParameters
        {
            canTargetPawns = true,
            canTargetBuildings = false,
            canTargetHumans = true,
            canTargetMechs = true,
            canTargetAnimals = true,
            canTargetLocations = true,
            validator = x => CanJumpTo((LocalTargetInfo)x)
        };
    }

    public bool JumpDistanceCheck(IntVec3 cell)
    {
        var num = parent.Position.DistanceTo(cell);
        return !(num > Props.maxJumpDistance) && !(num <= Props.minJumpDistance);
    }

    public bool CanBeJumpedTo(IntVec3 target)
    {
        var map = parent.Map;
        return !target.Roofed(map) && target.Walkable(map);
    }

    public void DoJump(LocalTargetInfo target)
    {
        if (parent is not Pawn pawn)
        {
            return;
        }

        if (Props.warmupTime > 0f)
        {
            var stance = new Stance_Wait(Props.warmupTime.SecondsToTicks(), target, pawn.CurrentEffectiveVerb)
            {
                neverAimWeapon = true
            };
            pawn.stances.SetStance(stance);
            if (Props.warmupEffecter != null)
            {
                effecter = Props.warmupEffecter.Spawn(pawn, pawn.Map);
                effecter.Trigger((TargetInfo)pawn, target.ToTargetInfo(pawn.Map));
            }

            curTarget = target;
            jumpTick = Find.TickManager.TicksGame + Props.warmupTime.SecondsToTicks();
        }
        else
        {
            DoJump(target, pawn);
        }
    }

    private void DoJump(LocalTargetInfo target, Pawn pawn)
    {
        effecter = null;
        curTarget = LocalTargetInfo.Invalid;
        jumpTick = -1;
        var map = parent.MapHeld;
        var intVec = target.Cell;
        if (pawn.Faction != Faction.OfPlayer && target.HasThing && Props.minLandDistanceToThingTarget.HasValue)
        {
            var source =
                from x in GenRadial.RadialCellsAround(target.Cell, Props.minLandDistanceToThingTarget.Value + 5f, true)
                where x.DistanceTo(target.Cell) > Props.minLandDistanceToThingTarget && JumpDistanceCheck(x) &&
                      GenSight.LineOfSight(x, target.Cell, map, false) && CanBeJumpedTo(x)
                select x;
            if (source.Any())
            {
                intVec = source.RandomElement();
            }
        }

        pawn.rotationTracker.FaceTarget(target);
        var val = (AbilityPawnFlyer)(PawnFlyer_Jump)PawnFlyer.MakeFlyer(RM_DefOf.RM_JumpPawn, pawn, intVec,
            Props.flightEffecterDef, Props.soundLanding);
        val.target = intVec;
        GenSpawn.Spawn(val, intVec, map);
        nextAllowedJumpTick = Find.TickManager.TicksGame + Props.jumpTickRateInterval.RandomInRange;
    }

    public override void CompTick()
    {
        base.CompTick();
        if (!curTarget.IsValid || jumpTick == -1)
        {
            return;
        }

        if (parent is not Pawn pawn)
        {
            return;
        }

        effecter?.EffectTick(pawn, curTarget.ToTargetInfo(pawn.Map));
        if (Find.TickManager.TicksGame >= jumpTick)
        {
            DoJump(curTarget, pawn);
        }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (parent.Faction != Faction.OfPlayer)
        {
            yield break;
        }

        var jumpButton = new Command_Action
        {
            defaultLabel = "RM.Jump".Translate(),
            defaultDesc = "RM.JumpDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Buttons/HarpyJump"),
            action = delegate
            {
                if (parent is not Pawn pawn)
                {
                    return;
                }

                var rTex = PortraitsCache.Get(pawn, ColonistBarColonistDrawer.PawnTextureSize, Rot4.South,
                    ColonistBarColonistDrawer.PawnTextureCameraOffset, 1.28205f);
                Find.Targeter.BeginTargeting(JumpTargetParameters(), DoJump, (Action<LocalTargetInfo>)Highlight,
                    (Func<LocalTargetInfo, bool>)CanJumpTo, null, null, GetTexture2D(rTex));
            },
            onHover = delegate { GenDraw.DrawRadiusRing(parent.Position, Props.maxJumpDistance); }
        };
        if (!JumpAllowed)
        {
            jumpButton.Disable("RM.OnCooldown".Translate((nextAllowedJumpTick - Find.TickManager.TicksGame)
                .ToStringTicksToPeriod()));
        }

        yield return jumpButton;
    }

    public Texture2D GetTexture2D(RenderTexture rTex)
    {
        var texture2D = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
        texture2D.Apply(false);
        Graphics.CopyTexture(rTex, texture2D);
        return texture2D;
    }

    private void Highlight(LocalTargetInfo target)
    {
        if (target.IsValid)
        {
            GenDraw.DrawTargetHighlight(target);
        }

        GenDraw.DrawRadiusRing(parent.Position, Props.maxJumpDistance);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref nextAllowedJumpTick, "CompPawnJumper_nextAllowedJumpTick");
        Scribe_TargetInfo.Look(ref curTarget, "CompPawnJumper_curTarget", LocalTargetInfo.Invalid);
        Scribe_Values.Look(ref jumpTick, "CompPawnJumper_jumpTick", -1);
    }
}
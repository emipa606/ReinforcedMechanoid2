using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace ReinforcedMechanoids
{
    public class CompProperties_PawnJumper : CompProperties
    {
        public IntRange jumpTickRateInterval;
        public float maxJumpDistance;
        public float minJumpDistance;
        public float? minLandDistanceToThingTarget;
        public EffecterDef flightEffecterDef;
        public SoundDef soundLanding;
        public float warmupTime;
        public EffecterDef warmupEffecter;
        public CompProperties_PawnJumper()
        {
            compClass = typeof(CompPawnJumper);
        }
    }

    public class CompPawnJumper : ThingComp
    {
        protected Effecter effecter;

        public LocalTargetInfo curTarget = LocalTargetInfo.Invalid;

        public int jumpTick = -1;

        public int nextAllowedJumpTick;
        public CompProperties_PawnJumper Props => base.props as CompProperties_PawnJumper;
        public override string CompInspectStringExtra()
        {
            var sb = new StringBuilder();
            int ticks = nextAllowedJumpTick - Find.TickManager.TicksGame;
            _ = ticks > 0
                ? sb.AppendLine("RM.CanJumpIn".Translate(ticks.ToStringTicksToPeriod()))
                : sb.AppendLine("RM.CanJumpNow".Translate());
            return sb.ToString().TrimEndNewlines();
        }
        public bool JumpAllowed => Find.TickManager.TicksGame >= nextAllowedJumpTick;
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
            if (parent.Spawned is false)
            {
                return false;
            }
            return CanBeJumpedTo(target.Cell);
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
                validator = (TargetInfo x) => CanJumpTo((LocalTargetInfo)x)
            };
        }
        public bool JumpDistanceCheck(IntVec3 cell)
        {
            float distance = parent.Position.DistanceTo(cell);
            if (distance > Props.maxJumpDistance || distance <= Props.minJumpDistance)
            {
                return false;
            }
            return true;
        }
        public bool CanBeJumpedTo(IntVec3 target)
        {
            var map = parent.Map;
            return !target.Roofed(map) && target.Walkable(map);
        }

        public void DoJump(LocalTargetInfo target)
        {
            var pawn = parent as Pawn;
            if (Props.warmupTime > 0)
            {
                var stance = new Stance_Wait(Props.warmupTime.SecondsToTicks(), target, pawn.CurrentEffectiveVerb)
                {
                    neverAimWeapon = true
                };
                pawn.stances.SetStance(stance);
                if (Props.warmupEffecter != null)
                {
                    effecter = Props.warmupEffecter.Spawn(pawn, pawn.Map);
                    effecter.Trigger(pawn, target.ToTargetInfo(pawn.Map));
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
            var cell = target.Cell;
            if (pawn.Faction != Faction.OfPlayer && target.HasThing && Props.minLandDistanceToThingTarget.HasValue)
            {
                var cells = GenRadial.RadialCellsAround(target.Cell, Props.minLandDistanceToThingTarget.Value + 5, true)
                    .Where(x => x.DistanceTo(target.Cell) > Props.minLandDistanceToThingTarget && JumpDistanceCheck(x)
                    && GenSight.LineOfSight(x, target.Cell, map) && CanBeJumpedTo(x));
                if (cells.Any())
                {
                    cell = cells.RandomElement();
                }
            }
            pawn.rotationTracker.FaceTarget(target);
            AbilityPawnFlyer flyer = (PawnFlyer_Jump)PawnFlyer.MakeFlyer(RM_DefOf.RM_JumpPawn, pawn, cell, Props.flightEffecterDef, Props.soundLanding);
            flyer.target = cell.ToVector3Shifted();
            var unused = GenSpawn.Spawn(flyer, cell, map);
            nextAllowedJumpTick = Find.TickManager.TicksGame + Props.jumpTickRateInterval.RandomInRange;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (curTarget.IsValid && jumpTick != -1)
            {
                var pawn = parent as Pawn;
                effecter?.EffectTick(pawn, curTarget.ToTargetInfo(pawn.Map));
                if (Find.TickManager.TicksGame >= jumpTick)
                {
                    DoJump(curTarget, pawn);
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction == Faction.OfPlayer)
            {
                var jumpButton = new Command_Action
                {
                    defaultLabel = "RM.Jump".Translate(),
                    defaultDesc = "RM.JumpDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/HarpyJump"),
                    action = delegate
                    {
                        var pawn = parent as Pawn;
                        var iconTex = PortraitsCache.Get(pawn, ColonistBarColonistDrawer.PawnTextureSize, Rot4.South,
                            ColonistBarColonistDrawer.PawnTextureCameraOffset, 1.28205f);
                        Find.Targeter.BeginTargeting(JumpTargetParameters(), DoJump, Highlight,
                        CanJumpTo, null, null, onGuiAction: null, mouseAttachment: GetTexture2D(iconTex));
                    },
                    onHover = delegate
                    {
                        GenDraw.DrawRadiusRing(parent.Position, Props.maxJumpDistance);
                    },
                };

                if (JumpAllowed is false)
                {
                    jumpButton.Disable("RM.OnCooldown".Translate((nextAllowedJumpTick - Find.TickManager.TicksGame).ToStringTicksToPeriod()));
                }
                yield return jumpButton;
            }
        }
        public Texture2D GetTexture2D(RenderTexture rTex)
        {
            var dest = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
            dest.Apply(false);
            Graphics.CopyTexture(rTex, dest);
            return dest;
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
}

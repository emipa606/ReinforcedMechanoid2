using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ReinforcedMechanoids;

public class Stance_Wait : Stance_Busy
{
    private Mote aimChargeMote;

    private Mote aimLineMote;

    private Mote aimTargetMote;

    private bool drawAimPie = true;

    private Effecter effecter;

    private bool needsReInitAfterLoad;
    private Sustainer sustainer;

    private bool targetStartedDowned;

    public Stance_Wait()
    {
    }

    public Stance_Wait(int ticks, LocalTargetInfo focusTarg, Verb verb)
        : base(ticks, focusTarg, verb)
    {
        if (focusTarg is { HasThing: true, Thing: Verse.Pawn })
        {
            var pawn = (Pawn)focusTarg.Thing;
            targetStartedDowned = pawn.Downed;
            if (pawn.apparel != null)
            {
                for (var i = 0; i < pawn.apparel.WornApparelCount; i++)
                {
                    var allComps = pawn.apparel.WornApparel[i].AllComps;
                    foreach (var thingComp in allComps)
                    {
                        if (thingComp is CompShield compShield)
                        {
                            compShield.KeepDisplaying();
                        }
                    }
                }
            }
        }

        InitEffects();
        drawAimPie = false;
    }

    private void InitEffects(bool afterReload = false)
    {
        if (verb == null)
        {
            return;
        }

        var verbProps = verb.verbProps;
        if (verbProps.soundAiming != null)
        {
            var info = SoundInfo.InMap(verb.caster, MaintenanceType.PerTick);
            if (verb.CasterIsPawn)
            {
                info.pitchFactor = 1f / verb.CasterPawn.GetStatValue(StatDefOf.AimingDelayFactor);
            }

            sustainer = verbProps.soundAiming.TrySpawnSustainer(info);
        }

        if (verbProps.warmupEffecter != null && verb.Caster != null)
        {
            effecter = verbProps.warmupEffecter.Spawn(verb.Caster, verb.Caster.Map);
            effecter.Trigger((TargetInfo)verb.Caster, focusTarg.ToTargetInfo(verb.Caster.Map));
        }

        if (verb.Caster == null)
        {
            return;
        }

        var map = verb.Caster.Map;
        if (verbProps.aimingLineMote != null)
        {
            var vector = TargetPos();
            var cell = vector.ToIntVec3();
            aimLineMote = MoteMaker.MakeInteractionOverlay(verbProps.aimingLineMote, verb.Caster,
                new TargetInfo(cell, map), Vector3.zero, vector - cell.ToVector3Shifted());
            if (afterReload)
            {
                aimLineMote?.ForceSpawnTick(startedTick);
            }
        }

        if (verbProps.aimingChargeMote != null)
        {
            aimChargeMote = MoteMaker.MakeStaticMote(verb.Caster.DrawPos, map, verbProps.aimingChargeMote, 1f, true);
            if (afterReload)
            {
                aimChargeMote?.ForceSpawnTick(startedTick);
            }
        }

        if (verbProps.aimingTargetMote == null)
        {
            return;
        }

        aimTargetMote = MoteMaker.MakeStaticMote(focusTarg.CenterVector3, map, verbProps.aimingTargetMote, 1f, true);
        if (aimTargetMote == null)
        {
            return;
        }

        aimTargetMote.exactRotation = AimDir().ToAngleFlat();
        if (afterReload)
        {
            aimTargetMote.ForceSpawnTick(startedTick);
        }
    }

    private Vector3 TargetPos()
    {
        var verbProps = verb.verbProps;
        var result = focusTarg.CenterVector3;
        if (verbProps.aimingLineMoteFixedLength.HasValue)
        {
            result = verb.Caster.DrawPos + (AimDir() * verbProps.aimingLineMoteFixedLength.Value);
        }

        return result;
    }

    private Vector3 AimDir()
    {
        var result = focusTarg.CenterVector3 - verb.Caster.DrawPos;
        result.y = 0f;
        result.Normalize();
        return result;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref targetStartedDowned, "targetStartDowned");
        Scribe_Values.Look(ref drawAimPie, "drawAimPie");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            needsReInitAfterLoad = true;
        }
    }

    public override void StanceDraw()
    {
        if (drawAimPie && Find.Selector.IsSelected(stanceTracker.pawn))
        {
            GenDraw.DrawAimPie(stanceTracker.pawn, focusTarg, (int)(ticksLeft * pieSizeFactor), 0.2f);
        }
    }

    public override void StanceTick()
    {
        if (needsReInitAfterLoad)
        {
            InitEffects(true);
            needsReInitAfterLoad = false;
        }

        if (sustainer is { Ended: false })
        {
            sustainer.Maintain();
        }

        effecter?.EffectTick(verb.Caster, focusTarg.ToTargetInfo(verb.Caster.Map));
        var vector = AimDir();
        var exactRotation = vector.AngleFlat();
        var stunned = stanceTracker.stunner.Stunned;
        if (aimLineMote != null)
        {
            aimLineMote.paused = stunned;
            aimLineMote.Maintain();
            var vector2 = TargetPos();
            var cell = vector2.ToIntVec3();
            ((MoteDualAttached)aimLineMote).UpdateTargets(verb.Caster, new TargetInfo(cell, verb.Caster.Map),
                Vector3.zero, vector2 - cell.ToVector3Shifted());
        }

        if (aimTargetMote != null)
        {
            aimTargetMote.paused = stunned;
            aimTargetMote.exactPosition = focusTarg.CenterVector3;
            aimTargetMote.exactRotation = exactRotation;
            aimTargetMote.Maintain();
        }

        if (aimChargeMote != null)
        {
            aimChargeMote.paused = stunned;
            aimChargeMote.exactRotation = exactRotation;
            aimChargeMote.exactPosition = verb.Caster.Position.ToVector3Shifted() +
                                          (vector * verb.verbProps.aimingChargeMoteOffset);
            aimChargeMote.Maintain();
        }

        base.StanceTick();
    }

    public void Interrupt()
    {
        base.Expire();
        effecter?.Cleanup();
    }

    public override void Expire()
    {
        verb?.WarmupComplete();
        effecter?.Cleanup();
        base.Expire();
    }
}
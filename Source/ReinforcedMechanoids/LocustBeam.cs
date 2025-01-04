using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ReinforcedMechanoids;

public class LocustBeam : OrbitalStrike
{
    private static readonly List<Thing> tmpThings = [];

    private Sustainer sustainer;

    public BeamExtension Props => def.GetModExtension<BeamExtension>();

    public override void StartStrike()
    {
        angle = AngleRange.RandomInRange;
        startTick = Find.TickManager.TicksGame;
        MakeLocustBeamMote(Position, Map);
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
        var mote = (Mote)ThingMaker.MakeThing(RM_DefOf.RM_Mote_LocustBeam);
        mote.exactPosition = cell.ToVector3Shifted();
        mote.Scale = Props.radius * 6f;
        mote.rotationRate = 1.2f;
        GenSpawn.Spawn(mote, cell, map);
    }

    public override void Tick()
    {
        base.Tick();
        if (instigator is Pawn pawn &&
            (pawn.stances.curStance is Stance_Mobile || pawn.Downed || pawn.Dead || pawn.Destroyed))
        {
            Destroy();
        }

        if (!Destroyed)
        {
            for (var i = 0; i < Props.firesStartedPerTick; i++)
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
        var intVec = (from x in GenRadial.RadialCellsAround(Position, Props.radius, true)
            where x.InBounds(Map)
            select x).RandomElementByWeight(x => 1f - Mathf.Min(x.DistanceTo(Position) / Props.radius, 1f) + 0.05f);
        FireUtility.TryStartFireIn(intVec, Map, Rand.Range(0.1f, 0.925f), this);
        tmpThings.Clear();
        tmpThings.AddRange(intVec.GetThingList(Map));
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < tmpThings.Count; i++)
        {
            var num = tmpThings[i] is Corpse
                ? Props.corpseFlameDamageAmountRange.RandomInRange
                : Props.flameDamageAmountRange.RandomInRange;
            var pawn = tmpThings[i] as Pawn;
            BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = null;
            if (pawn != null)
            {
                if (pawn.RaceProps.IsMechanoid)
                {
                    continue;
                }

                battleLogEntry_DamageTaken =
                    new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_PowerBeam, instigator as Pawn);
                Find.BattleLog.Add(battleLogEntry_DamageTaken);
            }

            tmpThings[i].TakeDamage(new DamageInfo(DamageDefOf.Flame, num, 0f, -1f, instigator, null, weaponDef))
                .AssociateWithLog(battleLogEntry_DamageTaken);
        }

        tmpThings.Clear();
    }
}
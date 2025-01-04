using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace GestaltEngine;

[HotSwappable]
public class CompGestaltEngine : CompUpgradeable
{
    public static readonly HashSet<CompGestaltEngine> compGestaltEngines = [];

    protected Effecter connectMechEffecter;

    protected Effecter connectProgressBarEffecter;

    public int connectTick = -1;

    public LocalTargetInfo curTarget = LocalTargetInfo.Invalid;

    public Pawn dummyPawn;

    public int hackCooldownTicks;

    public bool MechanitorActive => compPower.PowerOn && dummyPawn.mechanitor.TotalBandwidth > 0;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        compGestaltEngines.Add(this);
        base.PostSpawnSetup(respawningAfterLoad);
        if (dummyPawn == null)
        {
            dummyPawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Mechanitor_Basic, Faction.OfAncients);
            dummyPawn.SetFactionDirect(parent.Faction);
            dummyPawn.Name = new NameSingle(parent.LabelCap);
        }

        PawnComponentsUtility.AddComponentsForSpawn(dummyPawn);
        PawnComponentsUtility.AddAndRemoveDynamicComponents(dummyPawn);
        dummyPawn.mechanitor.Notify_BandwidthChanged();
        dummyPawn.mechanitor.Notify_ControlGroupAmountMayChanged();
        dummyPawn.gender = Gender.None;
        dummyPawn.equipment.DestroyAllEquipment();
        dummyPawn.story.title = "";
    }

    public override void PostDeSpawn(Map map)
    {
        compGestaltEngines.Remove(this);
        base.PostDeSpawn(map);
    }

    public override void ReceiveCompSignal(string signal)
    {
        if (signal == "PowerTurnedOn" || signal == "PowerTurnedOff")
        {
            dummyPawn.mechanitor.Notify_BandwidthChanged();
        }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var item in base.CompGetGizmosExtra())
        {
            yield return item;
        }

        if (parent.Faction != Faction.OfPlayer)
        {
            yield break;
        }

        foreach (var i in dummyPawn.mechanitor.GetGizmos())
        {
            if (i is not Command_CallBossgroup)
            {
                yield return i;
            }
        }

        var connectMech = new Command_Action
        {
            defaultLabel = "RM.ConnectMechanoid".Translate(),
            defaultDesc = "RM.ConnectMechanoidDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Buttons/ConnectMechanoid"),
            action = delegate
            {
                Find.Targeter.BeginTargeting(ConnectMechanoidTargetParameters(),
                    (Action<LocalTargetInfo>)StartConnect, (Action<LocalTargetInfo>)Highlight,
                    (Func<LocalTargetInfo, bool>)CanConnect);
            }
        };
        var hackMech = new Command_Action
        {
            defaultLabel = "RM.HackMechanoid".Translate(),
            defaultDesc = "RM.HackMechanoidDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Buttons/HackMechanoid"),
            action = delegate
            {
                Find.Targeter.BeginTargeting(ConnectNonColonyMechanoidTargetParameters(),
                    (Action<LocalTargetInfo>)StartConnectNonColonyMech, (Action<LocalTargetInfo>)Highlight,
                    (Func<LocalTargetInfo, bool>)CanConnectNonColonyMech);
            }
        };
        if (curTarget.IsValid)
        {
            connectMech.Disable("RM.BusyConnectingMechanoid".Translate());
            hackMech.Disable("RM.BusyConnectingMechanoid".Translate());
        }
        else if (!MechanitorActive)
        {
            connectMech.Disable("RM.IncapableOfConnectingMechanoid".Translate());
            hackMech.Disable("RM.IncapableOfConnectingMechanoid".Translate());
        }

        if (!hackMech.disabled && hackCooldownTicks > Find.TickManager.TicksGame)
        {
            hackMech.Disable(
                "RM.OnCooldown".Translate((hackCooldownTicks - Find.TickManager.TicksGame).ToStringTicksToPeriod()));
        }

        yield return connectMech;
        yield return hackMech;
    }

    public TargetingParameters ConnectMechanoidTargetParameters()
    {
        return new TargetingParameters
        {
            canTargetPawns = true,
            canTargetBuildings = false,
            canTargetHumans = false,
            canTargetMechs = true,
            canTargetAnimals = false,
            canTargetLocations = false,
            validator = x => CanConnect((LocalTargetInfo)x)
        };
    }

    public TargetingParameters ConnectNonColonyMechanoidTargetParameters()
    {
        return new TargetingParameters
        {
            canTargetPawns = true,
            canTargetBuildings = false,
            canTargetHumans = false,
            canTargetMechs = true,
            canTargetAnimals = false,
            canTargetLocations = false,
            validator = x => CanConnectNonColonyMech((LocalTargetInfo)x)
        };
    }

    private int MechControlTime(Pawn mech)
    {
        return Mathf.RoundToInt(mech.GetStatValue(StatDefOf.ControlTakingTime) * 60f);
    }

    public bool CanConnect(LocalTargetInfo target)
    {
        var pawn = target.Pawn;
        return pawn != null && CanControlMech(dummyPawn, pawn);
    }

    public bool CanConnectNonColonyMech(LocalTargetInfo target)
    {
        var pawn = target.Pawn;
        return pawn != null && CanControlNonColonyMech(dummyPawn, pawn) && HasEnoughBandwidth(dummyPawn, pawn) &&
               pawn.Faction != parent.Faction;
    }

    public static AcceptanceReport CanControlMech(Pawn pawn, Pawn mech)
    {
        if (pawn.mechanitor == null || !mech.IsColonyMech || mech.Downed || mech.Dead || mech.IsAttacking())
        {
            return false;
        }

        if (!MechanitorUtility.EverControllable(mech))
        {
            return "CannotControlMechNeverControllable".Translate();
        }

        if (mech.GetOverseer() == pawn)
        {
            return "CannotControlMechAlreadyControlled".Translate(pawn.LabelShort);
        }

        return true;
    }

    public static AcceptanceReport CanControlNonColonyMech(Pawn pawn, Pawn mech)
    {
        if (pawn.mechanitor == null || mech.Downed || mech.Dead)
        {
            return false;
        }

        if (!MechanitorUtility.EverControllable(mech))
        {
            return "CannotControlMechNeverControllable".Translate();
        }

        if (mech.GetOverseer() == pawn)
        {
            return "CannotControlMechAlreadyControlled".Translate(pawn.LabelShort);
        }

        return true;
    }

    public bool HasEnoughBandwidth(Pawn pawn, Pawn mech)
    {
        var num = pawn.mechanitor.TotalBandwidth - pawn.mechanitor.UsedBandwidth;
        var statValue = mech.GetStatValue(StatDefOf.BandwidthCost);
        return !(num < statValue);
    }

    public void StartConnect(LocalTargetInfo target)
    {
        curTarget = target;
        connectTick = Find.TickManager.TicksGame + MechControlTime(curTarget.Pawn);
        PawnUtility.ForceWait(curTarget.Pawn, MechControlTime(curTarget.Pawn), parent, true);
        if (!HasEnoughBandwidth(dummyPawn, curTarget.Pawn))
        {
            Messages.Message("RM.NotEnoughBandwidth".Translate(), curTarget.Pawn, MessageTypeDefOf.CautionInput);
        }
    }

    public void StartConnectNonColonyMech(LocalTargetInfo target)
    {
        curTarget = target;
        var num = MechControlTime(curTarget.Pawn) * 2;
        connectTick = Find.TickManager.TicksGame + num;
    }

    protected override void SetLevel()
    {
        base.SetLevel();
        dummyPawn.mechanitor.Notify_BandwidthChanged();
        dummyPawn.mechanitor.Notify_ControlGroupAmountMayChanged();
    }

    public override void CompTick()
    {
        base.CompTick();
        if (dummyPawn.Faction != parent.Faction)
        {
            dummyPawn.SetFaction(parent.Faction);
        }

        if (curTarget.IsValid && connectTick != -1)
        {
            var pawn = curTarget.Pawn;
            if (pawn.Faction == dummyPawn.Faction && !CanConnect(pawn) ||
                pawn.Faction != dummyPawn.Faction && !CanConnectNonColonyMech(pawn) || !MechanitorActive)
            {
                Reset();
            }
            else
            {
                ConnectEffects(pawn);
                if (Find.TickManager.TicksGame >= connectTick)
                {
                    Connect(curTarget, dummyPawn);
                }
            }
        }

        if (!dummyPawn.IsHashIntervalTick(60))
        {
            return;
        }

        foreach (var item in dummyPawn.health.hediffSet.hediffs.OfType<Hediff_BandNode>())
        {
            item.RecacheBandNodes();
        }
    }

    private void ConnectEffects(Pawn mech)
    {
        if (connectProgressBarEffecter == null)
        {
            connectProgressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
        }

        connectProgressBarEffecter.EffectTick(parent, TargetInfo.Invalid);
        var mote = ((SubEffecter_ProgressBar)connectProgressBarEffecter.children[0]).mote;
        mote.progress = 1f - ((connectTick - Find.TickManager.TicksGame) /
                              (mech.Faction != parent.Faction ? MechControlTime(mech) * 2f : MechControlTime(mech)));
        mote.offsetZ = -0.8f;
        if (connectMechEffecter == null)
        {
            connectMechEffecter = EffecterDefOf.ControlMech.Spawn();
            connectMechEffecter.Trigger((TargetInfo)parent, (TargetInfo)mech);
        }

        connectMechEffecter.EffectTick(parent, mech);
    }

    private void Connect(LocalTargetInfo target, Pawn pawn)
    {
        Reset();
        var pawn2 = target.Pawn;
        if (pawn2.Faction != dummyPawn.Faction)
        {
            pawn2.SetFaction(dummyPawn.Faction);
            hackCooldownTicks = Find.TickManager.TicksGame + 300000;
        }

        pawn2.GetOverseer()?.relations.RemoveDirectRelation(PawnRelationDefOf.Overseer, pawn2);
        pawn.relations.AddDirectRelation(PawnRelationDefOf.Overseer, pawn2);
    }

    private void Reset()
    {
        connectProgressBarEffecter.Cleanup();
        connectProgressBarEffecter = null;
        connectMechEffecter.Cleanup();
        connectMechEffecter = null;
        if (curTarget.Thing is Pawn { Dead: false } pawn)
        {
            pawn.jobs.StopAll();
            pawn.pather.StopDead();
        }

        curTarget = LocalTargetInfo.Invalid;
        connectTick = -1;
    }

    private void Highlight(LocalTargetInfo target)
    {
        if (target.IsValid)
        {
            GenDraw.DrawTargetHighlight(target);
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Deep.Look(ref dummyPawn, "dummyPawn");
        Scribe_TargetInfo.Look(ref curTarget, "CompGestaltEngine_curTarget", LocalTargetInfo.Invalid);
        Scribe_Values.Look(ref connectTick, "CompGestaltEngine_connectTick", -1);
        Scribe_Values.Look(ref hackCooldownTicks, "CompGestaltEngine_hackCooldownTicks");
    }
}
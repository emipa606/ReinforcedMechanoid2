using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace GestaltEngine
{
    [HotSwappable]
    public class CompGestaltEngine : CompUpgradeable
    {
        public static HashSet<CompGestaltEngine> compGestaltEngines = new();
        public Pawn dummyPawn;
        protected Effecter connectProgressBarEffecter;
        protected Effecter connectMechEffecter;
        public int hackCooldownTicks;
        public LocalTargetInfo curTarget = LocalTargetInfo.Invalid;
        public int connectTick = -1;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            _ = compGestaltEngines.Add(this);
            base.PostSpawnSetup(respawningAfterLoad);
            if (dummyPawn is null)
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
            _ = compGestaltEngines.Remove(this);
            base.PostDeSpawn(map);
        }

        public override void ReceiveCompSignal(string signal)
        {
            switch (signal)
            {
                case "PowerTurnedOn":
                case "PowerTurnedOff":
                    dummyPawn.mechanitor.Notify_BandwidthChanged();
                    break;
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra())
            {
                yield return g;
            }
            if (parent.Faction == Faction.OfPlayer)
            {
                foreach (var m in dummyPawn.mechanitor.GetGizmos())
                {
                    if (m is Command_CallBossgroup)
                    {
                        continue;
                    }
                    yield return m;
                }
                var connectMech = new Command_Action
                {
                    defaultLabel = "RM.ConnectMechanoid".Translate(),
                    defaultDesc = "RM.ConnectMechanoidDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/ConnectMechanoid"),
                    action = delegate
                    {
                        Find.Targeter.BeginTargeting(ConnectMechanoidTargetParameters(), StartConnect, Highlight,
                            CanConnect, null, null, onGuiAction: null, mouseAttachment: null);
                    }
                };
                var hackMech = new Command_Action
                {
                    defaultLabel = "RM.HackMechanoid".Translate(),
                    defaultDesc = "RM.HackMechanoidDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/HackMechanoid"),
                    action = delegate
                    {
                        Find.Targeter.BeginTargeting(ConnectNonColonyMechanoidTargetParameters(), StartConnectNonColonyMech, Highlight,
                            CanConnectNonColonyMech, null, null, onGuiAction: null, mouseAttachment: null);
                    }
                };
                if (curTarget.IsValid)
                {
                    connectMech.Disable("RM.BusyConnectingMechanoid".Translate());
                    hackMech.Disable("RM.BusyConnectingMechanoid".Translate());
                }
                else if (MechanitorActive is false)
                {
                    connectMech.Disable("RM.IncapableOfConnectingMechanoid".Translate());
                    hackMech.Disable("RM.IncapableOfConnectingMechanoid".Translate());
                }

                if (hackMech.disabled is false && hackCooldownTicks > Find.TickManager.TicksGame)
                {
                    hackMech.Disable("RM.OnCooldown".Translate((hackCooldownTicks - Find.TickManager.TicksGame).ToStringTicksToPeriod()));
                }
                yield return connectMech;
                yield return hackMech;
            }
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
                validator = (TargetInfo x) => CanConnect((LocalTargetInfo)x)
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
                validator = (TargetInfo x) => CanConnectNonColonyMech((LocalTargetInfo)x)
            };
        }
        public bool MechanitorActive => compPower.PowerOn && dummyPawn.mechanitor.TotalBandwidth > 0;
        private int MechControlTime(Pawn mech)
        {
            return Mathf.RoundToInt(mech.GetStatValue(StatDefOf.ControlTakingTime) * 60f);
        }

        public bool CanConnect(LocalTargetInfo target)
        {
            var mech = target.Pawn;
            if (mech != null && CanControlMech(dummyPawn, mech))
            {
                return true;
            }
            return false;
        }

        public bool CanConnectNonColonyMech(LocalTargetInfo target)
        {
            var mech = target.Pawn;
            if (mech != null && CanControlNonColonyMech(dummyPawn, mech) && HasEnoughBandwidth(dummyPawn, mech)
                && mech.Faction != parent.Faction)
            {
                return true;
            }
            return false;
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
            int num = pawn.mechanitor.TotalBandwidth - pawn.mechanitor.UsedBandwidth;
            float statValue = mech.GetStatValue(StatDefOf.BandwidthCost);
            if (num < statValue)
            {
                return false;
            }
            return true;
        }
        public void StartConnect(LocalTargetInfo target)
        {
            curTarget = target;
            connectTick = Find.TickManager.TicksGame + MechControlTime(curTarget.Pawn);
            PawnUtility.ForceWait(curTarget.Pawn, MechControlTime(curTarget.Pawn), parent, maintainPosture: true, maintainSleep: false);
            if (!HasEnoughBandwidth(dummyPawn, curTarget.Pawn))
            {
                Messages.Message("RM.NotEnoughBandwidth".Translate(), curTarget.Pawn, MessageTypeDefOf.CautionInput);
            }
        }

        public void StartConnectNonColonyMech(LocalTargetInfo target)
        {
            curTarget = target;
            int connectPeriod = MechControlTime(curTarget.Pawn) * 2;
            connectTick = Find.TickManager.TicksGame + connectPeriod;
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
                var mech = curTarget.Pawn;
                if ((mech.Faction == dummyPawn.Faction && !CanConnect(mech))
                    || (mech.Faction != dummyPawn.Faction && !CanConnectNonColonyMech(mech))
                    || MechanitorActive is false)
                {
                    Reset();
                }
                else
                {
                    ConnectEffects(mech);
                    if (Find.TickManager.TicksGame >= connectTick)
                    {
                        Connect(curTarget, dummyPawn);
                    }
                }
            }
            if (dummyPawn.IsHashIntervalTick(60))
            {
                foreach (var hediff in dummyPawn.health.hediffSet.hediffs.OfType<Hediff_BandNode>())
                {
                    hediff.RecacheBandNodes();
                }
            }
        }

        private void ConnectEffects(Pawn mech)
        {
            connectProgressBarEffecter ??= EffecterDefOf.ProgressBar.Spawn();
            connectProgressBarEffecter.EffectTick(parent, TargetInfo.Invalid);
            var mote = ((SubEffecter_ProgressBar)connectProgressBarEffecter.children[0]).mote;
            mote.progress = 1f - (connectTick - Find.TickManager.TicksGame) / (float)(mech.Faction != parent.Faction
                ? MechControlTime(mech) * 2f : MechControlTime(mech));
            mote.offsetZ = -0.8f;

            if (connectMechEffecter == null)
            {
                connectMechEffecter = EffecterDefOf.ControlMech.Spawn();
                connectMechEffecter.Trigger(parent, mech);
            }
            connectMechEffecter.EffectTick(parent, mech);
        }

        private void Connect(LocalTargetInfo target, Pawn pawn)
        {
            Reset();
            var mech = target.Pawn;
            if (mech.Faction != dummyPawn.Faction)
            {
                mech.SetFaction(dummyPawn.Faction);
                hackCooldownTicks = Find.TickManager.TicksGame + (GenDate.TicksPerDay * 5);
            }
            mech.GetOverseer()?.relations.RemoveDirectRelation(PawnRelationDefOf.Overseer, mech);
            pawn.relations.AddDirectRelation(PawnRelationDefOf.Overseer, mech);
        }

        private void Reset()
        {
            connectProgressBarEffecter.Cleanup();
            connectProgressBarEffecter = null;
            connectMechEffecter.Cleanup();
            connectMechEffecter = null;
            if (curTarget.Thing is Pawn pawn && pawn.Dead is false)
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
}

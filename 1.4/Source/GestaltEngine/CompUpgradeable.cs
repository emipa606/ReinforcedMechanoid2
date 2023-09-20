using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GestaltEngine
{
    public class Upgrade
    {
        public int upgradeCooldownTicks, downgradeCooldownTicks = -1;
        public int upgradeDurationTicks, downgradeDurationTicks = -1;
        public float powerConsumption;
        public float heatPerSecond;
        public float researchPointsPerSecond;
        public GraphicData overlayGraphic;
        public List<RecipeDef> unlockedRecipes = new List<RecipeDef>();
        public int totalMechBandwidth;
        public int totalControlGroups;
    }
    public class CompProperties_Upgradeable : CompProperties
    {
        public List<Upgrade> upgrades;
        public CompProperties_Upgradeable()
        {
            this.compClass = typeof(CompUpgradeable);
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }

    [HotSwappable]
    public class CompUpgradeable : ThingComp
    {
        public int level;
        public int upgradeOffset;
        public int upgradeProgressTick = -1;
        public int downgradeProgressTick = -1;
        public int cooldownPeriod;
        public CompProperties_Upgradeable Props => base.props as CompProperties_Upgradeable;
        public int MinLevel => 0;
        public int MaxLevel => Props.upgrades.Count - 1;
        public Upgrade CurrentUpgrade => Props.upgrades[level];
        public bool Upgrading => upgradeOffset > 0;
        public bool Downgrading => upgradeOffset < 0;
        public bool OnCooldown => cooldownPeriod > Find.TickManager.TicksGame;

        protected Effecter progressBarEffecter;

        public CompPowerTrader compPower;

        public Graphic cachedGraphic;
        public Graphic OverlayGraphic
        {
            get
            {
                if (cachedGraphic == null)
                {
                    cachedGraphic = CurrentUpgrade.overlayGraphic.GraphicColoredFor(this.parent);
                }
                return cachedGraphic;
            }
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compPower = this.parent.TryGetComp<CompPowerTrader>();
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            progressBarEffecter?.Cleanup();
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (CurrentUpgrade.overlayGraphic != null)
            {
                var vector = this.parent.DrawPos + Altitudes.AltIncVect;
                vector.y += 5;
                OverlayGraphic.Draw(vector, this.parent.Rotation, this.parent);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (this.parent.Faction == Faction.OfPlayer)
            {
                var upgrade = new Command_Action
                {
                    defaultLabel = "RM.Upgrade".Translate(),
                    defaultDesc = "RM.UpgradeDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/GestaltUpgrade"),
                    action = delegate
                    {
                        StartUpgrade(1);
                    }
                };

                var downgrade = new Command_Action
                {
                    defaultLabel = "RM.Downgrade".Translate(),
                    defaultDesc = "RM.DowngradeDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/GestaltDowngrade"),
                    action = delegate
                    {
                        StartUpgrade(-1);
                    }
                };

                var upgradeInstant = new Command_Action
                {
                    defaultLabel = "DEV: Instant upgrade",
                    defaultDesc = "RM.UpgradeDesc".Translate(),
                    action = delegate
                    {
                        StartUpgrade(1);
                        SetLevel();
                    }
                };

                var downgradeInstant = new Command_Action
                {
                    defaultLabel = "DEV: Instant downgrade",
                    defaultDesc = "RM.DowngradeDesc".Translate(),
                    action = delegate
                    {
                        StartUpgrade(-1);
                        SetLevel();
                    }
                };

                if (this.compPower.PowerOn is false)
                {
                    upgrade.Disable("NoPower".Translate());
                    downgrade.Disable("NoPower".Translate());
                }
                else if (Upgrading)
                {
                    upgrade.Disable("RM.Upgrading".Translate());
                    downgrade.Disable("RM.Upgrading".Translate());
                }
                else if (Downgrading)
                {
                    upgrade.Disable("RM.Downgrading".Translate());
                    downgrade.Disable("RM.Downgrading".Translate());
                }
                else if (OnCooldown)
                {
                    upgrade.Disable("RM.OnCooldown".Translate((cooldownPeriod - Find.TickManager.TicksGame).ToStringTicksToPeriod()));
                    downgrade.Disable("RM.OnCooldown".Translate((cooldownPeriod - Find.TickManager.TicksGame).ToStringTicksToPeriod()));
                }
                
                if (level == MinLevel)
                {
                    downgrade.disabled = true;
                    downgradeInstant.disabled = true;
                }
                else if (level == MaxLevel)
                {
                    upgrade.disabled = true;
                    upgradeInstant.disabled = true;
                }
                yield return upgrade;
                yield return downgrade;
                if (Prefs.DevMode)
                {
                    yield return upgradeInstant;
                    yield return downgradeInstant;
                }
            }
        }
        public override string CompInspectStringExtra()
        {
            var sb = new StringBuilder();
            sb.AppendLine("RM.Level".Translate(level));
            if (Upgrading)
            {
                var progress = upgradeProgressTick / (float)CurrentUpgrade.upgradeDurationTicks;
                sb.AppendLine("RM.UpgradeInProcess".Translate(progress.ToStringPercent()));
            }
            else if (Downgrading)
            {
                var progress = downgradeProgressTick / (float)CurrentUpgrade.downgradeDurationTicks;
                sb.AppendLine("RM.DowngradeInProcess".Translate(progress.ToStringPercent()));
            }
            return sb.ToString().TrimEndNewlines();
        }
        public void StartUpgrade(int upgradeOffset)
        {
            SoundDefOf.DragSlider.PlayOneShotOnCamera();
            this.upgradeOffset = upgradeOffset;
            if (upgradeOffset < 0)
            {
                this.downgradeProgressTick = 0;
            }
            else if (upgradeOffset > 0)
            {
                this.upgradeProgressTick = 0;
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            if (CurrentUpgrade.powerConsumption != 0)
            {
                compPower.powerOutputInt = -CurrentUpgrade.powerConsumption;
            }
            if (IsActive)
            {
                if (Upgrading)
                {
                    upgradeProgressTick++;

                    if (progressBarEffecter == null)
                    {
                        progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
                    }
                    progressBarEffecter.EffectTick(this.parent, TargetInfo.Invalid);
                    MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
                    mote.progress = upgradeProgressTick / (float)CurrentUpgrade.upgradeDurationTicks;
                    mote.offsetZ = -0.8f;

                    if (upgradeProgressTick >= CurrentUpgrade.upgradeDurationTicks)
                    {
                        SetLevel();
                    }
                }
                else if (Downgrading)
                {
                    downgradeProgressTick++;
                    if (progressBarEffecter == null)
                    {
                        progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
                    }
                    progressBarEffecter.EffectTick(this.parent, TargetInfo.Invalid);
                    MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
                    mote.progress = downgradeProgressTick / (float)CurrentUpgrade.downgradeDurationTicks;
                    mote.offsetZ = -0.8f;

                    if (downgradeProgressTick >= CurrentUpgrade.downgradeDurationTicks)
                    {
                        SetLevel();
                    }
                }
                DoWork();
            }
        }
        protected virtual void DoWork()
        {
            if (this.parent.IsHashIntervalTick(60))
            {
                if (CurrentUpgrade.heatPerSecond != 0)
                {
                    GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, CurrentUpgrade.heatPerSecond);
                }
                if (CurrentUpgrade.researchPointsPerSecond != 0)
                {
                    var proj = Find.ResearchManager.currentProj;
                    if (proj != null)
                    {
                        Dictionary<ResearchProjectDef, float> dictionary = Find.ResearchManager.progress;
                        if (dictionary.ContainsKey(proj))
                        {
                            dictionary[proj] += CurrentUpgrade.researchPointsPerSecond;
                        }
                        else
                        {
                            dictionary[proj] = CurrentUpgrade.researchPointsPerSecond;
                        }
                        if (proj.IsFinished)
                        {
                            Find.ResearchManager.FinishProject(proj, doCompletionDialog: true);
                        }
                    }
                }
            }
        }
        protected virtual bool IsActive => compPower != null && compPower.PowerOn || true;
        protected virtual void SetLevel()
        {
            if (upgradeOffset < 0)
            {
                cooldownPeriod = Find.TickManager.TicksGame + CurrentUpgrade.downgradeCooldownTicks;
                Messages.Message("RM.FinishedDowngrade".Translate(this.parent.LabelCap), this.parent, MessageTypeDefOf.NeutralEvent);
            }
            else if (upgradeOffset > 0)
            {
                cooldownPeriod = Find.TickManager.TicksGame + CurrentUpgrade.upgradeCooldownTicks;
                Messages.Message("RM.FinishedUpgrade".Translate(this.parent.LabelCap), this.parent, MessageTypeDefOf.NeutralEvent);
            }

            level += upgradeOffset;
            upgradeOffset = 0;
            downgradeProgressTick = upgradeProgressTick = -1;
            compPower.powerOutputInt = -CurrentUpgrade.powerConsumption;
            cachedGraphic = null;
            if (progressBarEffecter != null)
            {
                progressBarEffecter.Cleanup();
                progressBarEffecter = null;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref level, "level");
            Scribe_Values.Look(ref upgradeOffset, "upgradeOffset");
            Scribe_Values.Look(ref upgradeProgressTick, "upgradeProgressTick", -1);
            Scribe_Values.Look(ref downgradeProgressTick, "downgradeProgressTick", -1);
            Scribe_Values.Look(ref cooldownPeriod, "cooldownPeriod", -1);
        }
    }
}

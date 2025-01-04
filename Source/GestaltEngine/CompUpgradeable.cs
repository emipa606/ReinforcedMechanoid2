using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GestaltEngine;

[HotSwappable]
public class CompUpgradeable : ThingComp
{
    public Graphic cachedGraphic;

    public CompPowerTrader compPower;

    public int cooldownPeriod;

    public int downgradeProgressTick = -1;
    public int level;

    protected Effecter progressBarEffecter;

    public int upgradeOffset;

    public int upgradeProgressTick = -1;

    public CompProperties_Upgradeable Props => props as CompProperties_Upgradeable;

    public int MinLevel => 0;

    public int MaxLevel => Props.upgrades.Count - 1;

    public Upgrade CurrentUpgrade => Props.upgrades[level];

    public bool Upgrading => upgradeOffset > 0;

    public bool Downgrading => upgradeOffset < 0;

    public bool OnCooldown => cooldownPeriod > Find.TickManager.TicksGame;

    public Graphic OverlayGraphic
    {
        get
        {
            if (cachedGraphic == null)
            {
                cachedGraphic = CurrentUpgrade.overlayGraphic.GraphicColoredFor(parent);
            }

            return cachedGraphic;
        }
    }

    protected virtual bool IsActive => compPower is { PowerOn: true };

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        compPower = parent.TryGetComp<CompPowerTrader>();
    }

    public override void PostDeSpawn(Map map)
    {
        base.PostDeSpawn(map);
        progressBarEffecter?.Cleanup();
    }

    public override void PostDraw()
    {
        base.PostDraw();
        if (CurrentUpgrade.overlayGraphic == null)
        {
            return;
        }

        var loc = parent.DrawPos + Altitudes.AltIncVect;
        loc.y += 5f;
        OverlayGraphic.Draw(loc, parent.Rotation, parent);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (parent.Faction != Faction.OfPlayer)
        {
            yield break;
        }

        var upgrade = new Command_Action
        {
            defaultLabel = "RM.Upgrade".Translate(),
            defaultDesc = "RM.UpgradeDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Buttons/GestaltUpgrade"),
            action = delegate { StartUpgrade(1); }
        };
        var downgrade = new Command_Action
        {
            defaultLabel = "RM.Downgrade".Translate(),
            defaultDesc = "RM.DowngradeDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Buttons/GestaltDowngrade"),
            action = delegate { StartUpgrade(-1); }
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
        if (!compPower.PowerOn)
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
            upgrade.Disable(
                "RM.OnCooldown".Translate((cooldownPeriod - Find.TickManager.TicksGame).ToStringTicksToPeriod()));
            downgrade.Disable(
                "RM.OnCooldown".Translate((cooldownPeriod - Find.TickManager.TicksGame).ToStringTicksToPeriod()));
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
        if (!Prefs.DevMode)
        {
            yield break;
        }

        yield return upgradeInstant;
        yield return downgradeInstant;
    }

    public override string CompInspectStringExtra()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("RM.Level".Translate(level));
        if (Upgrading)
        {
            var f = upgradeProgressTick / (float)CurrentUpgrade.upgradeDurationTicks;
            stringBuilder.AppendLine("RM.UpgradeInProcess".Translate(f.ToStringPercent()));
        }
        else if (Downgrading)
        {
            var f2 = downgradeProgressTick / (float)CurrentUpgrade.downgradeDurationTicks;
            stringBuilder.AppendLine("RM.DowngradeInProcess".Translate(f2.ToStringPercent()));
        }

        return stringBuilder.ToString().TrimEndNewlines();
    }

    public void StartUpgrade(int upgradeOffset)
    {
        SoundDefOf.DragSlider.PlayOneShotOnCamera();
        this.upgradeOffset = upgradeOffset;
        if (upgradeOffset < 0)
        {
            downgradeProgressTick = 0;
        }
        else if (upgradeOffset > 0)
        {
            upgradeProgressTick = 0;
        }
    }

    public override void CompTick()
    {
        base.CompTick();
        if (CurrentUpgrade.powerConsumption != 0f)
        {
            compPower.powerOutputInt = 0f - CurrentUpgrade.powerConsumption;
        }

        if (!IsActive)
        {
            return;
        }

        if (Upgrading)
        {
            upgradeProgressTick++;
            if (progressBarEffecter == null)
            {
                progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
            }

            progressBarEffecter.EffectTick(parent, TargetInfo.Invalid);
            var mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
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

            progressBarEffecter.EffectTick(parent, TargetInfo.Invalid);
            var mote2 = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
            mote2.progress = downgradeProgressTick / (float)CurrentUpgrade.downgradeDurationTicks;
            mote2.offsetZ = -0.8f;
            if (downgradeProgressTick >= CurrentUpgrade.downgradeDurationTicks)
            {
                SetLevel();
            }
        }

        DoWork();
    }

    protected virtual void DoWork()
    {
        if (!parent.IsHashIntervalTick(60))
        {
            return;
        }

        if (CurrentUpgrade.heatPerSecond != 0f)
        {
            GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, CurrentUpgrade.heatPerSecond);
        }

        if (CurrentUpgrade.researchPointsPerSecond == 0f)
        {
            return;
        }

        var currentProj = Find.ResearchManager.currentProj;
        if (currentProj == null)
        {
            return;
        }

        var progress = Find.ResearchManager.progress;
        if (progress.ContainsKey(currentProj))
        {
            progress[currentProj] += CurrentUpgrade.researchPointsPerSecond;
        }
        else
        {
            progress[currentProj] = CurrentUpgrade.researchPointsPerSecond;
        }

        if (currentProj.IsFinished)
        {
            Find.ResearchManager.FinishProject(currentProj, true);
        }
    }

    protected virtual void SetLevel()
    {
        if (upgradeOffset < 0)
        {
            cooldownPeriod = Find.TickManager.TicksGame + CurrentUpgrade.downgradeCooldownTicks;
            Messages.Message("RM.FinishedDowngrade".Translate(parent.LabelCap), parent, MessageTypeDefOf.NeutralEvent);
        }
        else if (upgradeOffset > 0)
        {
            cooldownPeriod = Find.TickManager.TicksGame + CurrentUpgrade.upgradeCooldownTicks;
            Messages.Message("RM.FinishedUpgrade".Translate(parent.LabelCap), parent, MessageTypeDefOf.NeutralEvent);
        }

        level += upgradeOffset;
        upgradeOffset = 0;
        downgradeProgressTick = upgradeProgressTick = -1;
        compPower.powerOutputInt = 0f - CurrentUpgrade.powerConsumption;
        cachedGraphic = null;
        if (progressBarEffecter == null)
        {
            return;
        }

        progressBarEffecter.Cleanup();
        progressBarEffecter = null;
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
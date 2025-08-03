using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ReinforcedMechanoids;

public class CompCauseGameCondition_TemperatureOffset : CompCauseGameConditionPowerDependent
{
    private GameCondition_TemperatureOffsetSlow causedCondition;
    private float temperatureOffset;

    public new CompProperties_CausesGameCondition_ClimateAdjuster Props =>
        (CompProperties_CausesGameCondition_ClimateAdjuster)props;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref temperatureOffset, "temperatureOffset");
        Scribe_References.Look(ref causedCondition, "causedCondition");
    }

    private static string getFloatStringWithSign(float val)
    {
        val = (float)Math.Round(val, 1);
        return val <= 0f ? val.ToStringTemperature("F0") : $"+{val.ToStringTemperature("F0")}";
    }

    private void setTemperatureOffset(float offset)
    {
        causedCondition.startTick = Find.TickManager.TicksGame;
        causedCondition.tickSet = Find.TickManager.TicksGame;
        temperatureOffset += offset;
        temperatureOffset = new FloatRange(-50f, 50f).ClampToRange(temperatureOffset);
        SoundDefOf.DragSlider.PlayOneShotOnCamera();
        MoteMaker.ThrowText(parent.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), parent.Map,
            getFloatStringWithSign(temperatureOffset), Color.white);
        ReSetupAllConditions();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        var command_Minus50 = new Command_Action
        {
            defaultLabel = "-50",
            defaultDesc = "RM.Minus50".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Buttons/ChangeClimateMinusMax")
        };
        command_Minus50.action =
            (Action)Delegate.Combine(command_Minus50.action, (Action)delegate { setTemperatureOffset(-50f); });
        command_Minus50.disabled = !parent.GetComp<CompPowerTrader>().PowerOn;
        command_Minus50.disabledReason = "NoPower".Translate();
        command_Minus50.hotKey = KeyBindingDefOf.Misc1;
        yield return command_Minus50;
        var command_Minus10 = new Command_Action
        {
            defaultLabel = "-10",
            defaultDesc = "RM.Minus10".Translate(),
            disabled = !parent.GetComp<CompPowerTrader>().PowerOn,
            disabledReason = "NoPower".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Buttons/ChangeClimateMinusMin")
        };
        command_Minus10.action =
            (Action)Delegate.Combine(command_Minus10.action, (Action)delegate { setTemperatureOffset(-10f); });
        command_Minus10.hotKey = KeyBindingDefOf.Misc2;
        yield return command_Minus10;
        var command_Plus10 = new Command_Action
        {
            defaultLabel = "+10",
            defaultDesc = "RM.Plus10".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Buttons/ChangeClimatePlusMin"),
            disabled = !parent.GetComp<CompPowerTrader>().PowerOn,
            disabledReason = "NoPower".Translate()
        };
        command_Plus10.action =
            (Action)Delegate.Combine(command_Plus10.action, (Action)delegate { setTemperatureOffset(10f); });
        command_Plus10.hotKey = KeyBindingDefOf.Misc3;
        yield return command_Plus10;
        var command_Plus50 = new Command_Action
        {
            defaultLabel = "+50",
            defaultDesc = "RM.Plus50".Translate(),
            disabled = !parent.GetComp<CompPowerTrader>().PowerOn,
            disabledReason = "NoPower".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Buttons/ChangeClimatePlusMax")
        };
        command_Plus50.action =
            (Action)Delegate.Combine(command_Plus50.action, (Action)delegate { setTemperatureOffset(50f); });
        command_Plus50.hotKey = KeyBindingDefOf.Misc4;
        yield return command_Plus50;
    }

    public override string CompInspectStringExtra()
    {
        var stringBuilder = new StringBuilder();
        if (!base.CompInspectStringExtra().NullOrEmpty())
        {
            stringBuilder.AppendLine(base.CompInspectStringExtra());
        }

        stringBuilder.AppendLine("RM.TargetTemperatureOffset".Translate(getFloatStringWithSign(temperatureOffset)));
        if (causedCondition != null)
        {
            stringBuilder.AppendLine(
                "RM.CurrentTemperatureOffset".Translate(getFloatStringWithSign(causedCondition.curValue)));
        }

        return stringBuilder.ToString().TrimEndNewlines();
    }

    public override void SetupCondition(GameCondition condition, Map map)
    {
        base.SetupCondition(condition, map);
        causedCondition = (GameCondition_TemperatureOffsetSlow)condition;
        SetupVars(causedCondition);
    }

    private void SetupVars(GameCondition_TemperatureOffsetSlow tempCondition)
    {
        tempCondition.conditionCauser = parent;
        tempCondition.tempOffset = temperatureOffset;
        tempCondition.startTick = Find.TickManager.TicksGame;
    }

    public override GameCondition CreateConditionOn(Map map)
    {
        causedCondition = base.CreateConditionOn(map) as GameCondition_TemperatureOffsetSlow;
        SetupVars(causedCondition);
        return causedCondition;
    }
}
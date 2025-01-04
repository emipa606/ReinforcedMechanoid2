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
    public GameCondition_TemperatureOffsetSlow causedCondition;
    public float temperatureOffset;

    public new CompProperties_CausesGameCondition_ClimateAdjuster Props =>
        (CompProperties_CausesGameCondition_ClimateAdjuster)props;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref temperatureOffset, "temperatureOffset");
        Scribe_References.Look(ref causedCondition, "causedCondition");
    }

    private string GetFloatStringWithSign(float val)
    {
        val = (float)Math.Round(val, 1);
        if (val <= 0f)
        {
            return val.ToStringTemperature("F0");
        }

        return "+" + val.ToStringTemperature("F0");
    }

    public void SetTemperatureOffset(float offset)
    {
        temperatureOffset += offset;
        temperatureOffset = new FloatRange(-50f, 50f).ClampToRange(temperatureOffset);
        SoundDefOf.DragSlider.PlayOneShotOnCamera();
        MoteMaker.ThrowText(parent.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), parent.Map,
            GetFloatStringWithSign(temperatureOffset), Color.white);
        ReSetupAllConditions();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        var command_Action = new Command_Action
        {
            defaultLabel = "-50",
            defaultDesc = "RM.Minus50".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Buttons/ChangeClimateMinusMax")
        };
        command_Action.action =
            (Action)Delegate.Combine(command_Action.action, (Action)delegate { SetTemperatureOffset(-50f); });
        command_Action.disabled = !parent.GetComp<CompPowerTrader>().PowerOn;
        command_Action.disabledReason = "NoPower".Translate();
        command_Action.hotKey = KeyBindingDefOf.Misc1;
        yield return command_Action;
        var command_Action2 = new Command_Action
        {
            defaultLabel = "-10",
            defaultDesc = "RM.Minus10".Translate(),
            disabled = !parent.GetComp<CompPowerTrader>().PowerOn,
            disabledReason = "NoPower".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Buttons/ChangeClimateMinusMin")
        };
        command_Action2.action =
            (Action)Delegate.Combine(command_Action2.action, (Action)delegate { SetTemperatureOffset(-10f); });
        command_Action2.hotKey = KeyBindingDefOf.Misc2;
        yield return command_Action2;
        var command_Action3 = new Command_Action
        {
            defaultLabel = "+10",
            defaultDesc = "RM.Plus10".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Buttons/ChangeClimatePlusMin"),
            disabled = !parent.GetComp<CompPowerTrader>().PowerOn,
            disabledReason = "NoPower".Translate()
        };
        command_Action3.action =
            (Action)Delegate.Combine(command_Action3.action, (Action)delegate { SetTemperatureOffset(10f); });
        command_Action3.hotKey = KeyBindingDefOf.Misc3;
        yield return command_Action3;
        var command_Action4 = new Command_Action
        {
            defaultLabel = "+50",
            defaultDesc = "RM.Plus50".Translate(),
            disabled = !parent.GetComp<CompPowerTrader>().PowerOn,
            disabledReason = "NoPower".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Buttons/ChangeClimatePlusMax")
        };
        command_Action4.action =
            (Action)Delegate.Combine(command_Action4.action, (Action)delegate { SetTemperatureOffset(50f); });
        command_Action4.hotKey = KeyBindingDefOf.Misc4;
        yield return command_Action4;
    }

    public override string CompInspectStringExtra()
    {
        var stringBuilder = new StringBuilder();
        if (!base.CompInspectStringExtra().NullOrEmpty())
        {
            stringBuilder.AppendLine(base.CompInspectStringExtra());
        }

        stringBuilder.AppendLine("RM.TargetTemperatureOffset".Translate(GetFloatStringWithSign(temperatureOffset)));
        if (causedCondition != null)
        {
            stringBuilder.AppendLine(
                "RM.CurrentTemperatureOffset".Translate(GetFloatStringWithSign(causedCondition.curValue)));
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
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ReinforcedMechanoids
{
	public class CompProperties_CausesGameCondition_ClimateAdjuster : CompProperties_CausesGameCondition
	{
		public CompProperties_CausesGameCondition_ClimateAdjuster()
		{
			compClass = typeof(CompCauseGameCondition_TemperatureOffset);
		}
	}

    public class CompCauseGameCondition_TemperatureOffset : CompCauseGameConditionPowerDependent
	{
		public float temperatureOffset;

		public GameCondition_TemperatureOffsetSlow causedCondition;
        public new CompProperties_CausesGameCondition_ClimateAdjuster Props => (CompProperties_CausesGameCondition_ClimateAdjuster)props;
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref temperatureOffset, "temperatureOffset", 0f);
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
			temperatureOffset = new FloatRange(-50, 50).ClampToRange(temperatureOffset);
            SoundDefOf.DragSlider.PlayOneShotOnCamera();
            MoteMaker.ThrowText(parent.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), parent.Map, GetFloatStringWithSign(temperatureOffset), Color.white);
            ReSetupAllConditions();
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "-50";
			command_Action.defaultDesc = "RM.Minus50".Translate();
            command_Action.icon = ContentFinder<Texture2D>.Get("UI/Buttons/ChangeClimateMinusMax");
			command_Action.action = (Action)Delegate.Combine(command_Action.action, (Action)delegate
			{
				SetTemperatureOffset(-50f);
			});
			command_Action.disabled = !this.parent.GetComp<CompPowerTrader>().PowerOn;
			command_Action.disabledReason = "NoPower".Translate();
			command_Action.hotKey = KeyBindingDefOf.Misc1;
			yield return command_Action;
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "-10";
            command_Action2.defaultDesc = "RM.Minus10".Translate();
            command_Action2.disabled = !this.parent.GetComp<CompPowerTrader>().PowerOn;
			command_Action2.disabledReason = "NoPower".Translate();
			command_Action2.icon = ContentFinder<Texture2D>.Get("UI/Buttons/ChangeClimateMinusMin");
			command_Action2.action = (Action)Delegate.Combine(command_Action2.action, (Action)delegate
			{
				SetTemperatureOffset(-10f);
			});
			command_Action2.hotKey = KeyBindingDefOf.Misc2;
			yield return command_Action2;
			Command_Action command_Action3 = new Command_Action();
			command_Action3.defaultLabel = "+10";
            command_Action3.defaultDesc = "RM.Plus10".Translate();
            command_Action3.icon = ContentFinder<Texture2D>.Get("UI/Buttons/ChangeClimatePlusMin");
			command_Action3.disabled = !this.parent.GetComp<CompPowerTrader>().PowerOn;
			command_Action3.disabledReason = "NoPower".Translate();
			command_Action3.action = (Action)Delegate.Combine(command_Action3.action, (Action)delegate
			{
				SetTemperatureOffset(10f);
			});
			command_Action3.hotKey = KeyBindingDefOf.Misc3;
			yield return command_Action3;
			Command_Action command_Action4 = new Command_Action();
			command_Action4.defaultLabel = "+50";
            command_Action4.defaultDesc = "RM.Plus50".Translate();
            command_Action4.disabled = !this.parent.GetComp<CompPowerTrader>().PowerOn;
			command_Action4.disabledReason = "NoPower".Translate();
			command_Action4.icon = ContentFinder<Texture2D>.Get("UI/Buttons/ChangeClimatePlusMax");
			command_Action4.action = (Action)Delegate.Combine(command_Action4.action, (Action)delegate
			{
				SetTemperatureOffset(50f);
			});
			command_Action4.hotKey = KeyBindingDefOf.Misc4;
			yield return command_Action4;
		}

		public override string CompInspectStringExtra()
		{
			var text = new StringBuilder();
			if (base.CompInspectStringExtra().NullOrEmpty() is false)
			{
				text.AppendLine(base.CompInspectStringExtra());
			}
			text.AppendLine("RM.TargetTemperatureOffset".Translate(GetFloatStringWithSign(temperatureOffset)));
			if (causedCondition != null)
			{
				text.AppendLine("RM.CurrentTemperatureOffset".Translate(GetFloatStringWithSign(causedCondition.curValue)));
			}
			return text.ToString().TrimEndNewlines();
		}

		public override void SetupCondition(GameCondition condition, Map map)
		{
			base.SetupCondition(condition, map);
            causedCondition = ((GameCondition_TemperatureOffsetSlow)condition);
			SetupVars(causedCondition);
		}

		private void SetupVars(GameCondition_TemperatureOffsetSlow tempCondition)
		{
			tempCondition.conditionCauser = this.parent;
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
}

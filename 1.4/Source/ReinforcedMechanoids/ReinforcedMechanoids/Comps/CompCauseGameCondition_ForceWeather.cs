using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids
{
	public class CompCauseGameCondition_ForceWeather : CompCauseGameConditionPowerDependent
	{
		public WeatherDef weather;
		public int cooldownTicks;
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			weather = base.Props.conditionDef.weatherDef;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Defs.Look(ref weather, "weather");
			Scribe_Values.Look(ref cooldownTicks, "cooldownTicks");
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = weather.LabelCap;
			command_Action.defaultDesc = "RM.ForcedWeatherDesc".Translate();
            command_Action.icon = ContentFinder<Texture2D>.Get("UI/Buttons/ChangeWeather");
			if (cooldownTicks > Find.TickManager.TicksGame)
			{
				command_Action.Disable("RM.ForcedWeatherCooldown".Translate((cooldownTicks - Find.TickManager.TicksGame).ToStringTicksToPeriod()));
			}
			else if (!this.parent.GetComp<CompPowerTrader>().PowerOn)
			{
				command_Action.Disable("NoPower".Translate());
			}
            command_Action.action = delegate
            {
				var floatList = new List<FloatMenuOption>();
                foreach (WeatherDef weatherDef in DefDatabase<WeatherDef>.AllDefsListForReading)
                {
                    floatList.Add(new FloatMenuOption(weatherDef.LabelCap, delegate
                    {
                        this.ChangeWeather(weatherDef);
                    }, MenuOptionPriority.Default, null, null, 29f, null, null));
                }
				Find.WindowStack.Add(new FloatMenu(floatList));
            };
            command_Action.hotKey = KeyBindingDefOf.Misc1;
            yield return command_Action;
        }

		public void ChangeWeather(WeatherDef newWeather)
        {
			weather = newWeather;
			Messages.Message("RM.ForcedWeatherChanged".Translate(newWeather.label), MessageTypeDefOf.NeutralEvent);
			cooldownTicks = Find.TickManager.TicksGame + GenDate.TicksPerDay;
			ReSetupAllConditions();
		}
		public override void SetupCondition(GameCondition condition, Map map)
		{
			base.SetupCondition(condition, map);
			var cond = ((GameCondition_ForceWeather)condition);
			cond.conditionCauser = this.parent;
            cond.weather = weather;
		}

		public override string CompInspectStringExtra()
		{
			string text = base.CompInspectStringExtra();
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			return text + "Weather".Translate() + ": " + weather.LabelCap;
		}

		public override void RandomizeSettings(Site site)
		{
			weather = DefDatabase<WeatherDef>.AllDefsListForReading.Where((WeatherDef x) => x.isBad).RandomElement();
		}
	}
}

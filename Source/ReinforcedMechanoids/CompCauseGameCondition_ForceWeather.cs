using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids;

public class CompCauseGameCondition_ForceWeather : CompCauseGameConditionPowerDependent
{
    public int cooldownTicks;
    public WeatherDef weather;

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        weather = Props.conditionDef.weatherDef;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Defs.Look(ref weather, "weather");
        Scribe_Values.Look(ref cooldownTicks, "cooldownTicks");
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        var command_Action = new Command_Action
        {
            defaultLabel = weather.LabelCap,
            defaultDesc = "RM.ForcedWeatherDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Buttons/ChangeWeather")
        };
        if (cooldownTicks > Find.TickManager.TicksGame)
        {
            command_Action.Disable(
                "RM.ForcedWeatherCooldown".Translate(
                    (cooldownTicks - Find.TickManager.TicksGame).ToStringTicksToPeriod()));
        }
        else if (!parent.GetComp<CompPowerTrader>().PowerOn)
        {
            command_Action.Disable("NoPower".Translate());
        }

        command_Action.action = delegate
        {
            var list = new List<FloatMenuOption>();
            foreach (var weatherDef in DefDatabase<WeatherDef>.AllDefsListForReading)
            {
                list.Add(new FloatMenuOption(weatherDef.LabelCap, delegate { ChangeWeather(weatherDef); },
                    MenuOptionPriority.Default, null, null, 29f));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        };
        command_Action.hotKey = KeyBindingDefOf.Misc1;
        yield return command_Action;
    }

    public void ChangeWeather(WeatherDef newWeather)
    {
        weather = newWeather;
        Messages.Message("RM.ForcedWeatherChanged".Translate(newWeather.label), MessageTypeDefOf.NeutralEvent);
        cooldownTicks = Find.TickManager.TicksGame + 60000;
        ReSetupAllConditions();
    }

    public override void SetupCondition(GameCondition condition, Map map)
    {
        base.SetupCondition(condition, map);
        var gameCondition_ForceWeather = (GameCondition_ForceWeather)condition;
        gameCondition_ForceWeather.conditionCauser = parent;
        gameCondition_ForceWeather.weather = weather;
    }

    public override string CompInspectStringExtra()
    {
        var text = base.CompInspectStringExtra();
        if (!text.NullOrEmpty())
        {
            text += "\n";
        }

        return text + "Weather".Translate() + ": " + weather.LabelCap;
    }

    public override void RandomizeSettings(Site site)
    {
        weather = DefDatabase<WeatherDef>.AllDefsListForReading.Where(x => x.isBad).RandomElement();
    }
}
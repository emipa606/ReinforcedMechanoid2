using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids;

public class CommandAction_RightClickWeather(CompCauseGameCondition_ForceWeather comp) : Command_Action
{
    public readonly CompCauseGameCondition_ForceWeather comp = comp;

    public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
    {
        get
        {
            if (!comp.parent.GetComp<CompPowerTrader>().PowerOn)
            {
                yield break;
            }

            foreach (var weatherDef in DefDatabase<WeatherDef>.AllDefsListForReading)
            {
                yield return new FloatMenuOption(weatherDef.LabelCap, delegate { comp.ChangeWeather(weatherDef); },
                    MenuOptionPriority.Default, null, null, 29f);
            }
        }
    }
}
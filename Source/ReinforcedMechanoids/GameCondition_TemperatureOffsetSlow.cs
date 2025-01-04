using RimWorld;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids;

public class GameCondition_TemperatureOffsetSlow : GameCondition_TemperatureOffset
{
    public float curValue;

    public int tickSet;

    public override int TransitionTicks => 2500;

    public override float TemperatureOffset()
    {
        if (curValue == tempOffset || Find.TickManager.TicksGame == tickSet)
        {
            return curValue;
        }

        var num = Find.TickManager.TicksGame - tickSet;
        var num2 = num > 0 ? TransitionTicks / (float)num : TransitionTicks / 60f;
        var num3 = 1f / num2;
        if (!float.IsInfinity(num3) && !float.IsNaN(num3))
        {
            if (curValue > tempOffset)
            {
                curValue -= num3;
                if (Mathf.Ceil(curValue) == tempOffset)
                {
                    curValue = Mathf.Ceil(curValue);
                }
            }
            else if (curValue < tempOffset)
            {
                curValue += num3;
                if (Mathf.Floor(curValue) == tempOffset)
                {
                    curValue = Mathf.Floor(curValue);
                }
            }
        }

        tickSet = Find.TickManager.TicksGame;

        return curValue;
    }

    public static float LerpInOutValue(GameCondition gameCondition, float lerpTime, float lerpTarget = 1f)
    {
        return GameConditionUtility.LerpInOutValue(gameCondition.TicksPassed, lerpTime + 1f, lerpTime, lerpTarget);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref curValue, "curValue");
        Scribe_Values.Look(ref tickSet, "tickSet");
    }
}
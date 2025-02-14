using System;
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

        var timeSinceSet = Math.Min(Find.TickManager.TicksGame - tickSet, TransitionTicks);
        float timeElapsedFactor;
        if (timeSinceSet > 0)
        {
            timeElapsedFactor = TransitionTicks / (float)timeSinceSet;
        }
        else
        {
            timeElapsedFactor = TransitionTicks / 60f;
        }

        var degreeChange = 1f / timeElapsedFactor;
        if (!float.IsInfinity(degreeChange) && !float.IsNaN(degreeChange))
        {
            if (curValue > tempOffset)
            {
                curValue -= degreeChange;
                if (Mathf.Ceil(curValue) == tempOffset)
                {
                    curValue = Mathf.Ceil(curValue);
                }
            }
            else if (curValue < tempOffset)
            {
                curValue += degreeChange;
                if (Mathf.Floor(curValue) == tempOffset)
                {
                    curValue = Mathf.Floor(curValue);
                }
            }
        }

        tickSet = Find.TickManager.TicksGame;

        return curValue;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref curValue, "curValue");
        Scribe_Values.Look(ref tickSet, "tickSet");
    }
}
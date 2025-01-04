using RimWorld;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids;

public class GameCondition_NoSunlight : GameCondition
{
    private readonly SkyColorSet EclipseSkyColors =
        new SkyColorSet(new Color(0.482f, 0.603f, 0.682f), Color.white, new Color(0.6f, 0.6f, 0.6f), 1f);

    public float curValue;

    public int tickSet;

    public override int TransitionTicks => 60000;

    public override string Label => base.Label + " (" + curValue.ToStringPercent() + ")";

    public override float SkyTargetLerpFactor(Map map)
    {
        if (Find.TickManager.TicksGame != tickSet)
        {
            var num = Find.TickManager.TicksGame - tickSet;
            var num2 = num > 0 ? TransitionTicks / (float)num : TransitionTicks / 60f;
            var num3 = 1f / num2;
            if (!float.IsInfinity(num3) && !float.IsNaN(num3))
            {
                if (conditionCauser.TryGetComp<CompPowerTrader>().PowerOn)
                {
                    curValue += num3;
                }
                else
                {
                    curValue -= num3;
                }
            }

            tickSet = Find.TickManager.TicksGame;
        }

        curValue = Mathf.Clamp01(curValue);
        return curValue;
    }

    public static float LerpInOutValue(GameCondition gameCondition, float lerpTime, float lerpTarget = 1f)
    {
        return GameConditionUtility.LerpInOutValue(gameCondition.TicksPassed, lerpTime + 1f, lerpTime, lerpTarget);
    }

    public override void GameConditionTick()
    {
        base.GameConditionTick();
        if (curValue <= 0f && (conditionCauser == null || !conditionCauser.TryGetComp<CompPowerTrader>().PowerOn))
        {
            End();
        }
    }

    public override void End()
    {
        base.End();
        Messages.Message("RM.SunNoLongerBlocked".Translate(), MessageTypeDefOf.NeutralEvent);
    }

    public override SkyTarget? SkyTarget(Map map)
    {
        return new SkyTarget(0f, EclipseSkyColors, 1f, 0f);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref curValue, "curValue");
        Scribe_Values.Look(ref tickSet, "tickSet");
    }
}
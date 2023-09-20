using RimWorld;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids
{
    public class GameCondition_NoSunlight : GameCondition
    {
        public float curValue;
        public int tickSet;

        private SkyColorSet EclipseSkyColors = new SkyColorSet(new Color(0.482f, 0.603f, 0.682f), Color.white, new Color(0.6f, 0.6f, 0.6f), 1f);
        public override int TransitionTicks => GenDate.TicksPerDay;
        public override string Label => base.Label + " (" + curValue.ToStringPercent() + ")";
        public override float SkyTargetLerpFactor(Map map)
        {
            if (Find.TickManager.TicksGame != tickSet)
            {
                var multiply = Find.TickManager.TicksGame - tickSet;
                var transitionTicksAdjusted = multiply > 0 ? (TransitionTicks / multiply) : (TransitionTicks / 60f);
                var value = 1f / transitionTicksAdjusted;
                if (float.IsInfinity(value) is false && float.IsNaN(value) is false)
                {
                    if (this.conditionCauser.TryGetComp<CompPowerTrader>().PowerOn)
                    {
                        curValue += value;
                    }
                    else
                    {
                        curValue -= value;
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
            if (curValue <= 0f && (this.conditionCauser is null || this.conditionCauser.TryGetComp<CompPowerTrader>().PowerOn is false))
            {
                this.End();
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
}

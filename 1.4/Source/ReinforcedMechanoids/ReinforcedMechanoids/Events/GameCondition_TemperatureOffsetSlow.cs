using RimWorld;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids
{
    public class GameCondition_TemperatureOffsetSlow : GameCondition_TemperatureOffset
    {
        public float curValue;
		public int tickSet;
        public override int TransitionTicks => GenDate.TicksPerHour;
		public override float TemperatureOffset()
        {
			if (curValue != tempOffset && Find.TickManager.TicksGame != tickSet)
			{
				var multiply = Find.TickManager.TicksGame - tickSet;
				var transitionTicksAdjusted = multiply > 0 ? (TransitionTicks / multiply) : (TransitionTicks / 60f);
				var value = 1f / transitionTicksAdjusted;
				if (float.IsInfinity(value) is false && float.IsNaN(value) is false)
				{
					if (curValue > tempOffset)
					{
                        curValue -= value;
                        if (Mathf.Ceil(curValue) == tempOffset)
                        {
                            curValue = Mathf.Ceil(curValue);
                        }
                    }
                    else if (curValue < tempOffset)
					{
                        curValue += value;
                        if (Mathf.Floor(curValue) == tempOffset)
                        {
                            curValue = Mathf.Floor(curValue);
                        }
                    }
                }
				tickSet = Find.TickManager.TicksGame;
            }
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
}

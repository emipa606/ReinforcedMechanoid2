using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids
{
    public class CompCauseGameConditionPowerDependent : CompCauseGameCondition
    {
		public CompPowerTrader compPower;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
			compPower = this.parent.TryGetComp<CompPowerTrader>();
		}

        public override void SetupCondition(GameCondition condition, Map map)
        {
            base.SetupCondition(condition, map);
            condition.conditionCauser = this.parent;
        }

        [HarmonyPatch(typeof(CompCauseGameCondition), nameof(CompCauseGameCondition.Active), MethodType.Getter)]
		public static class CompCauseGameCondition_ActivePatch
        {
			public static void Postfix(ref bool __result, CompCauseGameCondition __instance)
            {
				if (__result && __instance is CompCauseGameConditionPowerDependent powerDependent)
                {
					if (!powerDependent.compPower.PowerOn)
                    {
						__result = false;
                    }
                }
            }
        }
    }
}

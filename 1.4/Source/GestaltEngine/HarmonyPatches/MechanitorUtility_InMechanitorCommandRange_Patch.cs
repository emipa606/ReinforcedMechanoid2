using HarmonyLib;
using Verse;

namespace GestaltEngine
{
    [HarmonyPatch(typeof(MechanitorUtility), "InMechanitorCommandRange")]
    public static class MechanitorUtility_InMechanitorCommandRange_Patch
    {
        public static void Postfix(Pawn mech, LocalTargetInfo target, ref bool __result)
        {
            Pawn overseer = mech.GetOverseer();
            if (overseer != null && overseer.TryGetGestaltEngineInstead(out _))
            {
                __result = true;
            }
        }
    }
}

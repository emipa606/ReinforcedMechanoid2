using HarmonyLib;
using Verse;

namespace GestaltEngine
{
    [HarmonyPatch(typeof(Pawn), "KindLabel", MethodType.Getter)]
    public static class Pawn_KindLabel_Patch
    {
        public static void Postfix(Pawn __instance, ref string __result)
        {
            if (__instance.TryGetGestaltEngineInstead(out var comp))
            {
                __result = "";
            }
        }
    }
}

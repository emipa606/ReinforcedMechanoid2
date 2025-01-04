using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace GestaltEngine;

[HarmonyPatch(typeof(CameraJumper), "TryJumpAndSelect")]
public static class CameraJumper_TryJumpAndSelect_Patch
{
    public static void Prefix(ref GlobalTargetInfo target)
    {
        if (target.Thing is Pawn overseer && overseer.TryGetGestaltEngineInstead(out var result))
        {
            target = result.parent;
        }
    }
}
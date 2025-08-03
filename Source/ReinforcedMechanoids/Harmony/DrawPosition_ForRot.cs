using HarmonyLib;
using MVCF.VerbComps;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(DrawPosition), nameof(DrawPosition.ForRot))]
public static class DrawPosition_ForRot
{
    public static bool Prefix(DrawPosition __instance, ref Vector3 __result, Rot4 rot)
    {
        if (__instance is not DrawPositionVector3 drawPosition)
        {
            return true;
        }

        __result = ForRot(drawPosition, rot);
        return false;
    }

    private static Vector3 ForRot(DrawPositionVector3 drawPosition, Rot4 rot)
    {
        var result = Vector3.positiveInfinity;
        switch (rot.AsInt)
        {
            case 0:
                result = drawPosition.up;
                break;
            case 1:
                result = drawPosition.right;
                break;
            case 2:
                result = drawPosition.down;
                break;
            case 3:
                result = drawPosition.left;
                break;
        }

        if (double.IsPositiveInfinity(result.x))
        {
            result = Vector3.positiveInfinity;
        }

        if (double.IsPositiveInfinity(result.x))
        {
            result = Vector2.zero;
        }

        return result;
    }
}
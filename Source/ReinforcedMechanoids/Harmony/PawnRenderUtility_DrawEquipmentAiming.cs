using HarmonyLib;
using Verse;
using Vector3 = UnityEngine.Vector3;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAiming))]
public static class PawnRenderUtility_DrawEquipmentAiming
{
    public static void Prefix(ref Vector3 drawLoc, ref Thing eq, out float __state)
    {
        __state = eq.def.equippedAngleOffset;

        if (eq.ParentHolder is not Pawn_EquipmentTracker pawnEquipmentTracker)
        {
            return;
        }

        if (pawnEquipmentTracker.pawn == null)
        {
            return;
        }

        var pawn = pawnEquipmentTracker.pawn;
        var modExtension = pawn.def.GetModExtension<EquipmentDrawPositionOffsetExtension>();
        if (modExtension == null)
        {
            return;
        }

        switch (pawn.Rotation.AsInt)
        {
            case 0:
                if (modExtension.northEquippedAngleOffset.HasValue)
                {
                    eq.def.equippedAngleOffset = modExtension.northEquippedAngleOffset.Value;
                }

                if (modExtension.northDrawOffset != null)
                {
                    drawLoc += modExtension.northDrawOffset.Value;
                }

                break;
            case 1:
                if (modExtension.eastEquippedAngleOffset.HasValue)
                {
                    eq.def.equippedAngleOffset = modExtension.eastEquippedAngleOffset.Value;
                }

                if (modExtension.eastDrawOffset != null)
                {
                    drawLoc += modExtension.eastDrawOffset.Value;
                }

                break;
            case 2:
                if (modExtension.southEquippedAngleOffset.HasValue)
                {
                    eq.def.equippedAngleOffset = modExtension.southEquippedAngleOffset.Value;
                }

                if (modExtension.southDrawOffset != null)
                {
                    drawLoc += modExtension.southDrawOffset.Value;
                }

                break;
            case 3:
                if (modExtension.westEquippedAngleOffset.HasValue)
                {
                    eq.def.equippedAngleOffset = modExtension.westEquippedAngleOffset.Value;
                }

                if (modExtension.westDrawOffset != null)
                {
                    drawLoc += modExtension.westDrawOffset.Value;
                }

                break;
        }
    }

    public static void Postfix(ref Thing eq, float __state)
    {
        eq.def.equippedAngleOffset = __state;
    }
}
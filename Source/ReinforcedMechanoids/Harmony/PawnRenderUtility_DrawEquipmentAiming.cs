using HarmonyLib;
using Verse;
using Vector3 = UnityEngine.Vector3;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAiming))]
public static class PawnRenderUtility_DrawEquipmentAiming
{
    public static void Prefix(ref Vector3 drawLoc, ref Thing eq)
    {
        if (eq.ParentHolder is not Pawn_EquipmentTracker pawnEquipmentTracker)
        {
            Log.Message($"Cannot find pawnEquipmentTracker for {eq} as parentholder is {eq.ParentHolder}");
            return;
        }

        if (pawnEquipmentTracker.pawn == null)
        {
            Log.Message($"Cannot find pawn for {eq} as pawn is {pawnEquipmentTracker.pawn}");
            return;
        }

        var pawn = pawnEquipmentTracker.pawn;
        var modExtension = pawn.def.GetModExtension<EquipmentDrawPositionOffsetExtension>();
        if (modExtension == null)
        {
            Log.Message($"Cannot find modextension for {eq}");
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
}
using HarmonyLib;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAndApparelExtras))]
public static class PawnRenderUtility_DrawEquipmentAndApparelExtras
{
    public static void Prefix(Pawn pawn, out float __state)
    {
        if (pawn.equipment?.Primary == null)
        {
            __state = 0;
            return;
        }

        var eq = pawn.equipment.Primary;

        __state = eq.def.equippedAngleOffset;
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

                break;
            case 1:
                if (modExtension.eastEquippedAngleOffset.HasValue)
                {
                    eq.def.equippedAngleOffset = modExtension.eastEquippedAngleOffset.Value;
                }

                break;
            case 2:
                if (modExtension.southEquippedAngleOffset.HasValue)
                {
                    eq.def.equippedAngleOffset = modExtension.southEquippedAngleOffset.Value;
                }

                break;
            case 3:
                if (modExtension.westEquippedAngleOffset.HasValue)
                {
                    eq.def.equippedAngleOffset = modExtension.westEquippedAngleOffset.Value;
                }

                break;
        }
    }

    public static void Postfix(Pawn pawn, float __state)
    {
        if (pawn.equipment?.Primary == null)
        {
            return;
        }

        pawn.equipment.Primary.def.equippedAngleOffset = __state;
    }
}
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(TransferableUIUtility), nameof(TransferableUIUtility.DoExtraIcons))]
public static class TransferableUIUtility_DoExtraIcons
{
    private static readonly float RideableIconWidth = 24f;

    private static readonly Texture2D RideableIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Rideable");

    public static void Postfix(Transferable trad, Rect rect, ref float curX)
    {
        if (trad.AnyThing is not Pawn pawn)
        {
            return;
        }

        var extension = pawn.def.GetModExtension<MechanoidExtension>();
        if (extension == null)
        {
            return;
        }

        if (!pawn.IsCaravanRideable())
        {
            return;
        }

        var rect2 = new Rect(curX - RideableIconWidth, (rect.height - RideableIconWidth) / 2f,
            RideableIconWidth, RideableIconWidth);
        curX -= rect2.width;
        GUI.DrawTexture(rect2, RideableIcon);
        if (Mouse.IsOver(rect2))
        {
            TooltipHandler.TipRegion(rect2, getIconTooltipText(pawn));
        }
    }

    private static string getIconTooltipText(Pawn pawn)
    {
        var statValue = pawn.GetStatValue(StatDefOf.CaravanRidingSpeedFactor);
        return "VFEMRideableMachineTip".Translate() + "\n\n" + StatDefOf.CaravanRidingSpeedFactor.LabelCap + ": " +
               statValue.ToStringPercent();
    }
}
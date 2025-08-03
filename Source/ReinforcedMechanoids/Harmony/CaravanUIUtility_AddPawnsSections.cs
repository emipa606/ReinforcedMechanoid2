using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(CaravanUIUtility), nameof(CaravanUIUtility.AddPawnsSections))]
public static class CaravanUIUtility_AddPawnsSections
{
    public static void Postfix(TransferableOneWayWidget widget, List<TransferableOneWay> transferables)
    {
        if (ModsConfig.BiotechActive)
        {
            return;
        }

        var source = transferables.Where(x => x.ThingDef.category == ThingCategory.Pawn);
        widget.AddSection("VEF.MechsSection".Translate(), source.Where(x
            => x.AnyThing is Pawn pawn && pawn.RaceProps.IsMechanoid && pawn.Faction == Faction.OfPlayer));
    }
}
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch]
public static class CaravanUIUtility_AddPawnsSections_MechSection
{
    public static MethodBase TargetMethod()
    {
        foreach (var type in typeof(CaravanUIUtility).GetNestedTypes(AccessTools.all))
        {
            foreach (var method in type.GetMethods(AccessTools.all))
            {
                if (method.Name.Contains("<AddPawnsSections>b__8_6"))
                {
                    return method;
                }
            }
        }

        return null;
    }

    public static void Postfix(TransferableOneWay x, ref bool __result)
    {
        __result = x.AnyThing is Pawn pawn && pawn.RaceProps.IsMechanoid && pawn.Faction == Faction.OfPlayer;
    }
}
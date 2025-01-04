using HarmonyLib;
using RimWorld;
using Verse.AI.Group;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(LordMaker), nameof(LordMaker.MakeNewLord))]
public static class LordMaker_MakeNewLord
{
    public static void Prefix(Faction faction, LordJob lordJob)
    {
        if (faction?.def == RM_DefOf.RM_Remnants && lordJob is LordJob_AssaultColony lordJob_AssaultColony)
        {
            lordJob_AssaultColony.canTimeoutOrFlee = false;
        }
    }
}
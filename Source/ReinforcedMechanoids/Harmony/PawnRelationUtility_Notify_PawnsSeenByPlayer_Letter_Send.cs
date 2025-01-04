using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(PawnRelationUtility), nameof(PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send))]
public static class PawnRelationUtility_Notify_PawnsSeenByPlayer_Letter_Send
{
    public static Exception Finalizer(Exception __exception)
    {
        if (__exception != null)
        {
            Log.Error(__exception.ToString());
        }

        return null;
    }
}
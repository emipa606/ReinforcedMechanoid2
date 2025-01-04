using HarmonyLib;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(PawnComponentsUtility), nameof(PawnComponentsUtility.AddAndRemoveDynamicComponents))]
public static class PawnComponentsUtility_AddAndRemoveDynamicComponents
{
    public static void Postfix(Pawn pawn)
    {
        if (pawn.kindDef == RM_DefOf.RM_Mech_Vulture)
        {
            AssignPawnComponents(pawn);
        }
    }

    public static void AssignPawnComponents(Pawn pawn)
    {
        var pawn2 = pawn;
        if (pawn2.story == null)
        {
            pawn2.story = new Pawn_StoryTracker(pawn);
        }

        pawn2 = pawn;
        if (pawn2.skills == null)
        {
            pawn2.skills = new Pawn_SkillTracker(pawn);
        }

        if (pawn.workSettings != null)
        {
            return;
        }

        pawn.workSettings = new Pawn_WorkSettings(pawn);
        var defMap = new DefMap<WorkTypeDef, int>();
        defMap.SetAll(0);
        pawn.workSettings.priorities = defMap;
    }
}
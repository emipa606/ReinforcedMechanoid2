using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;

namespace ReinforcedMechanoids
{
    [HarmonyPatch(typeof(QuestGen), "Generate")]
    public static class QuestGen_Generate_Patch
    {
        private static void Prefix(ref QuestScriptDef root, Slate initialVars)
        {
            var gameComp = QuestWatcher.Instance;
            if (!gameComp.PossibleToRunQuest(root, initialVars))
            {
                root = QuestScriptDefOf.OpportunitySite_ItemStash;
            }
            if (!gameComp.startedQuests.Contains(root))
            {
                gameComp.startedQuests.Add(root);
            }
        }
    }
}

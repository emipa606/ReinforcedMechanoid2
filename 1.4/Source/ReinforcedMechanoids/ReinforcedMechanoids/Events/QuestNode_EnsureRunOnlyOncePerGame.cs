using RimWorld;
using RimWorld.QuestGen;

namespace ReinforcedMechanoids
{
    public class QuestNode_EnsureRunOnlyOncePerGame : QuestNode_RunCondition
    {
        public SlateRef<QuestScriptDef> questDef;
        public override bool CanRun(Slate slate)
        {
            if (QuestWatcher.Instance.startedQuests.Contains(questDef.GetValue(slate)))
            {
                return false;
            }
            return true;
        }
    }
}

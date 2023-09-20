using RimWorld.QuestGen;

namespace ReinforcedMechanoids
{
    public abstract class QuestNode_RunCondition : QuestNode
    {
        public abstract bool CanRun(Slate slate);
        public override void RunInt()
        {

        }
        public override bool TestRunInt(Slate slate)
        {
            return CanRun(slate);
        }
    }
}

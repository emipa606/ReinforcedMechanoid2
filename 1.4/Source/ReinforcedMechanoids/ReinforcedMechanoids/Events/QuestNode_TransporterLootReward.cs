using RimWorld;
using RimWorld.QuestGen;

namespace ReinforcedMechanoids
{
    public class QuestNode_TransporterLootReward : QuestNode
    {
        public override void RunInt()
        {
            QuestPart_Choice questPart_Choice = new QuestPart_Choice();
            QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice();
            var reward = new Reward_TransporterLoot();
            choice.rewards.Add(reward);
            questPart_Choice.choices.Add(choice);
            QuestGen.quest.AddPart(questPart_Choice);
        }

        public override bool TestRunInt(Slate slate)
        {
            return true;
        }
    }
}


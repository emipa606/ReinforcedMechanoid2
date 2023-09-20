using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace ReinforcedMechanoids
{
    public class QuestNode_EnsureRunAfterDaysPassed : QuestNode_RunCondition
    {
        public SlateRef<int> daysPassed;
        public override bool CanRun(Slate slate)
        {
            if (daysPassed.GetValue(slate) > GenDate.DaysPassed)
            {
                return false;
            }
            return true;
        }
    }
}

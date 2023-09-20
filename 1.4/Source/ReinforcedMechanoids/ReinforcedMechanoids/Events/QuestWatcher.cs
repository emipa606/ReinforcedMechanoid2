using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace ReinforcedMechanoids
{
    public class QuestWatcher : GameComponent
    {
        public QuestWatcher()
        {
            Instance = this;
        }

        public QuestWatcher(Game game)
        {
            Instance = this;
        }

        public static QuestWatcher Instance;

        public HashSet<QuestScriptDef> startedQuests;
        public bool PossibleToRunQuest(QuestScriptDef questScriptDef, Slate slate)
        {
            if (questScriptDef.root is QuestNode_Sequence sequence)
            {
                foreach (var node in sequence.nodes)
                {
                    if (node is QuestNode_RunCondition runCondition && !runCondition.CanRun(slate))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void PreInit()
        {
            if (this.startedQuests == null)
                this.startedQuests = new HashSet<QuestScriptDef>();
            Instance = this;
        }
        public override void LoadedGame()
        {
            base.LoadedGame();
            PreInit();
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            PreInit();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref startedQuests, "startedQuests", LookMode.Def);
            Instance = this;
        }
    }
}

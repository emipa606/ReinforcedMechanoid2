using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace ReinforcedMechanoids
{
    public class SitePartWorker_AncientShip : SitePartWorker
    {
        public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(RM_DefOf.RM_Remnants);
            if (part.site.Faction != faction)
            {
                if (faction == null) faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Neolithic);
                part.site.SetFaction(faction);
            }
            base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
        }
    }
}

using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace ReinforcedMechanoids
{
    public class SitePartWorker_MechanoidPresense : SitePartWorker
    {
        public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
        {
            base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
            int enemiesCount = GetEnemiesCount(part.site, part.parms);
            outExtraDescriptionRules.Add(new Rule_String("enemiesCount", enemiesCount.ToString()));
            outExtraDescriptionRules.Add(new Rule_String("enemiesLabel", GetEnemiesLabel(part.site, enemiesCount)));
        }

        public override SitePartParams GenerateDefaultParams(float myThreatPoints, int tile, Faction faction)
        {
            myThreatPoints *= 2f;
            SitePartParams sitePartParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
            sitePartParams.threatPoints = Mathf.Max(sitePartParams.threatPoints, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Settlement));
            return sitePartParams;
        }
        protected int GetEnemiesCount(Site site, SitePartParams parms)
        {
            return PawnGroupMakerUtility.GeneratePawnKindsExample(new PawnGroupMakerParms
            {
                tile = site.Tile,
                faction = site.Faction,
                groupKind = PawnGroupKindDefOf.Settlement,
                points = parms.threatPoints,
                inhabitants = true,
                seed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms)
            }).Count();
        }
        protected string GetEnemiesLabel(Site site, int enemiesCount)
        {
            if (site.Faction == null)
            {
                return (enemiesCount == 1) ? "Enemy".Translate() : "Enemies".Translate();
            }
            if (enemiesCount != 1)
            {
                return site.Faction.def.pawnsPlural;
            }
            return site.Faction.def.pawnSingular;
        }
    }
}

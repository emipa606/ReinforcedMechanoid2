using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace ReinforcedMechanoids
{
    [StaticConstructorOnStartup]
    public class Reward_TransporterLoot : Reward
    {
        private static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("UI/Overlays/QuestionMark");
        public override IEnumerable<GenUI.AnonymousStackElement> StackElements
        {
            get
            {
                yield return QuestPartUtility.GetStandardRewardStackElement("RM.Reward_TransporterLoot_Label".Translate(), 
                    Icon, () => GetDescription(default(RewardsGeneratorParams)).CapitalizeFirst() + ".");
            }
        }

        public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<QuestPart> GenerateQuestParts(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
        {
            throw new NotImplementedException();
        }

        public override string GetDescription(RewardsGeneratorParams parms)
        {
            return "RM.Reward_TransporterLoot_Desc".Translate().Resolve();
        }
    }
}


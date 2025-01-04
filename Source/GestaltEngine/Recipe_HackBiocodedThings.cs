using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GestaltEngine;

public class Recipe_HackBiocodedThings : RecipeWorker
{
    public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
    {
    }

    public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
    {
        base.Notify_IterationCompleted(billDoer, ingredients);
        var thing = ingredients.FirstOrDefault(x => x.TryGetComp<CompBiocodable>() != null);
        var num = 0.3f;
        var num2 = billDoer.skills.GetSkill(SkillDefOf.Intellectual)?.levelInt ?? 0;
        var num3 = num2 - 10;
        for (var i = 0; i < num3; i++)
        {
            num -= 0.03f;
        }

        var labelShort = thing.LabelShort;
        if (!Rand.Chance(num))
        {
            var compBiocodable = thing.TryGetComp<CompBiocodable>();
            compBiocodable.UnCode();
            Messages.Message("AC.HackingBiocodedSuccess".Translate(labelShort), thing, MessageTypeDefOf.PositiveEvent);
        }
        else
        {
            thing.Destroy();
            Messages.Message("AC.HackingBiocodedFailed".Translate(labelShort), billDoer,
                MessageTypeDefOf.NegativeEvent);
        }
    }
}
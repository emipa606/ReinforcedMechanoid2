using VEF.AnimalBehaviours;
using Verse;

namespace ReinforcedMechanoids;

[StaticConstructorOnStartup]
public static class MechanoidDraftCompInitializer
{
    static MechanoidDraftCompInitializer()
    {
        foreach (var pawn in DefDatabase<PawnKindDef>.AllDefs)
        {
            var compPropsMachine = pawn.race.GetCompProperties<CompProperties_Machine>();
            if (compPropsMachine is not { violent: true })
            {
                continue;
            }

            if (pawn.race.GetCompProperties<CompProperties_Draftable>() is null)
            {
                pawn.race.comps.Add(new CompProperties_Draftable());
            }
        }
    }
}
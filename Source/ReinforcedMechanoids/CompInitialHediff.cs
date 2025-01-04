using System.Linq;
using Verse;

namespace ReinforcedMechanoids;

public class CompInitialHediff : ThingComp
{
    public CompProperties_InitialHediff Props => props as CompProperties_InitialHediff;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (respawningAfterLoad)
        {
            return;
        }

        if (parent is not Pawn pawn)
        {
            return;
        }

        var part = Props.bodyPart != null
            ? pawn.RaceProps.body.GetPartsWithDef(Props.bodyPart).FirstOrDefault()
            : null;
        pawn.health.AddHediff(Props.hediffDef, part).Severity += Props.severity;
    }
}
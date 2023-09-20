using System.Linq;
using Verse;

namespace ReinforcedMechanoids
{
    public class CompProperties_InitialHediff : CompProperties
    {
        public HediffDef hediffDef;
        public float severity;
        public BodyPartDef bodyPart;
        public CompProperties_InitialHediff()
        {
            this.compClass = typeof(CompInitialHediff);
        }
    }
    public class CompInitialHediff : ThingComp
    {
        public CompProperties_InitialHediff Props => base.props as CompProperties_InitialHediff;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                var pawn = this.parent as Pawn;
                BodyPartRecord part = Props.bodyPart != null ? pawn.RaceProps.body.GetPartsWithDef(Props.bodyPart).FirstOrDefault() : null;
                var hediff = pawn.health.AddHediff(Props.hediffDef, part);
                hediff.Severity += Props.severity;
            }
        }
    }
}

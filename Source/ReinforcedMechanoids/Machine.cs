using RimWorld;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids;

public class Machine : Pawn, IRenameable
{
    private string MachineName { get; set; }

    public string RenamableLabel
    {
        get => MachineName ?? BaseLabel;
        set => MachineName = value;
    }

    public string BaseLabel => def.label;

    public string InspectLabel => RenamableLabel;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        mindState.lastJobTag = JobTag.Idle;
        guest = new Pawn_GuestTracker(this);
        if (drafter is null)
        {
            if (this.TryGetComp<CompMachine>().Props.violent)
            {
                drafter = new Pawn_DraftController(this);
            }
        }

        if (Faction == Faction.OfPlayer && Name == null)
        {
            Name = PawnBioAndNameGenerator.GeneratePawnName(this, NameStyle.Numeric);
        }
    }

    public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.PostApplyDamage(dinfo, totalDamageDealt);
        if (!health.Dead && (!health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) ||
                             !health.capacities.CapableOf(PawnCapacityDefOf.Moving)))
        {
            Kill(dinfo);
        }
    }
}
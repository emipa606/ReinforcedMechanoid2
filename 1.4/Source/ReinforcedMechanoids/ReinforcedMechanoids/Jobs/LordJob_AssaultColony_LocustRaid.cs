using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids
{
    public class LordJob_AssaultColony_LocustRaid : LordJob_AssaultColony_ProtectPawn
    {
        public LordJob_AssaultColony_LocustRaid()
        {

        }
        public LordJob_AssaultColony_LocustRaid(Faction assaulterFaction, bool canKidnap = true, bool canTimeoutOrFlee = true, bool sappers = false, bool useAvoidGridSmart = false, bool canSteal = true, bool breachers = false, bool canPickUpOpportunisticWeapons = false)
        {
            this.assaulterFaction = assaulterFaction;
            this.canKidnap = canKidnap;
            this.canTimeoutOrFlee = canTimeoutOrFlee;
            this.sappers = sappers;
            this.useAvoidGridSmart = useAvoidGridSmart;
            this.canSteal = canSteal;
            this.breachers = breachers;
            this.canPickUpOpportunisticWeapons = canPickUpOpportunisticWeapons;
        }

        public override PawnKindDef ProtecteeKind => RM_DefOf.RM_Mech_Locust;
        public override float MaxDistanceFromProtectee => 12;
    }
}

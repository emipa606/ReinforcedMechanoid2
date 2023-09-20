using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace ReinforcedMechanoids
{
    public class IncidentWorker_VultureFlock : IncidentWorker
    {
        public override bool CanFireNowSub(IncidentParms parms)
        {
            var map = parms.target as Map;
            var comp = map.GetComponent<MapComponent_Events>();
            if (comp.lastVultureFlockEventTick != 0 && (Find.TickManager.TicksGame - comp.lastVultureFlockEventTick) 
                < (GenDate.TicksPerDay * GenDate.DaysPerSeason))
            {
                return false;
            }
            var corpses = map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).Where(x => x is Corpse corpse
                && corpse.InnerPawn.RaceProps.IsMechanoid && corpse.InnerPawn.Faction.HostileTo(Faction.OfPlayer)).ToList();
            if (corpses.Count(x => map.areaManager.Home[x.Position] is false) < 4)
            {
                return false;
            }
            return base.CanFireNowSub(parms);
        }

        public override bool TryExecuteWorker(IncidentParms parms)
        {
            var vultureCount = Rand.RangeInclusive(4, 5);
            var vultures = new List<Pawn>();
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            for (var i = 0; i < vultureCount; i++)
            {
                var vulture = PawnGenerator.GeneratePawn(RM_DefOf.RM_Mech_Vulture, Faction.OfMechanoids);
                vultures.Add(vulture);
            }

            if (parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
            {
                var map = parms.target as Map;
                var corpses = map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).OfType<Corpse>()
                    .Where(x => x.InnerPawn.RaceProps.IsMechanoid && x.InnerPawn.Faction.HostileTo(Faction.OfPlayer)).ToList();
                parms.raidArrivalMode.Worker.Arrive(vultures, parms);
                LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_BreakDownMechanoids(corpses), map, vultures);
                var comp = map.GetComponent<MapComponent_Events>();
                comp.lastVultureFlockEventTick = Find.TickManager.TicksGame;
                SendStandardLetter(parms, vultures);
                return true;
            }
            return false;
        }
    }
}

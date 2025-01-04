using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace ReinforcedMechanoids;

public class IncidentWorker_VultureFlock : IncidentWorker
{
    public override bool CanFireNowSub(IncidentParms parms)
    {
        if (parms.target is not Map map)
        {
            return false;
        }

        var component = map.GetComponent<MapComponent_Events>();
        if (component.lastVultureFlockEventTick != 0 &&
            Find.TickManager.TicksGame - component.lastVultureFlockEventTick < 900000)
        {
            return false;
        }

        var list = (from x in map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse)
            where x is Corpse corpse && corpse.InnerPawn.RaceProps.IsMechanoid &&
                  corpse.InnerPawn.Faction.HostileTo(Faction.OfPlayer)
            select x).ToList();
        return list.Count(x => !map.areaManager.Home[x.Position]) >= 4 && base.CanFireNowSub(parms);
    }

    public override bool TryExecuteWorker(IncidentParms parms)
    {
        var num = Rand.RangeInclusive(4, 5);
        var list = new List<Pawn>();
        parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
        for (var i = 0; i < num; i++)
        {
            var item = PawnGenerator.GeneratePawn(RM_DefOf.RM_Mech_Vulture, Faction.OfMechanoids);
            list.Add(item);
        }

        if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
        {
            return false;
        }

        if (parms.target is not Map map)
        {
            return false;
        }

        var mechanoidCorpses = (from x in map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).OfType<Corpse>()
            where x.InnerPawn.RaceProps.IsMechanoid && x.InnerPawn.Faction.HostileTo(Faction.OfPlayer)
            select x).ToList();
        parms.raidArrivalMode.Worker.Arrive(list, parms);
        LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_BreakDownMechanoids(mechanoidCorpses), map, list);
        var component = map.GetComponent<MapComponent_Events>();
        component.lastVultureFlockEventTick = Find.TickManager.TicksGame;
        SendStandardLetter(parms, list);
        return true;
    }
}
using Verse;

namespace ReinforcedMechanoids;

public class Building_BedMachine : Building, IBedMachine
{
    public Pawn occupant
    {
        get
        {
            var pawn = this.TryGetComp<CompMachineChargingStation>()?.myPawn;
            return pawn?.Position == Position ? pawn : null;
        }
    }
}
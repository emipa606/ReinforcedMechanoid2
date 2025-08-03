using System.Collections.Generic;
using RimWorld;
using VEF.Pawns;
using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_MachineChargingStation : CompProperties_PawnDependsOn
{
    public List<string> blackListTurretGuns = new();
    public List<WorkGiverDef> disallowedWorkGivers;
    public bool draftable = false;
    public float extraChargingPower;
    public float hoursToRecharge = 24;
    public bool showSetArea = true;
    public int skillLevel = 5;
    public ThingDef spawnWithWeapon = null;
    public bool turret = false;

    public CompProperties_MachineChargingStation()
    {
        compClass = typeof(CompMachineChargingStation);
    }
}
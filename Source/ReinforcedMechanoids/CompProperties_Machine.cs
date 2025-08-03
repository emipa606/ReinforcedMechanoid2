using System.Collections.Generic;
using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_Machine : CompProperties
{
    public List<string> blackListTurretGuns = [];
    public bool canPickupWeapons;
    public bool canUseTurrets;
    public float hoursActive = 24;
    public bool violent;

    public CompProperties_Machine()
    {
        compClass = typeof(CompMachine);
    }
}
using System.Collections.Generic;
using Verse;

namespace GestaltEngine;

public class CompProperties_Upgradeable : CompProperties
{
    public List<Upgrade> upgrades;

    public CompProperties_Upgradeable()
    {
        compClass = typeof(CompUpgradeable);
    }
}
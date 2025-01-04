using System.Collections.Generic;
using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_LootContainer : CompProperties
{
    public readonly int openingTicks = 100;
    public List<LootTable> lootTables;

    public CompProperties_LootContainer()
    {
        compClass = typeof(CompLootContainer);
    }
}
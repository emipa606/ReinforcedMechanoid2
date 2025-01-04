using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_PawnProducer : CompProperties
{
    public FactionDef faction;

    public List<PawnKindDef> pawnKindToProduceOneOf;

    public IntRange spawnCountRange;

    public IntRange tickSpawnIntervalRange;

    public CompProperties_PawnProducer()
    {
        compClass = typeof(CompPawnProducer);
    }
}
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_Explosions : CompProperties_Explosive
{
    public readonly int cooldownTicks = -1;

    public bool anyDamageCausesExplosion;
    public List<CompProperties_Explosions> explosions;

    public float healthPctThreshold;

    public int maxExplosionCount;

    public BodyPartDef missingBodyPartTrigger;

    public IntRange? postExplosionSpawnThingCountRange;

    public int ticksDelay;

    public CompProperties_Explosions()
    {
        compClass = typeof(CompExplosions);
    }
}
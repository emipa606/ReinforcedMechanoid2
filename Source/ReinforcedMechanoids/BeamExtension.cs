using Verse;

namespace ReinforcedMechanoids;

public class BeamExtension : DefModExtension
{
    public readonly int duration = 300;

    public readonly int firesStartedPerTick = 4;

    public readonly float radius = 2f;

    public IntRange corpseFlameDamageAmountRange = new(5, 10);

    public IntRange flameDamageAmountRange = new(65, 100);

    public SoundDef sustainerSound;
}
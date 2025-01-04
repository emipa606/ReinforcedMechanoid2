using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_PlaySoundInInterval : CompProperties
{
    public IntRange intervalRange;
    public SoundDef sound;

    public CompProperties_PlaySoundInInterval()
    {
        compClass = typeof(CompPlaySoundInInterval);
    }
}
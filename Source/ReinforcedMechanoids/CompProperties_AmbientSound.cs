using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_AmbientSound : CompProperties
{
    public SoundDef ambientSound;

    public CompProperties_AmbientSound()
    {
        compClass = typeof(CompAmbientSound);
    }
}
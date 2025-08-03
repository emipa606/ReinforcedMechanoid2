using Verse;
using Verse.Sound;

namespace ReinforcedMechanoids;

public class CompPlaySoundInInterval : ThingComp
{
    private int ticksToNextCall = -1;

    private CompProperties_PlaySoundInInterval Props => props as CompProperties_PlaySoundInInterval;

    public override void CompTick()
    {
        base.CompTick();
        if (ticksToNextCall < 0)
        {
            ticksToNextCall = Props.intervalRange.RandomInRange;
        }

        ticksToNextCall--;
        if (ticksToNextCall > 0)
        {
            return;
        }

        if (parent.Spawned && Find.CameraDriver.CurrentViewRect.ExpandedBy(10).Contains(parent.Position) &&
            !parent.Position.Fogged(parent.Map))
        {
            var info = SoundInfo.InMap(new TargetInfo(parent.Position, parent.Map));
            Props.sound.PlayOneShot(info);
        }

        ticksToNextCall = Props.intervalRange.RandomInRange;
    }
}
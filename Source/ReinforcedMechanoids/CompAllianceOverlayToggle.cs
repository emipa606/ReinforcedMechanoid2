using Verse;

namespace ReinforcedMechanoids;

public class CompAllianceOverlayToggle : ThingComp
{
    private bool isActive = true;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref isActive, "isActive", true);
    }
}
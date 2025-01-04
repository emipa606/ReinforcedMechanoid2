using System.Collections.Generic;
using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_ChangeGraphic : CompProperties
{
    public List<PawnGraphics> graphicsPerLifestages;

    public CompProperties_ChangeGraphic()
    {
        compClass = typeof(CompChangePawnGraphic);
    }
}
using Verse;

namespace ReinforcedMechanoids;

public class CompProperties_RemoveEquipment : CompProperties
{
    public float healthPctThreshold;

    public HediffDef removeHediff;

    public bool removePrimaryEquipment;

    public CompProperties_RemoveEquipment()
    {
        compClass = typeof(CompRemoveEquipment);
    }
}
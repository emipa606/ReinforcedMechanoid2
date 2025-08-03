using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace ReinforcedMechanoids;

public class PawnGroupMaker_CaretakerRaidWithMechPresence : PawnGroupMaker
{
    private static readonly MethodInfo MechPresenceInfo = AccessTools.Method("VFEMech.MechUtils:MechPresence");

    private readonly int maximumPresence = int.MaxValue;
    private readonly int minimumPresence = 0;

    public bool CanGenerate(PawnGroupMakerParms parms)
    {
        if (MechPresenceInfo == null)
        {
            return false;
        }

        var num = (int)MechPresenceInfo.Invoke(null, null);
        return num > minimumPresence && num < maximumPresence;
    }
}
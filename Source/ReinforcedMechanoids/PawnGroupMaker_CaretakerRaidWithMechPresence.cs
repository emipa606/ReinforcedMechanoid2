using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace ReinforcedMechanoids;

public class PawnGroupMaker_CaretakerRaidWithMechPresence : PawnGroupMaker
{
    public static readonly MethodInfo MechPresenceInfo = AccessTools.Method("VFEMech.MechUtils:MechPresence");

    public readonly int maximumPresence = int.MaxValue;
    public readonly int minimumPresence = 0;

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
using HarmonyLib;
using RimWorld;
using System.Reflection;

namespace ReinforcedMechanoids
{
    public class PawnGroupMaker_CaretakerRaid : PawnGroupMaker
    {

    }

    public class PawnGroupMaker_WraithSiege : PawnGroupMaker
    {

    }

    public class PawnGroupMaker_LocustRaid : PawnGroupMaker
    {

    }

    public class PawnGroupMaker_CaretakerRaidWithMechPresence : PawnGroupMaker
    {
        public int minimumPresence = 0;
        public int maximumPresence = int.MaxValue;
        public static MethodInfo MechPresenceInfo = AccessTools.Method("VFEMech.MechUtils:MechPresence");
        public bool CanGenerate(PawnGroupMakerParms parms)
        {
            if (MechPresenceInfo != null)
            {
                int mechPresence = (int)MechPresenceInfo.Invoke(null, null);
                return mechPresence > this.minimumPresence && mechPresence < this.maximumPresence;
            }
            return false;
        }
    }
}

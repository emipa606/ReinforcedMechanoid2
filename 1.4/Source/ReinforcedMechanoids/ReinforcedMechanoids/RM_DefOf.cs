using RimWorld;
using System.Reflection;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids
{
    [DefOf]
	public static class RM_DefOf
    {
		public static PawnKindDef RM_Mech_Caretaker;
		public static PawnKindDef RM_Mech_Vulture;
		public static PawnKindDef RM_Mech_Behemoth;
        public static PawnKindDef RM_Mech_WraithSiege;
        public static PawnKindDef RM_Mech_Locust;
        public static JobDef RM_RepairMechanoid;
		public static JobDef RM_RepairThing;
		public static JobDef RM_FollowClose;
		public static BodyPartDef RM_BehemothShield;
		public static ThingDef RM_VanometricGenerator;
		public static ThingDef RM_VanometricMechanoidCell;
		public static HediffDef RM_BehemothAttack;
        public static RaidStrategyDef RM_CaretakerRaid;
        public static RaidStrategyDef RM_WraithSiege;
        public static RaidStrategyDef RM_LocustRaid;
        public static ThingDef RM_JumpPawn;
        public static HediffDef RM_SentinelBerserk;
        public static DutyDef RM_BreakDownMechanoids;
        public static JobDef RM_BreakDownMechanoid;
        public static FactionDef RM_Remnants;
        public static ThingDef RM_LocustBeam;
        public static ThingDef RM_Mote_LocustBeam;
        public static HediffDef RM_LocustBeamEffect;
        public static ThingDef RM_AncientTrooperStorage;
        //public static SitePartDef RM_AncientWarship;
        //public static SitePartDef RM_MechanoidPresense;
        public static DutyDef RM_Build;
        public static TerrainDef MarshyTerrain;
        public static EffecterDef RM_JumpPawnEffect;
        public static HediffDef RM_ZealotInvisibility;
        // reclamation stuff (unused)
        //public static JobDef RM_HackMechanoidCorpseAtMechanoidStation;
        //public static JobDef RM_RepairPlayerMechanoid;
        //public static ThinkTreeDef VFE_Mechanoids_Machine_RiddableConstant;
        //public static ThinkTreeDef Downed;
        //public static ThinkTreeDef RM_MechanoidHacked_Behaviour;
        //public static ThinkTreeDef JoinAutoJoinableCaravan;
        //public static ThinkTreeDef LordDutyConstant;
        //public static DesignationDef RM_HackMechanoid;
        //public static EffecterDef RM_Hacking;
        //public static SoundDef Recipe_Machining;
        //public static HediffDef RM_ImprovisedRepairs;
    }
}

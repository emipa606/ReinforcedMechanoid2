using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using UnityEngine;
using VEF.Pawns;
using Verse;

namespace ReinforcedMechanoids;

public class CompMachineChargingStation : CompPawnDependsOn
{
    public static readonly List<CompMachineChargingStation> cachedChargingStations = [];
    public static readonly Dictionary<Thing, CompMachineChargingStation> cachedChargingStationsDict = new();
    private Area allowedArea;
    private CompPowerTrader compPower;
    public bool energyDrainMode = true;
    public bool forceStay;
    public bool wantsRespawn; //Used to determine whether a rebuild job is desired
    public bool wantsRest; //Used to force a machine to return to base, for healing or recharging

    private CompPowerTrader PowerComp
    {
        get
        {
            compPower ??= parent.TryGetComp<CompPowerTrader>();

            return compPower;
        }
    }

    public new CompProperties_MachineChargingStation Props => (CompProperties_MachineChargingStation)props;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        cachedChargingStations.Add(this);
        cachedChargingStationsDict.Add(parent, this);
        if (!respawningAfterLoad)
        {
            SpawnMyPawn();
        }
        else
        {
            CheckWantsRespawn();
        }

        if (myPawn != null && myPawn.Position == parent.Position &&
            myPawn.needs.TryGetNeed<Need_Power>().CurLevel >= 0.99f)
        {
            StopEnergyDrain();
        }
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);
        cachedChargingStations.Remove(this);
        cachedChargingStationsDict.Remove(parent);
    }

    public override void SpawnMyPawn()
    {
        base.SpawnMyPawn();
        myPawn.story ??= new Pawn_StoryTracker(myPawn);

        myPawn.skills ??= new Pawn_SkillTracker(myPawn);

        myPawn.workSettings ??= new Pawn_WorkSettings(myPawn);

        myPawn.relations ??= new Pawn_RelationsTracker(myPawn);

        var priorities = new DefMap<WorkTypeDef, int>();
        priorities.SetAll(0);
        typeof(Pawn_WorkSettings).GetField("priorities", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(myPawn.workSettings, priorities);

        if (myPawn.def.race.mechEnabledWorkTypes != null)
        {
            foreach (var workType in myPawn.def.race.mechEnabledWorkTypes)
            {
                foreach (var skill in workType.relevantSkills)
                {
                    var record = myPawn.skills.skills.Find(rec => rec.def == skill);
                    record.levelInt = Props.skillLevel;
                }

                myPawn.workSettings.SetPriority(workType, 1);
            }
        }

        if (myPawn.TryGetComp<CompMachine>().Props.violent)
        {
            myPawn.drafter ??= new Pawn_DraftController(myPawn);

            if (Props.spawnWithWeapon != null)
            {
                var thing = (ThingWithComps)ThingMaker.MakeThing(Props.spawnWithWeapon);
                myPawn.equipment.AddEquipment(thing);
            }
        }

        if (myPawn.needs.TryGetNeed<Need_Power>() == null)
        {
            typeof(Pawn_NeedsTracker).GetMethod("AddNeed", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(myPawn.needs, [DefDatabase<NeedDef>.GetNamed("VFE_Mechanoids_Power")]);
        }

        myPawn.needs.TryGetNeed<Need_Power>().CurLevel = 0;

        if (myPawn.playerSettings != null)
        {
            myPawn.playerSettings.AreaRestrictionInPawnCurrentMap = allowedArea;
        }

        wantsRespawn = false;
    }


    public override void CompTickRare()
    {
        base.CompTickRare();
        var bed = (IBedMachine)parent;
        var consumption = PowerComp.Props.PowerConsumption;

        if (bed.occupant != null)
        {
            if (energyDrainMode)
            {
                PowerComp.powerOutputInt = 0 - consumption - Props.extraChargingPower;
            }

            if (myPawn.health.hediffSet.HasNaturallyHealingInjury() && parent.TryGetComp<CompPowerTrader>().PowerOn)
            {
                var num3 = 12f;

                var hediffsInjury = new List<Hediff_Injury>();
                myPawn.health.hediffSet.GetHediffs(ref hediffsInjury);
                (from x in hediffsInjury where x.CanHealNaturally() select x).RandomElement()
                    .Heal(num3 * myPawn.HealthScale * 0.01f);
            }
        }
        else if (energyDrainMode)
        {
            PowerComp.powerOutputInt = 0 - consumption;
        }

        CheckWantsRespawn();
    }

    private void CheckWantsRespawn()
    {
        wantsRespawn = !MyPawnIsAlive;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref wantsRest, "wantsRest");
        Scribe_Values.Look(ref forceStay, "forceStay");
        Scribe_Values.Look(ref energyDrainMode, "energyDrainMode", true);
        Scribe_References.Look(ref allowedArea, "allowedArea");
    }

    public void StopEnergyDrain()
    {
        if (!(myPawn.needs.TryGetNeed<Need_Power>().CurLevel >= 0.99f))
        {
            return;
        }

        PowerComp.powerOutputInt = -1;
        energyDrainMode = false;
    }

    public void StartEnergyDrain()
    {
        if (!(myPawn.needs.TryGetNeed<Need_Power>().CurLevel < 0.99f))
        {
            return;
        }

        energyDrainMode = true;
        PowerComp.powerOutputInt = -PowerComp.Props.PowerConsumption;
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var g in base.CompGetGizmosExtra())
        {
            yield return g;
        }

        var forceRest = new Command_Toggle
        {
            defaultLabel = "VFEMechForceRecharge".Translate(),
            defaultDesc = "VFEMechForceRechargeDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/ForceRecharge"),
            toggleAction = delegate
            {
                foreach (var t in Find.Selector.SelectedObjects)
                {
                    if (t is not ThingWithComps thing ||
                        thing.TryGetComp<CompMachineChargingStation>() is not { } comp)
                    {
                        continue;
                    }

                    if (comp.forceStay)
                    {
                        comp.wantsRest = false;
                        comp.forceStay = false;
                    }
                    else
                    {
                        comp.wantsRest = true;
                        comp.forceStay = true;
                        var job = JobMaker.MakeJob(RM_DefOf.VFE_Mechanoids_Recharge, comp.parent);
                        comp.myPawn.jobs.StopAll();
                        comp.myPawn.jobs.TryTakeOrderedJob(job);
                    }
                }
            },
            isActive = () => forceStay
        };
        yield return forceRest;

        if (!Props.showSetArea)
        {
            yield break;
        }

        var setArea = new Command_Action
        {
            defaultLabel = "VFEMechSetArea".Translate(),
            defaultDesc = "VFEMechSetAreaDesc".Translate(),
            action = delegate
            {
                AreaUtility.MakeAllowedAreaListFloatMenu(delegate(Area area)
                {
                    foreach (var t in Find.Selector.SelectedObjects)
                    {
                        if (t is not ThingWithComps thing ||
                            thing.TryGetComp<CompMachineChargingStation>() is not { } comp)
                        {
                            continue;
                        }

                        comp.allowedArea = area;
                        if (comp.myPawn is { Dead: false })
                        {
                            comp.myPawn.playerSettings.AreaRestrictionInPawnCurrentMap = area;
                        }
                    }
                }, true, true, parent.Map);
            },
            icon = ContentFinder<Texture2D>.Get("UI/SelectZone")
        };
        yield return setArea;
    }

    public override string CompInspectStringExtra()
    {
        var builder = new StringBuilder(base.CompInspectStringExtra());
        if (Props.pawnToSpawn == null || myPawn is { Dead: false })
        {
            return builder.ToString().Trim();
        }

        var comma = false;
        string resources = "VFEMechReconstruct".Translate() + " ";
        foreach (var resource in Props.pawnToSpawn.race.butcherProducts)
        {
            if (comma)
            {
                resources += ", ";
            }

            comma = true;
            resources += $"{resource.thingDef.label} x{resource.count}";
        }

        builder.AppendLine(resources);

        return builder.ToString().Trim();
    }
}
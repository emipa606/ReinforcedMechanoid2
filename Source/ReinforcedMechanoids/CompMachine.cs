using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using VEF.AnimalBehaviours;
using VEF.Pawns;
using Verse;

namespace ReinforcedMechanoids;

public class CompMachine : CompDependsOnBuilding, PawnGizmoProvider
{
    private static readonly Dictionary<CompMachine, Pawn> cachedPawns = new();
    public static readonly Dictionary<Pawn, CompMachine> cachedMachinesPawns = new();
    public float turretAngle; //Purely cosmetic, don't need to save it
    private float turretAnglePerFrame = 0.1f;
    public ThingDef turretAttached;
    public ThingDef turretToInstall; //Used to specify a turret to put on the mobile turret

    public new CompProperties_Machine Props => props as CompProperties_Machine;

    private CompProperties_MachineChargingStation StationProps
    {
        get
        {
            var comp = myBuilding?.GetComp<CompMachineChargingStation>();
            return comp?.Props;
        }
    }

    public IEnumerable<Gizmo> GetGizmos()
    {
        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action
            {
                defaultLabel = "Recharge fully",
                action = delegate { ((Pawn)parent).needs.TryGetNeed<Need_Power>().CurLevel = 1; }
            };
        }

        if (!CanHaveTurret())
        {
            yield break;
        }

        var attachTurret = new Command_Action
        {
            defaultLabel = "VFEMechAttachTurret".Translate(),
            defaultDesc = "VFEMechAttachTurretDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/AttachTurret"),
            action = delegate
            {
                var options = new List<FloatMenuOption>();
                foreach (var thing in DefDatabase<ThingDef>.AllDefs.Where(IsTurretAllowed))
                {
                    var opt = new FloatMenuOption(thing.label, delegate
                    {
                        turretToInstall = thing;
                        var comp = myBuilding.GetComp<CompMachineChargingStation>();
                        if (comp != null)
                        {
                            comp.wantsRest = true;
                        }
                    }, thing.building.turretGunDef);
                    options.Add(opt);
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }
        };
        yield return attachTurret;
    }

    public override void OnBuildingDestroyed(CompPawnDependsOn compPawnDependsOn)
    {
        base.OnBuildingDestroyed(compPawnDependsOn);
        if (compPawnDependsOn.Props.killPawnAfterDestroying)
        {
            parent.Kill();
        }
    }

    public void AttachTurret()
    {
        if (turretAttached != null)
        {
            foreach (var stack in turretAttached.costList)
            {
                var thing = ThingMaker.MakeThing(stack.thingDef);
                thing.stackCount = stack.count;
                GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, ThingPlaceMode.Near);
            }

            ((Pawn)parent).equipment.DestroyAllEquipment();
        }

        turretAttached = turretToInstall;
        var turretThing = ThingMaker.MakeThing(turretToInstall.building.turretGunDef);
        ((Pawn)parent).equipment.AddEquipment((ThingWithComps)turretThing);
        turretToInstall = null;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Defs.Look(ref turretAttached, "turretAttached");
        Scribe_Defs.Look(ref turretToInstall, "turretToInstall");
    }

    public override void CompTick()
    {
        base.CompTick();
        if (turretAttached != null)
        {
            turretAngle += turretAnglePerFrame;
        }
    }

    public override void CompTickRare()
    {
        base.CompTickRare();
        turretAnglePerFrame = Rand.Range(-0.5f, 0.5f);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        cachedPawns.Add(this, (Pawn)parent);
        cachedMachinesPawns.Add((Pawn)parent, this);
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);
        cachedPawns.Remove(this);
        cachedMachinesPawns.Remove((Pawn)parent);
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        base.PostDestroy(mode, previousMap);
        cachedPawns.Remove(this);
        cachedMachinesPawns.Remove((Pawn)parent);
    }

    public override string CompInspectStringExtra()
    {
        var builder = new StringBuilder(base.CompInspectStringExtra());
        if (turretToInstall == null)
        {
            return builder.ToString().TrimEndNewlines();
        }

        var comma = false;
        string resources = "VFEMechTurretResources".Translate() + " ";
        foreach (var resource in turretToInstall.costList)
        {
            if (comma)
            {
                resources += ", ";
            }

            comma = true;
            resources += $"{resource.thingDef.label} x{resource.count}";
        }

        builder.AppendLine(resources);

        return builder.ToString().TrimEndNewlines();
    }

    private bool IsTurretAllowed(ThingDef t)
    {
        if (t.building == null || t.building.turretGunDef == null || t.costList == null
            || t.GetCompProperties<CompProperties_Mannable>() != null || t.size.x > 3 || t.size.z > 3 ||
            !t.IsResearchFinished)
        {
            return false;
        }

        var stationProps = StationProps;
        if (stationProps?.blackListTurretGuns != null &&
            stationProps.blackListTurretGuns.Contains(t.building.turretGunDef.defName))
        {
            return false;
        }

        return Props.blackListTurretGuns == null ||
               !Props.blackListTurretGuns.Contains(t.building.turretGunDef.defName);
    }

    private bool CanHaveTurret()
    {
        var stationProps = StationProps;
        return stationProps is { turret: true } || Props.canUseTurrets;
    }
}
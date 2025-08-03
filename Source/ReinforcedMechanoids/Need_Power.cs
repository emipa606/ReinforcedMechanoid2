using RimWorld;
using Verse;

namespace ReinforcedMechanoids;

public class Need_Power : Need
{
    private CompMachineChargingStation compChargingMachine;

    private CompMachine compMachine;
    private int lastRestTick = -999;

    public Need_Power(Pawn pawn)
        : base(pawn)
    {
        threshPercents =
        [
            0.28f,
            0.14f
        ];
    }

    private float RestFallFactor => pawn.health.hediffSet.RestFallFactor;

    private CompMachine MachineComp
    {
        get
        {
            if (compMachine is null && CompMachine.cachedMachinesPawns.TryGetValue(pawn, out var comp))
            {
                compMachine = comp;
            }

            return compMachine;
        }
    }

    private CompMachineChargingStation ChargingStationComp
    {
        get
        {
            compChargingMachine ??= MachineComp?.myBuilding?.TryGetComp<CompMachineChargingStation>();

            return compChargingMachine;
        }
    }

    private bool Enabled => MachineComp != null;

    public override bool ShowOnNeedList => Enabled;

    public RestCategory CurCategory
    {
        get
        {
            if (CurLevel < 0.01f)
            {
                return RestCategory.Exhausted;
            }

            if (CurLevel < 0.14f)
            {
                return RestCategory.VeryTired;
            }

            return CurLevel < 0.28f ? RestCategory.Tired : RestCategory.Rested;
        }
    }

    private float RestFallPerTick => 1 / MachineComp.Props.hoursActive / 2500;

    public override int GUIChangeArrow
    {
        get
        {
            if (Resting)
            {
                return 1;
            }

            return -1;
        }
    }

    private bool Resting => Find.TickManager.TicksGame < lastRestTick + 2;

    public override void SetInitialLevel()
    {
        CurLevel = 1f;
    }

    public override void NeedInterval()
    {
        var compChargingStation = ChargingStationComp;
        if (compChargingStation != null)
        {
            compChargingStation.myPawn ??= pawn;

            if (pawn.Position == compChargingStation.parent?.Position)
            {
                if (CurLevel >= 0.99f && compChargingStation.energyDrainMode)
                {
                    compChargingStation.StopEnergyDrain();
                }
                else if (CurLevel < 0.99f && !compChargingStation.energyDrainMode)
                {
                    compChargingStation.StartEnergyDrain();
                }
            }
            else if (CurLevel < 0.99f && !compChargingStation.energyDrainMode)
            {
                compChargingStation.StartEnergyDrain();
            }
        }

        if (!Enabled || IsFrozen)
        {
            return;
        }

        if (Resting && compChargingStation != null)
        {
            var num = 1f;
            num *= pawn.GetStatValue(StatDefOf.RestRateMultiplier);
            if (num > 0f)
            {
                CurLevel += 1 / compChargingStation.Props.hoursToRecharge / 2500 * num * 150f;
            }
        }
        else
        {
            if (compChargingStation?.parent != null && pawn.Position != compChargingStation.parent.Position)
            {
                CurLevel -= RestFallPerTick * 150f;
            }
        }
    }

    public void TickResting(float restEffectiveness)
    {
        if (Enabled && !(restEffectiveness <= 0f))
        {
            lastRestTick = Find.TickManager.TicksGame;
        }
    }
}
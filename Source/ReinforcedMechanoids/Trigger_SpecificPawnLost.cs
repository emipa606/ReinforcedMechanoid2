using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReinforcedMechanoids;

public class Trigger_SpecificPawnLost(
    float chance = 1f,
    bool requireInstigatorWithFaction = false,
    Faction requireInstigatorWithSpecificFaction = null,
    List<Pawn> requireSpecificPawn = null,
    DutyDef skipDuty = null,
    int? minTicks = null)
    : Trigger
{
    public readonly float chance = chance;

    public readonly bool requireInstigatorWithFaction = requireInstigatorWithFaction;

    public readonly Faction requireInstigatorWithSpecificFaction = requireInstigatorWithSpecificFaction;

    public readonly List<Pawn> requireSpecificPawn = requireSpecificPawn;

    public readonly DutyDef skipDuty = skipDuty;

    public int? minTick;

    public int? minTicks = minTicks;

    public override void SourceToilBecameActive(Transition transition, LordToil previousToil)
    {
        base.SourceToilBecameActive(transition, previousToil);
        if (minTicks != null)
        {
            minTick = GenTicks.TicksGame + minTicks.Value;
        }
    }

    public override bool ActivateOn(Lord lord, TriggerSignal signal)
    {
        if (requireSpecificPawn != null && !SignalIsLost(signal, requireSpecificPawn))
        {
            return false;
        }

        if (requireInstigatorWithFaction && signal.dinfo.Instigator?.Faction == null)
        {
            return false;
        }

        if (requireInstigatorWithSpecificFaction != null && (signal.dinfo.Instigator == null ||
                                                             signal.dinfo.Instigator.Faction !=
                                                             requireInstigatorWithSpecificFaction))
        {
            return false;
        }

        Pawn pawn;
        if ((pawn = signal.dinfo.IntendedTarget as Pawn) == null)
        {
            return !(minTick != null && GenTicks.TicksGame < minTick.Value) && Rand.Value < chance;
        }

        var mindState = pawn.mindState;
        DutyDef dutyDef;
        if (mindState == null)
        {
            dutyDef = null;
        }
        else
        {
            var duty = mindState.duty;
            dutyDef = duty?.def;
        }

        if (dutyDef == skipDuty)
        {
            return false;
        }

        return !(minTick != null && GenTicks.TicksGame < minTick.Value) && Rand.Value < chance;
    }

    private static bool SignalIsLost(TriggerSignal signal, List<Pawn> pawn)
    {
        if (signal.Pawn != null && !pawn.Contains(signal.Pawn))
        {
            return false;
        }

        if (signal.type == TriggerSignalType.PawnLost)
        {
            return !(signal.condition != PawnLostCondition.MadePrisoner &&
                     signal.condition != PawnLostCondition.Incapped) || signal.condition == PawnLostCondition.Killed;
        }

        return signal.type == TriggerSignalType.PawnArrestAttempted;
    }
}
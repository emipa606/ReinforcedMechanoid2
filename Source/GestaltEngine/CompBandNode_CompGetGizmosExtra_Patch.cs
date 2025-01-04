using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GestaltEngine;

[HarmonyPatch(typeof(CompBandNode), "CompGetGizmosExtra")]
public static class CompBandNode_CompGetGizmosExtra_Patch
{
    public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, CompBandNode __instance)
    {
        foreach (var gizmo in __result)
        {
            if (gizmo is Command_Action command &&
                command.icon == ContentFinder<Texture2D>.Get("UI/Gizmos/BandNodeTuning"))
            {
                var command_Action = new Command_Action
                {
                    defaultLabel = __instance.tunedTo == null
                        ? "BandNodeTuneTo".Translate() + "..."
                        : "BandNodeRetuneTo".Translate() + "...",
                    defaultDesc = __instance.tunedTo == null
                        ? "BandNodeTuningDesc".Translate("PeriodSeconds".Translate(__instance.Props.tuneSeconds))
                        : "BandNodeRetuningDesc".Translate(__instance.Props.retuneDays + " " + "Days".Translate())
                };
                command_Action.onHover = (Action)Delegate.Combine(command_Action.onHover, (Action)delegate
                {
                    var pawn = __instance.tuningTo ?? __instance.tunedTo;
                    if (pawn == null)
                    {
                        return;
                    }

                    GenDraw.DrawLineBetween(__instance.parent.DrawPos,
                        pawn.TryGetGestaltEngineInstead(out var result) ? result.parent.DrawPos : pawn.DrawPos);
                });
                var foundMechanitor = false;
                foreach (var item in __instance.parent.Map.mapPawns.AllPawnsSpawned)
                {
                    if (!MechanitorUtility.IsMechanitor(item) || item == __instance.tunedTo)
                    {
                        continue;
                    }

                    foundMechanitor = true;
                    break;
                }

                foreach (var comp in CompGestaltEngine.compGestaltEngines)
                {
                    if (comp.parent.Map != __instance.parent.Map || !MechanitorUtility.IsMechanitor(comp.dummyPawn) ||
                        comp.dummyPawn == __instance.tunedTo)
                    {
                        continue;
                    }

                    foundMechanitor = true;
                    break;
                }

                command_Action.Disabled = !foundMechanitor;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/BandNodeTuning");
                command_Action.action = (Action)Delegate.Combine(command_Action.action, (Action)delegate
                {
                    var list = new List<FloatMenuOption>();
                    foreach (var item2 in __instance.parent.Map.mapPawns.AllPawnsSpawned)
                    {
                        if (!MechanitorUtility.IsMechanitor(item2) || item2 == __instance.tunedTo)
                        {
                            continue;
                        }

                        var localPawn2 = item2;
                        var toStringFull = item2.Name.ToStringFull;
                        toStringFull = __instance.tunedTo != null
                            ? toStringFull + " (" + __instance.RetuneTimeTicks.ToStringTicksToPeriod() + ")"
                            : toStringFull + (" (" + __instance.Props.tuneSeconds + " " +
                                              "SecondsLower".Translate() + ")");
                        list.Add(new FloatMenuOption(toStringFull, delegate
                        {
                            __instance.tuningTimeLeft = __instance.tunedTo == null
                                ? __instance.TuningTimeTicks
                                : __instance.RetuneTimeTicks;
                            __instance.tuningTo = localPawn2;
                        }));
                    }

                    foreach (var compGestaltEngine in CompGestaltEngine.compGestaltEngines)
                    {
                        if (compGestaltEngine.parent.Map != __instance.parent.Map ||
                            !MechanitorUtility.IsMechanitor(compGestaltEngine.dummyPawn) ||
                            compGestaltEngine.dummyPawn == __instance.tunedTo)
                        {
                            continue;
                        }

                        var localPawn = compGestaltEngine.dummyPawn;
                        var toStringFull2 = compGestaltEngine.dummyPawn.Name.ToStringFull;
                        toStringFull2 = __instance.tunedTo != null
                            ? toStringFull2 + " (" + __instance.RetuneTimeTicks.ToStringTicksToPeriod() + ")"
                            : toStringFull2 + (" (" + __instance.Props.tuneSeconds + " " +
                                               "SecondsLower".Translate() + ")");
                        list.Add(new FloatMenuOption(toStringFull2, delegate
                        {
                            __instance.tuningTimeLeft = __instance.tunedTo == null
                                ? __instance.TuningTimeTicks
                                : __instance.RetuneTimeTicks;
                            __instance.tuningTo = localPawn;
                        }));
                    }

                    Find.WindowStack.Add(new FloatMenu(list));
                });
                yield return command_Action;
            }
            else
            {
                yield return gizmo;
            }
        }
    }
}
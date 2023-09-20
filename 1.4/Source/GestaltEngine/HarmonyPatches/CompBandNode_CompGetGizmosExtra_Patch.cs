using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace GestaltEngine
{
    [HarmonyPatch(typeof(CompBandNode), "CompGetGizmosExtra")]
    public static class CompBandNode_CompGetGizmosExtra_Patch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, CompBandNode __instance)
        {
            foreach (Gizmo gizmo in __result)
            {
                if (gizmo is Command_Action command && command.icon == ContentFinder<Texture2D>.Get("UI/Gizmos/BandNodeTuning"))
                {
                    Command_Action command_Action = new Command_Action();
                    command_Action.defaultLabel = ((__instance.tunedTo == null) ? ("BandNodeTuneTo".Translate() + "...") : ("BandNodeRetuneTo".Translate() + "..."));
                    command_Action.defaultDesc = ((__instance.tunedTo == null) ? "BandNodeTuningDesc".Translate("PeriodSeconds".Translate(__instance.Props.tuneSeconds)) : "BandNodeRetuningDesc".Translate(__instance.Props.retuneDays + " " + "Days".Translate()));
                    command_Action.onHover = (Action)Delegate.Combine(command_Action.onHover, (Action)delegate
                    {
                        Pawn pawn = ((__instance.tuningTo != null) ? __instance.tuningTo : __instance.tunedTo);
                        if (pawn != null)
                        {
                            if (pawn.TryGetGestaltEngineInstead(out var comp))
                            {
                                GenDraw.DrawLineBetween(__instance.parent.DrawPos, comp.parent.DrawPos);
                            }
                            else
                            {
                                GenDraw.DrawLineBetween(__instance.parent.DrawPos, pawn.DrawPos);
                            }
                        }
                    });
                    bool flag = false;

                    foreach (Pawn item in __instance.parent.Map.mapPawns.AllPawnsSpawned)
                    {
                        if (MechanitorUtility.IsMechanitor(item) && item != __instance.tunedTo)
                        {
                            flag = true;
                            break;
                        }
                    }
                    foreach (var comp in CompGestaltEngine.compGestaltEngines)
                    {
                        if (comp.parent.Map == __instance.parent.Map)
                        {
                            if (MechanitorUtility.IsMechanitor(comp.dummyPawn) && comp.dummyPawn != __instance.tunedTo)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }

                    command_Action.disabled = !flag;
                    command_Action.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/BandNodeTuning");
                    command_Action.action = (Action)Delegate.Combine(command_Action.action, (Action)delegate
                    {
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        foreach (Pawn item2 in __instance.parent.Map.mapPawns.AllPawnsSpawned)
                        {
                            if (MechanitorUtility.IsMechanitor(item2) && item2 != __instance.tunedTo)
                            {
                                Pawn localPawn = item2;
                                string toStringFull = item2.Name.ToStringFull;
                                toStringFull = ((__instance.tunedTo != null) ? (toStringFull + " (" + __instance.RetuneTimeTicks.ToStringTicksToPeriod() + ")") : ((string)(toStringFull + (" (" + __instance.Props.tuneSeconds + " " + "SecondsLower".Translate() + ")"))));
                                list.Add(new FloatMenuOption(toStringFull, delegate
                                {
                                    __instance.tuningTimeLeft = ((__instance.tunedTo == null) ? __instance.TuningTimeTicks : __instance.RetuneTimeTicks);
                                    __instance.tuningTo = localPawn;
                                }));
                            }
                        }
                        foreach (var comp in CompGestaltEngine.compGestaltEngines)
                        {
                            if (comp.parent.Map == __instance.parent.Map)
                            {
                                if (MechanitorUtility.IsMechanitor(comp.dummyPawn) && comp.dummyPawn != __instance.tunedTo)
                                {
                                    Pawn localPawn = comp.dummyPawn;
                                    string toStringFull = comp.dummyPawn.Name.ToStringFull;
                                    toStringFull = ((__instance.tunedTo != null) ? (toStringFull + " (" + __instance.RetuneTimeTicks.ToStringTicksToPeriod() + ")") : ((string)(toStringFull + (" (" + __instance.Props.tuneSeconds + " " + "SecondsLower".Translate() + ")"))));
                                    list.Add(new FloatMenuOption(toStringFull, delegate
                                    {
                                        __instance.tuningTimeLeft = ((__instance.tunedTo == null) ? __instance.TuningTimeTicks : __instance.RetuneTimeTicks);
                                        __instance.tuningTo = localPawn;
                                    }));
                                }
                            }
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
}

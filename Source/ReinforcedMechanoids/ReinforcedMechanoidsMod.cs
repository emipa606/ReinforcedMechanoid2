using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids;

internal class ReinforcedMechanoidsMod : Mod
{
    public static ReinforcedMechanoidsSettings settings;

    private float scrollHeight;

    private Vector2 scrollPosition;

    public ReinforcedMechanoidsMod(ModContentPack mcp)
        : base(mcp)
    {
        settings = GetSettings<ReinforcedMechanoidsSettings>();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        ApplySettings();
    }

    public static void ApplySettings()
    {
        RM_DefOf.RM_VanometricMechanoidCell.SetStatBaseValue(StatDefOf.MarketValue,
            ReinforcedMechanoidsSettings.marketValue);
        RM_DefOf.RM_VanometricGenerator.GetCompProperties<CompProperties_Power>().basePowerConsumption =
            0f - ReinforcedMechanoidsSettings.powerOutput;
        if (ReinforcedMechanoidsSettings.dropWeaponOnDeath)
        {
            foreach (var item in DefDatabase<PawnKindDef>.AllDefs.Where(x => x.RaceProps.IsMechanoid))
            {
                item.destroyGearOnDrop = false;
            }
        }

        if (ReinforcedMechanoidsSettings.disableMechDropUponInstallingMechLink)
        {
            var compProperties = ThingDefOf.Mechlink.comps.FirstOrDefault(x =>
                x is CompProperties_UseEffectGiveQuest compProperties_UseEffectGiveQuest &&
                compProperties_UseEffectGiveQuest.quest.root is QuestNode_Root_MechanitorStartingMech);
            if (compProperties != null)
            {
                ThingDefOf.Mechlink.comps.Remove(compProperties);
            }
        }

        foreach (var disabledMechanoid in ReinforcedMechanoidsSettings.disabledMechanoids)
        {
            var pawnKind = DefDatabase<PawnKindDef>.GetNamedSilentFail(disabledMechanoid);
            if (pawnKind == null)
            {
                continue;
            }

            foreach (var allDef in DefDatabase<FactionDef>.AllDefs)
            {
                if (allDef.pawnGroupMakers == null)
                {
                    continue;
                }

                foreach (var pawnGroupMaker in allDef.pawnGroupMakers)
                {
                    pawnGroupMaker.traders?.RemoveAll(x => x.kind == pawnKind);
                    pawnGroupMaker.carriers?.RemoveAll(x => x.kind == pawnKind);
                    pawnGroupMaker.guards?.RemoveAll(x => x.kind == pawnKind);
                    pawnGroupMaker.options?.RemoveAll(x => x.kind == pawnKind);
                }

                allDef.pawnGroupMakers.RemoveAll(delegate(PawnGroupMaker x)
                {
                    var carriers = x.carriers;
                    int result;
                    if (carriers is { Count: 0 })
                    {
                        var traders = x.traders;
                        if (traders is { Count: 0 })
                        {
                            var guards = x.guards;
                            if (guards is { Count: 0 })
                            {
                                var options = x.options;
                                result = options is { Count: 0 } ? 1 : 0;
                                goto IL_005d;
                            }
                        }
                    }

                    result = 0;
                    IL_005d:
                    return (byte)result != 0;
                });
            }

            if (pawnKind == RM_DefOf.RM_Mech_Caretaker &&
                DefDatabase<RaidStrategyDef>.AllDefs.Contains(RM_DefOf.RM_CaretakerRaid))
            {
                DefDatabase<RaidStrategyDef>.Remove(RM_DefOf.RM_CaretakerRaid);
            }

            if (pawnKind == RM_DefOf.RM_Mech_WraithSiege &&
                DefDatabase<RaidStrategyDef>.AllDefs.Contains(RM_DefOf.RM_WraithSiege))
            {
                DefDatabase<RaidStrategyDef>.Remove(RM_DefOf.RM_WraithSiege);
            }

            if (pawnKind == RM_DefOf.RM_Mech_Locust &&
                DefDatabase<RaidStrategyDef>.AllDefs.Contains(RM_DefOf.RM_LocustRaid))
            {
                DefDatabase<RaidStrategyDef>.Remove(RM_DefOf.RM_LocustRaid);
            }
        }
    }

    public override string SettingsCategory()
    {
        return Content.Name;
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        DrawSettings(inRect);
    }

    private void DrawSettings(Rect rect)
    {
        var rect2 = new Rect(rect.x, rect.y, rect.width - 16f, 10000f);
        Widgets.BeginScrollView(viewRect: new Rect(rect.x, rect.y, rect.width - 16f, scrollHeight), outRect: rect,
            scrollPosition: ref scrollPosition);
        scrollHeight = 0f;
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect2);
        listing_Standard.Gap(10f);
        var rect3 = listing_Standard.GetRect(Text.LineHeight);
        var rect4 = rect3.LeftHalf().Rounded();
        var rect5 = rect3.RightHalf().Rounded();
        var rect6 = rect4.LeftHalf().Rounded();
        var rect7 = rect4.RightHalf().Rounded();
        Widgets.Label(rect6, "<b>Power Cell</b> market value");
        Widgets.Label(rect7,
            $"<b>{ReinforcedMechanoidsSettings.marketValue:00}</b> <color=#ababab>(Influence on difficulty)</color>");
        if (Widgets.ButtonText(new Rect(rect5.xMin, rect5.y, rect5.height, rect5.height), "-", true, false) &&
            ReinforcedMechanoidsSettings.marketValue >= 500f)
        {
            ReinforcedMechanoidsSettings.marketValue -= 50f;
        }

        ReinforcedMechanoidsSettings.marketValue = Widgets.HorizontalSlider(
            new Rect(rect5.xMin + rect5.height + 10f, rect5.y, rect5.width - ((rect5.height * 2f) + 20f), rect5.height),
            ReinforcedMechanoidsSettings.marketValue, 500f, 4000f, true);
        if (Widgets.ButtonText(new Rect(rect5.xMax - rect5.height, rect5.y, rect5.height, rect5.height), "+", true,
                false) && ReinforcedMechanoidsSettings.marketValue < 4000f)
        {
            ReinforcedMechanoidsSettings.marketValue += 50f;
        }

        listing_Standard.Gap(10f);
        var rect8 = listing_Standard.GetRect(Text.LineHeight);
        var rect9 = rect8.LeftHalf().Rounded();
        var rect10 = rect8.RightHalf().Rounded();
        var rect11 = rect9.LeftHalf().Rounded();
        var rect12 = rect9.RightHalf().Rounded();
        Widgets.Label(rect11, "<b>Power Cell</b> power output (W)");
        Widgets.Label(rect12,
            $"<b>{ReinforcedMechanoidsSettings.powerOutput:00}W</b> <color=#ababab>(recommended: 5000W)</color>");
        if (Widgets.ButtonText(new Rect(rect10.xMin, rect10.y, rect10.height, rect10.height), "-", true, false) &&
            ReinforcedMechanoidsSettings.powerOutput >= 2000f)
        {
            ReinforcedMechanoidsSettings.powerOutput -= 500f;
        }

        ReinforcedMechanoidsSettings.powerOutput = Widgets.HorizontalSlider(
            new Rect(rect10.xMin + rect10.height + 10f, rect10.y, rect10.width - ((rect10.height * 2f) + 20f),
                rect10.height), ReinforcedMechanoidsSettings.powerOutput, 2000f, 20000f, true);
        if (Widgets.ButtonText(new Rect(rect10.xMax - rect10.height, rect10.y, rect10.height, rect10.height), "+", true,
                false) && ReinforcedMechanoidsSettings.powerOutput < 20000f)
        {
            ReinforcedMechanoidsSettings.powerOutput += 500f;
        }

        listing_Standard.Gap();
        listing_Standard.CheckboxLabeled("Mechanoids will drop weapons upon death",
            ref ReinforcedMechanoidsSettings.dropWeaponOnDeath);
        if (ModsConfig.BiotechActive)
        {
            listing_Standard.CheckboxLabeled("Disable mechanoid drop pod upon installing mechlink",
                ref ReinforcedMechanoidsSettings.disableMechDropUponInstallingMechLink);
        }

        listing_Standard.GapLine();
        listing_Standard.Label("Disable mechanoids from spawning in the game");
        foreach (var allDef in DefDatabase<PawnKindDef>.AllDefs)
        {
            if (!allDef.RaceProps.IsMechanoid)
            {
                continue;
            }

            var checkOn = !ReinforcedMechanoidsSettings.disabledMechanoids.Contains(allDef.defName);
            listing_Standard.CheckboxLabeled(allDef.LabelCap, ref checkOn);
            if (checkOn && ReinforcedMechanoidsSettings.disabledMechanoids.Contains(allDef.defName))
            {
                ReinforcedMechanoidsSettings.disabledMechanoids.Remove(allDef.defName);
            }
            else if (!checkOn && !ReinforcedMechanoidsSettings.disabledMechanoids.Contains(allDef.defName))
            {
                ReinforcedMechanoidsSettings.disabledMechanoids.Add(allDef.defName);
            }
        }

        scrollHeight = listing_Standard.CurHeight;
        listing_Standard.End();
        Widgets.EndScrollView();
    }
}
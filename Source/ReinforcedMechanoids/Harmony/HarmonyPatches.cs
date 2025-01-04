using Verse;

namespace ReinforcedMechanoids.Harmony;

[StaticConstructorOnStartup]
public static class HarmonyPatches
{
    public static readonly HarmonyLib.Harmony harmony;

    static HarmonyPatches()
    {
        harmony = new HarmonyLib.Harmony("ReinforcedMechanoids.Mod");
        harmony.PatchAll();
        ReinforcedMechanoidsMod.ApplySettings();
        //foreach (var allDef in DefDatabase<ThingDef>.AllDefs)
        //{
        //    if (allDef.race is { IsMechanoid: true })
        //    {
        //        allDef.comps.Add(new CompProperties_AllianceOverlayToggle());
        //    }
        //}
    }
}
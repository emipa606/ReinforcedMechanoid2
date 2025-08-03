using System.Reflection;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[StaticConstructorOnStartup]
public static class HarmonyPatches
{
    static HarmonyPatches()
    {
        new HarmonyLib.Harmony("ReinforcedMechanoids.Mod").PatchAll(Assembly.GetExecutingAssembly());
        ReinforcedMechanoidsMod.ApplySettings();
    }
}
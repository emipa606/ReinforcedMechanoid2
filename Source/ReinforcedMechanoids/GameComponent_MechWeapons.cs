using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ReinforcedMechanoids;

public class GameComponent_MechWeapons : GameComponent
{
    private List<Pawn> mechWeaponKeys = [];
    private Dictionary<Pawn, ThingDef> MechWeapons = new Dictionary<Pawn, ThingDef>();
    private List<ThingDef> mechWeaponsValues = [];

    public GameComponent_MechWeapons(Game game)
    {
    }

    public override void GameComponentTick()
    {
        base.GameComponentTick();
        if (GenTicks.IsTickInterval(GenTicks.TickRareInterval))
        {
            MechWeapons = MechWeapons.Where(pair => pair.Key is { Destroyed: false })
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }

    public void SaveWeapon(Pawn pawn, ThingDef thingDef)
    {
        MechWeapons[pawn] = thingDef;
    }

    public ThingWithComps LoadWeapon(Pawn pawn)
    {
        var weaponDef = MechWeapons.GetValueOrDefault(pawn);
        if (weaponDef == null)
        {
            return null;
        }

        return (ThingWithComps)ThingMaker.MakeThing(weaponDef);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref MechWeapons, "MechWeapons", LookMode.Reference, LookMode.Def,
            ref mechWeaponKeys, ref mechWeaponsValues);
    }
}
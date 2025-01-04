using RimWorld;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids;

public class CompLootContainer : ThingComp
{
    public CompProperties_LootContainer Props => props as CompProperties_LootContainer;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (respawningAfterLoad)
        {
            return;
        }

        foreach (var lootTable in Props.lootTables)
        {
            if (!Rand.Chance(lootTable.chance))
            {
                continue;
            }

            var lootOption2 = lootTable.lootOptions.RandomElementByWeight(x => x.weight);
            var num = lootOption2.quantity.RandomInRange;
            while (num > 0)
            {
                var def = lootOption2.GetDef();
                Thing thing2;
                if (def is PawnKindDef pawnKindDef)
                {
                    var applyCryptoEffects = lootOption2.applyCryptoEffects;
                    var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKindDef, null,
                        PawnGenerationContext.NonPlayer, -1, false, false, false, lootOption2.canGeneratePawnRelations,
                        false, 1f, false, true, false, true, true, false, applyCryptoEffects));
                    if (lootOption2.applyCryptoEffects)
                    {
                        if (Rand.Chance(0.25f))
                        {
                            HealthUtility.DamageUntilDowned(pawn);
                        }

                        var thingSetMaker_MapGen_AncientPodContents = new ThingSetMaker_MapGen_AncientPodContents();
                        thingSetMaker_MapGen_AncientPodContents.GiveRandomLootInventoryForTombPawn(pawn);
                    }

                    if (lootOption2.hediffsToApply != null)
                    {
                        foreach (var item in lootOption2.hediffsToApply)
                        {
                            pawn.health.AddHediff(item);
                        }
                    }

                    thing2 = pawn;
                    num--;
                }
                else
                {
                    var thingDef = def as ThingDef;
                    thing2 = ThingMaker.MakeThing(thingDef, lootOption2.stuff ?? GenStuff.RandomStuffFor(thingDef));
                    if (thingDef != null)
                    {
                        var num2 = Mathf.Min(num, thingDef.stackLimit);
                        num -= num2;
                        thing2.stackCount = num2;
                    }
                }

                ProcessThing(thing2, lootOption2);
            }
        }

        return;

        void ProcessThing(Thing thing, LootOption lootOption)
        {
            thing.TryGetComp<CompQuality>()?.SetQuality(lootOption.GetQuality(), ArtGenerationContext.Outsider);
            if (lootOption.faction != null)
            {
                var faction = Find.FactionManager.FirstFactionOfDef(lootOption.faction);
                if (faction != null)
                {
                    thing.SetFaction(faction);
                }
            }
            else if (lootOption.inheritFaction)
            {
                var faction2 = parent.Faction;
                if (faction2 != null)
                {
                    thing.SetFaction(faction2);
                }
            }

            (parent as Building_Casket)?.TryAcceptThing(thing);
        }
    }
}
using RimWorld;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ReinforcedMechanoids
{
    public class ThingDefRecord
    {
        public ThingDef thing;

        public float commonality;
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thing", xmlRoot);
            commonality = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
        }
    }

    public class PawnKindRecord
    {
        public PawnKindDef pawnKind;

        public float commonality;
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "pawnKind", xmlRoot);
            commonality = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
        }
    }

    public class LootOption
    {
        public float weight;
        public ThingCategoryDef category;
        public ThingDef thingDef;
        public PawnKindDef pawnKind;
        public bool applyCryptoEffects;
        public bool canGeneratePawnRelations = true;
        public List<HediffDef> hediffsToApply;
        public List<PawnKindRecord> pawnKinds;
        public FactionDef faction;
        public bool inheritFaction;
        public ThingDef stuff;
        public List<ThingDefRecord> thingDefs;
        public IntRange quantity;
        public List<QualityCategory> allowedQualities;
        public List<QualityCategory> disallowedQualities;
        public Def GetDef()
        {
            if (pawnKind != null)
            {
                return pawnKind;
            }
            else if (pawnKinds != null)
            {
                return pawnKinds.RandomElementByWeight(x => x.commonality).pawnKind;
            }
            if (thingDef != null)
            {
                return thingDef;
            }
            else if (thingDefs != null)
            {
                return thingDefs.RandomElementByWeight(x => x.commonality).thing;
            }
            return category.DescendantThingDefs.RandomElement();
        }

        public QualityCategory GetQuality()
        {
            var qualities = new List<QualityCategory>();
            if (allowedQualities != null)
            {
                qualities.AddRange(allowedQualities);
            }
            else
            {
                qualities.AddRange(Enum.GetValues(typeof(QualityCategory)).Cast<QualityCategory>());
            }
            if (disallowedQualities != null)
            {
                qualities.RemoveAll(x => disallowedQualities.Contains(x));
            }
            return qualities.RandomElement();
        }
    }
    public class LootTable
    {
        public float chance;
        public List<LootOption> lootOptions;
    }
    public class CompProperties_LootContainer : CompProperties
    {
        public List<LootTable> lootTables;

        public int openingTicks = 100;
        public CompProperties_LootContainer()
        {
            this.compClass = typeof(CompLootContainer);
        }
    }
    public class CompLootContainer : ThingComp
    {
        public CompProperties_LootContainer Props => base.props as CompProperties_LootContainer;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                foreach (var lootTable in Props.lootTables)
                {
                    if (Rand.Chance(lootTable.chance))
                    {
                        var lootOption = lootTable.lootOptions.RandomElementByWeight(x => x.weight);
                        var quantityAmount = lootOption.quantity.RandomInRange;
                        while (quantityAmount > 0)
                        {
                            var def = lootOption.GetDef();
                            ThingDef thingDef = null;
                            Thing thing = null;
                            if (def is PawnKindDef pawnKind)
                            {
                                var pawn = PawnGenerator.GeneratePawn(new 
                                    PawnGenerationRequest(pawnKind, certainlyBeenInCryptosleep: lootOption.applyCryptoEffects, canGeneratePawnRelations: lootOption.canGeneratePawnRelations));
                                if (lootOption.applyCryptoEffects)
                                {
                                    if (Rand.Chance(0.25f))
                                    {
                                        HealthUtility.DamageUntilDowned(pawn);
                                    }
                                    var set = new ThingSetMaker_MapGen_AncientPodContents();
                                    set.GiveRandomLootInventoryForTombPawn(pawn);
                                }

                                if (lootOption.hediffsToApply != null)
                                {
                                    foreach (var hediffDef in lootOption.hediffsToApply)
                                    {
                                        pawn.health.AddHediff(hediffDef);
                                    }
                                }
                                thing = pawn;
                                thingDef = pawnKind.race;
                                quantityAmount--;
                            }
                            else
                            {
                                thingDef = def as ThingDef;
                                thing = ThingMaker.MakeThing(thingDef, lootOption.stuff ?? 
                                    GenStuff.RandomStuffFor(thingDef));
                                var newStack = Mathf.Min(quantityAmount, thingDef.stackLimit);
                                quantityAmount -= newStack;
                                thing.stackCount = newStack;
                            }
                            ProcessThing(thing, lootOption);
                        }
                    }
                }
            }

            void ProcessThing(Thing thing, LootOption lootOption)
            {
                var qualityComp = thing.TryGetComp<CompQuality>();
                if (qualityComp != null)
                {
                    qualityComp.SetQuality(lootOption.GetQuality(), ArtGenerationContext.Outsider);
                }
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
                    var faction = this.parent.Faction;
                    if (faction != null)
                    {
                        thing.SetFaction(faction);
                    }
                }
                (this.parent as Building_Casket).TryAcceptThing(thing);
            }
        }
    }

    public class Building_Container : Building_Casket
    {
        private static List<Pawn> tmpAllowedPawns = new List<Pawn>();
        public CompProperties_LootContainer Props => this.def.GetCompProperties<CompProperties_LootContainer>();
        public override int OpenTicks => Props.openingTicks;
        public override void EjectContents()
        {
            innerContainer.TryDropAll(this.InteractionCell, base.Map, ThingPlaceMode.Near, null, (IntVec3 c) => c.GetEdifice(base.Map) == null);
            contentsKnown = true;
            if (def.building.openingEffect != null)
            {
                Effecter effecter = def.building.openingEffect.Spawn();
                effecter.Trigger(new TargetInfo(base.Position, base.Map), null);
                effecter.Cleanup();
            }
        }
        public override void Open()
        {
            if (CanOpen)
            {
                base.Open();
            }
        }

        public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(List<Pawn> selPawns)
        {
            foreach (FloatMenuOption multiSelectFloatMenuOption in base.GetMultiSelectFloatMenuOptions(selPawns))
            {
                yield return multiSelectFloatMenuOption;
            }
            if (!CanOpen)
            {
                yield break;
            }
            tmpAllowedPawns.Clear();
            for (int i = 0; i < selPawns.Count; i++)
            {
                if (selPawns[i].CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
                {
                    tmpAllowedPawns.Add(selPawns[i]);
                }
            }
            if (tmpAllowedPawns.Count <= 0)
            {
                yield return new FloatMenuOption("CannotOpen".Translate(this) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                yield break;
            }
            tmpAllowedPawns.Clear();
            yield return new FloatMenuOption("Open".Translate(this), delegate
            {
                tmpAllowedPawns[0].jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Open, this), JobTag.Misc);
                for (int l = 1; l < tmpAllowedPawns.Count; l++)
                {
                    FloatMenuMakerMap.PawnGotoAction(base.Position, tmpAllowedPawns[l], RCellFinder.BestOrderedGotoDestNear(base.Position, tmpAllowedPawns[l]));
                }
            });
        }
    }
}

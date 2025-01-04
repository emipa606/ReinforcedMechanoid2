using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ReinforcedMechanoids;

public class LootOption
{
    public readonly bool canGeneratePawnRelations = true;

    public List<QualityCategory> allowedQualities;

    public bool applyCryptoEffects;

    public ThingCategoryDef category;

    public List<QualityCategory> disallowedQualities;

    public FactionDef faction;

    public List<HediffDef> hediffsToApply;

    public bool inheritFaction;

    public PawnKindDef pawnKind;

    public List<PawnKindRecord> pawnKinds;

    public IntRange quantity;

    public ThingDef stuff;

    public ThingDef thingDef;

    public List<ThingDefRecord> thingDefs;
    public float weight;

    public Def GetDef()
    {
        if (pawnKind != null)
        {
            return pawnKind;
        }

        if (pawnKinds != null)
        {
            return pawnKinds.RandomElementByWeight(x => x.commonality).pawnKind;
        }

        if (thingDef != null)
        {
            return thingDef;
        }

        return thingDefs != null
            ? thingDefs.RandomElementByWeight(x => x.commonality).thing
            : category.DescendantThingDefs.RandomElement();
    }

    public QualityCategory GetQuality()
    {
        var list = new List<QualityCategory>();
        if (allowedQualities != null)
        {
            list.AddRange(allowedQualities);
        }
        else
        {
            list.AddRange(Enum.GetValues(typeof(QualityCategory)).Cast<QualityCategory>());
        }

        if (disallowedQualities != null)
        {
            list.RemoveAll(x => disallowedQualities.Contains(x));
        }

        return list.RandomElement();
    }
}
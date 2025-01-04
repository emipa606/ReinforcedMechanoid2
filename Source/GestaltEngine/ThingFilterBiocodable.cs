using RimWorld;
using Verse;

namespace GestaltEngine;

public class ThingFilterBiocodable : ThingFilter
{
    public override bool Allows(Thing t)
    {
        var compBiocodable = t.TryGetComp<CompBiocodable>();
        return compBiocodable is { Biocoded: true };
    }
}
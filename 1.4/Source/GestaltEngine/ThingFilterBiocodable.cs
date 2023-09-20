using RimWorld;
using Verse;

namespace GestaltEngine
{
    public class ThingFilterBiocodable : ThingFilter
    {
        public override bool Allows(Thing t)
        {
            var comp = t.TryGetComp<CompBiocodable>();
            if (comp != null && comp.Biocoded)
            {
                return true;
            }
            return false;
        }
    }
}

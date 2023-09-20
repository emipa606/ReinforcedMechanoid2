using RimWorld.BaseGen;
using System.Collections.Generic;
using Verse;

namespace ReinforcedMechanoids
{
    public class GenStep_CustomResolvers : GenStep
    {
        public List<string> symbolResolvers = new List<string>();
        public override int SeedPart => 514251654;
        public override void Generate(Map map, GenStepParams parms)
        {
            CellRect cellRect = CellRect.CenteredOn(map.Center, 50, 50);
            var rp = new ResolveParams
            {
                faction = map.ParentFaction,
                rect = cellRect
            };
            for (int i = 0; i < symbolResolvers.Count; i++)
            {
                BaseGen.symbolStack.Push(symbolResolvers[i], rp, null);
            }
        }
    }
}

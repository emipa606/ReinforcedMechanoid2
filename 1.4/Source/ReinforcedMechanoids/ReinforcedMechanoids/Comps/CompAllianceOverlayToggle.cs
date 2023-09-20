using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids
{
    public class CompProperties_AllianceOverlayToggle : CompProperties
    {
        public CompProperties_AllianceOverlayToggle()
        {
            this.compClass = typeof(CompAllianceOverlayToggle);
        }
    }
    public class CompAllianceOverlayToggle : ThingComp
    {
        public bool isActive = true;
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            var pawn = this.parent as Pawn;
            if (pawn.Faction == Faction.OfPlayer)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "RM.ToggleAllegianceOverlay".Translate(),
                    defaultDesc = "RM.ToggleAllegianceOverlayDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/ToggleAllegianceOverlays"),
                    isActive = () => isActive,
                    toggleAction = delegate
                    {
                        isActive = !isActive;
                        PortraitsCache.SetDirty(pawn);
                    }
                };
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isActive, "isActive", true);
        }
    }
}


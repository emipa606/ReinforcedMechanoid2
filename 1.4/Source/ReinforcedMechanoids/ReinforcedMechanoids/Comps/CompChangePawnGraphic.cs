using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids
{
    public class PawnGraphics
    {
        public List<PawnGraphic> pawnGraphics;
    }
    public class PawnGraphic
    {
        public BodyPartDef missingPart;
        public float healthPctThreshold = -1;
        public GraphicData bodyGraphicData;
    }
    public class CompProperties_ChangeGraphic : CompProperties
    {
        public List<PawnGraphics> graphicsPerLifestages;
        public CompProperties_ChangeGraphic()
        {
            this.compClass = typeof(CompChangePawnGraphic);
        }
    }

    [StaticConstructorOnStartup]
    public class CompChangePawnGraphic : ThingComp
    {
        public Pawn Pawn => this.parent as Pawn;
        public PawnRenderer PawnRenderer => Pawn.Drawer.renderer;
        public CompProperties_ChangeGraphic Props => base.props as CompProperties_ChangeGraphic;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.TryChangeGraphic();
        }

        public void TryChangeGraphic()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                var pawn = Pawn;
                var pawnGraphics = Props.graphicsPerLifestages[pawn.ageTracker.CurLifeStageIndex];
                bool changedGraphic = false;
                foreach (var pawnGraphic in pawnGraphics.pawnGraphics)
                {
                    if (pawnGraphic.missingPart != null)
                    {
                        var bodyPart = Utils.GetNonMissingBodyPart(pawn, pawnGraphic.missingPart);
                        if (bodyPart is null)
                        {
                            ChangeGraphic(pawnGraphic.bodyGraphicData);
                            changedGraphic = true;
                        }
                    }
                    if (pawnGraphic.healthPctThreshold != -1)
                    {
                        var healthPct = pawn.health.summaryHealth.SummaryHealthPercent;
                        if (pawnGraphic.healthPctThreshold > healthPct)
                        {
                            ChangeGraphic(pawnGraphic.bodyGraphicData);
                            changedGraphic = true;
                        }
                    }
                }

                if (changedGraphic is false && pawn.Drawer.renderer.graphics.nakedGraphic
                    != pawn.ageTracker.CurKindLifeStage.bodyGraphicData.Graphic)
                {
                    ChangeGraphic(pawn.ageTracker.CurKindLifeStage.bodyGraphicData);
                }
            });
        }

        private void ChangeGraphic(GraphicData graphicData)
        {
            var nakedGraphic = graphicData.Graphic;
            PawnRenderer.graphics.ResolveAllGraphics();
            PawnRenderer.graphics.nakedGraphic = nakedGraphic;
        }
    }
}

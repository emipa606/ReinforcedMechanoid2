using Verse;

namespace ReinforcedMechanoids;

[StaticConstructorOnStartup]
public class CompChangePawnGraphic : ThingComp
{
    private Pawn Pawn => parent as Pawn;

    private PawnRenderer PawnRenderer => Pawn.Drawer.renderer;

    private CompProperties_ChangeGraphic Props => props as CompProperties_ChangeGraphic;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        TryChangeGraphic();
    }

    public void TryChangeGraphic()
    {
        LongEventHandler.ExecuteWhenFinished(delegate
        {
            var pawn = Pawn;
            var pawnGraphics = Props.graphicsPerLifestages[pawn.ageTracker.CurLifeStageIndex];
            var graphicChange = false;
            foreach (var pawnGraphic in pawnGraphics.pawnGraphics)
            {
                if (pawnGraphic.missingPart != null)
                {
                    var nonMissingBodyPart = Utils.GetNonMissingBodyPart(pawn, pawnGraphic.missingPart);
                    if (nonMissingBodyPart == null)
                    {
                        ChangeGraphic();
                        graphicChange = true;
                    }
                }

                if (pawnGraphic.healthPctThreshold == -1f)
                {
                    continue;
                }

                var summaryHealthPercent = pawn.health.summaryHealth.SummaryHealthPercent;
                if (!(pawnGraphic.healthPctThreshold > summaryHealthPercent))
                {
                    continue;
                }

                ChangeGraphic();
                graphicChange = true;
            }

            if (!graphicChange && pawn.Drawer.renderer.BodyGraphic !=
                pawn.ageTracker.CurKindLifeStage.bodyGraphicData.Graphic)
            {
                ChangeGraphic();
            }
        });
    }

    private void ChangeGraphic()
    {
        PawnRenderer.EnsureGraphicsInitialized();
        PawnRenderer.SetAllGraphicsDirty();
        //var graphic = graphicData.Graphic;
        //PawnRenderer.graphics.ResolveAllGraphics();
        //PawnRenderer.graphics.nakedGraphic = graphic;
    }
}
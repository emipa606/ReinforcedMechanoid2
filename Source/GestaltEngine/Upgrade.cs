using System.Collections.Generic;
using Verse;

namespace GestaltEngine;

public class Upgrade
{
    public readonly int downgradeCooldownTicks = -1;

    public readonly int downgradeDurationTicks = -1;

    public readonly List<RecipeDef> unlockedRecipes = [];

    public float heatPerSecond;

    public GraphicData overlayGraphic;

    public float powerConsumption;

    public float researchPointsPerSecond;

    public int totalControlGroups;

    public int totalMechBandwidth;
    public int upgradeCooldownTicks;

    public int upgradeDurationTicks;
}
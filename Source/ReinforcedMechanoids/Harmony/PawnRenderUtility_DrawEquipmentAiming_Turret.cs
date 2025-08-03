using HarmonyLib;
using UnityEngine;
using VEF;
using Verse;

namespace ReinforcedMechanoids.Harmony;

[HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAiming))]
public static class PawnRenderUtility_DrawEquipmentAiming_Turret
{
    private static readonly Vector3 south = new(0, 0, -0.33f);
    private static readonly Vector3 north = new(0, -1, -0.22f);
    private static readonly Vector3 east = new(0.2f, 0f, -0.22f);
    private static readonly Vector3 west = new(-0.2f, 0, -0.22f);

    public static bool Prefix(Thing eq, out (Pawn pawn, CompMachine comp) __state)
    {
        __state = default;
        var pawn = (eq.ParentHolder as Pawn_EquipmentTracker)?.pawn;
        if (pawn == null || CompMachine.cachedMachinesPawns?.TryGetValue(pawn, out var compMachine) == null ||
            compMachine == null)
        {
            return true;
        }

        if (compMachine.turretAttached == null)
        {
            return true;
        }

        __state.comp = compMachine;
        __state.pawn = pawn;
        return false;
    }

    public static void Postfix(Thing eq, Vector3 drawLoc, float aimAngle, (Pawn pawn, CompMachine comp) __state)
    {
        if (__state == default)
        {
            return;
        }

        if (__state.pawn.stances.curStance is not Stance_Busy { focusTarg.IsValid: true })
        {
            aimAngle = __state.comp.turretAngle;
        }

        if (__state.pawn.Rotation == Rot4.South)
        {
            drawLoc -= south;
        }
        else if (__state.pawn.Rotation == Rot4.North)
        {
            drawLoc -= north;
        }
        else if (__state.pawn.Rotation == Rot4.East)
        {
            drawLoc -= east;
        }
        else if (__state.pawn.Rotation == Rot4.West)
        {
            drawLoc -= west;
        }

        Mesh mesh;
        var num = aimAngle - 90f;
        switch (aimAngle)
        {
            case > 20f and < 160f:
                mesh = MeshPool.plane20;
                num += eq.def.equippedAngleOffset;
                break;
            case > 200f and < 340f:
                mesh = Startup.plane20Flip;
                num -= 180f;
                num -= eq.def.equippedAngleOffset;
                break;
            default:
                mesh = MeshPool.plane20;
                num += eq.def.equippedAngleOffset;
                break;
        }

        num %= 360f;
        Graphics.DrawMesh(
            material: eq.Graphic is not Graphic_StackCount graphic_StackCount
                ? eq.Graphic.MatSingle
                : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle, mesh: mesh, position: drawLoc,
            rotation: Quaternion.AngleAxis(num, Vector3.up), layer: 0);
    }
}